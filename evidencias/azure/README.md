# Evidencias - Azure App Service

Guarda aquí las capturas legibles (PNG) con estos nombres. **No muestres contraseñas, cadenas de conexión completas ni claves** (usa "Hidden value" o recorta).

| Archivo | Qué debe mostrar |
|---|---|
| `01_resource_group.png` | Grupo de recursos con App Service, App Service Plan y Azure SQL |
| `02_app_service_overview.png` | Overview del App Service (nombre, región, plan, URL) |
| `03_azure_sql.png` | Azure SQL Database creada y regla de firewall "Allow Azure services" |
| `04_publicacion.png` | Publicación (Visual Studio / VS Code / `az webapp deploy`) exitosa |
| `05_application_settings.png` | Environment variables: `ASPNETCORE_ENVIRONMENT=Production` (valores secretos ocultos) |
| `06_connection_string.png` | Connection string `DefaultConnection` tipo SQLAzure con valor OCULTO |
| `07_https_only.png` | Configuration -> General settings -> HTTPS Only = On |
| `08_app_url_publica.png` | `https://<app>.azurewebsites.net` mostrando ShopCore |
| `09_health.png` | `https://<app>.azurewebsites.net/health` respondiendo `Healthy` |
