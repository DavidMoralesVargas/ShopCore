using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Tienda_Online.Frontend.Repositories;
using Tienda_Online.Frontend.Services;
using Tienda_Online.Shared.DTOs;
using Tienda_Online.Shared.Enums;

namespace Tienda_Online.Frontend.Pages.Usuarios
{
    public partial class UsuarioForm
    {
        private UserDTO userDTO = new();
        private bool loading;
        public int TipoUsuarioId;
        [Inject] NavigationManager NavigationManager { get; set; } = null!;
        [Inject] SweetAlertService SweetAlertService { get; set; } = null!;
        [Inject] IRepository Repository { get; set; } = null!;
        [Inject] ILoginService LoginService { get; set; } = null!;

        private async Task CreateUserAsync()
        {
            loading = true;
            userDTO.UserName = userDTO.Email;
            if(TipoUsuarioId == 1)
            {
                userDTO.userType = UserType.Supervisor;
            }
            if (TipoUsuarioId == 2)
            {
                userDTO.userType = UserType.AsesorComercial;
            }
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