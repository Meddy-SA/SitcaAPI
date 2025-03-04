using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sitca.Models;

namespace Sitca.DataAccess.Configurations;

public class ProcesoArchivosConfiguration : IEntityTypeConfiguration<ProcesoArchivos>
{
    public void Configure(EntityTypeBuilder<ProcesoArchivos> builder)
    {
        // Configuración de la tabla
        builder.ToTable("ProcesoArchivos");
        builder.HasKey(e => e.Id);

        // Propiedades básicas
        builder.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Ruta).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Tipo).HasMaxLength(10).IsRequired();

        // Propiedad Enum
        builder
            .Property(e => e.FileTypesCompany)
            .HasDefaultValue(Sitca.Models.Enums.FileCompany.Informativo);

        // Ignorar propiedad de transporte
        builder.Ignore(e => e.Base64Str);

        // Configuración de AuditableEntity
        AuditableEntityConfiguration.ConfigureAuditableEntity(builder);

        // Relación con ProcesoCertificacion
        builder
            .HasOne(p => p.ProcesoCertificacion)
            .WithMany(p => p.ProcesosArchivos)
            .HasForeignKey(p => p.ProcesoCertificacionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices para mejorar rendimiento
        builder.HasIndex(e => e.ProcesoCertificacionId);
        builder.HasIndex(e => e.CreatedBy);
    }
}
