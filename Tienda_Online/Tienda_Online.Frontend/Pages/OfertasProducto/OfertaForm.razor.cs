using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Tienda_Online.Shared.Entidades;
using Microsoft.AspNetCore.Components.Routing;

namespace Tienda_Online.Frontend.Pages.OfertasProducto
{
    public partial class OfertaForm
    {
        private EditContext _editContext = null!;

        protected override void OnInitialized()
        {
            _editContext = new(oferta);
        }

        [EditorRequired][Parameter] public PromocionProducto oferta { get; set; } = null!;

        [EditorRequired][Parameter] public EventCallback OnValidSubmit { get; set; }

        [EditorRequired][Parameter] public EventCallback ReturnAction { get; set; }

        [Inject] SweetAlertService sweetAlertService { get; set; } = null!;

        public bool FormPostedSuccessfully { get; set; } = false;

        private async Task OnBeforeInternalNavigation(LocationChangingContext context)
        {
            var formWasEdited = _editContext.IsModified();

            if (!formWasEdited || FormPostedSuccessfully)
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