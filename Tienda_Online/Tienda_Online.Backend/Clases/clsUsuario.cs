using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Tienda_Online.Backend.Data;
using Tienda_Online.Backend.Helpers;
using Tienda_Online.Shared.DTOs;
using Tienda_Online.Shared.Entidades;
using Tienda_Online.Shared.Respuesta;

namespace Tienda_Online.Backend.Clases
{
    public class clsUsuario
    {
        private readonly DataContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<Usuario> _signInManager;

        public clsUsuario(DataContext context, UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager, SignInManager<Usuario> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<AccionRespuesta<IEnumerable<Usuario>>> GetAsync(PaginacionDTO paginacion)
        {
            var queryable = _context.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(paginacion.Filtro))
            {
                queryable = queryable.Where(x=>x.Nombre.ToLower().Contains(paginacion.Filtro.ToLower()) || x.Apellido.ToLower().Contains(paginacion.Filtro));
            }
            return new AccionRespuesta<IEnumerable<Usuario>>
            {
                Exitoso = true,
                Respuesta = await queryable.OrderBy(n => n.Nombre).Paginate(paginacion).ToListAsync()

            };
        }

        public async Task<AccionRespuesta<int>> GetTotalPagesAsync(PaginacionDTO paginacion)
        {
            var queryable = _context.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(paginacion.Filtro))
            {
                queryable = queryable.Where(x => x.Nombre.ToLower().Contains(paginacion.Filtro.ToLower()) || x.Apellido.ToLower().Contains(paginacion.Filtro));
            }
            double count = await queryable.CountAsync();
            double totalPages = Math.Ceiling(count / paginacion.NumeroRegistros);
            return new AccionRespuesta<int>
            {
                Exitoso = true,
                Respuesta = (int)totalPages
            };
        }

        public async Task<SignInResult> LoginAsync(LoginDTO model)
        {
            return await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, true);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> AddUserAsync(Usuario user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task AddUserToRoleAsync(Usuario user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task CheckRoleAsync(string roleName)
        {
            var roleExits = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExits)
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = roleName
                });
            }
        }

        public async Task<Usuario> GetUserAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            return user!;
        }

        public async Task<bool> IsUserInRoleAsync(Usuario user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<Usuario> GetUserAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId.ToString());
            return user!;
        }

        public async Task<IdentityResult> ChangePasswordAsync(Usuario user, string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<IdentityResult> UpdateUserAsync(Usuario user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(Usuario user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(Usuario user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(Usuario user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordAsync(Usuario user, string token, string password)
        {
            return await _userManager.ResetPasswordAsync(user, token, password);
        }
    }
}
