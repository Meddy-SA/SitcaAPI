using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
  public class Distintivo
  {
    [Key]
    public int Id { get; set; }

    [StringLength(40)]
    public string Name { get; set; } = null!;

    [StringLength(40)]
    public string NameEnglish { get; set; } = null!;

    public int? Importancia { get; set; }

    [StringLength(60)]
    public string File { get; set; } = null!;
    public bool Activo { get; set; }
  }
}
