using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Tienda_Online.Frontend.Repositories;
using Tienda_Online.Shared.Entidades;

namespace Tienda_Online.Frontend.Pages.Comidas
{
    public partial class ComidasIndex
    {

        private int currentPage = 1;
        private int totalPages;
        [Inject] SweetAlertService SweetAlertService { get; set; } = null!;
        [Inject] NavigationManager navigationManager { get; set; } = null!;
        [Inject] IRepository repository { get; set; } = null!;
        [Parameter, SupplyParameterFromQuery] public string Pagina { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public string Filtro { get; set; } = string.Empty;


        public List<Producto>? Productos { get; set; }

        public CarritoDeCompra carritoDeCompra = new();

        protected async override Task OnInitializedAsync()
        {
            await LoadAsync();
        }

        private async Task IngresarCarritoCompra(int IdProducto)
        {
            carritoDeCompra.ProductoId = IdProducto;
            carritoDeCompra.CantidadProducto = 1;
            var responseHttp = await repository.PostAsync("/api/carritoCompra/CrearCarrito", carritoDeCompra);
            if(responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message);
                return;
            }
            var toast = SweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.BottomLeft,
                ShowConfirmButton = true,
                Timer = 3000
            });
            await toast.FireAsync(icon: SweetAlertIcon.Success, message: "Producto añadido al carrito de compra.");

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
            var url = $"api/productos/?pagina={page}";
            if (!string.IsNullOrEmpty(Filtro))
            {
                url += $"&filtro={Filtro}";
            }
            var responseHttp = await repository.GetAsync<List<Producto>>(url);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return false;
            }
            Productos = responseHttp.Response;
            return true;
        }

        private async Task LoadPagesAsync()
        {
            var url = $"api/productos/ObtenerTotalPaginas";
            if (!string.IsNullOrEmpty(Filtro))
            {
                url += $"?filtro={Filtro}";
            }
            var responseHttp = await repository.GetAsync<int>(url);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
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

    }
}