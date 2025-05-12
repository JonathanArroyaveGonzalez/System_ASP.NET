using System;
using System.Collections.Generic;

namespace VotingSystem.Models;

public partial class Asamblea
{
    public int Id { get; set; }

    public string Titulo { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public int CreadorId { get; set; }

    public string? Descripcion { get; set; }

    public string? Estado { get; set; }

    public string? Acta { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual Usuario Creador { get; set; } = null!;

    public virtual ICollection<Votacion> Votacions { get; set; } = new List<Votacion>();
}
