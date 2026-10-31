# Evidencias - Publicación en Azure App Service

> Publicación realizada con **Visual Studio** (Publicar → Azure App Service Windows).

- **Infraestructura:** David Morales (integrante D)
- **Publicación y verificación:** Brayan Arango (integrante E)
- **Fecha de publicación:** 2026-10-29

### Resumen de la configuración

| Elemento | API (Backend) | Tienda (Frontend Blazor WASM) |
|---|---|---|
| Grupo de recursos | `ShopCore` | `ShopCore` |
| App Service | `ShopCoreAPI` | `ShopCoreWeb` |
| App Service Plan | `ASP-ShopCore-92e8` | `ASP-ShopCore-92e8` |
| Sistema operativo / región | Windows · Central US | Windows · Central US |
| URL | https://shopcoreapi-bgegbefuf2bnavbr.centralus-01.azurewebsites.net | https://shopcoreweb-g4eqcddtajh2gph5.centralus-01.azurewebsites.net |
| Application Settings | `App__Version`, `ASPNETCORE_ENVIRONMENT`, `ConnectionStrings__DefaultConnection`, `jwtKey`, `Cloudinary__ApiKey`, `Cloudinary__ApiSecret`, `Cloudinary__CloudName` (valores ocultos) | — |
| HTTPS Only | On | On |
| Base de datos | Azure SQL Database | — |

---

## Capturas

**No mostrar contraseñas, cadenas de conexión ni claves**: no pulsar "Show values" y, si hace falta, recortar o tapar.

### Infraestructura (David Morales)

| Archivo | Qué muestra | Estado |
|---|---|---|
| `01_resource_group.PNG` | Grupo de recursos `ShopCore` en la suscripción Azure for Students | ✅ |
| `02_AppServiceAPI.PNG` | *Overview* de `ShopCoreAPI`: estado Running, región Central US, Windows, .NET 8 y dominio | ✅ |
| `03_AppServiceWEB.PNG` | *Overview* de `ShopCoreWeb`: estado Running, región Central US, Windows y dominio | ✅ |
| `04_EnvironmentVariables.PNG` | *Environment variables → App settings* de la API: entorno, cadena de conexión, Cloudinary y `jwtKey` (valores ocultos) | ✅ |
| `05_CreacionAppVersion.PNG` | Application Setting `App__Version` agregada (valor oculto) | ✅ |
| `06_https_only.png` | *Configuration → General settings*: **HTTPS Only = On** (API y tienda) | ⏳ Pendiente |
| `07_azure_sql.png` | Azure SQL Database creada | ⏳ Pendiente |

### Publicación y verificación (Brayan Arango)

| Archivo | Qué debe mostrar | Estado |
|---|---|---|
| `08_publicacion_visual_studio.png` | Visual Studio con el perfil de publicación de Azure App Service y el mensaje de publicación correcta | ⏳ Pendiente |
| `09_web_url_publica.png` | La tienda funcionando en su URL pública de Azure, con la barra de direcciones visible | ⏳ Pendiente |
| `10_api_health.png` | `https://<api>/health` con `status: OK`, `database: OK`, `environment: Production` y `version: 1.0.0` | ⏳ Pendiente |
| `11_web_health.png` | `https://<tienda>/health` con "Frontend: OK" y la respuesta de la API | ⏳ Pendiente |
| `12_redireccion_https.png` | Al entrar con `http://`, la redirección a `https://` (HTTPS Only) | ⏳ Pendiente |

---

## Guía de infraestructura (David Morales)

_Completar: pasos para crear el grupo de recursos, el plan, las dos aplicaciones y Azure SQL; configuración de Application Settings (incluida la cadena de conexión) y HTTPS Only; problemas encontrados._

## Guía de publicación (Brayan Arango)

_Completar: pasos para publicar la API y la tienda desde Visual Studio, verificación de la URL pública y de `/health`, problemas encontrados._
