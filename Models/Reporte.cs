using System;
using System.Collections.Generic;

namespace VotingSystem.Models;

public partial class Reporte
{
    public int Id { get; set; }

    public string Tipo { get; set; } = null!;

    public string Titulo { get; set; } = null!;

    public DateTime? FechaGeneracion { get; set; }

    public int GeneradoPor { get; set; }

    public string UrlArchivo { get; set; } = null!;

    public string Formato { get; set; } = null!;

    public virtual Usuario GeneradoPorNavigation { get; set; } = null!;
}
