using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sitca.Models;

namespace Sitca.DataAccess.Configurations;

public class CrossCountryAuditRequestConfiguration
    : IEntityTypeConfiguration<CrossCountryAuditRequest>
{
    public void Configure(EntityTypeBuilder<CrossCountryAuditRequest> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).HasConversion<int>();

        builder
            .HasOne(e => e.RequestingCountry)
            .WithMany()
            .HasForeignKey(e => e.RequestingCountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(e => e.ApprovingCountry)
            .WithMany()
            .HasForeignKey(e => e.ApprovingCountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(e => e.AssignedAuditor)
            .WithMany()
            .HasForeignKey(e => e.AssignedAuditorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasIndex(e => new { e.RequestingCountryId, e.Status })
            .HasDatabaseName("IX_CrossCountryAuditRequest_RequestingCountry_Status");

        builder
            .HasIndex(e => new { e.ApprovingCountryId, e.Status })
            .HasDatabaseName("IX_CrossCountryAuditRequest_ApprovingCountry_Status");
    }
}
