using Microsoft.Identity.Client;
using Tienda_Online.Backend.Clases;
using Tienda_Online.Shared.Entidades;
using Tienda_Online.Shared.Enums;

namespace Tienda_Online.Backend.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly clsUsuario _usuarios;

        public SeedDb(DataContext context, clsUsuario usuarios)
        {
            _context = context;
            _usuarios = usuarios;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            await CheckRoleAsync();
            await CheckUserAsync("Super", "Admin", "david@gmail.com", "3232942877", "Cr 23 8965", UserType.Administrador);
        }

        public async Task CheckRoleAsync()
        {
            await _usuarios.CheckRoleAsync(UserType.Administrador.ToString());
            await _usuarios.CheckRoleAsync(UserType.Supervisor.ToString());
            await _usuarios.CheckRoleAsync(UserType.AsesorComercial.ToString());
            await _usuarios.CheckRoleAsync(UserType.Cliente.ToString());
        }

        private async Task<Usuario> CheckUserAsync(string nombre, string apellido, string email, string telefono, string direccion, UserType userType)
        {
            var user = await _usuarios.GetUserAsync(email);
            if(user == null)
            {
                user = new Usuario
                {
                    Nombre = nombre,
                    Apellido = apellido,
                    Email = email,
                    PhoneNumber = telefono,
                    Direccion = direccion,
                    UserName = email,
                    userType = userType
                };
                await _usuarios.AddUserAsync(user, "123456");
                await _usuarios.AddUserToRoleAsync(user, userType.ToString());
                var token =  await _usuarios.GenerateEmailConfirmationTokenAsync(user);
                await _usuarios.ConfirmEmailAsync(user, token);
            }
            return user;
        }
    }
}
