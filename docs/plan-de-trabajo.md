# Práctica 3 · Despliegue de Software — Plan de trabajo del equipo (ShopCore)

> Documento guía para el equipo. **La palabra final la tiene el PDF `Practica3_Despliegue.pdf`** y el anuncio de Teams. Este plan reemplaza a `PRACTICA3_SHOPCORE.md` (el `.md` de ayuda), que asumía una app Blazor Server de un solo proyecto, y eso **no es lo que hay en el repo**.

---

## 0. Cómo usar este documento

### Reglas para todos

1. **Una rama por paquete de trabajo (WP)** y **un Pull Request por rama**. Otro integrante revisa el PR antes del merge.
2. **Cada integrante hace commit desde su propia cuenta de GitHub.** Antes de empezar, cada uno verifica:
   ```bash
   git config user.name     # su nombre real
   git config user.email    # un correo VINCULADO a su cuenta de GitHub
   ```
   Si el correo no está vinculado, el commit no aparece en *Insights → Contributors* y la rúbrica no lo cuenta.
3. **Conventional Commits:** `feat:`, `ops:`, `ci:`, `docs:`, `chore:`. No se aceptan commits vacíos ni cambios artificiales (lo dice el PDF).
4. **Nunca** se suben contraseñas, connection strings reales, publish profiles, IDs de suscripción ni tokens: ni en el repo, ni en las capturas, ni en el video.
5. **No se modifica la lógica de negocio** del e-commerce. Solo se cambia lo necesario para desplegar: configuración, `/health`, scripts y documentación.
6. Todos deben **entender el flujo completo**, no solo su parte, porque el docente puede preguntarle a cualquiera.

### Orden de trabajo (dependencias)

```
WP-A1 (estructura) ──► WP-C1 (config + /health) ──┬──► WP-B1 (IIS) ───────────────┐
                                                  └──► WP-D1 (infra Azure) ──► WP-E1 (deploy Azure)
                                                                                    │
                    WP-C2 (guion video) ◄───────────────────────────────────────────┘
                    WP-A2 (README final + PDF + tag) ◄── todo lo anterior
                    VIDEO (todos)
```

WP-B1 y WP-D1 pueden avanzar en paralelo. D puede crear los recursos de Azure sin esperar a C, pero el deploy de E sí necesita WP-C1 mergeado.

---

## 1. Qué hay realmente en el repo

Repo: `DavidMoralesVargas/ShopCore`. El nombre es válido porque no es genérico. El código está en `Tienda_Online/` y esa carpeta **se queda así**: el PDF permite cualquier nombre para la carpeta de código.

| Proyecto | Tipo | Notas |
|---|---|---|
| `Tienda_Online.Frontend` | **Blazor WebAssembly Standalone** (`Microsoft.NET.Sdk.BlazorWebAssembly`), net8.0 | Son archivos estáticos que corren en el navegador. La URL de la API está **fija** en `Program.cs:16` → `https://localhost:7000` |
| `Tienda_Online.Backend` | ASP.NET Core **Web API**, net8.0 | EF Core + **SQL Server** (`Program.cs:22`, `UseSqlServer("name=DockerConnection")`), Identity, JWT, Cloudinary (fotos), MailKit (correos). Swagger solo en Development |
| `Tienda_Online.Shared` | Librería de clases | DTOs y entidades compartidas |

### Problemas que hay que resolver para desplegar

| # | Problema | Dónde | Quién lo resuelve |
|---|---|---|---|
| 1 | El frontend apunta siempre a `https://localhost:7000` | `Frontend/Program.cs:16` | C (WP-C1) |
| 2 | La API **se cae al arrancar** si no hay `jwtKey` o si no hay base de datos, porque el seed (`Data/SeedDb.cs`) ejecuta `EnsureCreated()` al iniciar | `Backend/Program.cs:89`, `SeedDb.cs` | C (config) + B/D (BD) |
| 3 | **Hay secretos reales versionados**: contraseña de Gmail, ApiKey/ApiSecret de Cloudinary y jwtKey | `Backend/appsettings.Development.json` | C + David (dueño de las cuentas) |
| 7 | El envío de correos (Gmail) **no se usará** en esta práctica | `Helpers/MailHelper.cs`, `AccountsController.cs` | Ver la nota de abajo |

> **Correo fuera de alcance.** No se configura `Mail:*` ni `Url Frontend` en ningún entorno. `MailHelper` atrapa el error y no tumba la app. La consecuencia es que **el registro de usuarios nuevos y la recuperación de contraseña no funcionan**: la API exige confirmar el correo (`RequireConfirmedEmail = true`) y responde `BadRequest` si el correo no se envía. **La demostración se hace con el administrador que crea el seed** (`SeedDb.cs`), que ya viene confirmado. Esto se deja escrito en el README, en "Notas".
| 4 | LocalDB no funciona en Azure ni con la identidad de un App Pool | `appsettings.Development.json` | B (SQL Express) / D (Azure SQL) |
| 5 | No existe `/health` | — | C (WP-C1) |
| 6 | No hay configuración de Production | `appsettings.json` | C (WP-C1) |

### Arquitectura de despliegue (decidida)

**Dos sitios en IIS y dos App Services en Azure** (frontend y API separados), con **SQL Server real**.

```
IIS (Windows 11 local)
  Navegador ──► http://localhost:8080  [ShopCore-Web · archivos estáticos WASM + URL Rewrite]
           └──► http://localhost:8081  [ShopCore-Api · ASP.NET Core 8 vía AspNetCoreModuleV2]
                                              └──► SQL Server Express (.\SQLEXPRESS, BD ShopCore)

Azure (App Service Windows, plan F1)
  Navegador ──► https://shopcore-web-<sufijo>.azurewebsites.net  [archivos estáticos WASM]
           └──► https://shopcore-api-<sufijo>.azurewebsites.net  [ASP.NET Core 8]
                                              └──► Azure SQL Database (oferta gratuita)
```

El navegador descarga el frontend desde el sitio Web y luego llama **directamente** a la API. Por eso la URL de la API se configura según el entorno (`ApiUrl`). La API ya tiene CORS abierto (`Program.cs`, `UseCors(... SetIsOriginAllowed(origin => true))`), así que no hay que tocarlo.

