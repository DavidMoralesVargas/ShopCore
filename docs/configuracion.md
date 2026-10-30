# Configuración por entorno y manejo de secretos

Este documento explica de dónde toma cada valor la aplicación ShopCore y cómo cargar los secretos **sin escribirlos nunca en el repositorio**.

> Los secretos (`jwtKey`, `Cloudinary:ApiKey`, `Cloudinary:ApiSecret`, cadenas de conexión con contraseña) **no se versionan**. Si alguna vez se ven en un archivo del repo, en un PR, en una captura o en el video, hay que rotarlos.

## 1. Cargar los secretos en desarrollo (User Secrets)

Los User Secrets guardan los valores en tu propio equipo (en `%APPDATA%\Microsoft\UserSecrets` en Windows), fuera de la carpeta del repositorio. .NET los carga solo cuando `ASPNETCORE_ENVIRONMENT=Development`, que es lo que usa el perfil `https` de `launchSettings.json`.

El proyecto ya tiene `UserSecretsId`, así que basta con ejecutar, desde la raíz del repositorio:

```bash
dotnet user-secrets set "jwtKey" "<clave-larga-aleatoria>" --project Tienda_Online/Tienda_Online.Backend
dotnet user-secrets set "Cloudinary:ApiKey" "<api-key>" --project Tienda_Online/Tienda_Online.Backend
dotnet user-secrets set "Cloudinary:ApiSecret" "<api-secret>" --project Tienda_Online/Tienda_Online.Backend
```

Para generar una `jwtKey` aleatoria y guardarla sin mostrarla en pantalla (Git Bash):

```bash
dotnet user-secrets set "jwtKey" "$(openssl rand -base64 64 | tr -d '\n')" --project Tienda_Online/Tienda_Online.Backend
```

Para comprobar que quedaron las tres claves:

```bash
dotnet user-secrets list --project Tienda_Online/Tienda_Online.Backend
```

> Los valores de `Cloudinary:ApiKey` y `Cloudinary:ApiSecret` los entrega el dueño de la cuenta de Cloudinary por un canal privado del equipo. Nunca por el repositorio, issues ni Pull Requests.

## 2. Qué se versiona y qué no

| Archivo | ¿Se versiona? | Contenido |
|---|---|---|
| `Backend/appsettings.json` | Sí | Logging y `AllowedHosts` |
| `Backend/appsettings.Development.json` | Sí | Connection string de LocalDB, `Url Frontend`, `Cloudinary:CloudName`. **Sin secretos** |
| `Backend/appsettings.Production.json` | Sí | Solo `Cloudinary:CloudName` (no es un secreto). **Sin secretos** |
| User Secrets (`jwtKey`, `Cloudinary:ApiKey`, `Cloudinary:ApiSecret`) | **No** (quedan en el equipo de cada desarrollador) | Secretos de desarrollo |
| `Frontend/wwwroot/appsettings.json` | Sí | `ApiUrl` por defecto (`http://localhost:8081`, el sitio de la API en IIS) |
| `Frontend/wwwroot/appsettings.Development.json` | Sí | `ApiUrl` para desarrollo (`https://localhost:7000`) |

## 3. Configuración por entorno (API)

| Clave | Development (local) | IIS | Azure | ¿Secreto? |
|---|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` (variable del pool) | `Production` (App Setting) | No |
| `App:Version` | — (muestra `sin-configurar`) | `1.0.0-iis` (variable del pool) | `1.0.0` (App Setting `App__Version`) | No |
| `ConnectionStrings:DefaultConnection` | LocalDB (`appsettings.Development.json`) | SQL Server Express (variable del pool) | Azure SQL (*Connection strings*, tipo `SQLAzure`) | **Sí** en Azure |
| `jwtKey` | User Secrets | Variable del pool | App Setting | **Sí** |
| `Cloudinary:ApiKey` / `Cloudinary:ApiSecret` | User Secrets | Variables del pool | App Settings `Cloudinary__ApiKey` / `Cloudinary__ApiSecret` | **Sí** |
| `Cloudinary:CloudName` | `appsettings.Development.json` | `appsettings.Production.json` | `appsettings.Production.json` | No |
| `Mail:*`, `Url Frontend` | — | — | — | No se usan (correo fuera de alcance) |

En ASP.NET Core, `:` se escribe como `__` (doble guion bajo) en las variables de entorno y en los App Settings. Por ejemplo, `Cloudinary__ApiSecret` equivale a `Cloudinary:ApiSecret`.

## 4. Configuración por entorno (Frontend)

El Frontend es Blazor WebAssembly: corre en el navegador del usuario, por eso **no puede leer variables de entorno del servidor**. Su configuración viaja como archivos estáticos en `wwwroot/` y se descarga junto con la aplicación.

| Archivo | `ApiUrl` | Cuándo se usa |
|---|---|---|
| `wwwroot/appsettings.Development.json` | `https://localhost:7000` | Desarrollo con `dotnet run` (entorno `Development`) |
| `wwwroot/appsettings.json` | `http://localhost:8081` | Lo que se publica en IIS |
| Azure | URL de la API en Azure | El workflow de despliegue reemplaza el valor de `appsettings.json` al publicar |

En `Frontend/Program.cs` la URL se lee con `builder.Configuration["ApiUrl"]`, y si no existe se usa `https://localhost:7000` como valor de respaldo.

## 5. Endpoints de verificación (`/health`)

| Componente | URL (desarrollo) | Respuesta esperada |
|---|---|---|
| API | `https://localhost:7000/health` | HTTP 200 con JSON: `status`, `database`, `environment`, `version`, `utc`. Si la base de datos no responde: HTTP 503 y `status: DEGRADED` |
| Frontend | `https://localhost:8000/health` | Página "Frontend: OK" y el estado que devuelve la API |

## 6. Alcance: el correo no se usa

El envío de correos (confirmación de cuenta y recuperación de contraseña) está **fuera del alcance** de esta práctica, por lo que no se configuran `Mail:*` ni `Url Frontend` en ningún entorno de despliegue. Consecuencia: el registro de usuarios nuevos y la recuperación de contraseña no funcionan, porque la API exige correo confirmado. La verificación de la aplicación se hace con el **usuario administrador que crea el seed** (`Data/SeedDb.cs`), que ya viene confirmado.

## 7. Base de datos

La API crea el esquema al arrancar con `EnsureCreatedAsync()` (en el seed) si la base de datos está vacía. En desarrollo usa la base `Orders` de LocalDB. Si la base de datos no está disponible o falta `jwtKey`, la API **no arranca**.