using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
namespace VotingSystem.Models;

public partial class DbAb85acVotacionesdbContext : DbContext
{
    public DbAb85acVotacionesdbContext()
    {
    }

    public DbAb85acVotacionesdbContext(DbContextOptions<DbAb85acVotacionesdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Asamblea> Asambleas { get; set; }

    public virtual DbSet<Notificacion> Notificacions { get; set; }

    public virtual DbSet<OpcionVotacion> OpcionVotacions { get; set; }

    public virtual DbSet<Reporte> Reportes { get; set; }

    public virtual DbSet<Restriccion> Restriccions { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Votacion> Votacions { get; set; }

    public virtual DbSet<Voto> Votos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=MiConexion");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asamblea>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Asamblea__3213E83FB30A2E56");

            entity.ToTable("Asamblea");

            entity.HasIndex(e => e.Estado, "idx_estado");

            entity.HasIndex(e => e.Fecha, "idx_fecha");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Acta)
                .HasColumnType("text")
                .HasColumnName("acta");
            entity.Property(e => e.CreadorId).HasColumnName("creador_id");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("programada")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Titulo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("titulo");

            entity.HasOne(d => d.Creador).WithMany(p => p.Asambleas)
                .HasForeignKey(d => d.CreadorId)
                .HasConstraintName("FK__Asamblea__creado__14270015");
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3213E83FCCDAF6FE");

            entity.ToTable("Notificacion");

            entity.HasIndex(e => e.Estado, "idx_estado_notificacion");

            entity.HasIndex(e => new { e.FechaCreacion, e.FechaEnvio, e.FechaLectura }, "idx_fechas_notificacion");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Contenido)
                .HasColumnType("text")
                .HasColumnName("contenido");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaEnvio)
                .HasColumnType("datetime")
                .HasColumnName("fecha_envio");
            entity.Property(e => e.FechaLectura)
                .HasColumnType("datetime")
                .HasColumnName("fecha_lectura");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo");
            entity.Property(e => e.Titulo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("titulo");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Notificacions)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificac__usuar__282DF8C2");
        });

        modelBuilder.Entity<OpcionVotacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OpcionVo__3213E83F25C6E54F");

            entity.ToTable("OpcionVotacion");

            entity.HasIndex(e => e.Orden, "idx_orden");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Orden)
                .HasDefaultValue(0)
                .HasColumnName("orden");
            entity.Property(e => e.Texto)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("texto");
            entity.Property(e => e.VotacionId).HasColumnName("votacion_id");

            entity.HasOne(d => d.Votacion).WithMany(p => p.OpcionVotacions)
                .HasForeignKey(d => d.VotacionId)
                .HasConstraintName("FK__OpcionVot__votac__1CBC4616");
        });

        modelBuilder.Entity<Reporte>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reporte__3213E83F61D7FB49");

            entity.ToTable("Reporte");

            entity.HasIndex(e => e.FechaGeneracion, "idx_fecha_reporte");

            entity.HasIndex(e => e.Tipo, "idx_tipo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FechaGeneracion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_generacion");
            entity.Property(e => e.Formato)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("formato");
            entity.Property(e => e.GeneradoPor).HasColumnName("generado_por");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo");
            entity.Property(e => e.Titulo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("titulo");
            entity.Property(e => e.UrlArchivo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("url_archivo");

            entity.HasOne(d => d.GeneradoPorNavigation).WithMany(p => p.Reportes)
                .HasForeignKey(d => d.GeneradoPor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reporte__generad__2BFE89A6");
        });

        modelBuilder.Entity<Restriccion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Restricc__3213E83F71BA252E");

            entity.ToTable("Restriccion");

            entity.HasIndex(e => new { e.FechaInicio, e.FechaFin }, "idx_fechas");

            entity.HasIndex(e => e.UsuarioId, "idx_usuario");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio)
                .HasColumnType("datetime")
                .HasColumnName("fecha_inicio");
            entity.Property(e => e.Motivo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("motivo");
            entity.Property(e => e.TipoRestriccion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo_restriccion");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.RestriccionCreadoPorNavigations)
                .HasForeignKey(d => d.CreadoPor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Restricci__cread__0F624AF8");

            entity.HasOne(d => d.Usuario).WithMany(p => p.RestriccionUsuarios)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__Restricci__usuar__0E6E26BF");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3213E83F20EA9486");

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.Email, "UQ__Usuario__AB6E616447B8B9FE").IsUnique();

            entity.HasIndex(e => e.Email, "idx_email");

            entity.HasIndex(e => e.Estado, "idx_estado_usuario");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellido");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("activo")
                .HasColumnName("estado");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Rol)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("miembro")
                .HasColumnName("rol");
            entity.Property(e => e.UltimoAcceso)
                .HasColumnType("datetime")
                .HasColumnName("ultimo_acceso");
        });

        modelBuilder.Entity<Votacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Votacion__3213E83F0663BCAC");

            entity.ToTable("Votacion");

            entity.HasIndex(e => e.Estado, "idx_estado_votacion");

            entity.HasIndex(e => new { e.FechaInicio, e.FechaFin }, "idx_fechas_votacion");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AsambleaId).HasColumnName("asamblea_id");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio)
                .HasColumnType("datetime")
                .HasColumnName("fecha_inicio");
            entity.Property(e => e.QuorumRequerido)
                .HasDefaultValue(5m)
                .HasColumnType("decimal(5, 4)")
                .HasColumnName("quorum_requerido");
            entity.Property(e => e.TipoVotacion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo_votacion");
            entity.Property(e => e.Titulo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("titulo");

            entity.HasOne(d => d.Asamblea).WithMany(p => p.Votacions)
                .HasForeignKey(d => d.AsambleaId)
                .HasConstraintName("FK__Votacion__asambl__18EBB532");
        });

        modelBuilder.Entity<Voto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Voto__3213E83FD7FC7F56");

            entity.ToTable("Voto");

            entity.HasIndex(e => e.Fecha, "idx_fecha_voto");

            entity.HasIndex(e => new { e.VotacionId, e.UsuarioId }, "uk_voto_usuario").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IpOrigen)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ip_origen");
            entity.Property(e => e.OpcionId).HasColumnName("opcion_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.ValorPonderado)
                .HasDefaultValue(10000m)
                .HasColumnType("decimal(10, 4)")
                .HasColumnName("valor_ponderado");
            entity.Property(e => e.VotacionId).HasColumnName("votacion_id");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Votos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Voto__usuario_id__236943A5");


            entity.HasOne(d => d.Votacion).WithMany(p => p.Votos)
                .HasForeignKey(d => d.VotacionId)
                .HasConstraintName("FK__Voto__votacion_i__22751F6C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