---

## 2. Valores fijos del proyecto

| Clave | Valor |
|---|---|
| Repositorio | `ShopCore` (GitHub `DavidMoralesVargas/ShopCore`) |
| Colaborador docente | usuario GitHub **`oalarconpe`** (por usuario, **NO** por correo) |
| .NET | **8.0** (Hosting Bundle 8.0.x · runtime Azure `dotnet:8` · `DOTNET_VERSION: 8.0.x`) |
| IIS · Sitio Web | `ShopCore-Web` · pool `ShopCoreWebPool` · `C:\inetpub\shopcore\web` · puerto **8080** |
| IIS · Sitio API | `ShopCore-Api` · pool `ShopCoreApiPool` (No Managed Code, Integrated) · `C:\inetpub\shopcore\api` · puerto **8081** |
| IIS · SQL | `.\SQLEXPRESS` · BD `ShopCore` · login Windows `IIS APPPOOL\ShopCoreApiPool` (db_owner) |
| Azure · Resource Group | `rg-shopcore` |
| Azure · Plan | `plan-shopcore` (Windows, SKU `F1`) |
| Azure · Apps | `shopcore-api-<sufijo>` y `shopcore-web-<sufijo>` (sufijo = iniciales del equipo, p. ej. `dc`) |
| Azure · SQL | servidor `sql-shopcore-<sufijo>` · BD `ShopCore` (oferta gratuita, serverless) |
| Azure · Región | `eastus2` (cambiarla si la suscripción de estudiante la bloquea) |
| Health API | `GET /health` → JSON con `status`, `database`, `environment`, `version` |
| Health Web | `GET /health` → página "Frontend: OK" + estado de la API |
| Tag final | `practica3-v1.0` |

### Configuración por entorno (API)

| Clave | Development (local) | IIS | Azure | ¿Secreto? |
|---|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | Development | `Production` (variable del pool) | `Production` (App Setting) | No |
| `App:Version` | — | `1.0.0-iis` (variable del pool) | `1.0.0` (App Setting `App__Version`) | No |
| `ConnectionStrings:DockerConnection` | LocalDB (`appsettings.Development.json`) | SQL Express (variable del pool) | Azure SQL (*Connection strings*, tipo `SQLAzure`) | **Sí** en Azure |
| `jwtKey` | User Secrets | variable del pool | App Setting | **Sí** |
| `Cloudinary:ApiKey` / `ApiSecret` | User Secrets | variables del pool | App Settings `Cloudinary__ApiKey` / `Cloudinary__ApiSecret` | **Sí** |
| `Cloudinary:CloudName` | `appsettings.Development.json` | `appsettings.Production.json` | `appsettings.Production.json` | No |
| `Mail:*`, `Url Frontend` | — | — | — | No se usan (correo fuera de alcance) |

> En ASP.NET Core, `:` se escribe como `__` en las variables de entorno y en los App Settings (por ejemplo, `Cloudinary__ApiSecret` equivale a `Cloudinary:ApiSecret`).

### Configuración por entorno (Frontend)

| Archivo | `ApiUrl` |
|---|---|
| `wwwroot/appsettings.Development.json` | `https://localhost:7000` (desarrollo con `dotnet run`) |
| `wwwroot/appsettings.json` | `http://localhost:8081` (lo que se publica en IIS) |
| Azure | El workflow lo **reemplaza** al publicar por `https://shopcore-api-<sufijo>.azurewebsites.net` |

---

## 3. Roles y paquetes de trabajo

| Integrante | Rol | Paquetes | Rama(s) |
|---|---|---|---|
| **A** | Coordinación e integración | WP-A1, WP-A2 | `chore/estructura-repo`, `docs/readme-final` |
| **B** | Publicación en IIS | WP-B1 | `ops/iis-setup` |
| **C** | Preparación de la app | WP-C1, WP-C2 | `feat/config-health`, `docs/guion-video` |
| **D** | Infraestructura Azure | WP-D1 | `ops/azure-provision` |
| **E** | Despliegue y verificación en Azure | WP-E1 | `ci/azure-deploy` |

Cada integrante necesita **al menos un commit propio y no trivial** en su paquete.

**Informe PDF para Teams:** cada rol escribe **su sección** en `docs/informe.md` dentro de su propio PR, y A lo ensambla y lo exporta a PDF en WP-A2. Así el informe también queda como contribución distribuida.

---

### A · Coordinación — WP-A1 `chore/estructura-repo`

**Tareas manuales (GitHub):**
- [ ] Verificar que todos los integrantes sean colaboradores del repo con permiso *Write*. El dueño (DavidMoralesVargas) lo hace en *Settings → Collaborators*.
- [ ] Dar rol *Admin* a quien vaya a crear los secrets de GitHub Actions (E), o que el dueño los cree.
- [ ] Proteger `main` (opcional pero recomendado): exigir PR con 1 aprobación.
- [ ] Invitar a **`oalarconpe` por nombre de usuario**, como máximo **7 días antes** de la entrega (la invitación expira a los 7 días).

**Cambios en el repo:**

Estructura objetivo:
```
ShopCore/
├─ README.md
├─ .gitignore
├─ Tienda_Online/            # código existente (no se mueve)
├─ deploy/
│  ├─ iis/                   # B
│  └─ azure/                 # D y E
├─ docs/
│  ├─ plan-de-trabajo.md     # copia de este documento
│  ├─ configuracion.md       # C
│  ├─ guion-video.md         # C
│  └─ informe.md             # todos (cada uno su sección)
├─ evidencias/
│  ├─ iis/.gitkeep
│  └─ azure/.gitkeep
└─ .github/workflows/        # E
```

Agregar al final del `.gitignore` existente:
```
# Despliegue (Práctica 3)
profile.xml
*.PublishSettings
app.zip
appsettings.*.local.json
```
(`publish/`, `*.pubxml`, `*.user` y `*.env` ya están ignorados. **No** ignorar `evidencias/` ni `*.png`.)

- README esqueleto con la plantilla de la sección 7 y los placeholders.
- `docs/informe.md` con la estructura: portada, integrantes y roles, descripción de la app, Parte 1 IIS, Parte 2 Azure, trabajo colaborativo y conclusiones.

