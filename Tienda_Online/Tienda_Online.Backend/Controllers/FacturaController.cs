using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tienda_Online.Backend.Clases;
using Tienda_Online.Shared.DTOs;
using Tienda_Online.Shared.Entidades;

namespace Tienda_Online.Backend.Controllers
{
    [ApiController]
    [Route("/api/facturas")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FacturaController : ControllerBase
    {

        private readonly clsFactura _factura;
        public FacturaController(clsFactura factura)
        {
            _factura = factura;
        }

        [HttpPost("VerResumenFactura")]
        public IActionResult VerResumenFactura(List<CarritoConProductoDTO> carritos)
        {
            var factura = _factura.VerResumenFactura(carritos);
            if (factura.Exitoso)
            {
                return Ok(factura);
            }
            return BadRequest(factura.Mensaje);
        }

        [HttpPost("GenerarFactura")]
        public async Task<IActionResult> GenerarFactura(List<CarritoConProductoDTO> carritos)
        {
            var FacturaGenerada = await _factura.GenerarFactura(carritos);
            if (FacturaGenerada.Exitoso)
            {
                return Ok(FacturaGenerada.Respuesta);
            }
            return BadRequest("La factura no se pudo generar correctamente");
        }

        [HttpPost("MostrarFactura")]
        public async Task<IActionResult> MostrarDetalleFactura(List<CarritoConProductoDTO> carritos)
        {
            var factura = await _factura.MostrarDetalleFactura(carritos);
            if (factura.Exitoso)
            {
                return Ok(factura);
            }
            return BadRequest(factura.Mensaje);
        }

    }
}
