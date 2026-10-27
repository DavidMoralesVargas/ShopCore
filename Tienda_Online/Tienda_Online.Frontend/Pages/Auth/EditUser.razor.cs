using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Tienda_Online.Frontend.Repositories;
using Tienda_Online.Shared.Entidades;

namespace Tienda_Online.Frontend.Pages.Auth
{
    public partial class EditUser
    {
        private Usuario? user;
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;
        [Inject] IRepository Repository { get; set; } = null!;
        [Inject] SweetAlertService SweetAlertService { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            await LoadUserAsync();
        }

        private async Task LoadUserAsync()
        {
            var responseHttp = await Repository.GetAsync<Usuario>($"/api/cuentas");
            if (responseHttp.Error)
            {
                if(responseHttp.HttpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/");
                    return;
                }
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            user = responseHttp.Response;
        }

        private async Task SaveUserAsync()
        {
            var responseHttp = await Repository.PutAsync<Usuario>("/api/cuentas", user!);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            NavigationManager.NavigateTo("/");
        }
    }
}