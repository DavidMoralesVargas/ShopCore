using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Data;
using Tienda_Online.Frontend.Repositories;
using Tienda_Online.Shared.Entidades;

namespace Tienda_Online.Frontend.Pages.Productos
{
    [Authorize(Roles = "Administrador, Supervisor, AsesorComercial")]
    public partial class ProductoEdit
    {
        private Producto? producto { get; set; }
        private ProductosForm? ProductoForm { get; set; }
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;
        [Inject] private IRepository Repository { get; set; } = null!;
        [Inject] SweetAlertService SweetAlertService { get; set; } = null!;
        [Parameter] public int Id { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            var responseHttp = await Repository.GetAsync<Producto>($"/api/productos/{Id}");
            if (responseHttp.Error)
            {
                if(responseHttp.HttpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/");
                }
                else
                {
                    var messageError = await responseHttp.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync("Error", messageError, SweetAlertIcon.Error);
                }
            }
            else
            {
                producto = responseHttp.Response;
                producto!.Foto = string.Empty;
            }
        }

        private async Task EditAsync()
        {
            if (string.IsNullOrEmpty(producto!.Foto))
            {
                producto.Foto = "";
                producto.NombreFoto = "";
            }
            var responseHttp = await Repository.PutAsync("/api/productos/ActualizarProducto", producto);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            Return();

            var toast = SweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.BottomLeft,
                ShowConfirmButton = true,
                Timer = 3000
            });
            await toast.FireAsync(icon: SweetAlertIcon.Success, message:"Cambios guardados con éxito.");
            
        }

        private void Return()
        {
            ProductoForm!.FormPostedSuccessfully = true;
            NavigationManager.NavigateTo("/productos");
        }
    }
}