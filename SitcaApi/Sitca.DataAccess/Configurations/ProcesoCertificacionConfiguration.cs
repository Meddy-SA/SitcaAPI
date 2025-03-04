using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sitca.Models;

namespace Sitca.DataAccess.Configurations;

public class ProcesoCertificacionConfiguration : IEntityTypeConfiguration<ProcesoCertificacion>
{
    public void Configure(EntityTypeBuilder<ProcesoCertificacion> builder)
    {
        builder.HasKey(e => e.Id);

        // Propiedades básicas
        builder.Property(e => e.NumeroExpediente).HasMaxLength(40).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Recertificacion).HasDefaultValue(false);
        builder.Property(e => e.Cantidad).HasDefaultValue(0);

        // Propiedades de fecha
        builder.Property(e => e.FechaInicio).IsRequired();
        builder.Property(e => e.FechaFinalizacion).IsRequired(false);
        builder.Property(e => e.FechaSolicitudAuditoria).IsRequired(false);
        builder.Property(e => e.FechaFijadaAuditoria).IsRequired(false);
        builder.Property(e => e.FechaVencimiento).IsRequired(false);

        // Configuración de AuditableEntity
        AuditableEntityConfiguration.ConfigureAuditableEntity(builder);

        builder
            .HasOne(p => p.AsesorProceso)
            .WithMany()
            .HasForeignKey(p => p.AsesorId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(p => p.AuditorProceso)
            .WithMany()
            .HasForeignKey(p => p.AuditorId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(p => p.UserGenerador)
            .WithMany()
            .HasForeignKey(p => p.UserGeneraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(p => p.Tipologia)
            .WithMany()
            .HasForeignKey(p => p.TipologiaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(p => p.Empresa)
            .WithMany(e => e.Certificaciones)
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices para mejorar rendimiento
        builder.HasIndex(e => e.EmpresaId);
        builder.HasIndex(e => e.AsesorId);
        builder.HasIndex(e => e.AuditorId);
        builder.HasIndex(e => e.TipologiaId);
        builder.HasIndex(e => e.CreatedBy);
    }
}
