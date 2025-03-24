using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models.ViewModels;

[NotMapped]
public class ProcessStartedVm
{
    public int Id { get; set; }
    public string AdviserId { get; set; } = null!;
    public int? NewStatus { get; set; }
    public string? AdviserName { get; set; }
    public string? GeneratorId { get; set; }
    public string? GeneratorName { get; set; }
}
