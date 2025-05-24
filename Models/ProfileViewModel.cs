using System;
using System.ComponentModel.DataAnnotations;

namespace VotingSystem.Models
{
    public class ProfileViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Nombre { get; set; } = null!;
        
        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100, ErrorMessage = "El apellido no puede tener más de 100 caracteres")]
        public string Apellido { get; set; } = null!;
        
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato de correo electrónico no es válido")]
        public string Email { get; set; } = null!;
        
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string? CurrentPassword { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 20 caracteres")]
        public string? NewPassword { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string? ConfirmNewPassword { get; set; }
        
        public DateTime FechaRegistro { get; set; }
        
        public DateTime? UltimoAcceso { get; set; }
        
        // Propiedades de solo lectura
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}