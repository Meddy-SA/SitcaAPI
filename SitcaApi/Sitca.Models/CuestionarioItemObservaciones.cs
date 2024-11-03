using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
  public class CuestionarioItemObservaciones
  {
    [Key]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    [StringLength(1000)]
    public string Observaciones { get; set; } = null!;

    public int CuestionarioItemId { get; set; }
    public CuestionarioItem CuestionarioItem { get; set; } = null!;

    public string UsuarioCargaId { get; set; } = null!;
  }
}
