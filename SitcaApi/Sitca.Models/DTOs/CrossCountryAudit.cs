using System.ComponentModel.DataAnnotations;

namespace Sitca.Models.DTOs;

public class CreateCrossCountryAuditRequestDTO
{
    [Required]
    public int ApprovingCountryId { get; set; }

    [MaxLength(1000)]
    public string? NotesRequest { get; set; }
}

public class ApproveCrossCountryAuditRequestDTO
{
    [Required]
    public string AssignedAuditorId { get; set; } = null!;

    [Required]
    public DateTime DeadlineDate { get; set; }

    [MaxLength(1000)]
    public string? NotesApproval { get; set; }
}

public class RejectCrossCountryAuditRequestDTO
{
    [MaxLength(1000)]
    public string? NotesApproval { get; set; }
}

public class CrossCountryAuditRequestDTO
{
    public int Id { get; set; }
    public int RequestingCountryId { get; set; }
    public string RequestingCountryName { get; set; } = null!;
    public int ApprovingCountryId { get; set; }
    public string ApprovingCountryName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? AssignedAuditorId { get; set; }
    public string? AssignedAuditorName { get; set; }
    public DateTime? DeadlineDate { get; set; }
    public string? NotesRequest { get; set; }
    public string? NotesApproval { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;
    public string? CreatedByName { get; set; }
}
