using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace Tienda_Online.Frontend.Pages.Facturas
{
    [Authorize(Roles = "Administrador, Supervisor, AsesorComercial, Cliente")]
    public partial class DetalleFactura
    {
        [Parameter]
        public string EncodedHtml { get; set; } = null!;

        private MarkupString HtmlContent => new MarkupString(Encoding.UTF8.GetString(Convert.FromBase64String(EncodedHtml)));
    }


}
