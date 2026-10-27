using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Tienda_Online.Frontend.Repositories;
using Tienda_Online.Frontend.Services;
using Tienda_Online.Shared.DTOs;
using Tienda_Online.Shared.Enums;

namespace Tienda_Online.Frontend.Pages.Auth
{
    public partial class Register
    {
        private UserDTO userDTO = new();
        private bool loading;
        [Inject] NavigationManager NavigationManager { get; set; } = null!;
        [Inject] SweetAlertService SweetAlertService { get; set; } = null!;
        [Inject] IRepository Repository { get; set; } = null!;
        [Inject] ILoginService LoginService { get; set; } = null!;

        private async Task CreateUserAsync()
        {
            loading = true;
            userDTO.UserName = userDTO.Email;
            userDTO.userType = UserType.Cliente;
            var responseHttp = await Repository.PostAsync<UserDTO>("/api/cuentas/CreateUser", userDTO);
            loading = false;    
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            await SweetAlertService.FireAsync("Confirmación", "Su cuenta ha sido creada con exito. Se te ha enviado un correo electrónico con las instrucciones para activa su cuenta", SweetAlertIcon.Info);
            NavigationManager.NavigateTo("/");
        }
    }
}