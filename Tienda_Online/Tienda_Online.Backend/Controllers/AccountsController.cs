using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tienda_Online.Backend.Clases;
using Tienda_Online.Backend.Helpers;
using Tienda_Online.Shared.DTOs;
using Tienda_Online.Shared.Entidades;
using Tienda_Online.Shared.Respuesta;

namespace Tienda_Online.Backend.Controllers
{
    [ApiController]
    [Route("/api/cuentas")]
    public class AccountsController:ControllerBase
    {
        private readonly clsUsuario _usuarios;
        private readonly IConfiguration _configuration;
        private readonly IMailHelper _mailHelper;

        public AccountsController(clsUsuario usuarios, IConfiguration configuration, IMailHelper mailHelper)
        {
            _usuarios = usuarios;
            _configuration = configuration;
            _mailHelper = mailHelper;
        }

        [HttpGet("all")]
        public async Task<IActionResult> ObtenerUsuarios([FromQuery] PaginacionDTO paginacion)
        {
            var usuarios = await _usuarios.GetAsync(paginacion);
            if (usuarios.Exitoso)
            {
                return Ok(usuarios.Respuesta);
            }
            return BadRequest();
        }

        [HttpGet("totalPages")]
        public async Task<IActionResult> ObtenerTotalPaginas([FromQuery] PaginacionDTO paginacion)
        {
            var usuarios = await _usuarios.GetTotalPagesAsync(paginacion);
            if (usuarios.Exitoso)
            {
                return Ok(usuarios.Respuesta);
            }
            return BadRequest();
        }


        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody]UserDTO model)
        {
            Usuario user = model;
            var result = await _usuarios.AddUserAsync(user, model.Password);
            if(result.Succeeded)
            {
                await _usuarios.AddUserToRoleAsync(user, user.userType.ToString());
                var response = await SendConfirmationEmailAsync(user);
                if (response.Exitoso)
                {
                    return NoContent();
                }
                return BadRequest(response.Mensaje);
            }
            return BadRequest(result.Errors.FirstOrDefault());
        }

        [HttpPost("RecoverPassword")]
        public async Task<IActionResult> RecoverPassword([FromBody] EmailDTO model)
        {
            var user = await _usuarios.GetUserAsync(model.Email);
            if (user == null)
            {
                return NotFound();
            }
            var myToken = await _usuarios.GeneratePasswordResetTokenAsync(user);
            var tokenLink = Url.Action("ResetPassword", "accounts", new
            {
                userid = user.Id,
                token = myToken
            }, HttpContext.Request.Scheme, _configuration["Url Frontend"]);
            var response = _mailHelper.SendMail(user.Nombre, user.Email!,
            $"Tienda_Online - Recuperación de contraseña",
            $"<h1>Tienda_Onlie - Recuperación de contraseña</h1>" +
            $"<p>Para recuperar su contraseña, por favor hacer clic 'Recuperar Contraseña':</p>" +
            $"<b><a href ={tokenLink}>Recuperar Contraseña</a></b>");
            if (response.Exitoso)
            {
                return NoContent();
            }
            return BadRequest(response.Mensaje);
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDTO model)
        {
            var user = await _usuarios.GetUserAsync(model.Email);
            if(user == null)
            {
                return NotFound();
            }
            var result = await _usuarios.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest(result.Errors.FirstOrDefault());
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody]LoginDTO model)
        {
            var result = await _usuarios.LoginAsync(model);
            if (result.Succeeded)
            {
                var user = await _usuarios.GetUserAsync(model.Email);
                return Ok(BuildToken(user));
            }
            if (result.IsLockedOut)
            {
                return BadRequest("Ha superado el maximo número de intentos. su cuenta está bloqueada, intente de nuevo en 5 minutos");
            }
            if (result.IsNotAllowed)
            {
                return BadRequest("El usuario no ha sido habilitado, debes de seguir las instrucciones del correo enviado para poder habilitar el usuario");
            }
            return BadRequest("Email o contraseña incorrectos");
        }

        private TokenDTO BuildToken(Usuario user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Email!),
                new(ClaimTypes.Role, user.userType.ToString()),
                new("Nombre", user.Nombre),
                new("Apellido", user.Apellido),
                new("Direccion", user.Direccion),
                new("FechaNacimiento", user.FechaNacimiento.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwtKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddDays(30);
            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials);
            return new TokenDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PutAsync(Usuario user)
        {
            try
            {
                var currentUser = await _usuarios.GetUserAsync(User.Identity!.Name!);
                if(currentUser == null)
                {
                    return NotFound();
                }

                currentUser.Nombre = user.Nombre;
                currentUser.Apellido = user.Apellido;
                currentUser.Direccion = user.Direccion;
                currentUser.FechaNacimiento = user.FechaNacimiento;
                currentUser.PhoneNumber = user.PhoneNumber;

                var result = await _usuarios.UpdateUserAsync(currentUser);
                if(result.Succeeded)
                {
                    return NoContent();
                }
                return BadRequest(result.Errors.FirstOrDefault());
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAsync()
        {
            return Ok(await _usuarios.GetUserAsync(User.Identity!.Name!));
        }

        [HttpPost("changePassword")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _usuarios.GetUserAsync(User.Identity!.Name!);
            if(user == null)
            {
                return NotFound();
            }

            var result = await _usuarios.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.FirstOrDefault()!.Description);
            }
            return NoContent();
        }

        private async Task<AccionRespuesta<string>> SendConfirmationEmailAsync(Usuario user)
        {
            var myToken = await _usuarios.GenerateEmailConfirmationTokenAsync(user);
            var tokenLink = Url.Action("ConfirmEmail", "accounts", new
            {
                userid = user.Id,
                token = myToken
            }, HttpContext.Request.Scheme, _configuration["Url Frontend"]);
            return _mailHelper.SendMail(user.Nombre, user.Email!,
            $"Tienda_Online - Confirmación de cuenta",
            $"<h1>Tienda_Online - Confirmación de cuenta</h1>" +
            $"<p>Para habilitar el usuario, por favor hacer clic 'Confirmar Email':</p>" +
            $"<b><a href ={tokenLink}>Confirmar Email</a></b>");
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmailAsync(string userId, string token)
        {
            token = token.Replace(" ", "+");
            var user = await _usuarios.GetUserAsync(new Guid(userId));
            if(user == null)
            {
                return NotFound();
            }
            var result = await _usuarios.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.FirstOrDefault());
            }
            return NoContent();
        }

        [HttpPost("ResendToken")]
        public async Task<IActionResult> ResedTokenAsync([FromBody] EmailDTO model)
        {
            var user = await _usuarios.GetUserAsync(model.Email);
            if(user == null)
            {
                return NotFound();
            }
            var response = await SendConfirmationEmailAsync(user);
            if(response.Exitoso)
            {
                return NoContent();
            }
            return BadRequest(response.Mensaje);
        }
    }
}
