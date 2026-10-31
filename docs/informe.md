# Informe — Práctica 3 · Despliegue de Software

> Fuente del **documento PDF que se entrega en Teams**. Cada integrante completa **su** sección en su propio PR.
> A ensambla y exporta a PDF al final (WP-A2). Las imágenes se referencian desde `../evidencias/`.
> No incluir contraseñas, tokens, connection strings ni claves (ni en texto ni en capturas).

---

## 1. Portada · *A*

- **Proyecto:** ShopCore — Tienda en línea
- **Asignatura:** Despliegue de Software · Práctica 3 (20 %)
- **Repositorio:** [enlace al repositorio]
- **Video de evidencia:** [enlace al video]
- **Fecha de entrega:** AAAA-MM-DD

## 2. Integrantes y responsabilidades · *A*

| Integrante | Responsabilidad | Contribución verificable (PR / commits) |
|---|---|---|
| Sebastian Villa | Coordinación e integración | |
| Jhon Eduardo Zabala Garzón | Publicación en IIS | |
| Laura Posada | Preparación de la app | |
| David Morales Vargas | Infraestructura Azure | |
| Brayan Arango | Despliegue y verificación en Azure | |

## 3. Descripción de la aplicación · *A*

- Qué hace la aplicación (funcionalidades principales).
- Arquitectura: Frontend Blazor WebAssembly + API ASP.NET Core 8 + SQL Server (diagrama).
- Alcance: el envío de correos está fuera del alcance; se verifica con el usuario administrador del seed.

## 4. Preparación de la aplicación para el despliegue · *C*

Antes de publicar en IIS y Azure hubo que preparar el código para que se pudiera configurar según el entorno y verificar una vez desplegado. No se modificó la lógica de negocio: solo configuración, un endpoint de salud y una página de verificación.

### 4.1 Problemas encontrados

| # | Problema | Dónde estaba | Consecuencia |
|---|---|---|---|
| 1 | La URL de la API estaba escrita en el código (`https://localhost:7000`) | `Frontend/Program.cs` | El Frontend publicado en IIS o Azure seguiría llamando a `localhost` y no encontraría la API |
| 2 | Había secretos reales versionados: contraseña de la cuenta de correo, `ApiKey` y `ApiSecret` de Cloudinary y la `jwtKey` | `Backend/appsettings.Development.json` | Cualquiera con acceso al repositorio podía usarlos |
| 3 | No existía un endpoint para comprobar que la aplicación y la base de datos estaban vivas | — | No había forma simple de verificar un despliegue |
| 4 | No había configuración para el entorno `Production` | `Backend/appsettings.json` | La API publicada no tenía de dónde tomar sus valores |

### 4.2 Cambios realizados

| Cambio | Archivos | Para qué |
|---|---|---|
| Secretos fuera del repositorio | `appsettings.Development.json`, `Tienda_Online.Backend.csproj` (`UserSecretsId`) | `jwtKey`, `Cloudinary:ApiKey` y `Cloudinary:ApiSecret` se cargan con *User Secrets* en desarrollo. Se eliminó por completo la sección `Mail` (el correo está fuera de alcance) |
| Configuración de Production sin secretos | `appsettings.Production.json` | Solo contiene `Cloudinary:CloudName`, que no es secreto. Los demás valores llegan por variables de entorno (IIS) o App Settings (Azure) |
| Endpoint `GET /health` en la API | `Backend/Program.cs` | Devuelve JSON con `status`, `database`, `environment`, `version` y `utc`. Responde 200 si la base de datos responde y 503 si no |
| URL de la API configurable | `Frontend/Program.cs`, `Frontend/wwwroot/appsettings.json`, `Frontend/wwwroot/appsettings.Development.json` | La URL se lee de `ApiUrl`, con un valor por entorno |
| Página `/health` del Frontend | `Frontend/Pages/Health.razor` | Muestra "Frontend: OK" y consulta `/health` de la API, así una sola página prueba que el Frontend carga y que llega a la API |

**Por qué `ApiUrl` va en `wwwroot/appsettings.json`:** el Frontend es Blazor WebAssembly y corre en el navegador, que no tiene acceso a las variables de entorno del servidor. Por eso su configuración se publica como archivo estático y se descarga con la aplicación. En Azure, el workflow de despliegue reemplaza ese valor por la URL de la API en Azure.

**Por qué User Secrets:** es el mecanismo de .NET para guardar secretos de desarrollo fuera del repositorio. En IIS y Azure esos mismos valores se entregan por variables de entorno del *Application Pool* y por App Settings, respectivamente. La explicación completa está en [`configuracion.md`](configuracion.md).

### 4.3 Rotación de credenciales

Los valores anteriores siguen presentes en el historial de Git, por lo que retirarlos del archivo no es suficiente y hay que invalidarlos:

- **Cuenta de correo:** se revocó la *App Password*. No se generó otra porque el correo no se usa.
- **Cloudinary:** se regeneraron la `ApiKey` y el `ApiSecret` desde *Settings → API Keys → Regenerate*.
- **`jwtKey`:** se generó una nueva (`openssl rand -base64 64`). Los tokens anteriores dejan de funcionar, y eso es lo esperado.


### 4.4 Verificación

Ambas pruebas se hicieron en desarrollo con `dotnet run --launch-profile https`:

