using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sitca.Models.Enums;

namespace Sitca.Models;

public class CrossCountryAuditRequest : AuditableEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RequestingCountryId { get; set; }

    [Required]
    public int ApprovingCountryId { get; set; }

    [Required]
    public CrossCountryAuditRequestStatus Status { get; set; } =
        CrossCountryAuditRequestStatus.Pending;

    public string? AssignedAuditorId { get; set; }

    [ForeignKey(nameof(AssignedAuditorId))]
    public ApplicationUser? AssignedAuditor { get; set; }

    public DateTime? DeadlineDate { get; set; }

    [MaxLength(1000)]
    public string? NotesRequest { get; set; }

    [MaxLength(1000)]
    public string? NotesApproval { get; set; }

    [ForeignKey(nameof(RequestingCountryId))]
    public Pais RequestingCountry { get; set; } = null!;

    [ForeignKey(nameof(ApprovingCountryId))]
    public Pais ApprovingCountry { get; set; } = null!;
}
