using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sitca.Models;

namespace Sitca.DataAccess.Configurations;

public class ProcesoCertificacionConfiguration : IEntityTypeConfiguration<ProcesoCertificacion>
{
  public void Configure(EntityTypeBuilder<ProcesoCertificacion> builder)
  {
    builder.HasKey(e => e.Id);

    builder.Property(e => e.NumeroExpediente).HasMaxLength(40).IsRequired();
    builder.Property(e => e.Status).HasMaxLength(30).IsRequired();

    builder.Property(e => e.FechaFinalizacion).IsRequired(false);
    builder.Property(e => e.FechaSolicitudAuditoria).IsRequired(false);
    builder.Property(e => e.FechaFijadaAuditoria).IsRequired(false);
    builder.Property(e => e.FechaVencimiento).IsRequired(false);

    builder.HasOne(p => p.AsesorProceso)
           .WithMany()
           .HasForeignKey(p => p.AsesorId)
           .IsRequired(false)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(p => p.AuditorProceso)
           .WithMany()
           .HasForeignKey(p => p.AuditorId)
           .IsRequired(false)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(p => p.UserGenerador)
           .WithMany()
           .HasForeignKey(p => p.UserGeneraId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(p => p.Tipologia)
           .WithMany()
           .HasForeignKey(p => p.TipologiaId)
           .IsRequired(false)
           .OnDelete(DeleteBehavior.SetNull);
  }
}
