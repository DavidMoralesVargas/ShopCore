using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Text;
using Tienda_Online.Frontend.Pages.Facturas;
using Tienda_Online.Frontend.Repositories;
using Tienda_Online.Shared.DTOs;
using Tienda_Online.Shared.Entidades;
using Tienda_Online.Shared.Respuesta;

namespace Tienda_Online.Frontend.Pages.CarritosDeCompra
{
    [Authorize(Roles = "Administrador, Supervisor, AsesorComercial, Cliente")]
    public partial class CarritoDeCompraIndex
    {
        [Inject] private IRepository Repository { get; set; } = null!;
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;
        public string Detalle { get; set; } = null!;

        public List<CarritoConProductoDTO>? CarritoConProducto { get; set; }
        public DetalleFactura detalleFactura { get; set; } = null!;

        public int? TotalCompra = 0;
        protected async override Task OnInitializedAsync()
        {
            await CargarDatos();
        }


        private void IncrementarTotalAPagar()
        {
            TotalCompra = 0;
            foreach (var carrito in CarritoConProducto!)
            {
                TotalCompra += Convert.ToInt32(carrito.PrecioTotal);
            }
        }



        private async Task CargarDatos()
        {
            var responseHttp = await Repository.GetAsync<List<CarritoConProductoDTO>>("/api/carritoCompra/ObtenerCarritos");
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }


            CarritoConProducto = responseHttp.Response;
            IncrementarTotalAPagar();
        }

        private async Task IncrementarCantidad(CarritoConProductoDTO carrito)
        {
            var responseHttp = await Repository.GetAsync<CarritoDeCompra>($"/api/carritoCompra/{carrito.IdCarrito}");
            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/");
                }
                else
                {
                    var messageError = await responseHttp.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync("Error", messageError, SweetAlertIcon.Error);
                }
                return;
            }
            var producto = responseHttp.Response;
            producto!.CantidadProducto = producto.CantidadProducto + 1;

            var HttpResponse = await Repository.PutAsync($"/api/carritoCompra/Actualizar", producto);
            if (HttpResponse.Error)
            {
                var meesageError = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", meesageError, SweetAlertIcon.Error);
                return;
            }
            await CargarDatos();
            IncrementarTotalAPagar();
        }

        private async Task DecrementarCantidad(CarritoConProductoDTO carrito)
        {
            var responseHttp = await Repository.GetAsync<CarritoDeCompra>($"/api/carritoCompra/{carrito.IdCarrito}");
            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/");
                }
                else
                {
                    var messageError = await responseHttp.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync("Error", messageError, SweetAlertIcon.Error);
                }
                return;
            }
            var producto = responseHttp.Response;

            if (carrito.CantidadProductos > 1)
            {
                producto!.CantidadProducto = producto.CantidadProducto - 1;

                var HttpResponse = await Repository.PutAsync($"/api/carritoCompra/Actualizar", producto);
                if (HttpResponse.Error)
                {
                    var meesageError = await responseHttp.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync("Error", meesageError, SweetAlertIcon.Error);
                    return;
                }
                await CargarDatos();
                IncrementarTotalAPagar();
            }

        }

        private async Task DeleteAsync(CarritoConProductoDTO carrito)
        {

            var responseHttp = await Repository.DeleteAsync($"/api/carritoCompra/{carrito.IdCarrito}");
            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/");
                }
                else
                {
                    var messageError = await responseHttp.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync("Error", messageError, SweetAlertIcon.Error);
                }
                return;
            }

            await CargarDatos();
            IncrementarTotalAPagar();
        }

        public async Task GenerarFactura(List<CarritoConProductoDTO> carritos)
        {
            var responseHttp = await Repository.PostAsync<List<CarritoConProductoDTO>, AccionRespuesta<string>>("/api/facturas/VerResumenFactura", carritos);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            string resumen = responseHttp.Response!.Respuesta!;

            var VentanaResumen = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "Resumende compra:",
                Html = resumen,
                ShowCancelButton = true,
                Width = "800px",
                ConfirmButtonText = "Confirmar pedido",
                ConfirmButtonColor = "#0D6EFD"
            });

            var confirm = string.IsNullOrEmpty(VentanaResumen.Value);

            if (confirm)
            {
                return;
            }

            var response = await Repository.PostAsync<List<CarritoConProductoDTO>, Factura>("/api/facturas/GenerarFactura", carritos);

            if (response.Error)
            {
                var messageError = await response.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", messageError, SweetAlertIcon.Error);
                return;
            }

            var ventanaConfirm = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "Gracias por su compra",
                ShowCancelButton = true,
                ConfirmButtonText = "Ver detalles",
                ConfirmButtonColor = "#0D6EFD"
            });

            var confirm2 = string.IsNullOrEmpty(ventanaConfirm.Value);


            if (confirm2)
            {
                NavigationManager.NavigateTo("/");
                await EliminarCarritos();
                await ActualizarInformes(carritos);
                return;
            }

            foreach (var carrito in carritos)
            {
                carrito.IdFactura = response.Response!.Id;
            }

            var DetallesFactura = await Repository.PostAsync<List<CarritoConProductoDTO>, AccionRespuesta<string>>("/api/facturas/MostrarFactura", carritos);
            if (DetallesFactura.Error)
            {
                var error = await DetallesFactura.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", error, SweetAlertIcon.Error);
                return;
            }
            Detalle = DetallesFactura.Response!.Respuesta!;
            AbrirNuevaPestana(Detalle);
            await EliminarCarritos();
            await ActualizarInformes(carritos);

        }


        private async Task ActualizarInformes(List<CarritoConProductoDTO> carritos)
        {
            var responseHttp = await Repository.PostAsync<List<CarritoConProductoDTO>, AccionRespuesta<bool>>("/api/informes/GuardarInforme", carritos);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
        }

        private async Task EliminarCarritos()
        {
            var responseHttp = await Repository.GetAsync<AccionRespuesta<bool>>("/api/carritoCompra/EliminarCarritos");
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
        }

        private void AbrirNuevaPestana(string resumen)
        {
            // Codificar el contenido en base64
            var encodedHtml = Convert.ToBase64String(Encoding.UTF8.GetBytes(resumen));

            // Construir la URL
            var url = $"/resumen-factura/{encodedHtml}";

            // Abrir la URL en una nueva pestaña
            NavigationManager.NavigateTo(url, true);
        }
    }
}