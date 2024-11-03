using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
  public class ItemTemplate
  {
    [Key]
    public int Id { get; set; }

    [StringLength(30)]
    public string BarCode { get; set; } = null!;

    [StringLength(250)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string Description { get; set; } = null!;
  }
}
