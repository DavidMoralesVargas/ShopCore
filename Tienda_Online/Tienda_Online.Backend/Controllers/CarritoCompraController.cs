using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tienda_Online.Backend.Clases;
using Tienda_Online.Shared.Entidades;

namespace Tienda_Online.Backend.Controllers
{
    [ApiController]
    [Route("/api/carritoCompra")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CarritoCompraController:ControllerBase
    {
        private clsCarritoCompra _carritoCompra;
        private clsUsuario _usuario;

        public CarritoCompraController(clsCarritoCompra carritoCompra, clsUsuario usuario)
        {
            _carritoCompra = carritoCompra;
            _usuario = usuario;
        }


        [HttpPost("CrearCarrito")]
        public async Task<IActionResult> CrearCarritoCompra(CarritoDeCompra carrito)
        {
            var user = await _usuario.GetUserAsync(User.Identity!.Name!);
            var _carrito = await _carritoCompra.CrearCarritoCompra(carrito, user);
            if (_carrito.Exitoso)
            {
                return Ok(_carrito.Respuesta);
            }
            return BadRequest();
        }

        [HttpGet("ObtenerCarritos")]
        public async Task<IActionResult> ObtenerListaCarrito()
        {
            var user = await _usuario.GetUserAsync(User.Identity!.Name!);
            var _carrito = await _carritoCompra.ObtenerListaCarrito(user);
            if (_carrito.Exitoso)
            {
                return Ok(_carrito.Respuesta);
            }
            return BadRequest();
        }

        [HttpPut("Actualizar")]
        public async Task<IActionResult> Actualizar(CarritoDeCompra carrito)
        {
            var _carrito = await _carritoCompra.Actualizar(carrito);
            if(_carrito.Exitoso)
            {
                return Ok(_carrito.Respuesta);
            }
            return BadRequest(_carrito.Mensaje);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarCarritoCompra(int id)
        {
            var _carrito = await _carritoCompra.BuscarCarritoCompra(id);
            if (_carrito.Exitoso)
            {
                return Ok(_carrito.Respuesta);
            }
            return BadRequest(_carrito.Mensaje);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var _carrito = await _carritoCompra.EliminarProducto(id);
            if(_carrito.Exitoso)
            {
                return NoContent();
            }
            return BadRequest(_carrito.Mensaje);
        }

        [HttpGet("EliminarCarritos")]
        public async Task<IActionResult> EliminarRegistros()
        {
            var user = await _usuario.GetUserAsync(User.Identity!.Name!);
            var resultado = await _carritoCompra.EliminarRegistros(user);
            if (resultado.Exitoso)
            {
                return Ok(resultado);
            }
            return BadRequest();
        }
    }
}
