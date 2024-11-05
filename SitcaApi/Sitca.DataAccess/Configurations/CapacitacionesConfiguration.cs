using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sitca.Models;

namespace Sitca.DataAccess.Configurations;

public class CapacitacionesConfiguration : IEntityTypeConfiguration<Capacitaciones>
{
  public void Configure(EntityTypeBuilder<Capacitaciones> builder)
  {
    builder.HasKey(e => e.Id);

    builder.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
    builder.Property(e => e.Descripcion).HasMaxLength(250).IsRequired();
    builder.Property(e => e.Ruta).HasMaxLength(500).IsRequired();
    builder.Property(e => e.Tipo).HasMaxLength(10).IsRequired();


    builder.HasOne(c => c.UsuarioCarga)
           .WithMany()
           .HasForeignKey(c => c.UsuarioCargaId)
           .OnDelete(DeleteBehavior.Restrict);
  }
}
