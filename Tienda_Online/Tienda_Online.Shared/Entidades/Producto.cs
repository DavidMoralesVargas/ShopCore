using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tienda_Online.Shared.Entidades
{
    public class Producto
    {
        public int Id { get; set; }
        [Display(Name = "Nombre Producto")]
        [Required(ErrorMessage ="El campo {0} es obligatorio")]
        public string Nombre { get; set; } = null!;
        [Display(Name = "Descripción Producto")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Descripcion { get; set; } =null!;

        [Display(Name = "Precio Producto")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public double Precio { get; set; } = 0!;
        public string NombreFoto { get; set; } = null!;

        [Display(Name = "Foto Producto")]
        public string Foto { get; set; } = null!;

        [JsonIgnore]
        public ICollection<PromocionProducto>? Promociones { get; set; }
        [JsonIgnore]
        public ICollection<CarritoDeCompra>? CarritosDeCompra {  get; set; }
    }
}
