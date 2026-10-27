using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using Tienda_Online.Backend.Data;
using Tienda_Online.Shared.DTOs;
using Tienda_Online.Shared.Entidades;
using Tienda_Online.Shared.Respuesta;

namespace Tienda_Online.Backend.Clases
{
    public class clsFactura
    {
        private readonly DataContext _context;
        private readonly clsCarritoCompra _carrito;

        public clsFactura(DataContext context, clsCarritoCompra carrito)
        {
            _context = context;
            _carrito = carrito;
        }

        public AccionRespuesta<string> VerResumenFactura(List<CarritoConProductoDTO> carritos)
        {
            try
            {
                string resumen = "<table style='width: 100%; border-collapse: collapse; text-align: left;'>";
                resumen += "<tr><th style='padding: 8px;'>Producto</th><th style='padding: 8px; text-align: center;'>Cantidad</th><th style='padding: 8px; text-align: right;'>Precio Total</th></tr>";
                double Total = 0;
                double IVA = 1000;
                foreach(var factura in carritos)
                {
                    resumen += $"<tr><td style='padding: 8px;'>{factura.NombreProducto}</td>" +
                                  $"<td style='padding: 8px; text-align: center;'>{factura.CantidadProductos}</td>" +
                                  $"<td style='padding: 8px; text-align: right;'>{factura.PrecioTotal:C}</td></tr>";
                    Total += factura.PrecioTotal;
                }

                resumen += $"Subtotal: {Total}" + "<br/>";
                resumen += $"IVA: {IVA}" + "<br/>";
                resumen += $"Total: {Total + IVA}";
                return new AccionRespuesta<string>
                {
                    Exitoso = true,
                    Respuesta = resumen
                };

            }
            catch(Exception ex)
            {
                return new AccionRespuesta<string>()
                {
                    Mensaje = ex.Message,
                    Exitoso = false
                };
            }
        }

        public async Task<AccionRespuesta<Factura>> GenerarFactura(List<CarritoConProductoDTO> carritos)
        {
            try
            {
                Factura factura = new Factura();
                factura.CodigoCliente = 1;
                double iva = 1000, subtotal = 0, total = 0;
                foreach(var carrito in carritos)
                {
                    subtotal += carrito.PrecioTotal;
                }
                factura.Subtotal = subtotal;
                factura.IVA = iva;
                factura.Total = total + subtotal + iva;
                factura.FechaFactura = DateTime.Now;

                _context.Add(factura);
                await _context.SaveChangesAsync();
                return new AccionRespuesta<Factura>
                {
                    Respuesta = factura,
                    Exitoso = true
                };
            }
            catch (Exception ex)
            {
                return new AccionRespuesta<Factura>
                {
                    Mensaje = ex.Message,
                    Exitoso = false
                };
            }
        }

        public async Task<AccionRespuesta<string>> MostrarDetalleFactura(List<CarritoConProductoDTO> carrito)
        {
            string detalleFactura = "<h1 class='mb-3'>Factura de compra:</h1>" +
                           "<table style='width: 100%; border-collapse: collapse;' class='mb-5 table table-striped'>" +
                           "<tr><th>Producto</th><th>Cantidad</th><th>Precio total</th></tr>";
            try
            {
                var factura = await _context.Facturas.FirstOrDefaultAsync(c => c.Id == carrito[0].IdFactura);
                if (factura == null)
                {
                    return new AccionRespuesta<string>
                    {
                        Mensaje = "Factura no encontrada dentro de la base de datos",
                        Exitoso = false
                    };
                }

                foreach(var carritos in carrito)
                {
                    detalleFactura += $"<tr>" +
                               $"<td>{carritos.NombreProducto}</td>" +
                               $"<td>{carritos.CantidadProductos}</td>" +
                               $"<td>${carritos.PrecioTotal}</td></tr><br/>" ;
                }
                detalleFactura += "<br/>";
                detalleFactura += $"<p>Subtotal: {factura.Subtotal}</p>"+
                                  $"<p>IVA: {factura.IVA}</p>"+
                                  $"<p>Total: {factura.Total}</p>"+
                                  $"<p>Fecha de factura: {factura.FechaFactura.ToString("dd/MM/yyyy")}</p>";
                return new AccionRespuesta<string>
                {
                    Exitoso = true,
                    Respuesta = detalleFactura
                };
                
            }
            catch(Exception e)
            {
                return new AccionRespuesta<string>
                {
                    Mensaje = e.Message,
                    Exitoso = false
                };
            }

        }
    }
}
