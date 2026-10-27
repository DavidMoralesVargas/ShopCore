
namespace Tienda_Online.Shared.Entidades
{
    public class Factura
    {
        public int Id { get; set; } 
        public int CodigoCliente { get; set; }
        public DateTime FechaFactura { get; set; }
        public double Subtotal { get; set; } = 0!;
        public double IVA { get; set; } = 0.19;
        public double Total { get; set; } = 0!;


    }
}
