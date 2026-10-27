using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Tienda_Online.Frontend.Repositories;
using Tienda_Online.Shared.Entidades;

namespace Tienda_Online.Frontend.Pages.Usuarios
{
    [Authorize(Roles = "Administrador, Supervisor")]
    public partial class UsuarioIndex
    {
        public List<Usuario>? Usuarios { get; set; }
        private int currentPage = 1;
        private int totalPages;

        [Inject] IRepository Repository { get; set; } = null!;
        [Inject] SweetAlertService SweetAlertService { get; set; } = null!;
        [Inject] NavigationManager NavigationManager { get; set; } = null!;

        [Parameter, SupplyParameterFromQuery] public string Pagina { get; set; } = null!;
        [Parameter, SupplyParameterFromQuery] public string Filtro { get; set; } = null!;

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
            if(!string.IsNullOrWhiteSpace(Pagina))
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
            var url = $"/api/cuentas/all?pagina={page}";
            if (!string.IsNullOrEmpty(Filtro))
            {
                url += $"&filtro={Filtro}";
            }
            var response = await Repository.GetAsync<List<Usuario>>(url);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return false;
            }
            Usuarios = response.Response;
            return true;
        }

        private async Task LoadPagesAsync()
        {
            var url = $"/api/cuentas/totalPages";
            if(!string.IsNullOrEmpty(Filtro))
            {
                url += $"?filtro={Filtro}";
            }

            var response = await Repository.GetAsync<int>(url);
            if(response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            totalPages = response.Response;
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