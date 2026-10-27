using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tienda_Online.Shared.Entidades
{
    public class PromocionProducto
    {
        public int Id { get; set; }
        [Display(Name = "Precio Oferta Producto")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public double PrecioOferta { get; set; } = 0!;

        public int ProductoId { get; set; }
        [JsonIgnore]
        public Producto? Producto { get; set; }

        [Display(Name = "Precio Antiguo Producto")]
        public double? PrecioAntiguo {  get; set; } 

    }
}
