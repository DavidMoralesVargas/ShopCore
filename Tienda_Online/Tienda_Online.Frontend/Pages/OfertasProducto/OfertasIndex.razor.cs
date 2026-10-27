using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Tienda_Online.Frontend.Repositories;
using Tienda_Online.Shared.DTOs;
using Tienda_Online.Shared.Entidades;

namespace Tienda_Online.Frontend.Pages.OfertasProducto
{
    public partial class OfertasIndex
    {
        [Inject] SweetAlertService sweetAlertService { get; set; } = null!;
        [Inject] IRepository repository { get; set; } = null!;
        [Inject] NavigationManager navigationManager { get; set; } = null!;

        private int currentPage = 1;
        private int totalPages;
        [Parameter, SupplyParameterFromQuery] public string Pagina { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public string Filtro { get; set; } = string.Empty;

        public CarritoDeCompra carritoDeCompra = new();

        public List<OfertaDTO>? Ofertas { get; set; }

        protected async override Task OnInitializedAsync()
        {
            await LoadAsync();
        }

        private async Task SelectedPageAsync(int page)
        {
            currentPage = page;
            await LoadAsync(page);
        }

        private async Task LoadAsync(int page = 1)
        {
            if (!string.IsNullOrWhiteSpace(Pagina))
            {
                page = Convert.ToInt32(Pagina);
            }

            var ok = await LoadListAsync(page);
            if (ok)
            {
                await LoadPagesAsync();
            }
        }

        private async Task<bool> LoadListAsync(int page)
        {
            var url = $"api/promociones/ObtenerListaPromocion/?pagina={page}";
            if (!string.IsNullOrEmpty(Filtro))
            {
                url += $"&filtro={Filtro}";
            }
            var responseHttp = await repository.GetAsync<List<OfertaDTO>>(url);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return false;
            }
            Ofertas = responseHttp.Response;
            return true;
        }
        private async Task LoadPagesAsync()
        {
            var url = $"api/promociones/ObtenerTotalPaginas";
            if (!string.IsNullOrEmpty(Filtro))
            {
                url += $"?filtro={Filtro}";
            }
            var responseHttp = await repository.GetAsync<int>(url);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            totalPages = responseHttp.Response;
        }

        private async Task CleanFilterAsync()
        {
            Filtro = string.Empty;
            await ApplyFilterAsync();
        }

        private async Task ApplyFilterAsync()
        {
            int page = 1;
            await LoadAsync(page);
            await SelectedPageAsync(page);
        }
        private async Task IngresarCarritoCompra(int IdProducto)
        {
            carritoDeCompra.ProductoId = IdProducto;
            carritoDeCompra.CantidadProducto = 1;
            var responseHttp = await repository.PostAsync("/api/carritoCompra/CrearCarrito", carritoDeCompra);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await sweetAlertService.FireAsync("Error", message);
                return;
            }
            var toast = sweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.BottomLeft,
                ShowConfirmButton = true,
                Timer = 3000
            });
            await toast.FireAsync(icon: SweetAlertIcon.Success, message: "Producto añadido al carrito de compra.");

        }

        private async Task DeleteAsyc(OfertaDTO oferta)
        {
            var result = await sweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "Confirmación",
                Text = $"Estás seguro que quieres borrar el producto: {oferta.DescripcionProducto}",
                Icon = SweetAlertIcon.Question,
                ShowCancelButton = true
            });

            var confirm = string.IsNullOrEmpty(result.Value);

            if (confirm)
            {
                return;
            }

            var responseHttp = await repository.DeleteAsync($"/api/promociones/{oferta.IdPromocion}");
            if(responseHttp.Error)
            {
                if(responseHttp.HttpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    navigationManager.NavigateTo("/");
                }
                else
                {
                    var message = await responseHttp.GetErrorMessageAsync();
                    await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                    return;
                }
            }
            await LoadAsync();
            var toast = sweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.BottomLeft,
                ShowConfirmButton = true,
                Timer = 3000,
                ConfirmButtonColor = "#0047FF",
                CancelButtonText = "Cancelar"
            });
            await toast.FireAsync(icon: SweetAlertIcon.Success, message: "Registro borrado con éxito.");
        }

    }
}