# AA01 · Módulo integral de autenticación y seguridad

**Materia:** Programación Web I · ITS Cordillera · Paralelo 3AN
**Autor:** Bolívar Flores
**Repositorio:** `WEB-3AN-B1-AA01-AutenticacionSegura-FloresBolivar`

Aplicación web ASP.NET Core MVC con registro, inicio de sesión, recuperación de contraseña por correo real, perfil, y administración de usuarios con roles, usando ASP.NET Core Identity y SQL Server.

## Tecnologías

- ASP.NET Core MVC (.NET 11) y Razor
- Entity Framework Core + SQL Server
- ASP.NET Core Identity (UserManager, SignInManager, PasswordHasher)
- MailKit (envío de correo real por SMTP)
- Arquitectura en 3 capas

## Arquitectura

| Proyecto | Capa | Responsabilidad |
|---|---|---|
| `AutenticacionSegura.Web` | Presentación | Controladores, vistas, configuración (`Program.cs`) |
| `AutenticacionSegura.Negocio` | Negocio | Servicios y reglas (autenticación, recuperación, perfil, administración, correo) |
| `AutenticacionSegura.Datos` | Datos | Entidades, `DbContext`, migraciones, creación del administrador inicial |

Dependencias: `Web → Negocio → Datos`. El correo se define con la interfaz `IEmailService` (Negocio) y se implementa por SMTP.

## Funcionalidades

- Registro de clientes con validación de contraseña (mínimo 8, mayúscula, minúscula, número y símbolo).
- Inicio y cierre de sesión con cookie; opción "Recordarme".
- Recuperación de contraseña por correo real, con enlace de un solo uso y vencimiento (30 min, configurable).
- Perfil: editar nombre y cambiar contraseña (exige la contraseña actual).
- Roles **Cliente** y **Administrador**. El administrador lista usuarios, cambia roles y habilita o deshabilita cuentas.
- Control de acceso en el servidor con `[Authorize]` y página de acceso denegado (403).

## Medidas de seguridad

- Contraseñas con hash (PasswordHasher de Identity); nunca en texto plano.
- Token antifalsificación (`[ValidateAntiForgeryToken]`) en todos los formularios POST.
- Bloqueo temporal: 5 intentos fallidos, 5 minutos. Mensaje de error genérico en el login.
- La recuperación de contraseña responde igual exista o no el correo.
- Cookie de autenticación `AutenticacionSegura.Auth`: `HttpOnly`, `Secure`, `SameSite=Lax`.
  - Sin "Recordarme": cookie **de sesión** (se borra al cerrar el navegador).
  - Con "Recordarme": cookie **persistente** de 7 días.
- Al cambiar el rol o deshabilitar una cuenta se invalidan sus sesiones abiertas.
- No se puede quitar el rol ni deshabilitar al **último administrador activo**.
- Un Cliente no puede asignarse el rol Administrador (se rechaza en el servidor).
- Secretos fuera del repositorio (User Secrets).

## Requisitos

- .NET SDK 11
- SQL Server (probado con instancia local y autenticación de Windows)
- Visual Studio 2026 (opcional)
- Una cuenta de Gmail **de prueba** con verificación en dos pasos y una *contraseña de aplicación* (para enviar correos)

## Cómo ejecutar

1. Clonar el repositorio y entrar a la carpeta.

2. **Base de datos.** Revisar la cadena de conexión en `AutenticacionSegura.Web/appsettings.json` (`ConnectionStrings:ConexionPrincipal`) y crear la base con **una** de estas opciones:

   ```powershell
   dotnet ef database update --project AutenticacionSegura.Datos --startup-project AutenticacionSegura.Web
   ```

   o ejecutar el script `SQL/crear-base-de-datos.sql` en SQL Server.

3. **Secretos** (no se guardan en el repositorio). Desde la carpeta del repositorio:

   ```powershell
   dotnet user-secrets set "SemillaAdmin:Correo" "admin@autenticacionsegura.local" --project AutenticacionSegura.Web
   dotnet user-secrets set "SemillaAdmin:Clave" "<contraseña-del-administrador>" --project AutenticacionSegura.Web
   dotnet user-secrets set "Smtp:Usuario" "<correo-gmail-de-prueba>" --project AutenticacionSegura.Web
   dotnet user-secrets set "Smtp:Clave" "<contraseña-de-aplicación-de-16-caracteres>" --project AutenticacionSegura.Web
   ```

   La contraseña del administrador debe cumplir las reglas (8+ caracteres, mayúscula, minúscula, número y símbolo).

4. **Ejecutar:**

   ```powershell
   dotnet run --launch-profile https --project AutenticacionSegura.Web
   ```

   Abrir `https://localhost:7079`. Al arrancar, la aplicación crea el administrador inicial si no existe.

## Configuración de correo (sin secretos)

En `appsettings.json` solo van datos no sensibles:

```json
"Smtp": {
  "Servidor": "smtp.gmail.com",
  "Puerto": 587,
  "UsarStartTls": true,
  "NombreRemitente": "AutenticacionSegura"
}
```

`Smtp:Usuario` y `Smtp:Clave` se guardan **solo** en User Secrets (paso 3). La vigencia del enlace de recuperación se ajusta en `Autenticacion:VigenciaTokenRecuperacionMinutos`.

## Usuarios de prueba (ficticios)

| Correo | Rol | Cómo se obtiene |
|---|---|---|
| `admin@autenticacionsegura.local` | Administrador | Lo crea la aplicación con los secretos `SemillaAdmin:*` |
| `cliente1@prueba.local` | Cliente | Registrarse desde `/Cuenta/Registro` |
| `cliente2@prueba.local` | Cliente | Registrarse desde `/Cuenta/Registro` |

Para probar la recuperación de contraseña se necesita registrar una cuenta con un **correo real** al que se tenga acceso.

## Pruebas realizadas (14 mínimas)

| # | Prueba | Resultado |
|---|---|---|
| 1 | Registro válido de un Cliente | Correcto |
| 2 | Correo duplicado y contraseña inválida rechazados | Correcto |
| 3 | Inicio y cierre de sesión | Correcto |
| 4 | Contraseña incorrecta repetida: error genérico y bloqueo temporal | Correcto |
| 5 | Recuperación por correo real | Correcto |
| 6 | Restablecer contraseña con token válido | Correcto |
| 7 | Token inventado, reutilizado y vencido rechazados | Correcto |
| 8 | Cliente en la URL de administración: acceso denegado | Correcto |
| 9 | Administrador cambia rol y estado | Correcto |
| 10 | Cliente intenta asignar Administrador: rechazado | Correcto |
| 11 | Cambio de contraseña con la actual incorrecta: no se modifica | Correcto |
| 12 | Recargar y otra pestaña mantienen la sesión | Correcto |
| 13 | Ventana de incógnito no hereda la sesión | Correcto |
| 14 | "Recordarme" (cookie persistente) y cierre de sesión | Correcto |

## Uso de inteligencia artificial

El detalle del uso de IA (prompts y fuentes) se documenta en el PDF entregado junto con este repositorio.
