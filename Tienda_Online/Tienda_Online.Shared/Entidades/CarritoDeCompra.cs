using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tienda_Online.Shared.Entidades
{
    public class CarritoDeCompra
    {
        public int Id { get; set; }
        [Display(Name = "Cantidad de Producto")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int CantidadProducto { get; set; } = 0!;
        [Display(Name = "Valor de Cantidad Producto")]
        public double ValorCantidadProducto { get; set; }

        public int ProductoId { get; set; }
        [JsonIgnore]
        public Producto? Producto { get; set; }

        public string? UsuarioId { get; set; }
        [JsonIgnore]
        public Usuario? usuario { get; set; }
    }
}
