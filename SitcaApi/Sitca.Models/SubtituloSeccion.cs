using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
  public class SubtituloSeccion
  {
    [Key]
    public int Id { get; set; }
    [StringLength(100)]
    public string Name { get; set; } = null!;
    [StringLength(100)]
    public string NameEnglish { get; set; } = null!;

    [StringLength(5)]
    public string Orden { get; set; } = null!;
    [StringLength(10)]
    public string Nomenclatura { get; set; } = null!;
    public int SeccionModuloId { get; set; }
    public SeccionModulo SeccionModulo { get; set; } = null!;

  }
}
