using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Tienda_Online.Frontend.Repositories;

namespace Tienda_Online.Frontend.Pages.Auth
{
    public partial class ConfirmEmail
    {
        private string? message;

        [Inject] NavigationManager NavigationManager { get; set; } = null!;
        [Inject] SweetAlertService SweetAlertService { get; set; } = null!;
        [Inject] IRepository Repository { get; set; } = null!;

        [Parameter, SupplyParameterFromQuery]
        public string UserId { get; set; } =string.Empty;

        [Parameter]
        [SupplyParameterFromQuery]
        public string Token {  get; set; } = string.Empty;

        protected async Task ConfirmAccountAsync()
        {
            var responseHttp = await Repository.GetAsync($"/api/cuentas/ConfirmEmail/?userId={UserId}&token={Token}");
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                NavigationManager.NavigateTo("/");
                return;
            }

            await SweetAlertService.FireAsync("Confirmación", "Gracias por confirmar su email, ahora puedes ingresar al sistema", SweetAlertIcon.Info);
            NavigationManager.NavigateTo("/Login");
        }
    }
}