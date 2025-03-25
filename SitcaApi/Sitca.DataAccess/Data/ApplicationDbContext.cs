using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sitca.DataAccess.Configurations;
using Sitca.Models;

namespace Sitca.DataAccess.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<
            ApplicationUser,
            ApplicationRole,
            string,
            IdentityUserClaim<string>,
            ApplicationUserRole,
            IdentityUserLogin<string>,
            IdentityRoleClaim<string>,
            IdentityUserToken<string>
        >
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // A
        public DbSet<ActivityLog> ActivityLog { get; set; }
        public DbSet<AppMenu> AppMenu { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<ApplicationRole> ApplicationRole { get; set; }
        public DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }
        public DbSet<Archivo> Archivo { get; set; }

        // C
        public DbSet<Capacitaciones> Capacitaciones { get; set; }
        public DbSet<CompAuditoras> CompAuditoras { get; set; }
        public DbSet<Cumplimiento> Cumplimiento { get; set; }
        public DbSet<Cuestionario> Cuestionario { get; set; }
        public DbSet<CuestionarioItem> CuestionarioItem { get; set; }
        public DbSet<CuestionarioItemHistory> CuestionarioItemHistories { get; set; }
        public DbSet<CuestionarioItemObservaciones> CuestionarioItemObservaciones { get; set; }
        public DbSet<CustomsToNotificate> CustomsToNotificate { get; set; }

        // D
        public DbSet<Distintivo> Distintivo { get; set; }

        // E
        public DbSet<Empresa> Empresa { get; set; }

        // H
        public DbSet<Homologacion> Homologacion { get; set; }

        // I
        public DbSet<ItemTemplate> ItemTemplate { get; set; }

        // M
        public DbSet<Modulo> Modulo { get; set; }

        // N
        public DbSet<Notificacion> Notificacion { get; set; }
        public DbSet<NotificacionesEnviadas> NotificacionesEnviadas { get; set; }
        public DbSet<NotificationCustomUsers> NotificationCustomUsers { get; set; }
        public DbSet<NotificationGroups> NotificationGroups { get; set; }

        // P
        public DbSet<Pais> Pais { get; set; }
        public DbSet<Pregunta> Pregunta { get; set; }
        public DbSet<ProcesoArchivos> ProcesoArchivos { get; set; }
        public DbSet<ProcesoCertificacion> ProcesoCertificacion { get; set; }

        // R
        public DbSet<ResultadoCertificacion> ResultadoCertificacion { get; set; }

        // S
        public DbSet<SeccionModulo> SeccionModulo { get; set; }
        public DbSet<SubtituloSeccion> SubtituloSeccion { get; set; }

        // T
        public DbSet<Tipologia> Tipologia { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Aplicar todas las configuraciones
            builder.ApplyConfiguration(new ApplicationUserConfiguration());
            builder.ApplyConfiguration(new ArchivoConfiguration());
            builder.ApplyConfiguration(new CapacitacionesConfiguration());
            builder.ApplyConfiguration(new CompAuditorasConfiguration());
            builder.ApplyConfiguration(new ProcesoArchivosConfiguration());
            builder.ApplyConfiguration(new ProcesoCertificacionConfiguration());

            // Configurar tablas de Identity
            builder.ConfigureIdentityTables();

            #region TipologiasEmpresa
            builder.Entity<TipologiasEmpresa>().HasKey(te => new { te.IdTipologia, te.IdEmpresa });

            builder
                .Entity<TipologiasEmpresa>()
                .HasOne(te => te.Tipologia)
                .WithMany(te => te.Empresas)
                .HasForeignKey(te => te.IdTipologia);

            builder
                .Entity<TipologiasEmpresa>()
                .HasOne(te => te.Empresa)
                .WithMany(te => te.Tipologias)
                .HasForeignKey(te => te.IdEmpresa);
            #endregion
        }
    }
}
