using System.ComponentModel.DataAnnotations;

namespace Sitca.Models;

public class Pregunta
{
  [Key]
  public int Id { get; set; }

  [StringLength(2500)]
  public string Texto { get; set; } = null!;
  [StringLength(2500)]
  public string Text { get; set; } = null!;
  public bool NoAplica { get; set; }
  public bool Obligatoria { get; set; }
  public bool Status { get; set; }

  [StringLength(20)]
  public string Nomenclatura { get; set; } = null!;
  [StringLength(5)]
  public string Orden { get; set; } = null!;

  public int? TipologiaId { get; set; }
  public Tipologia Tipologia { get; set; } = null!;

  public int ModuloId { get; set; }
  public Modulo Modulo { get; set; } = null!;

  public int? SeccionModuloId { get; set; }
  public SeccionModulo SeccionModulo { get; set; } = null!;

  public int? SubtituloSeccionId { get; set; }
  public SubtituloSeccion SubtituloSeccion { get; set; } = null!;
}

