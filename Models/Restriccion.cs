using System;
using System.Collections.Generic;

namespace VotingSystem.Models;

public partial class Restriccion
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string TipoRestriccion { get; set; } = null!;

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public string? Motivo { get; set; }

    public int CreadoPor { get; set; }

    public virtual Usuario CreadoPorNavigation { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
