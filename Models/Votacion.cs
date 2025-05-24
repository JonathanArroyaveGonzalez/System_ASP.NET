using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VotingSystem.Models
{
    public partial class Votacion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una asamblea")]
        public int AsambleaId { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        public string Titulo { get; set; } = null!;

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string Descripcion { get; set; } = null!;

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es requerida")]
        public DateTime FechaFin { get; set; }

        [Range(0.01, 9.99, ErrorMessage = "El quórum debe estar entre 0.01 y 9.99")]
        public decimal? QuorumRequerido { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estado")]
        public string? Estado { get; set; }

        [Required(ErrorMessage = "El tipo de votación es requerido")]
        public string TipoVotacion { get; set; } = null!;

        // Propiedades de navegación como nullable para evitar validación automática
        public virtual Asamblea? Asamblea { get; set; }

        public virtual ICollection<OpcionVotacion> OpcionVotacions { get; set; } = new List<OpcionVotacion>();

        public virtual ICollection<Voto> Votos { get; set; } = new List<Voto>();

        // Validación personalizada para fechas
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaFin <= FechaInicio)
            {
                yield return new ValidationResult(
                    "La fecha de fin debe ser posterior a la fecha de inicio",
                    new[] { nameof(FechaFin) });
            }

            if (FechaInicio < DateTime.Now)
            {
                yield return new ValidationResult(
                    "La fecha de inicio no puede ser en el pasado",
                    new[] { nameof(FechaInicio) });
            }
        }
    }

    public partial class OpcionVotacion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La votación es requerida")]
        public int VotacionId { get; set; }

        [Required(ErrorMessage = "El texto de la opción es requerido")]
        [StringLength(500, ErrorMessage = "El texto no puede exceder 500 caracteres")]
        public string Texto { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Descripcion { get; set; }

        public int? Orden { get; set; }

        // Propiedad de navegación como nullable
        public virtual Votacion? Votacion { get; set; }
    }
}