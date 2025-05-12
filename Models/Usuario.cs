using System;
using System.Collections.Generic;

namespace VotingSystem.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public DateTime? FechaRegistro { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public virtual ICollection<Asamblea> Asambleas { get; set; } = new List<Asamblea>();

    public virtual ICollection<Notificacion> Notificacions { get; set; } = new List<Notificacion>();

    public virtual ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();

    public virtual ICollection<Restriccion> RestriccionCreadoPorNavigations { get; set; } = new List<Restriccion>();

    public virtual ICollection<Restriccion> RestriccionUsuarios { get; set; } = new List<Restriccion>();

    public virtual ICollection<Voto> Votos { get; set; } = new List<Voto>();
}
