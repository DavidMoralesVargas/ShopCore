using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tienda_Online.Backend.Clases;
using Tienda_Online.Shared.DTOs;

namespace Tienda_Online.Backend.Controllers
{
    [ApiController]
    [Route("/api/informes")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class InformesController:ControllerBase
    {
        private readonly clsInforme _informe;

        public InformesController(clsInforme informe)
        {
            _informe = informe;
        }

        [HttpPost("GuardarInforme")]
        public async Task<IActionResult> GuardarInforme(List<CarritoConProductoDTO> carritos)
        {
            var informe = await _informe.GuardarInforme(carritos);
            if (informe.Exitoso)
            {
                return Ok(informe);
            }
            return BadRequest(informe.Mensaje);
        }

        [HttpGet("ObtenerTotalInformes")]
        public async Task<IActionResult> ObtenerTotalInformes([FromQuery]PaginacionDTO paginacion)
        {
            var informes = await _informe.ObtenerTotalInformes(paginacion);
            if(informes.Exitoso)
            {
                return Ok(informes.Respuesta);
            }
            return BadRequest(informes.Mensaje);
        }

        [HttpGet("ObtenerTotalPaginas")]
        public async Task<IActionResult> ObtenerTotalPaginas([FromQuery] PaginacionDTO paginacion)
        {
            var numeroPagInfomes = await _informe.ObtenerTotalPaginas(paginacion);
            if (numeroPagInfomes.Exitoso)
            {
                return Ok(numeroPagInfomes.Respuesta);
            }
            return BadRequest(numeroPagInfomes.Mensaje);
        }
    }
}