**Aceptación:** la estructura existe, `dotnet build Tienda_Online/Tienda_Online.sln` sigue compilando y `git check-ignore evidencias/iis/x.png` no devuelve nada.

---

### C · Preparación de la app — WP-C1 `feat/config-health`

> Atención: `Backend/Program.cs` está guardado en **ISO-8859-1**, no en UTF-8 (por el comentario `//Conexión`). Hay que configurar el editor para que **no cambie la codificación ni reformatee el archivo**; si no, el diff se llena de ruido. En VS Code: "Reopen with Encoding → Western (ISO 8859-1)".

**1. Sacar los secretos del repo**
- [ ] En `Backend/appsettings.Development.json`, **eliminar** los valores de `Cloudinary:ApiKey`, `Cloudinary:ApiSecret` y `jwtKey`, y **eliminar completa** la sección `Mail` (incluye la contraseña de Gmail; el correo no se usará). Se conservan la connection string de LocalDB, `Cloudinary:CloudName` y `Url Frontend`.
- [ ] `dotnet user-secrets init --project Tienda_Online/Tienda_Online.Backend`, que agrega `UserSecretsId` al `.csproj`.
- [ ] Documentar en `docs/configuracion.md` cómo cada desarrollador carga sus secretos locales:
  ```bash
  cd Tienda_Online/Tienda_Online.Backend
  dotnet user-secrets set "jwtKey" "<clave-larga-aleatoria>"
  dotnet user-secrets set "Cloudinary:ApiKey" "<api-key>"
  dotnet user-secrets set "Cloudinary:ApiSecret" "<api-secret>"
  ```
- [ ] **Rotar las credenciales**, junto con **David**, que es el dueño de las cuentas. Los valores viejos siguen en el historial de git, así que hay que invalidarlos:
  - Gmail (David): **revocar** la *App Password* actual en la cuenta de Google. No se genera una nueva, porque el correo no se usará.
  - Cloudinary (David): *Settings → API Keys → Regenerate*.
  - jwtKey: generar una nueva (`openssl rand -base64 64`). Los tokens viejos dejan de servir, y eso es lo esperado.
  - Los valores nuevos **se comparten por un canal privado** del equipo, nunca en el repo.

**2. `appsettings.Production.json` de la API** (nuevo, **sin secretos**)
```json
{
  "Cloudinary": {
    "CloudName": "<cloud-name>"
  }
}
```
Sin `Mail` ni `Url Frontend`, porque el correo está fuera de alcance.

**3. Endpoint `/health` en la API.** En `Backend/Program.cs`, justo antes de `app.MapControllers();`:
```csharp
app.MapGet("/health", async (DataContext db, IWebHostEnvironment env, IConfiguration config) =>
{
    var dbOk = await db.Database.CanConnectAsync();
    var body = new
    {
        status = dbOk ? "OK" : "DEGRADED",
        database = dbOk ? "OK" : "ERROR",
        environment = env.EnvironmentName,
        version = config["App:Version"] ?? "sin-configurar",
        utc = DateTime.UtcNow
    };
    return dbOk ? Results.Ok(body) : Results.Json(body, statusCode: 503);
}).AllowAnonymous();
```

**4. URL de la API configurable en el frontend**
- Crear `Frontend/wwwroot/appsettings.json`:
  ```json
  { "ApiUrl": "http://localhost:8081" }
  ```
- Crear `Frontend/wwwroot/appsettings.Development.json`:
  ```json
  { "ApiUrl": "https://localhost:7000" }
  ```
- `Frontend/Program.cs`: reemplazar la línea 16 por:
  ```csharp
  var apiUrl = builder.Configuration["ApiUrl"] ?? "https://localhost:7000";
  builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });
  ```

**5. Página `/health` del frontend.** Crear `Frontend/Pages/Health.razor`:
```razor
@page "/health"
@inject HttpClient Http

<PageTitle>Health</PageTitle>

<h3>Frontend: OK</h3>
<p>API (@Http.BaseAddress): @apiStatus</p>

@code {
    private string apiStatus = "consultando...";

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var response = await Http.GetAsync("health");
            apiStatus = $"{(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}";
        }
        catch (Exception ex)
        {
            apiStatus = $"ERROR: {ex.Message}";
        }
    }
}
```
Con esto, `/health` del sitio Web demuestra dos cosas: que el frontend carga y que llega a la API.

**6. Verificar**
```bash
cd Tienda_Online
dotnet build Tienda_Online.sln
dotnet publish Tienda_Online.Backend/Tienda_Online.Backend.csproj -c Release -o ../publish/api
dotnet publish Tienda_Online.Frontend/Tienda_Online.Frontend.csproj -c Release -o ../publish/web
```
- `dotnet run` de la API → `https://localhost:7000/health` devuelve 200 con JSON.
- `dotnet run` del frontend → `https://localhost:8000/health` muestra "Frontend: OK" y el JSON de la API.
- `git diff` **no** contiene ningún secreto.

**7. Informe:** escribir en `docs/informe.md` la sección "Preparación de la aplicación".

---

### B · Publicación en IIS — WP-B1 `ops/iis-setup`

Cubre las actividades 1 a 8 del PDF. Se trabaja en un **Windows 11** con permisos de administrador.

**Prerrequisitos (manuales, documentarlos en `deploy/iis/README.md`):**
1. Habilitar IIS (PowerShell como administrador):
   ```powershell
   Enable-WindowsOptionalFeature -Online -All -NoRestart -FeatureName `
     IIS-WebServerRole, IIS-WebServer, IIS-ManagementConsole, IIS-StaticContent, `
     IIS-DefaultDocument, IIS-HttpErrors, IIS-HttpCompressionStatic
   ```
   Comprobar en `http://localhost` que aparece la página por defecto de IIS.
2. Instalar el **ASP.NET Core 8.0 Hosting Bundle** (dotnet.microsoft.com → .NET 8 → *Hosting Bundle*) y luego `iisreset`. Para verificar: `dotnet --list-runtimes` debe mostrar `Microsoft.AspNetCore.App 8.0.x`, y en IIS Manager → *Modules* debe aparecer `AspNetCoreModuleV2`. Si IIS se instaló **después** del bundle, hay que reparar el bundle.
3. Instalar el módulo **URL Rewrite 2.1** (iis.net). El frontend WASM lo necesita para las rutas profundas (`/health`, `/productos`, etc.).
4. Instalar **SQL Server Express** (instancia `SQLEXPRESS`) y SSMS o `sqlcmd`.

