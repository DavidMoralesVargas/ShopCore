using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Tienda_Online.Frontend.Repositories;
using Tienda_Online.Shared.DTOs;
using Tienda_Online.Shared.Entidades;

namespace Tienda_Online.Frontend.Pages.InformesProducto
{
    [Authorize(Roles = "Administrador, Supervisor")]
    public partial class InformeIndex
    {
        private int currentPage = 1;
        private int totalPages;
        [Inject] private IRepository Repository { get; set; } = null!;
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;

        [Parameter, SupplyParameterFromQuery] public string Pagina { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public string Filtro { get; set; } = string.Empty;


        public List<InformesDTO>? informes { get; set; }
        protected override async Task OnInitializedAsync()
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
            var url = $"api/informes/ObtenerTotalInformes/?pagina={page}";
            if (!string.IsNullOrEmpty(Filtro))
            {
                url += $"&filtro={Filtro}";
            }
            var responseHttp = await Repository.GetAsync<List<InformesDTO>>(url);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return false;
            }
            informes = responseHttp.Response;
            return true;
        }

        private async Task LoadPagesAsync()
        {
            var url = $"api/informes/ObtenerTotalPaginas";
            if (!string.IsNullOrEmpty(Filtro))
            {
                url += $"?filtro={Filtro}";
            }
            var responseHttp = await Repository.GetAsync<int>(url);
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