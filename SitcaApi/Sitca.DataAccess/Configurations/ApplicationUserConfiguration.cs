using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sitca.Models;

namespace Sitca.DataAccess.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // Configuración de la tabla
        builder.ToTable("AspNetUsers");

        // Configuración de propiedades opcionales
        builder.Property(e => e.EmpresaId).IsRequired(false);
        builder.Property(e => e.PaisId).IsRequired(false);
        builder.Property(e => e.CompAuditoraId).IsRequired(false);
        builder.Property(e => e.VencimientoCarnet).IsRequired(false);
        builder.Property(e => e.AvisoVencimientoCarnet).IsRequired(false);

        // Configuración de strings con longitud
        builder.Property(e => e.FirstName).HasMaxLength(60).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(60).IsRequired();
        builder.Property(e => e.Codigo).HasMaxLength(60).IsRequired();
        builder.Property(e => e.Direccion).HasMaxLength(200).IsRequired();
        builder.Property(e => e.NumeroCarnet).HasMaxLength(20).IsRequired();
        builder.Property(e => e.FechaIngreso).HasMaxLength(20).IsRequired();
        builder.Property(e => e.HojaDeVida).HasMaxLength(60).IsRequired();
        builder.Property(e => e.DocumentoAcreditacion).HasMaxLength(60).IsRequired();
        builder.Property(e => e.Departamento).HasMaxLength(120).IsRequired();
        builder.Property(e => e.Ciudad).HasMaxLength(120).IsRequired();
        builder.Property(e => e.DocumentoIdentidad).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Profesion).HasMaxLength(120).IsRequired();
        builder.Property(e => e.Nacionalidad).HasMaxLength(60).IsRequired();
        builder.Property(e => e.Lenguage).HasMaxLength(3).IsRequired();

        // Valores por defecto
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Notificaciones).HasDefaultValue(true);

        // Relaciones
        builder
            .HasOne(e => e.CompAuditora)
            .WithMany(c => c.Usuarios)
            .HasForeignKey(e => e.CompAuditoraId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Índice para mejorar el rendimiento de las búsquedas
        builder.HasIndex(u => u.CompAuditoraId);
    }
}