**Archivos a crear:**

`deploy/iis/sql-setup.sql`: crea la BD y da acceso a la identidad del pool. El esquema lo crea la app con `EnsureCreated()` cuando arranca, si la BD está vacía.
```sql
IF DB_ID('ShopCore') IS NULL CREATE DATABASE ShopCore;
GO
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'IIS APPPOOL\ShopCoreApiPool')
    CREATE LOGIN [IIS APPPOOL\ShopCoreApiPool] FROM WINDOWS;
GO
USE ShopCore;
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'IIS APPPOOL\ShopCoreApiPool')
    CREATE USER [IIS APPPOOL\ShopCoreApiPool] FOR LOGIN [IIS APPPOOL\ShopCoreApiPool];
ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\ShopCoreApiPool];
GO
```
(Se ejecuta **después** de crear el pool: `sqlcmd -S .\SQLEXPRESS -E -i deploy\iis\sql-setup.sql`.)

`deploy/iis/setup-iis.ps1`: **idempotente** (se puede ejecutar dos veces sin error), parametrizado, `#Requires -RunAsAdministrator`. Debe:
- Crear los pools `ShopCoreWebPool` y `ShopCoreApiPool` con `managedRuntimeVersion = ""` (No Managed Code) y el pipeline Integrated.
- Crear las carpetas `C:\inetpub\shopcore\web` y `C:\inetpub\shopcore\api`.
- Crear los sitios `ShopCore-Web` (puerto 8080, pool Web) y `ShopCore-Api` (puerto 8081, pool API) si no existen.
- Dar permisos de lectura: `icacls <ruta> /grant "IIS AppPool\<Pool>:(OI)(CI)RX"`.
- Imprimir el resumen (sitios, puertos, pools).

Ejemplo de la parte del pool:
```powershell
Import-Module WebAdministration
if (-not (Test-Path "IIS:\AppPools\$PoolName")) { New-WebAppPool -Name $PoolName | Out-Null }
Set-ItemProperty "IIS:\AppPools\$PoolName" -Name managedRuntimeVersion -Value ""
Set-ItemProperty "IIS:\AppPools\$PoolName" -Name managedPipelineMode -Value 0
if (-not (Test-Path "IIS:\Sites\$SiteName")) {
    New-Website -Name $SiteName -Port $Port -PhysicalPath $PhysicalPath -ApplicationPool $PoolName | Out-Null
}
```

`deploy/iis/set-iis-secrets.ps1`: guarda la configuración de la API como **variables de entorno del App Pool**, en `applicationHost.config` y no en el repo. Así sobreviven a cada publicación (que reescribe el `web.config`). Los secretos se piden con `Read-Host -AsSecureString` y **nunca van escritos en el script**:
```powershell
#Requires -RunAsAdministrator
param([string]$PoolName = "ShopCoreApiPool")
Import-Module WebAdministration
$apphost = 'MACHINE/WEBROOT/APPHOST'
$filter  = "system.applicationHost/applicationPools/add[@name='$PoolName']/environmentVariables"

function Set-PoolVar([string]$Name, [string]$Value) {
    Remove-WebConfigurationProperty -PSPath $apphost -Filter $filter -Name '.' -AtElement @{name=$Name} -ErrorAction SilentlyContinue
    Add-WebConfigurationProperty -PSPath $apphost -Filter $filter -Name '.' -Value @{name=$Name; value=$Value}
}
function Read-Secret([string]$Prompt) {
    [System.Net.NetworkCredential]::new('', (Read-Host $Prompt -AsSecureString)).Password
}

Set-PoolVar 'ASPNETCORE_ENVIRONMENT' 'Production'
Set-PoolVar 'App__Version' '1.0.0-iis'
Set-PoolVar 'ConnectionStrings__DockerConnection' 'Server=.\SQLEXPRESS;Database=ShopCore;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true'
Set-PoolVar 'jwtKey'                (Read-Secret 'jwtKey')
Set-PoolVar 'Cloudinary__ApiKey'    (Read-Secret 'Cloudinary:ApiKey')
Set-PoolVar 'Cloudinary__ApiSecret' (Read-Secret 'Cloudinary:ApiSecret')
Restart-WebAppPool $PoolName
```

`deploy/iis/publish-iis.ps1`: debe:
1. Detener `ShopCoreApiPool` si está iniciado, porque las DLLs quedan bloqueadas.
2. `dotnet publish Tienda_Online\Tienda_Online.Backend\... -c Release -o C:\inetpub\shopcore\api`.
3. Iniciar el pool.
4. `dotnet publish Tienda_Online\Tienda_Online.Frontend\... -c Release -o C:\inetpub\shopcore\web`.
5. Verificar que `C:\inetpub\shopcore\web\wwwroot\appsettings.json` tenga `ApiUrl = http://localhost:8081` y borrar `appsettings.json.br` y `.gz` si existen.
6. Smoke test: `Invoke-WebRequest http://localhost:8081/health` y `Invoke-WebRequest http://localhost:8080`, e imprimir los códigos de estado.

> **Punto de entrada (actividad 6 del PDF):**
> - **Web:** la ruta física es la **carpeta de publicación** (no `wwwroot`). El `web.config` que genera el publish tiene las reglas de URL Rewrite que mandan a `wwwroot\` y el *fallback* SPA a `index.html`. Hay que verificar que `index.html` esté en *Default Document*. Si el `web.config` generado no trae la regla "Serve subdir", se apunta el sitio a `...\web\wwwroot`.
> - **API:** el `web.config` generado usa `AspNetCoreModuleV2` con `hostingModel="inprocess"`. Aquí el documento predeterminado no aplica.

`deploy/iis/README.md`: prerrequisitos, orden de ejecución (`setup-iis.ps1` → `sql-setup.sql` → `set-iis-secrets.ps1` → `publish-iis.ps1`) y troubleshooting:

| Síntoma | Causa probable | Solución |
|---|---|---|
| HTTP 500.19 en la API | Falta el Hosting Bundle | Instalarlo (o repararlo) y ejecutar `iisreset` |
| HTTP 500.30 / 500.31 | La app no arranca: falta `jwtKey`, la BD no responde o falta el runtime | Poner `stdoutLogEnabled="true"` en `C:\inetpub\shopcore\api\web.config`, crear la carpeta `logs` y leer el log; revisar el Visor de eventos |
| `Login failed for user 'IIS APPPOOL\ShopCoreApiPool'` | No se ejecutó `sql-setup.sql` | Ejecutarlo |
| 404 al recargar `/health` o `/productos` en la Web | Falta URL Rewrite | Instalar URL Rewrite 2.1 |
| 401/403 | Permisos de carpeta | Revisar el `icacls` del script |
| El frontend carga pero "no hay datos" | `ApiUrl` incorrecto o la API caída | Abrir `http://localhost:8080/health` y la consola del navegador (F12) |

