using AutenticacionSegura.Negocio.Correo;
using AutenticacionSegura.Datos.Contexto;   // AppDbContext
using AutenticacionSegura.Datos.Entidades;  // Usuario
using Microsoft.AspNetCore.Identity;        // IdentityRole, AddIdentity
using Microsoft.EntityFrameworkCore;        // UseSqlServer
using AutenticacionSegura.Datos.Inicializacion;  // InicializadorDatos (seeding)
using AutenticacionSegura.Datos.Identidad;    // DescriptorErroresIdentity
using AutenticacionSegura.Negocio.Servicios;  // IServicioAutenticacion

// Crea el "constructor" de la aplicación: aquí se registran los servicios
// (inyección de dependencias) antes de arrancar el servidor.
var builder = WebApplication.CreateBuilder(args);

// Habilita controladores y vistas Razor (la capa de Presentación MVC).
builder.Services.AddControllersWithViews();
// --- Servicio de correo (SMTP) ---
// Une la sección "Smtp" de appsettings.json (servidor, puerto, remitente)
// con User Secrets (Smtp:Usuario y Smtp:Clave) en un objeto ConfiguracionSmtp.
builder.Services.Configure<ConfiguracionSmtp>(
    builder.Configuration.GetSection(ConfiguracionSmtp.NombreSeccion));

// Cuando algún servicio pida IEmailService, se le entrega ServicioCorreoSmtp.
builder.Services.AddScoped<IEmailService, ServicioCorreoSmtp>();

// Lee la cadena de conexión desde appsettings.json.
// Si no existe, detiene el arranque con un mensaje claro en lugar de fallar después.
var cadenaConexion = builder.Configuration.GetConnectionString("ConexionPrincipal")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'ConexionPrincipal'.");

// Registra AppDbContext para usar SQL Server.
// MigrationsAssembly: las migraciones se guardan en el proyecto Datos
// (donde vive el contexto) y no en Web, para respetar las 3 capas.
builder.Services.AddDbContext<AppDbContext>(opciones =>
    opciones.UseSqlServer(
        cadenaConexion,
        sql => sql.MigrationsAssembly("AutenticacionSegura.Datos")));

// Registra ASP.NET Core Identity:
//  - Usuario / IdentityRole: nuestros usuarios y los roles (Cliente, Administrador)
//  - AddEntityFrameworkStores: guarda usuarios y roles en SQL Server mediante AppDbContext
//  - AddDefaultTokenProviders: genera los tokens (por ejemplo, para recuperar contraseña)
// Las reglas de contraseña, bloqueo y cookies se configuran en pasos posteriores.
// Registra ASP.NET Core Identity con la política de seguridad de la actividad.
builder.Services
    .AddIdentity<Usuario, IdentityRole>(opciones =>
    {
        // Política de contraseña razonable
        opciones.Password.RequiredLength = 8;          // mínimo 8 caracteres
        opciones.Password.RequireDigit = true;         // al menos un número
        opciones.Password.RequireLowercase = true;     // al menos una minúscula
        opciones.Password.RequireUppercase = true;     // al menos una mayúscula
        opciones.Password.RequireNonAlphanumeric = true; // al menos un símbolo
       

        // Correo único: no se permiten dos cuentas con el mismo correo
        opciones.User.RequireUniqueEmail = true;

        // Bloqueo temporal por intentos fallidos de inicio de sesión
        opciones.Lockout.MaxFailedAccessAttempts = 5;                       // 5 intentos fallidos...
        opciones.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);  // ...bloquean la cuenta 5 minutos
        opciones.Lockout.AllowedForNewUsers = true;                         // aplica también a cuentas nuevas
    })

    .AddEntityFrameworkStores<AppDbContext>()      // guarda usuarios y roles en SQL Server
    .AddDefaultTokenProviders()                    // tokens (recuperar contraseña)
    .AddErrorDescriber<DescriptorErroresIdentity>(); // mensajes de error en español
// Registra el servicio de la capa de Negocio. "Scoped" = una instancia por petición web.
builder.Services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();

// Configuración de la cookie de autenticación.
// La vigencia (en minutos) se lee de appsettings.json; si falta, usa 7 días.
var vigenciaCookieMinutos = builder.Configuration.GetValue("Autenticacion:VigenciaCookieMinutos", 10080);

builder.Services.ConfigureApplicationCookie(opciones =>
{
    opciones.Cookie.Name = "AutenticacionSegura.Auth";
    opciones.Cookie.HttpOnly = true;                          // JavaScript no puede leer la cookie
    opciones.Cookie.SecurePolicy = CookieSecurePolicy.Always; // solo viaja por HTTPS
    opciones.Cookie.SameSite = SameSiteMode.Lax;              // no se envía en peticiones desde otros sitios (protege contra CSRF)

    opciones.ExpireTimeSpan = TimeSpan.FromMinutes(vigenciaCookieMinutos);
    opciones.SlidingExpiration = false;                       // vencimiento fijo, fácil de comprobar en las pruebas

    opciones.LoginPath = "/Cuenta/Login";                     // a dónde redirige si no hay sesión
    opciones.AccessDeniedPath = "/Cuenta/AccesoDenegado";     // a dónde redirige si no tiene permiso
    opciones.LogoutPath = "/Cuenta/Logout";
});

// La cookie del token antiforgery (formularios POST) con protecciones similares.
builder.Services.AddAntiforgery(opciones =>
{
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    opciones.Cookie.SameSite = SameSiteMode.Strict;
});

// Construye la aplicación con todo lo registrado arriba.
var app = builder.Build();

// En producción muestra una página de error amigable y activa HSTS
// (obliga al navegador a usar siempre HTTPS).
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();  // Redirige HTTP a HTTPS
app.UseRouting();           // Habilita el enrutamiento de URLs

// ORDEN IMPORTANTE: primero se identifica quién es el usuario (Authentication)
// y después se decide qué puede hacer (Authorization).
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();      // Sirve CSS, JS e imágenes de wwwroot

// Ruta por defecto: /Controlador/Accion/id (por ejemplo, /Home/Index)
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
// SEEDING: al arrancar, crea los roles y el administrador inicial si no existen.
// El correo y la clave se leen de User Secrets ("SemillaAdmin"), nunca del código.
await InicializadorDatos.EjecutarAsync(
    app.Services,
    builder.Configuration["SemillaAdmin:Correo"],
    builder.Configuration["SemillaAdmin:Clave"]);

app.Run();  // Inicia el servidor web