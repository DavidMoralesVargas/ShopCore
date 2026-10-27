namespace Tienda_Online.Shared.DTOs
{
    public class OfertaDTO
    {
        public int IdPromocion { get; set; }
        public int IdProducto { get; set; }
        public string FotoProducto { get; set; } = null!;
        public double PrecioAntiguo { get; set; } = 0!;
        public double PrecioOferta { get; set; } = 0!;
        public string DescripcionProducto { get; set; } = null!;

    }
}
