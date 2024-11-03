using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models
{
  public class ProcesoCertificacion
  {
    [Key]
    public int Id { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFinalizacion { get; set; }

    public DateTime? FechaSolicitudAuditoria { get; set; }

    public DateTime? FechaFijadaAuditoria { get; set; }

    public bool Recertificacion { get; set; }

    [StringLength(40)]
    public string NumeroExpediente { get; set; } = null!;

    [StringLength(30)]
    public string Status { get; set; } = null!;

    [ForeignKey("AsesorProceso")]
    [StringLength(450)]
    public string AsesorId { get; set; } = null!;
    public ApplicationUser AsesorProceso { get; set; } = null!;


    public int? TipologiaId { get; set; }
    public Tipologia Tipologia { get; set; } = null!;



    [ForeignKey("AuditorProceso")]
    [StringLength(450)]
    public string AuditorId { get; set; } = null!;
    public ApplicationUser AuditorProceso { get; set; } = null!;


    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;


    [ForeignKey("UserGenerador")]
    [StringLength(450)]
    public string UserGeneraId { get; set; } = null!;
    public ApplicationUser UserGenerador { get; set; } = null!;

    public ICollection<ResultadoCertificacion> Resultados { get; set; } = [];

    public DateTime? FechaVencimiento { get; set; }
  }
}