> Las variables de entorno del pool quedan en texto plano en `applicationHost.config` de la máquina local. Es aceptable en un entorno local, pero **no se muestran sus valores en capturas ni en el video**.

**Evidencias:** ver la sección 5 (`evidencias/iis/`). **Informe:** sección "Parte 1 – IIS" en `docs/informe.md`.

**Aceptación:**
- Los scripts se ejecutan dos veces sin error.
- No hay rutas personales (`C:\Users\...`) ni credenciales.
- `http://localhost:8080`, `http://localhost:8080/health` y `http://localhost:8081/health` responden.
- Se puede iniciar sesión en la app.

---

### D · Infraestructura Azure — WP-D1 `ops/azure-provision`

Cubre las actividades 9, 10, 12 y 13 del PDF.

`deploy/azure/provision.sh`: se ejecuta en Azure Cloud Shell (Bash) o Git Bash con `az login`. Todas las variables se pueden sobreescribir y **los secretos se leen de variables de entorno**, nunca van escritos en el script:
```bash
#!/usr/bin/env bash
set -euo pipefail

SUFFIX="${SUFFIX:?define SUFFIX, p. ej. dc}"
RG="${RG:-rg-shopcore}"
PLAN="${PLAN:-plan-shopcore}"
LOCATION="${LOCATION:-eastus2}"
API_APP="${API_APP:-shopcore-api-$SUFFIX}"
WEB_APP="${WEB_APP:-shopcore-web-$SUFFIX}"
SQL_SERVER="${SQL_SERVER:-sql-shopcore-$SUFFIX}"
SQL_DB="${SQL_DB:-ShopCore}"
APP_VERSION="${APP_VERSION:-1.0.0}"
: "${SQL_ADMIN:?}" "${SQL_PASSWORD:?}" "${JWT_KEY:?}" "${CLOUDINARY_API_KEY:?}" "${CLOUDINARY_API_SECRET:?}"

az group create -n "$RG" -l "$LOCATION" -o none
az appservice plan create -n "$PLAN" -g "$RG" -l "$LOCATION" --sku F1 -o none   # Windows por defecto
az webapp create -n "$API_APP" -g "$RG" -p "$PLAN" --runtime "dotnet:8" -o none
az webapp create -n "$WEB_APP" -g "$RG" -p "$PLAN" --runtime "dotnet:8" -o none

# Azure SQL (oferta gratuita)
az sql server create -g "$RG" -n "$SQL_SERVER" -l "$LOCATION" \
  --admin-user "$SQL_ADMIN" --admin-password "$SQL_PASSWORD" -o none
az sql db create -g "$RG" -s "$SQL_SERVER" -n "$SQL_DB" \
  --edition GeneralPurpose --family Gen5 --capacity 2 --compute-model Serverless \
  --use-free-limit --free-limit-exhaustion-behavior AutoPause -o none
az sql server firewall-rule create -g "$RG" -s "$SQL_SERVER" -n AllowAzureServices \
  --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0 -o none

# Configuración de la API (-o none: evita imprimir los valores en la consola)
az webapp config appsettings set -g "$RG" -n "$API_APP" -o none --settings \
  ASPNETCORE_ENVIRONMENT=Production App__Version="$APP_VERSION" \
  jwtKey="$JWT_KEY" \
  Cloudinary__ApiKey="$CLOUDINARY_API_KEY" Cloudinary__ApiSecret="$CLOUDINARY_API_SECRET"

CONN="Server=tcp:$SQL_SERVER.database.windows.net,1433;Database=$SQL_DB;User ID=$SQL_ADMIN;Password=$SQL_PASSWORD;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=True;Connection Timeout=60;"
az webapp config connection-string set -g "$RG" -n "$API_APP" -t SQLAzure -o none \
  --settings DockerConnection="$CONN"

# HTTPS Only en ambas apps
az webapp update -g "$RG" -n "$API_APP" --https-only true -o none
az webapp update -g "$RG" -n "$WEB_APP" --https-only true -o none

az webapp list -g "$RG" --query "[].{name:name, host:defaultHostName, httpsOnly:httpsOnly}" -o table
```
Los secretos se cargan **en la sesión** antes de ejecutar el script, por ejemplo con `read -s JWT_KEY && export JWT_KEY`, y no se pegan en ningún archivo.

`deploy/azure/README.md` debe explicar:
- Cómo verificar el runtime disponible: `az webapp list-runtimes --os windows | grep -i dotnet`.
- Cómo verificar que el nombre esté libre: `nslookup shopcore-api-<sufijo>.azurewebsites.net` (si resuelve, cambiar el sufijo). Si Azure asigna un *hostname* único (con hash o región), se usa el que aparece en *Overview*.
- **Por qué App Service Windows:** es el mismo modelo de hosting que IIS (ANCM + `web.config`) y trae URL Rewrite incorporado para el frontend WASM.
- **Limitaciones de F1:** *cold start* (no hay *Always On*), 60 min de CPU al día y proceso de 32 bits. La BD serverless se **auto-pausa**, así que la primera petición puede tardar unos segundos (por eso `Connection Timeout=60`). Antes de grabar el video conviene "despertar" la app abriendo `/health`.
- Si aparece un error de **política de región** en la suscripción de estudiante, cambiar `LOCATION` (por ejemplo `centralus` o `westus2`).
- Si `--use-free-limit` falla, crear la BD desde el portal marcando *"Apply offer"* (oferta gratuita de Azure SQL).

