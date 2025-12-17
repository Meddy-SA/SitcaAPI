namespace Sitca.Models.DTOs.Dashboard
{
    public class RecentActivityDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int? ProcesoCertificacionId { get; set; }
        public string? Distintivo { get; set; }
        public DateTime? AuditDate { get; set; }
    }
}