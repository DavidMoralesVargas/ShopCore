using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Tienda_Online.Frontend.Repositories;
using Tienda_Online.Shared.DTOs;

namespace Tienda_Online.Frontend.Pages.Auth
{
    public partial class RecoverPassword
    {
        private EmailDTO email = new();
        private bool loading;

        [Inject] private NavigationManager NavigationManager { get; set; } = null!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;
        [Inject] private IRepository Repository { get; set; } = null!;

        private async Task SendRecoverPasswordEmailTokenAsync()
        {
            loading = true;
            var responseHttp = await Repository.PostAsync("/api/cuentas/RecoverPassword", email);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                loading = false;
                return;
            }
            loading = false;
            await SweetAlertService.FireAsync("Confirmación", "Se te han enviado un correo electrónico con las instrucciones para recuperar su contraseña", SweetAlertIcon.Info);
            NavigationManager.NavigateTo("/");
        }
    }
}