**Evidencias:** ver la sección 5 (las capturas de configuración de `evidencias/azure/`). **Informe:** sección "Parte 2 – Infraestructura Azure".

**Aceptación:** el script no contiene IDs de suscripción, contraseñas ni claves; todas las variables se pueden sobreescribir; HTTPS Only está en `true` en las dos apps.

---

### E · Despliegue y verificación en Azure — WP-E1 `ci/azure-deploy`

Cubre las actividades 11, 14 y 15 del PDF.

**Secrets y variables de GitHub** (*Settings → Secrets and variables → Actions*). Lo hace alguien con rol Admin:

| Tipo | Nombre | Valor |
|---|---|---|
| Secret | `AZURE_API_PUBLISH_PROFILE` | XML del publish profile de la API |
| Secret | `AZURE_WEB_PUBLISH_PROFILE` | XML del publish profile de la Web |
| Variable | `AZURE_API_APP_NAME` | `shopcore-api-<sufijo>` |
| Variable | `AZURE_WEB_APP_NAME` | `shopcore-web-<sufijo>` |
| Variable | `AZURE_API_URL` | `https://<host-api>` (sin `/` final) |
| Variable | `AZURE_WEB_URL` | `https://<host-web>` |

Cómo obtener el publish profile. Las apps nuevas traen deshabilitada la autenticación básica de SCM, así que primero hay que habilitarla:
```bash
for APP in shopcore-api-<sufijo> shopcore-web-<sufijo>; do
  az resource update -g rg-shopcore --name scm --namespace Microsoft.Web \
    --resource-type basicPublishingCredentialsPolicies --parent sites/$APP --set properties.allow=true
done
az webapp deployment list-publishing-profiles -g rg-shopcore -n shopcore-api-<sufijo> --xml > profile.xml
# pegar el contenido en el secret de GitHub y luego:
rm profile.xml
```
**El XML nunca se commitea** (`profile.xml` está en `.gitignore`).

`.github/workflows/azure-deploy.yml`:
```yaml
name: Deploy ShopCore to Azure App Service

on:
  push:
    branches: [main]
  workflow_dispatch:

env:
  DOTNET_VERSION: '8.0.x'
  API_PROJECT: Tienda_Online/Tienda_Online.Backend/Tienda_Online.Backend.csproj
  WEB_PROJECT: Tienda_Online/Tienda_Online.Frontend/Tienda_Online.Frontend.csproj

jobs:
  api:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      - name: Publish API
        run: dotnet publish ${{ env.API_PROJECT }} -c Release -o ${{ runner.temp }}/api
      - name: Deploy API
        uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ vars.AZURE_API_APP_NAME }}
          publish-profile: ${{ secrets.AZURE_API_PUBLISH_PROFILE }}
          package: ${{ runner.temp }}/api
      - name: Smoke test API /health
        run: |
          for i in 1 2 3 4 5 6; do
            curl -fsS "${{ vars.AZURE_API_URL }}/health" && exit 0
            sleep 20
          done
          exit 1

  web:
    needs: api
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      - name: Publish Web
        run: dotnet publish ${{ env.WEB_PROJECT }} -c Release -o ${{ runner.temp }}/web
      - name: Configurar ApiUrl para Azure
        working-directory: ${{ runner.temp }}/web/wwwroot
        run: |
          jq --arg url "${{ vars.AZURE_API_URL }}" '.ApiUrl = $url' appsettings.json > tmp.json
          mv tmp.json appsettings.json
          rm -f appsettings.json.br appsettings.json.gz
          cat appsettings.json
      - name: Deploy Web
        uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ vars.AZURE_WEB_APP_NAME }}
          publish-profile: ${{ secrets.AZURE_WEB_PUBLISH_PROFILE }}
          package: ${{ runner.temp }}/web
      - name: Smoke test Web
        run: curl -fsS -o /dev/null -w "%{http_code}\n" "${{ vars.AZURE_WEB_URL }}/"
```
Se despliega **toda** la carpeta de publicación de la Web (`web.config` + `wwwroot/`), no solo `wwwroot`.

`deploy/azure/deploy-zip.sh`: alternativa manual sin Actions, útil si el workflow falla el día del video:
```bash
#!/usr/bin/env bash
set -euo pipefail
RG="${RG:-rg-shopcore}"
API_APP="${API_APP:?}"; WEB_APP="${WEB_APP:?}"; API_URL="${API_URL:?https://<host-api>}"
rm -rf publish app-api.zip app-web.zip
dotnet publish Tienda_Online/Tienda_Online.Backend/Tienda_Online.Backend.csproj -c Release -o publish/api
dotnet publish Tienda_Online/Tienda_Online.Frontend/Tienda_Online.Frontend.csproj -c Release -o publish/web
jq --arg url "$API_URL" '.ApiUrl = $url' publish/web/wwwroot/appsettings.json > tmp.json && mv tmp.json publish/web/wwwroot/appsettings.json
rm -f publish/web/wwwroot/appsettings.json.br publish/web/wwwroot/appsettings.json.gz
(cd publish/api && zip -qr ../../app-api.zip .)
(cd publish/web && zip -qr ../../app-web.zip .)
az webapp deploy -g "$RG" -n "$API_APP" --src-path app-api.zip --type zip
az webapp deploy -g "$RG" -n "$WEB_APP" --src-path app-web.zip --type zip
```
(Agregar `app-api.zip` y `app-web.zip` al `.gitignore` en este mismo PR.)

**Verificación (actividades 14 y 15):**
- [ ] `https://<host-web>/` carga la tienda.
- [ ] `https://<host-api>/health` → `"status":"OK"`, `"database":"OK"`, `"environment":"Production"`, `"version":"1.0.0"`.
- [ ] `https://<host-web>/health` → "Frontend: OK" + JSON de la API.
- [ ] `curl -I http://<host-web>` → `301` hacia `https://` (HTTPS Only).
- [ ] Iniciar sesión con el usuario administrador del seed y navegar por productos, ofertas y carrito. El registro de usuarios nuevos no se prueba, porque depende del correo, que está fuera de alcance.

**Evidencias:** ver la sección 5 (las capturas de deploy y verificación de `evidencias/azure/`). **Informe:** sección "Parte 2 – Despliegue y verificación".

