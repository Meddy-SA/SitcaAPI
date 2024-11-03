using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
  public class Modulo
  {
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(100)]
    public string EnglishName { get; set; } = null!;

    public bool Transversal { get; set; }

    public int Orden { get; set; }

    [StringLength(10)]
    public string Nomenclatura { get; set; } = null!;



    public int? TipologiaId { get; set; }
    public Tipologia Tipologia { get; set; } = null!;

    public ICollection<SeccionModulo> Secciones { get; set; } = [];

    public ICollection<Pregunta> Preguntas { get; set; } = [];

  }
}
