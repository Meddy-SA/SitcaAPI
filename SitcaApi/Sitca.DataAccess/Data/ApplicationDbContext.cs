using Sitca.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sitca.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }

        public DbSet<ItemTemplate> ItemTemplate { get; set; }
        public DbSet<Tipologia> Tipologia { get; set; }
        public DbSet<Pais> Pais { get; set; }
        public DbSet<Empresa> Empresa { get; set; }

        public DbSet<Cuestionario> Cuestionario { get; set; }

        public DbSet<CuestionarioItem> CuestionarioItem { get; set; }

        public DbSet<Pregunta> Pregunta { get; set; }
        public DbSet<Modulo> Modulo { get; set; }

        public DbSet<Distintivo> Distintivo { get; set; }
        public DbSet<Cumplimiento> Cumplimiento { get; set; }

        public DbSet<ApplicationUser> ApplicationUser { get; set; }

        public DbSet<Archivo> Archivo { get; set; }
        public DbSet<AppMenu> AppMenu { get; set; }

        public DbSet<SeccionModulo> SeccionModulo { get; set; }

        public DbSet<ProcesoCertificacion> ProcesoCertificacion { get; set; }

        public DbSet<SubtituloSeccion> SubtituloSeccion { get; set; }


        public DbSet<Notificacion> Notificacion { get; set; }
        public DbSet<NotificationCustomUsers> NotificationCustomUsers { get; set; }

        public DbSet<CustomsToNotificate> CustomsToNotificate { get; set; }

        public DbSet<NotificationGroups> NotificationGroups { get; set; }

        //public DbSet<NotifiyUsers> NotifiyUsers { get; set; }

        public DbSet<CuestionarioItemHistory> CuestionarioItemHistories { get; set; }

        public DbSet<ResultadoCertificacion> ResultadoCertificacion { get; set; }

        public DbSet<Capacitaciones> Capacitaciones { get; set; }

        public DbSet<CompAuditoras> CompAuditoras { get; set; }

        public DbSet<Homologacion> Homologacion { get; set; }

        public DbSet<CuestionarioItemObservaciones> CuestionarioItemObservaciones { get; set; }

        public DbSet<ActivityLog> ActivityLog { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region TipologiasEmpresa
            builder.Entity<TipologiasEmpresa>()
                .HasKey(te => new { te.IdTipologia, te.IdEmpresa });

            builder.Entity<TipologiasEmpresa>()
                .HasOne(te => te.Tipologia)
                .WithMany(te => te.Empresas)
                .HasForeignKey(te => te.IdTipologia);

            builder.Entity<TipologiasEmpresa>()
                .HasOne(te => te.Empresa)
                .WithMany(te => te.Tipologias)
                .HasForeignKey(te => te.IdEmpresa);
            #endregion
        }

    }
}