**Aceptación:** el workflow pasa en verde, no hay ningún secreto escrito en el YAML y los smoke tests pasan.

---

### C · Guion del video — WP-C2 `docs/guion-video`

Crear `docs/guion-video.md` (ver la sección 6). Además, se coordina el ensayo con todos.

### A · Cierre — WP-A2 `docs/readme-final`

- [ ] Completar el README (sección 7) con los nombres, las fechas reales de publicación, las URLs, el enlace al video y los enlaces a los PRs de cada integrante.
- [ ] Ensamblar `docs/informe.md` y exportarlo a PDF para Teams. Debe incluir las capturas de `evidencias/`.
- [ ] Copiar este plan a `docs/plan-de-trabajo.md`.
- [ ] Después del merge final: `git tag practica3-v1.0 && git push origin practica3-v1.0` (es un plus recomendado por el PDF).
- [ ] Ejecutar el checklist de la sección 8.

---

## 4. Snippets de referencia

Están dentro de cada paquete:
- `/health` de la API: C, punto 3.
- `ApiUrl` del frontend: C, punto 4.
- `Health.razor`: C, punto 5.
- `appsettings.Production.json`: C, punto 2.
- Scripts de IIS: B.
- Script de Azure: D.
- Workflow: E.

---

## 5. Checklist de evidencias

Todas las capturas deben ser **legibles**, mostrar la **barra de direcciones** cuando sea un navegador y **ocultar cualquier valor sensible** (pixelar o usar "Hidden value" en el portal). El PDF pide que permitan **reconstruir la configuración**, así que no basta con la captura final del navegador.

### `evidencias/iis/` (responsable: B)

| Archivo | Contenido |
|---|---|
| `01-iis-habilitado.png` | `http://localhost` con la página por defecto de IIS |
| `02-hosting-bundle.png` | `dotnet --list-runtimes` + `AspNetCoreModuleV2` en *Modules* |
| `03-url-rewrite.png` | Módulo URL Rewrite instalado |
| `04-sql-login-pool.png` | SSMS: login `IIS APPPOOL\ShopCoreApiPool` y BD `ShopCore` |
| `05-app-pools.png` | `ShopCoreWebPool` y `ShopCoreApiPool` con *No Managed Code* |
| `06-sitio-web-config.png` | *Basic Settings* de `ShopCore-Web` (ruta física) + binding http 8080 |
| `07-sitio-api-config.png` | *Basic Settings* de `ShopCore-Api` (ruta física) + binding http 8081 |
| `08-punto-entrada-web.png` | *Default Document* con `index.html` + reglas de URL Rewrite del `web.config` |
| `09-punto-entrada-api.png` | `web.config` de la API con `AspNetCoreModuleV2` / `inprocess` |
| `10-variables-pool.png` | Variables de entorno del pool (**solo los nombres**, valores ocultos) |
| `11-scripts-ejecucion.png` | Salida de `setup-iis.ps1` y `publish-iis.ps1` |
| `12-app-navegador.png` | `http://localhost:8080` con la tienda funcionando (idealmente con sesión iniciada) |
| `13-api-health.png` | `http://localhost:8081/health` → JSON `OK` |
| `14-web-health.png` | `http://localhost:8080/health` → "Frontend: OK" + API |

### `evidencias/azure/` (responsables: D las de configuración y E las de deploy y verificación)

| Archivo | Resp. | Contenido |
|---|---|---|
| `01-resource-group.png` | D | `rg-shopcore` con plan, 2 apps, servidor SQL y BD |
| `02-api-overview.png` | D | *Overview* de `shopcore-api-<sufijo>` (nombre, plan, región, URL) |
| `03-web-overview.png` | D | *Overview* de `shopcore-web-<sufijo>` |
| `04-api-app-settings.png` | D | *Environment variables → App settings* con **valores ocultos** |
| `05-api-connection-string.png` | D | *Connection strings* → `DockerConnection` (tipo SQLAzure, valor oculto) |
| `06-https-only.png` | D | HTTPS Only en **On** (ambas apps) |
| `07-sql-database.png` | D | BD `ShopCore` con la oferta gratuita |
| `08-github-actions.png` | E | Ejecución exitosa del workflow (los 2 jobs) |
| `09-web-url-publica.png` | E | `https://<host-web>` con la tienda funcionando |
| `10-api-health.png` | E | `https://<host-api>/health` → `Production`, `1.0.0`, `database: OK` |
| `11-web-health.png` | E | `https://<host-web>/health` |
| `12-redireccion-https.png` | E | `curl -I http://<host-web>` → 301 a https |

---

## 6. Guion del video (`docs/guion-video.md`)

Debe durar entre 8 y 12 minutos, en **grabación continua y narrada**. **Todos hablan**: uno puede manejar la pantalla, pero no puede explicar por todos. Cada quien explica **su** parte y **sus decisiones**.

| # | Min | Quién | Contenido |
|---|---|---|---|
| 1 | 1 | A | Presentación del equipo, la app ShopCore (e-commerce: productos, ofertas, carrito, facturas, informes), el repo y la distribución de roles |
| 2 | 1.5 | C | Arquitectura (WASM + API + SQL), qué se cambió para desplegar (`ApiUrl`, `/health`, configuración por entorno) y cómo se sacaron y rotaron los secretos |
| 3 | 3 | B | IIS: Hosting Bundle, URL Rewrite, los 2 pools, los 2 sitios, rutas, bindings 8080/8081, `web.config`/ANCM, SQL Express con la identidad del pool, scripts, navegador y `/health` |
| 4 | 2 | D | Azure: resource group, plan F1, 2 apps, Azure SQL, App Settings y connection string (**valores ocultos**), HTTPS Only, por qué Windows |
| 5 | 2 | E | Workflow de GitHub Actions en ejecución, URL pública, `/health` con `version: 1.0.0`, redirección HTTP→HTTPS, login en la app |
| 6 | 1 | A | Commits y PRs por integrante (*Insights → Contributors*), carpeta de evidencias, tag y cierre |

**Antes de grabar:**
- "Despertar" la app y la BD de Azure abriendo `/health`.
- Cerrar las pestañas que muestren secretos.
- **No** abrir el valor de los App Settings, el publish profile, la connection string ni las variables del pool.

