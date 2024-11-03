using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
  public class SeccionModulo
  {
    [Key]
    public int Id { get; set; }
    [StringLength(500)]
    public string Name { get; set; } = null!;
    [StringLength(500)]
    public string NameEnglish { get; set; } = null!;

    [StringLength(5)]
    public string Orden { get; set; } = null!;
    [StringLength(10)]
    public string Nomenclatura { get; set; } = null!;
    public int ModuloId { get; set; }
    public Modulo Modulo { get; set; } = null!;


    public int? TipologiaId { get; set; }
    public Tipologia Tipologia { get; set; } = null!;

    public ICollection<SubtituloSeccion> SubtituloSeccion { get; set; } = [];

  }
}
