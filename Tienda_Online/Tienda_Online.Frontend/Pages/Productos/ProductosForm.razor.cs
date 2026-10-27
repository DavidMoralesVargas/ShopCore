using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Tienda_Online.Frontend.Repositories;
using Tienda_Online.Shared.Entidades;

namespace Tienda_Online.Frontend.Pages.Productos
{
    public partial class ProductosForm
    {
        private EditContext _editContext = null!;

        protected override void OnInitialized()
        {
            _editContext = new(producto);
        }

        [EditorRequired][Parameter] public Producto producto { get; set; } = null!;

        [EditorRequired] [Parameter] public EventCallback OnValidSubmit { get; set; }

        [EditorRequired] [Parameter] public EventCallback ReturnAction { get; set; }

        [Inject] SweetAlertService sweetAlertService { get; set; } = null!;

        public bool FormPostedSuccessfully { get; set; } = false;


        private async Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            var file = e.File;

            using (var stream = new MemoryStream())
            {
                await file.OpenReadStream().CopyToAsync(stream);
                var base64 = Convert.ToBase64String(stream.ToArray());

                // Aquí guardamos la cadena Base64 en el atributo Cronograma
                producto.Foto = $"data:{file.ContentType};base64,{base64}";
            }
        }


        private async Task OnBeforeInternalNavigation(LocationChangingContext context)
        {
            var formWasEdited = _editContext.IsModified();

            if(!formWasEdited || FormPostedSuccessfully)
            {
                return;
            }

            var result = await sweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "Confirmación",
                Text = "¿Deseas abandonar la página y perder los cambios?",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true
            });

            var confirm = !string.IsNullOrEmpty(result.Value);
            if (confirm)
            {
                return;
            }
            context.PreventNavigation();
        }
    }
}