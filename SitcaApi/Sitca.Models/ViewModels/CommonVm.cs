using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models.ViewModels
{
  [NotMapped]
  public class CommonVm
  {
    public int id { get; set; }
    public string name { get; set; } = null!;
    public bool isSelected { get; set; }
  }


  [NotMapped]
  public class CommonUserVm
  {
    public string id { get; set; } = null!;
    public string email { get; set; } = null!;
    public string fullName { get; set; } = null!;
    public string phone { get; set; } = null!;
    public string codigo { get; set; } = null!;

  }
}
