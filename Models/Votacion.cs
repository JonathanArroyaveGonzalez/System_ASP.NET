using System;
using System.Collections.Generic;

namespace VotingSystem.Models;

public partial class Votacion
{
    public int Id { get; set; }

    public int AsambleaId { get; set; }

    public string Titulo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public decimal? QuorumRequerido { get; set; }

    public string? Estado { get; set; }

    public string TipoVotacion { get; set; } = null!;

    public virtual Asamblea Asamblea { get; set; } = null!;

    public virtual ICollection<OpcionVotacion> OpcionVotacions { get; set; } = new List<OpcionVotacion>();

    public virtual ICollection<Voto> Votos { get; set; } = new List<Voto>();
}
