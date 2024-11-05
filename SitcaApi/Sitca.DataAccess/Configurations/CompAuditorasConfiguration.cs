using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sitca.Models;

namespace Sitca.DataAccess.Configurations;

public class CompAuditorasConfiguration : IEntityTypeConfiguration<CompAuditoras>
{
  public void Configure(EntityTypeBuilder<CompAuditoras> builder)
  {
    builder.HasKey(e => e.Id);

    builder.Property(e => e.Name).HasMaxLength(120).IsRequired();
    builder.Property(e => e.Direccion).HasMaxLength(200).IsRequired();
    builder.Property(e => e.Representante).HasMaxLength(200).IsRequired();
    builder.Property(e => e.NumeroCertificado).HasMaxLength(30).IsRequired();
    builder.Property(e => e.Tipo).HasMaxLength(100).IsRequired();
    builder.Property(e => e.Email).HasMaxLength(120).IsRequired();
    builder.Property(e => e.Telefono).HasMaxLength(20).IsRequired();

    builder.Property(e => e.FechaInicioConcesion).IsRequired(false);
    builder.Property(e => e.FechaFinConcesion).IsRequired(false);

    builder.HasOne(e => e.Pais)
          .WithMany()
          .HasForeignKey(e => e.PaisId)
          .OnDelete(DeleteBehavior.Restrict);
  }
}
