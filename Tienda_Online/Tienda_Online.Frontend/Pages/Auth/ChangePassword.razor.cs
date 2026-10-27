using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Tienda_Online.Frontend.Repositories;
using Tienda_Online.Shared.DTOs;

namespace Tienda_Online.Frontend.Pages.Auth
{
    public partial class ChangePassword
    {
        private ChangePasswordDTO changePasswordDTO = new();

        private bool loading;

        [Inject] private NavigationManager NavigationManager { get; set; } = null!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;
        [Inject] IRepository Repository { get; set; } = null!;

        private async Task ChangePasswordAsync()
        {
            loading = true;
            var responseHttp = await Repository.PostAsync("/api/cuentas/changePassword", changePasswordDTO);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            loading = false;
            NavigationManager.NavigateTo("/");
            var toast = SweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Timer = 3000,
                ShowConfirmButton = true,
                Position = SweetAlertPosition.BottomLeft
            });
            await toast.FireAsync(icon: SweetAlertIcon.Success, message: "Se cambió la contraseña con éxito");
        }
    }
}