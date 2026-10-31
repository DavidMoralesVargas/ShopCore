# ShopCore — Tienda en línea

Práctica 3 · Despliegue de Software — Publicación en **IIS (On-Premises)** y **Azure App Service**.

## Integrantes
- [Nombre completo de A]
- Jhon Eduardo Zabala Garzon
- [Nombre completo de C]
- [Nombre completo de D]
- [Nombre completo de E]

## Descripción
ShopCore es una tienda en línea (e-commerce) donde los usuarios consultan productos y ofertas, gestionan su
carrito de compras y generan facturas; los administradores gestionan productos, usuarios e informes de productos.

## Responsabilidades
| Integrante | Responsabilidad | Contribución verificable |
|---|---|---|
| [Nombre de A] | Coordinación e integración | Estructura del repo, `.gitignore`, README, informe PDF, tag `practica3-v1.0` (PR #, PR #) |
| Jhon Eduardo Zabala Garzon | Publicación en IIS | Publicación en IIS (Windows Server), guía y capturas en `evidencias/iis/` (PR #3) |
| [Nombre de C] | Preparación de la app | `/health`, `ApiUrl`, configuración por entorno, retiro de secretos, guion del video (PR #, PR #) |
| [Nombre de D] | Infraestructura Azure | Recursos de Azure (App Services, Azure SQL, App Settings, HTTPS Only), `deploy/azure/README.md` (infraestructura), evidencias de configuración (PR #) |
| [Nombre de E] | Despliegue y verificación en Azure | Publicación desde Visual Studio, `deploy/azure/README.md` (publicación), evidencias de URL y `/health` (PR #) |

## Tecnología
- Frontend: Blazor WebAssembly (.NET 8) — `Tienda_Online/Tienda_Online.Frontend`
- Backend: ASP.NET Core Web API (.NET 8), EF Core, ASP.NET Identity + JWT — `Tienda_Online/Tienda_Online.Backend`
- Librería compartida: `Tienda_Online/Tienda_Online.Shared`
- Base de datos: SQL Server Express (IIS) · Azure SQL Database (Azure)
- Servicios externos: Cloudinary (imágenes)
- Hosting: IIS 10 + ASP.NET Core Module V2 · Azure App Service Windows (F1)
- Publicación en Azure: Visual Studio (Publicar → Azure App Service Windows)

## Publicación
| Entorno | Fecha | Frontend | API / health |
|---|---|---|---|
| IIS (local) | 2026-10-27 | https://192.168.11.81:8000 | https://192.168.11.81:7000/health |
| Azure App Service | AAAA-MM-DD | https://shopcoreweb-g4eqcddtajh2gph5.centralus-01.azurewebsites.net | https://shopcoreapi-bgegbefuf2bnavbr.centralus-01.azurewebsites.net/health |

## Video de evidencia
[enlace al video]

## Cómo verificar el despliegue
1. Abrir https://shopcoreweb-g4eqcddtajh2gph5.centralus-01.azurewebsites.net (la primera carga puede tardar unos segundos: plan F1 y base de datos serverless).
2. Abrir https://shopcoreapi-bgegbefuf2bnavbr.centralus-01.azurewebsites.net/health → `status: OK`, `environment: Production`, `version: 1.0.0`.
3. Abrir https://shopcoreweb-g4eqcddtajh2gph5.centralus-01.azurewebsites.net/health → "Frontend: OK" y estado de la API.
4. `http://` redirige a `https://` (HTTPS Only).
5. Evidencias en [`evidencias/iis/`](evidencias/iis/) y [`evidencias/azure/`](evidencias/azure/).

## Reproducir el despliegue
- IIS: ver [`evidencias/iis/README.md`](evidencias/iis/README.md)
- Azure: ver [`deploy/azure/README.md`](deploy/azure/)
- Configuración y secretos: ver [`docs/configuracion.md`](docs/configuracion.md)

## Configuración por entorno
| Entorno | ASPNETCORE_ENVIRONMENT | App:Version | Base de datos |
|---|---|---|---|
| Desarrollo | Development | — | LocalDB |
| IIS | Production (variable del pool) | 1.0.0-iis | SQL Server Express |
| Azure | Production (App Setting) | 1.0.0 (App Setting) | Azure SQL |

## Estructura del repositorio
```
ShopCore/
├─ README.md
├─ Tienda_Online/          # código fuente (.NET 8: Frontend, Backend, Shared)
├─ deploy/
│  ├─ iis/                 # scripts y guía de publicación en IIS
│  └─ azure/               # guía de publicación en Azure App Service
├─ docs/                   # configuración, guion del video, informe
└─ evidencias/
   ├─ iis/
   └─ azure/
```

## Notas y problemas resueltos
- El envío de correos (confirmación de cuenta y recuperación de contraseña) está fuera del alcance de esta práctica; la verificación se hace con el usuario administrador creado por el seed.
- [problema → causa → solución]
