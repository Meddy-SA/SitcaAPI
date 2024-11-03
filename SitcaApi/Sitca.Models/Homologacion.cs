using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models
{
  public class Homologacion
  {
    [Key]
    public int Id { get; set; }

    [StringLength(70)]
    public string Distintivo { get; set; } = null!;

    [StringLength(70)]
    public string DistintivoExterno { get; set; } = null!;

    public bool? EnProcesoSiccs { get; set; }

    [StringLength(1000)]
    public string DatosProceso { get; set; } = null!;

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaUltimaEdicion { get; set; }

    public DateTime FechaOtorgamiento { get; set; }

    public DateTime FechaVencimiento { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public ProcesoCertificacion Certificacion { get; set; } = null!;

    [ForeignKey("Certificacion")]
    public int CertificacionId { get; set; }
  }
}
