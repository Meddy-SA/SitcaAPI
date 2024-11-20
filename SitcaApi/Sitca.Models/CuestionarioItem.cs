using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
  public class CuestionarioItem
  {
    public CuestionarioItem()
    {
      Archivos = new HashSet<Archivo>();  // Inicializar como HashSet
    }

    [Key]
    public int Id { get; set; }

    [StringLength(2500)]
    public string Texto { get; set; } = null!;
    [StringLength(30)]
    public string Nomenclatura { get; set; } = null!;

    public bool ResultadoAuditor { get; set; } //si, no, no Aplica
    public int Resultado { get; set; }

    public bool Obligatorio { get; set; }

    public int CuestionarioId { get; set; }
    public Cuestionario Cuestionario { get; set; } = null!;

    public int PreguntaId { get; set; }
    public Pregunta Pregunta { get; set; } = null!;
    public DateTime? FechaActualizado { get; set; }

    public IEnumerable<Archivo> Archivos { get; set; } = [];

    public IEnumerable<CuestionarioItemObservaciones> CuestionarioItemObservaciones { get; set; } = [];
  }
}
