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
| [Nombre completo de A] | Coordinación e integración | |
| [Nombre completo de B] | Publicación en IIS | |
| [Nombre completo de C] | Preparación de la app | |
| [Nombre completo de D] | Infraestructura Azure | |
| [Nombre completo de E] | Despliegue y verificación en Azure | |

## 3. Descripción de la aplicación · *A*

- Qué hace la aplicación (funcionalidades principales).
- Arquitectura: Frontend Blazor WebAssembly + API ASP.NET Core 8 + SQL Server (diagrama).
- Alcance: el envío de correos está fuera del alcance; se verifica con el usuario administrador del seed.

## 4. Preparación de la aplicación para el despliegue · *C*

- Endpoint `/health` de la API y página `/health` del frontend.
- URL de la API configurable (`ApiUrl`).
- Configuración por entorno (`appsettings.Production.json`, `App:Version`).
- Retiro y rotación de secretos (User Secrets / variables de entorno).

## 5. Parte 1 — Publicación en IIS · *B*

Describir cada actividad (1–8 del enunciado) con su captura:

| Actividad | Evidencia |
|---|---|
| IIS habilitado | `evidencias/iis/01-iis-habilitado.png` |
| Hosting Bundle / ANCM | `evidencias/iis/02-hosting-bundle.png` |
| URL Rewrite | `evidencias/iis/03-url-rewrite.png` |
| SQL Server Express y login del pool | `evidencias/iis/04-sql-login-pool.png` |
| Application Pools | `evidencias/iis/05-app-pools.png` |
| Sitio Web: ruta física y binding 8080 | `evidencias/iis/06-sitio-web-config.png` |
| Sitio API: ruta física y binding 8081 | `evidencias/iis/07-sitio-api-config.png` |
| Punto de entrada Web | `evidencias/iis/08-punto-entrada-web.png` |
| Punto de entrada API (`web.config`) | `evidencias/iis/09-punto-entrada-api.png` |
| Variables del pool (valores ocultos) | `evidencias/iis/10-variables-pool.png` |
| Ejecución de scripts | `evidencias/iis/11-scripts-ejecucion.png` |
| Aplicación en el navegador | `evidencias/iis/12-app-navegador.png` |
| `/health` API | `evidencias/iis/13-api-health.png` |
| `/health` Web | `evidencias/iis/14-web-health.png` |

## 6. Parte 2 — Infraestructura en Azure App Service · *D*

Actividades 9, 10, 12 y 13 del enunciado:

| Actividad | Evidencia |
|---|---|
| Resource group y recursos | `evidencias/azure/01-resource-group.png` |
| App Service API | `evidencias/azure/02-api-overview.png` |
| App Service Web | `evidencias/azure/03-web-overview.png` |
| Application Settings (valores ocultos) | `evidencias/azure/04-api-app-settings.png` |
| Connection string (valor oculto) | `evidencias/azure/05-api-connection-string.png` |
| HTTPS Only | `evidencias/azure/06-https-only.png` |
| Azure SQL Database | `evidencias/azure/07-sql-database.png` |

## 7. Parte 2 — Despliegue y verificación en Azure · *E*

Actividades 11, 14 y 15 del enunciado:

| Actividad | Evidencia |
|---|---|
| Workflow de GitHub Actions | `evidencias/azure/08-github-actions.png` |
| Aplicación en la URL pública | `evidencias/azure/09-web-url-publica.png` |
| `/health` API | `evidencias/azure/10-api-health.png` |
| `/health` Web | `evidencias/azure/11-web-health.png` |
| Redirección HTTP → HTTPS | `evidencias/azure/12-redireccion-https.png` |

## 8. Trabajo colaborativo · *A*

- Flujo de trabajo: una rama y un Pull Request por paquete, revisión por otro integrante.
- Captura de *Insights → Contributors* y lista de PRs mergeados.
- Tag `practica3-v1.0`.

## 9. Problemas encontrados y conclusiones · *Todos*

| Problema | Causa | Solución | Quién |
|---|---|---|---|
| | | | |

Conclusiones del equipo.
