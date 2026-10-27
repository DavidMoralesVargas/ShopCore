using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Tienda_Online.Frontend.Pages.Productos;
using Tienda_Online.Frontend.Repositories;
using Tienda_Online.Shared.Entidades;

namespace Tienda_Online.Frontend.Pages.OfertasProducto
{
    public partial class OfertaCreate
    {
        [Inject] IRepository repository { get; set; } = null!;
        [Inject] NavigationManager navigationManager { get; set; } = null!;
        [Inject] SweetAlertService sweetAlertService { get; set; } = null!;


        [Parameter] public int Id { get; set; }

        private Producto? producto { get; set; }
        private PromocionProducto promocion = new();
        private OfertaForm? ofertasForm { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            var responseHttp = await repository.GetAsync<Producto>($"/api/productos/{Id}");
            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    navigationManager.NavigateTo("/");
                }
                else
                {
                    var messageError = await responseHttp.GetErrorMessageAsync();
                    await sweetAlertService.FireAsync("Error", messageError, SweetAlertIcon.Error);
                }
            }
            else
            {
                producto = responseHttp.Response;
            }
        }

        private void Return()
        {
            ofertasForm!.FormPostedSuccessfully = true;
            navigationManager.NavigateTo("/productos");
        }

        private void ReturnCancel()
        {
            ofertasForm!.FormPostedSuccessfully = true;
            navigationManager.NavigateTo($"/productos/editar/{Id}");
        }

        public async Task CreateAsync()
        {
            promocion.ProductoId = Id;
            var responseHttp = await repository.PostAsync("/api/promociones/CrearPromocion", promocion);
            if (responseHttp.Error)
            {
                var messageError = await responseHttp.GetErrorMessageAsync();
                await sweetAlertService.FireAsync("Error", messageError, SweetAlertIcon.Error);
                return;
            }
            Return();

            var toast = sweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.BottomLeft,
                ShowConfirmButton = true,
                Timer = 3000
            });
            await toast.FireAsync(icon: SweetAlertIcon.Success, message: "Cambios guardados con éxito.");
        }
    }
}