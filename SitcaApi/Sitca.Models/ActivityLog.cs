using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
  public class ActivityLog
  {
    [Key]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string User { get; set; } = null!;

    [StringLength(300)]
    public string Observaciones { get; set; } = null!;
  }
}
