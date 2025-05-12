using System;
using System.Collections.Generic;

namespace VotingSystem.Models;

public partial class Notificacion
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string Tipo { get; set; } = null!;

    public string Titulo { get; set; } = null!;

    public string Contenido { get; set; } = null!;

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public DateTime? FechaLectura { get; set; }

    public string? Estado { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
