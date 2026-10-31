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

> En desarrollo, la cadena de conexión `DefaultConnection` apunta a **LocalDB**, que solo existe en Windows. En macOS o Linux hay que reemplazarla (por ejemplo, con User Secrets) por un SQL Server accesible, como uno en Docker.

## 2. Qué se versiona y qué no

| Archivo | ¿Se versiona? | Contenido |
|---|---|---|
| `Backend/appsettings.json` | Sí | Claves vacías como plantilla (`ConnectionStrings:DefaultConnection`, `Cloudinary:*`, `jwtKey`), logging y `AllowedHosts`. **Sin secretos** |
| `Backend/appsettings.Development.json` | Sí | Connection string `DefaultConnection` de LocalDB, `Url Frontend`, `Cloudinary:CloudName`. **Sin secretos** |
| `Backend/appsettings.Production.json` | Sí | Solo `Cloudinary:CloudName` (no es un secreto). **Sin secretos** |
| User Secrets (`jwtKey`, `Cloudinary:ApiKey`, `Cloudinary:ApiSecret`) | **No** (quedan en el equipo de cada desarrollador) | Secretos de desarrollo |
| `appsettings.Production.json` **con secretos** en el servidor IIS | **No** | Copia local del servidor; ver sección 3.1 |
| `Frontend/wwwroot/appsettings.json` y `appsettings.Development.json` | Sí | `ApiUrl`. **Hoy no se usan**: ver sección 4 |

## 3. Configuración por entorno (API)

| Clave | Development (local) | IIS (Windows Server) | Azure App Service | ¿Secreto? |
|---|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` (`launchSettings.json`) | `Production` (valor por defecto de ASP.NET Core) | `Production` (App Setting) | No |
| `App:Version` | — (muestra `sin-configurar`) | — (muestra `sin-configurar`) | `1.0.0` (App Setting `App__Version`) | No |
| `ConnectionStrings:DefaultConnection` | LocalDB (`appsettings.Development.json`) | SQL Server Express `localhost\SQLEXPRESS`, login SQL `shopcore` (`appsettings.Production.json` del servidor) | Azure SQL (App Setting `ConnectionStrings__DefaultConnection`) | **Sí** |
| `jwtKey` | User Secrets | `appsettings.Production.json` del servidor | App Setting `jwtKey` | **Sí** |
| `Cloudinary:ApiKey` / `Cloudinary:ApiSecret` | User Secrets | `appsettings.Production.json` del servidor | App Settings `Cloudinary__ApiKey` / `Cloudinary__ApiSecret` | **Sí** |
| `Cloudinary:CloudName` | `appsettings.Development.json` | `appsettings.Production.json` | App Setting `Cloudinary__CloudName` | No |
| `Mail:*`, `Url Frontend` | — | Presentes en el archivo del servidor, pero no se usan | — | No se usan (correo fuera de alcance) |

En ASP.NET Core, `:` se escribe como `__` (doble guion bajo) en las variables de entorno y en los App Settings. Por ejemplo, `Cloudinary__ApiSecret` equivale a `Cloudinary:ApiSecret`.

### 3.1 IIS: secretos en `appsettings.Production.json` del servidor

En IIS los valores de producción se cargaron editando `appsettings.Production.json` en la copia del proyecto del servidor antes de ejecutar `dotnet publish`, así que el archivo publicado en `C:\inetpub\shopcore-api` ya los contiene (captura `evidencias/iis/12_configuracion_produccion.png`, con los valores tapados).

Ese archivo **está versionado** (la versión del repo solo tiene `Cloudinary:CloudName`), por lo que la copia con secretos **nunca debe subirse**. En el equipo donde se editó, conviene que git ignore los cambios locales:

```bash
git update-index --skip-worktree Tienda_Online/Tienda_Online.Backend/appsettings.Production.json
```

Para volver a versionarlo normalmente: `git update-index --no-skip-worktree <ruta>`.

## 4. Configuración por entorno (Frontend)

El Frontend es Blazor WebAssembly: corre en el navegador del usuario, por eso **no puede leer variables de entorno del servidor**.

Para la publicación en Azure, la URL de la API quedó **fija en `Frontend/Program.cs`**:

```csharp
builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri("https://shopcoreapi-bgegbefuf2bnavbr.centralus-01.azurewebsites.net") });
```

Consecuencias:

- El frontend publicado en **IIS y en Azure** consulta la **API de Azure**. La API publicada en IIS se verifica directamente en `https://192.168.11.81:7000/health` o en `https://192.168.11.81:7000/swagger`.
- Los archivos `wwwroot/appsettings.json` (`http://localhost:8081`) y `wwwroot/appsettings.Development.json` (`https://localhost:7000`) siguen en el repositorio, pero **ya no se leen**.
- Para que cada frontend use su propia API, habría que volver a leer `builder.Configuration["ApiUrl"]` en `Program.cs` y ajustar `ApiUrl` en cada publicación.

## 5. Endpoints de verificación

| Componente | Desarrollo | IIS | Azure |
|---|---|---|---|
| API `/health` | `https://localhost:7000/health` | `https://192.168.11.81:7000/health` | `https://shopcoreapi-bgegbefuf2bnavbr.centralus-01.azurewebsites.net/health` |
| API Swagger | `https://localhost:7000/swagger` | `https://192.168.11.81:7000/swagger` | `https://shopcoreapi-bgegbefuf2bnavbr.centralus-01.azurewebsites.net/swagger` |
| Frontend `/health` | `https://localhost:8000/health` | `https://192.168.11.81:8000/health` | `https://shopcoreweb-g4eqcddtajh2gph5.centralus-01.azurewebsites.net/health` |

- **API `/health`:** HTTP 200 con JSON `status`, `database`, `environment`, `version`, `utc`. Si la base de datos no responde: HTTP 503 y `status: DEGRADED`.
- **Frontend `/health`:** página "Frontend: OK" y el estado que devuelve la API (la de Azure, ver sección 4).
- **Swagger** está habilitado en **todos** los entornos, no solo en Development.

## 6. Alcance: el correo no se usa

El envío de correos (confirmación de cuenta y recuperación de contraseña) está **fuera del alcance** de esta práctica, por lo que no se configuran `Mail:*` ni `Url Frontend` en Azure. Consecuencia: el registro de usuarios nuevos y la recuperación de contraseña no funcionan, porque la API exige correo confirmado. La verificación de la aplicación se hace con el **usuario administrador que crea el seed** (`Data/SeedDb.cs`), que ya viene confirmado.

## 7. Base de datos

La API crea el esquema al arrancar con `EnsureCreatedAsync()` (en el seed) si la base de datos está vacía.

| Entorno | Base de datos |
|---|---|
| Desarrollo | `Orders` en LocalDB |
| IIS | `ShopCore` en SQL Server Express (`localhost\SQLEXPRESS`), creada por la app con el login SQL `shopcore` |
| Azure | Azure SQL Database |

Si la base de datos no está disponible o falta `jwtKey`, la API **no arranca**.
