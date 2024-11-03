using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
  public class AppMenu
  {
    [Key]
    public int Id { get; set; }
    public int? Parent_MenuID { get; set; }
    public int? Order { get; set; }
    [StringLength(30)]
    public string MenuName { get; set; } = null!;
    [StringLength(30)]
    public string MenuNameEn { get; set; } = null!;
    [StringLength(300)]
    public string Roles { get; set; } = null!;
    [StringLength(100)]
    public string Icon { get; set; } = null!;
    public bool IconIsImage { get; set; }
    [StringLength(120)]
    public string MenuURL { get; set; } = null!;

  }
}
