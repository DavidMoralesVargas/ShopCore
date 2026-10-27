using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using Tienda_Online.Backend.Clases;
using Tienda_Online.Shared.DTOs;
using Tienda_Online.Shared.Entidades;

namespace Tienda_Online.Backend.Controllers
{
    [ApiController]
    [Route("/api/promociones")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PromocionProductoController : ControllerBase
    {
        private readonly clsPromocionProducto _promocion;
        public PromocionProductoController(clsPromocionProducto promocion)
        {
            _promocion = promocion;
        }

        [HttpGet("ObtenerListaPromocion")]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerListaPromocionProducto([FromQuery] PaginacionDTO paginacion)
        {
            var promociones = await _promocion.ObtenerListaPromocionProducto(paginacion);
            if (promociones.Exitoso)
            {
                return Ok(promociones.Respuesta);
            }
            return BadRequest();
        }

        [HttpGet("ObtenerTotalPaginas")]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerTotalPaginas([FromQuery] PaginacionDTO paginacion)
        {
            var promociones = await _promocion.ObtenerTotalPaginas(paginacion);
            if (promociones.Exitoso)
            {
                return Ok(promociones.Respuesta);
            }
            return BadRequest();
        }

        [HttpPost("CrearPromocion")]
        public async Task<IActionResult> CrearPromocion(PromocionProducto promocion)
        {
            var promociones = await _promocion.CrearPromocion(promocion);
            if(promociones.Exitoso)
            {
                return Ok(promociones.Respuesta);
            }
            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarOferta(int id)
        {
            var promocion = await _promocion.EliminarOferta(id);
            if (promocion.Exitoso)
            {
                return NoContent();
            }
            return BadRequest(promocion.Mensaje);
        }
    }
}
