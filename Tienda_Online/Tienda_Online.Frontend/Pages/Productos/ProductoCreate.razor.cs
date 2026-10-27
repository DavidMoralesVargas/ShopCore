using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Tienda_Online.Frontend.Repositories;
using Tienda_Online.Shared.Entidades;

namespace Tienda_Online.Frontend.Pages.Productos
{
    [Authorize(Roles = "Administrador, Supervisor, AsesorComercial")]
    public partial class ProductoCreate
    {
        private ProductosForm? productosForm;

        [Inject] private IRepository Repository { get; set; } = null!;

        [Inject] private NavigationManager NavigationManager { get; set; } = null!;

        [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;

        private Producto Producto = new();

        private async Task CreateAsync()
        {
            Producto.NombreFoto = Producto.Foto;
            var responseHttp = await Repository.PostAsync("/api/productos/CrearProducto", Producto);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message);
                return;
            }
            Return();

            var toast = SweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.BottomEnd,
                ShowConfirmButton = true,
                Timer = 3000
            });
            await toast.FireAsync(icon: SweetAlertIcon.Success, message: "Registro creado con éxito.");
        }

        private void Return()
        {
            productosForm!.FormPostedSuccessfully = true;
            NavigationManager.NavigateTo("/productos");
        }
    }
}