Al terminar, subir el video (YouTube **no listado**, Drive o Stream con acceso "cualquiera con el enlace") y **probar el enlace en una ventana de incógnito**.

---

## 7. Plantilla de README.md

```markdown
# ShopCore — Tienda en línea

## Integrantes
- <Nombre completo de A>
- <Nombre completo de B>
- <Nombre completo de C>
- <Nombre completo de D>
- <Nombre completo de E>

## Descripción
ShopCore es una tienda en línea (e-commerce) donde los usuarios se registran, consultan productos y ofertas,
gestionan su carrito de compras y generan facturas; los administradores gestionan productos, usuarios e informes.

## Responsabilidades
| Integrante | Responsabilidad | Contribución verificable |
|---|---|---|
| <Nombre de A> | Coordinación e integración | Estructura del repo, `.gitignore`, README, informe PDF, tag `practica3-v1.0` (PR #, PR #) |
| <Nombre de B> | Publicación en IIS | `deploy/iis/*`, `evidencias/iis/` (PR #) |
| <Nombre de C> | Preparación de la app | `/health`, `ApiUrl`, configuración por entorno, retiro de secretos, guion del video (PR #, PR #) |
| <Nombre de D> | Infraestructura Azure | `deploy/azure/provision.sh`, `deploy/azure/README.md`, evidencias de configuración (PR #) |
| <Nombre de E> | Despliegue y verificación en Azure | `.github/workflows/azure-deploy.yml`, `deploy/azure/deploy-zip.sh`, evidencias de URL y `/health` (PR #) |

## Tecnología
- Frontend: Blazor WebAssembly (.NET 8)
- Backend: ASP.NET Core Web API (.NET 8), EF Core, ASP.NET Identity + JWT
- Base de datos: SQL Server Express (IIS) · Azure SQL Database (Azure)
- Servicios externos: Cloudinary (imágenes)
- Hosting: IIS 10 + ASP.NET Core Module V2 · Azure App Service Windows (F1)
- CI/CD: GitHub Actions (`azure/webapps-deploy`)

## Publicación
| Entorno | Fecha | Frontend | API / health |
|---|---|---|---|
| IIS (local) | AAAA-MM-DD | http://localhost:8080 | http://localhost:8081/health |
| Azure App Service | AAAA-MM-DD | https://<host-web> | https://<host-api>/health |

## Video de evidencia
<enlace>

## Cómo verificar el despliegue
1. Abrir https://<host-web> (la primera carga puede tardar unos segundos: plan F1 y BD serverless).
2. Abrir https://<host-api>/health → `status: OK`, `environment: Production`, `version: 1.0.0`.
3. Abrir https://<host-web>/health → "Frontend: OK" y estado de la API.
4. `http://` redirige a `https://` (HTTPS Only).
5. Evidencias en `evidencias/iis/` y `evidencias/azure/`.

## Reproducir el despliegue
- IIS: ver `deploy/iis/README.md`
- Azure: ver `deploy/azure/README.md`
- Configuración y secretos: ver `docs/configuracion.md`

## Configuración por entorno
| Entorno | ASPNETCORE_ENVIRONMENT | App:Version | Base de datos |
|---|---|---|---|
| Desarrollo | Development | — | LocalDB |
| IIS | Production (variable del pool) | 1.0.0-iis | SQL Server Express |
| Azure | Production (App Setting) | 1.0.0 (App Setting) | Azure SQL |

## Notas y problemas resueltos
- El envío de correos (confirmación de cuenta y recuperación de contraseña) está fuera del alcance de esta práctica; la verificación se hace con el usuario administrador del seed.
- <problema → causa → solución>
```

> **Credenciales de prueba:** si el docente necesita iniciar sesión, la cuenta de prueba se indica en el **PDF entregado en Teams**, no en el README. Además, conviene cambiar la contraseña del administrador del seed en Azure, porque la que viene en `SeedDb.cs` es pública dentro del repo.

---

## 8. Checklist final antes de entregar en Teams

**Técnico**
- [ ] `http://localhost:8080`, `http://localhost:8080/health` y `http://localhost:8081/health` funcionan (demostrado en evidencias y video).
- [ ] `https://<host-web>`, `https://<host-web>/health` y `https://<host-api>/health` funcionan **desde otro equipo o red**.
- [ ] `http://` redirige a `https://` en Azure.
- [ ] Al menos un App Setting pertinente (`ASPNETCORE_ENVIRONMENT=Production`) y HTTPS Only, visibles en las evidencias.

**Repositorio y equipo**
- [ ] El repo no tiene nombre genérico (`ShopCore` ✔).
- [ ] **`oalarconpe` aparece en *Settings → Collaborators*** (por usuario, no por correo) y aceptó la invitación o esta sigue vigente.
- [ ] Cada integrante tiene al menos un commit o PR propio **no trivial** (*Insights → Contributors*).
- [ ] README completo: nombres completos, descripción, responsabilidades, tecnología, fechas, URL de Azure, enlace al video y notas de verificación.
- [ ] `evidencias/iis/` y `evidencias/azure/` completas.
- [ ] **No hay secretos** en el repo (`git grep -nE "Password|ApiSecret|jwtKey" -- ':!docs'` solo devuelve claves sin valor), ni en las capturas, ni en el video. Las credenciales viejas fueron rotadas.
- [ ] El tag `practica3-v1.0` está publicado (plus).

**Entrega en Teams** (solo dentro de la tarea)
- [ ] Enlace al repositorio de GitHub.
- [ ] Enlace al video (probado en incógnito).
- [ ] Documento **PDF** con los puntos solicitados y las evidencias.
- [ ] Un integrante distinto al coordinador revisa que **todos los enlaces funcionen**.

---

## 9. Prompts opcionales para Claude Code

Para usar en la máquina de cada integrante. Revisen siempre el diff y **hagan ustedes el commit**: tienen que poder explicar cada línea.

```
Lee docs/plan-de-trabajo.md. Crea la rama <rama> desde main y ejecuta solo el paquete <WP-XX>.
No hagas commit ni push. No modifiques la lógica de negocio ni escribas secretos.
Al terminar muestra el diff y propone un mensaje de commit (Conventional Commits).
```
