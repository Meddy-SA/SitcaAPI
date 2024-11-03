using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sitca.Models;

namespace Sitca.DataAccess.Extensions;

public static class IdentityConfiguration
{
  public static void ConfigureIdentityTables(this ModelBuilder builder)
  {
    // Configuración específica para ApplicationUser
    builder.Entity<ApplicationUser>(entity =>
    {
      entity.ToTable("AspNetUsers");

      // Valores por defecto para propiedades nullable
      entity.Property(e => e.Active).HasDefaultValue(true);
      entity.Property(e => e.Notificaciones).HasDefaultValue(true);
      entity.Property(e => e.PaisId).IsRequired(false);
      entity.Property(e => e.EmpresaId).IsRequired(false);
      entity.Property(e => e.CompAuditoraId).IsRequired(false);

      // Configuración de propiedades string
      entity.Property(e => e.FirstName).HasMaxLength(60).IsRequired();
      entity.Property(e => e.LastName).HasMaxLength(60).IsRequired();
      entity.Property(e => e.Codigo).HasMaxLength(60).IsRequired();
      entity.Property(e => e.Direccion).HasMaxLength(200).IsRequired();
      entity.Property(e => e.NumeroCarnet).HasMaxLength(20).IsRequired();
      entity.Property(e => e.FechaIngreso).HasMaxLength(20).IsRequired();
      entity.Property(e => e.HojaDeVida).HasMaxLength(60).IsRequired();
      entity.Property(e => e.DocumentoAcreditacion).HasMaxLength(60).IsRequired();
      entity.Property(e => e.Departamento).HasMaxLength(120).IsRequired();
      entity.Property(e => e.Ciudad).HasMaxLength(120).IsRequired();
      entity.Property(e => e.DocumentoIdentidad).HasMaxLength(30).IsRequired();
      entity.Property(e => e.Profesion).HasMaxLength(120).IsRequired();
      entity.Property(e => e.Nacionalidad).HasMaxLength(60).IsRequired();
      entity.Property(e => e.Lenguage).HasMaxLength(3).IsRequired();

      // Índices
      entity.HasIndex(e => e.NormalizedEmail).HasDatabaseName("EmailIndex");
      entity.HasIndex(e => e.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique();
    });

    // Configuración de las tablas de Identity
    builder.Entity<IdentityRole>(entity =>
    {
      entity.ToTable("AspNetRoles");
    });

    builder.Entity<IdentityUserRole<string>>(entity =>
    {
      entity.ToTable("AspNetUserRoles");
    });

    builder.Entity<IdentityUserClaim<string>>(entity =>
    {
      entity.ToTable("AspNetUserClaims");
    });

    builder.Entity<IdentityUserLogin<string>>(entity =>
    {
      entity.ToTable("AspNetUserLogins");
    });

    builder.Entity<IdentityRoleClaim<string>>(entity =>
    {
      entity.ToTable("AspNetRoleClaims");
    });

    builder.Entity<IdentityUserToken<string>>(entity =>
    {
      entity.ToTable("AspNetUserTokens");
    });
  }
}
