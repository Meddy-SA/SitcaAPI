using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sitca.Models;

namespace Sitca.DataAccess.Configurations;

/// <summary>
/// Configuración base para entidades auditables
/// </summary>
public static class AuditableEntityConfiguration
{
    /// <summary>
    /// Aplica la configuración estándar para entidades auditables
    /// </summary>
    public static void ConfigureAuditableEntity<T>(EntityTypeBuilder<T> builder)
        where T : AuditableEntity
    {
        builder.Property(e => e.Enabled).HasDefaultValue(true).IsRequired();

        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired(false);

        builder.Property(e => e.CreatedAt).IsRequired(false);

        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired(false);

        builder.Property(e => e.UpdatedAt).IsRequired(false);

        // Relación con el usuario que creó la entidad
        builder
            .HasOne(e => e.UserCreate)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
