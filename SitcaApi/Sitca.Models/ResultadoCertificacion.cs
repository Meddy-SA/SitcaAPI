using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models
{
  public class ResultadoCertificacion
  {
    [Key]
    public int Id { get; set; }
    public bool Aprobado { get; set; }

    [StringLength(500)]
    public string Observaciones { get; set; } = null!;

    [StringLength(50)]
    public string NumeroDictamen { get; set; } = null!;
    public int? DistintivoId { get; set; }

    public Distintivo Distintivo { get; set; } = null!;

    [ForeignKey("ProcesoCertificacion")]
    public int CertificacionId { get; set; }
    public ProcesoCertificacion ProcesoCertificacion { get; set; } = null!;

  }
}
