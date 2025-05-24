using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VotingSystem.Models
{
    public partial class Asamblea
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        public string Titulo { get; set; } = null!;

        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un creador")]
        public int CreadorId { get; set; }

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estado")]
        public string? Estado { get; set; }

        public string? Acta { get; set; }

        public DateTime? FechaCreacion { get; set; }

        public virtual Usuario? Creador { get; set; }

        public virtual ICollection<Votacion> Votacions { get; set; } = new List<Votacion>();
    }
}