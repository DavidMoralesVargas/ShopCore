using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Tienda_Online.Shared.Enums;

namespace Tienda_Online.Shared.Entidades
{
    public class Usuario : IdentityUser
    {
        [Display(Name = "Nombre")]
        [MaxLength(50, ErrorMessage ="El campo {0} debe tener máximo 50 caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Nombre { get; set; } = null!;

        [Display(Name = "Apellido")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo 50 caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Apellido { get; set; } = null!;

        [Display(Name = "Dirección")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Direccion {  get; set; } = null!;

        [Display(Name = "Fecha de Nacimiento")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime FechaNacimiento { get; set; }

        public UserType userType { get; set; }

    }
}
