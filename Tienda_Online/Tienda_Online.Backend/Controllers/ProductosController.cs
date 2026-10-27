using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tienda_Online.Backend.Clases;
using Tienda_Online.Shared.DTOs;
using Tienda_Online.Shared.Entidades;

namespace Tienda_Online.Backend.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("/api/productos")]
    public class ProductosController:ControllerBase
    {
        private readonly clsProducto _clsProducto;

        public ProductosController(clsProducto clsProducto)
        {
            _clsProducto = clsProducto;
        }

        [HttpPost("CrearProducto")]
        public async Task<IActionResult> CrearProducto(Producto producto)
        {
           var _producto = await _clsProducto.CrearProducto(producto);
            if (_producto.Exitoso)
            {
                return Ok(_producto.Respuesta);
            }
            return BadRequest();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerListaProducto([FromQuery] PaginacionDTO paginacion)
        {
            var _productos = await _clsProducto.ObtenerListaProducto(paginacion);
            if (_productos.Exitoso)
            {
                return Ok(_productos.Respuesta);
            }
            return BadRequest();
        }

        [HttpGet("ObtenerTotalPaginas")]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerTotalPaginas([FromQuery] PaginacionDTO paginacion)
        {
            var _paginas = await _clsProducto.ObtenerTotalPaginas(paginacion);
            if (_paginas.Exitoso)
            {
                return Ok(_paginas.Respuesta);
            }
            return BadRequest();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarProducto(int id)
        {
            var _producto = await _clsProducto.BuscarProducto(id);
            if (_producto.Exitoso)
            {
                return Ok(_producto.Respuesta);
            }
            return NotFound();  
        }

        [HttpPut("ActualizarProducto")]
        public async Task<IActionResult> ActualizarProducto(Producto producto)
        {
            var _producto = await _clsProducto.ActualizarProducto(producto);
            if (_producto.Exitoso)
            {
                return Ok(producto);
            }
            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var _producto = await _clsProducto.EliminarProducto(id);
            if (_producto.Exitoso)
            {
                return NoContent();
            }
            return BadRequest();
        }
    }
}
