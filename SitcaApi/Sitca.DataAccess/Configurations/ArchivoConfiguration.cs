using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sitca.Models;

namespace Sitca.DataAccess.Configurations;

public class ArchivoConfiguration : IEntityTypeConfiguration<Archivo>
{
  public void Configure(EntityTypeBuilder<Archivo> builder)
  {
    // Configuración de la tabla
    builder.ToTable("Archivo");
    builder.HasKey(e => e.Id);

    // Propiedades
    builder.Property(e => e.FechaCarga).IsRequired();
    builder.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
    builder.Property(e => e.Ruta).HasMaxLength(50).IsRequired();
    builder.Property(e => e.Tipo).HasMaxLength(10).IsRequired();
    builder.Property(e => e.Activo).IsRequired();

    // Ignorar propiedad Base64Str ya que es solo para transporte de datos
    builder.Ignore(e => e.Base64Str); // NotMapped

    // Relación con Usuario que carga
    builder.HasOne(a => a.UsuarioCarga)
           .WithMany()
           .HasForeignKey(a => a.UsuarioCargaId)
           .OnDelete(DeleteBehavior.Restrict);

    // Relación con Usuario
    builder.HasOne(a => a.Usuario)
           .WithMany()
           .HasForeignKey(a => a.UsuarioId)
           .IsRequired(false)
           .OnDelete(DeleteBehavior.Restrict);

    // Relación con CuestionarioItem
    builder.HasOne(a => a.CuestionarioItem)
           .WithMany(p => p.Archivos)
           .HasForeignKey(a => a.CuestionarioItemId)
           .IsRequired(false)
           .OnDelete(DeleteBehavior.SetNull);

    // Relación con Empresa
    builder.HasOne(a => a.Empresa)
           .WithMany(p => p.Archivos)
           .HasForeignKey(a => a.EmpresaId)
           .IsRequired(false)
           .OnDelete(DeleteBehavior.SetNull);

    // Configuración de índices para mejorar el rendimiento
    builder.HasIndex(e => e.CuestionarioItemId);
    builder.HasIndex(e => e.EmpresaId);
    builder.HasIndex(e => e.UsuarioCargaId);
    builder.HasIndex(e => e.UsuarioId);
  }
}
