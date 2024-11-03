using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
  public class CuestionarioItemHistory
  {
    [Key]
    public int Id { get; set; }
    public DateTime Date { get; set; }
    [StringLength(30)]
    public string Item { get; set; } = null!;

    [StringLength(20)]
    public string Type { get; set; } = null!;
    public int Result { get; set; }

    public int CuestionarioItemId { get; set; }
    public CuestionarioItem CuestionarioItem { get; set; } = null!;

  }
}
