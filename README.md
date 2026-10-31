# ShopCore — Tienda en línea

Práctica 3 · Despliegue de Software — Publicación en **IIS (On-Premises)** y **Azure App Service**.

## Integrantes
- Sebastian Villa
- Jhon Eduardo Zabala Garzón
- Laura Posada
- David Morales Vargas
- Brayan Arango

## Descripción
ShopCore es una tienda en línea (e-commerce) donde los usuarios consultan productos y ofertas, gestionan su
carrito de compras y generan facturas; los administradores gestionan productos, usuarios e informes de productos.

## Responsabilidades
| Integrante | Responsabilidad | Contribución verificable |
|---|---|---|
| Sebastian Villa | Coordinación e integración | Estructura del repo, `.gitignore`, README, documentación, informe PDF y tag `practica3-v1.0` (PR #1 y commits de documentación) |
| Jhon Eduardo Zabala Garzón | Publicación en IIS | Publicación en IIS (Windows Server), guía y capturas en `evidencias/iis/` (PR #3) |
| Laura Posada | Preparación de la app | `/health`, `ApiUrl`, configuración por entorno, retiro de secretos (PR #2) |
| David Morales Vargas | Infraestructura Azure | Recursos de Azure (App Services, Azure SQL, App Settings, HTTPS Only), ajustes de configuración para Azure y capturas de infraestructura en `evidencias/azure/` (commits en `main`) |
| Brayan Arango | Despliegue y verificación en Azure | Publicación desde Visual Studio, guía de publicación en `evidencias/azure/README.md`, evidencias de URL y `/health` (PR #) |

## Tecnología
- Frontend: Blazor WebAssembly (.NET 8) — `Tienda_Online/Tienda_Online.Frontend`
- Backend: ASP.NET Core Web API (.NET 8), EF Core, ASP.NET Identity + JWT — `Tienda_Online/Tienda_Online.Backend`
- Librería compartida: `Tienda_Online/Tienda_Online.Shared`
- Base de datos: SQL Server Express (IIS) · Azure SQL Database (Azure)
- Servicios externos: Cloudinary (imágenes)
- Hosting: IIS 10 + ASP.NET Core Module V2 · Azure App Service Windows
- Publicación en Azure: Visual Studio (Publicar → Azure App Service Windows)

## Publicación
| Entorno | Fecha | Frontend | API / health |
|---|---|---|---|
| IIS (local) | 2026-10-27 | https://192.168.11.81:8000 | https://192.168.11.81:7000/health |
| Azure App Service | 2026-10-29 | https://shopcoreweb-g4eqcddtajh2gph5.centralus-01.azurewebsites.net | https://shopcoreapi-bgegbefuf2bnavbr.centralus-01.azurewebsites.net/health |

## Video de evidencia
[enlace al video]

## Cómo verificar el despliegue
1. Abrir https://shopcoreweb-g4eqcddtajh2gph5.centralus-01.azurewebsites.net (la primera carga puede tardar unos segundos si la aplicación estaba inactiva).
2. Abrir https://shopcoreapi-bgegbefuf2bnavbr.centralus-01.azurewebsites.net/health → `status: OK`, `environment: Production`, `version: 1.0.0`.
3. Abrir https://shopcoreweb-g4eqcddtajh2gph5.centralus-01.azurewebsites.net/health → "Frontend: OK" y estado de la API.
4. `http://` redirige a `https://` (HTTPS Only).
5. Evidencias en [`evidencias/iis/`](evidencias/iis/) y [`evidencias/azure/`](evidencias/azure/).

## Reproducir el despliegue
- IIS: ver [`evidencias/iis/README.md`](evidencias/iis/README.md)
- Azure: ver [`evidencias/azure/README.md`](evidencias/azure/README.md)
- Configuración y secretos: ver [`docs/configuracion.md`](docs/configuracion.md)

## Configuración por entorno
| Entorno | ASPNETCORE_ENVIRONMENT | App:Version | Base de datos |
|---|---|---|---|
| Desarrollo | Development | — | LocalDB |
| IIS | Production (por defecto) | sin-configurar | SQL Server Express |
| Azure | Production (App Setting) | 1.0.0 (App Setting) | Azure SQL |

## Estructura del repositorio
```
ShopCore/
├─ README.md
├─ Tienda_Online/          # código fuente (.NET 8: Frontend, Backend, Shared)
├─ docs/                   # configuración, informe
└─ evidencias/
   ├─ iis/                 # capturas y guía de publicación en IIS
   └─ azure/               # capturas y guía de publicación en Azure App Service
```

## Notas y problemas resueltos
- El envío de correos (confirmación de cuenta y recuperación de contraseña) está fuera del alcance de esta práctica; la verificación se hace con el usuario administrador creado por el seed.
- **Secretos en el repositorio:** `appsettings.Development.json` tenía la contraseña del correo, las claves de Cloudinary y la clave JWT → se retiraron del repositorio y se cargan con User Secrets (desarrollo), el archivo de producción del servidor (IIS) y Application Settings (Azure).
- **LocalDB no sirve fuera de desarrollo:** no funciona con la identidad de IIS ni en Azure → SQL Server Express en IIS y Azure SQL Database en Azure, con la cadena de conexión `DefaultConnection`.
- **`/health` mostraba `version: sin-configurar` en Azure:** faltaba la Application Setting → se agregó `App__Version = 1.0.0`.
- **La tienda de IIS consume la API de Azure:** la dirección de la API quedó fija en el frontend para la publicación en Azure; la API de IIS se verifica con su propio `/health`.
