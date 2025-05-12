using System;
using System.Collections.Generic;

namespace VotingSystem.Models;

public partial class Voto
{
    public int Id { get; set; }

    public int VotacionId { get; set; }

    public int UsuarioId { get; set; }

    public int OpcionId { get; set; }

    public DateTime? Fecha { get; set; }

    public decimal? ValorPonderado { get; set; }

    public string? IpOrigen { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;

    public virtual Votacion Votacion { get; set; } = null!;
}