1. **API:** `https://localhost:7000/health` respondió HTTP 200 con `status: OK`, `database: OK` y `environment: Development`. `version` aparece como `sin-configurar` porque `App:Version` solo se define en IIS y Azure.


2. **Frontend:** `https://localhost:8000/health` mostró "Frontend: OK" y el estado de la API (`200` con el mismo JSON), lo que confirma que `ApiUrl` se lee de la configuración y que el Frontend llega a la API.


3. **Compilación y revisión de secretos:** `dotnet build Tienda_Online/Tienda_Online.sln` compila sin errores nuevos, y `git grep` de `ApiSecret|ApiKey|jwtKey|Password` en los `.json` del Backend y del Frontend no devuelve ningún valor.

### 4.5 Problemas encontrados durante la preparación

| Problema | Causa | Solución |
|---|---|---|
| El diff de `Program.cs` del Backend mostraba una línea modificada que no se había tocado | El archivo está guardado en ISO-8859-1 y el editor lo guardó como UTF-8, dañando el carácter `ó` del comentario | Se restauró el archivo, se insertó el bloque de `/health` sin cambiar la codificación y se verificó que el diff solo tuviera las líneas nuevas |
| La API no arrancaba con "You must install or update .NET" | El equipo solo tenía el runtime de .NET 10 y el proyecto apunta a net8.0 | Se instalaron los runtimes de .NET 8 (ASP.NET Core Runtime y .NET Runtime) |

## 5. Parte 1 — Publicación en IIS · *B*

La publicación se realizó en **Windows Server** (consultado con el docente) con dos sitios: `ShopCore-API` (https, puerto 7000) y `ShopCore-Web` (https, puerto 8000). La explicación de cada captura está en [`evidencias/iis/README.md`](../evidencias/iis/README.md).

| Actividad del enunciado | Evidencia |
|---|---|
| 1. IIS habilitado | `evidencias/iis/01_iis_rol_instalado.png` |
| Hosting Bundle de ASP.NET Core 8 | `evidencias/iis/02_hosting_bundle.png` |
| URL Rewrite (requerido por Blazor WebAssembly) | `evidencias/iis/03_url_rewrite.png` |
| SQL Server Express y login de la aplicación | `evidencias/iis/04_sql_express.png`, `evidencias/iis/04b_sql_login_shopcore.png` |
| 2. Versión publicable (`dotnet publish`) | `evidencias/iis/05a_dotnet_publish_inicio.png`, `evidencias/iis/05_dotnet_publish.png` |
| 3. Application Pools dedicados | `evidencias/iis/06a_app_pool_api.png`, `evidencias/iis/06b_app_pool_web.png`, `evidencias/iis/06_app_pools.png` |
| Certificado autofirmado para HTTPS | `evidencias/iis/07a_certificado_autofirmado.png` |
| 4–5. Sitio API: ruta física y binding (7000) | `evidencias/iis/07_api_ruta_fisica.png`, `evidencias/iis/08_api_binding.png` |
| 4–5. Sitio Web: ruta física y binding (8000) | `evidencias/iis/09_web_ruta_fisica.png`, `evidencias/iis/10_web_binding.png` |
| 6. Documento predeterminado (`index.html`) | `evidencias/iis/11_documento_predeterminado.png` |
| Configuración de producción (valores ocultos) | `evidencias/iis/12_configuracion_produccion.png` |
| 7. Aplicación en el navegador | `evidencias/iis/13_app_navegador.png` |
| 8. `/health` de la API | `evidencias/iis/14_health.png` |

## 6. Parte 2 — Infraestructura en Azure App Service · *D*

Actividades 9, 10, 12 y 13 del enunciado:

| Actividad | Evidencia |
|---|---|
| Grupo de recursos `ShopCore` | `evidencias/azure/01_resource_group.PNG` |
| App Service de la API (`ShopCoreAPI`) | `evidencias/azure/02_AppServiceAPI.PNG` |
| App Service de la tienda (`ShopCoreWeb`) | `evidencias/azure/03_AppServiceWEB.PNG` |
| Application Settings, incluida la cadena de conexión (valores ocultos) | `evidencias/azure/04_EnvironmentVariables.PNG` |
| Creación de la Application Setting `App__Version` | `evidencias/azure/05_CreacionAppVersion.PNG` |
| HTTPS Only | `evidencias/azure/06_https_only.png` |
| Azure SQL Database | `evidencias/azure/07_azure_sql.png` |

## 7. Parte 2 — Despliegue y verificación en Azure · *E*

Actividades 11, 14 y 15 del enunciado:

| Actividad | Evidencia |
|---|---|
| Publicación desde Visual Studio (perfil de publicación) | `evidencias/azure/08_publicacion_visual_studio.png` |
| Aplicación en la URL pública | `evidencias/azure/09_web_url_publica.png` |
| `/health` API | `evidencias/azure/10_api_health.png` |
| `/health` Web | `evidencias/azure/11_web_health.png` |
| Redirección HTTP → HTTPS | `evidencias/azure/12_redireccion_https.png` |

## 8. Trabajo colaborativo · *A*

- Flujo de trabajo: una rama y un Pull Request por paquete, revisión por otro integrante.
- Captura de *Insights → Contributors* y lista de PRs mergeados.
- Tag `practica3-v1.0`.

## 9. Problemas encontrados y conclusiones · *Todos*

| Problema | Causa | Solución | Quién |
|---|---|---|---|
| | | | |

Conclusiones del equipo.
