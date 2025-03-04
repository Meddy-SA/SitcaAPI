namespace Sitca.Models;

public class ProcesoCertificacion : AuditableEntity
{
    // Propiedades básicas
    public int Id { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
    public DateTime? FechaSolicitudAuditoria { get; set; }
    public DateTime? FechaFijadaAuditoria { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public bool Recertificacion { get; set; }
    public string NumeroExpediente { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int? Cantidad { get; set; } = 0;

    // Claves foráneas
    public string? AsesorId { get; set; }
    public string? AuditorId { get; set; }
    public int EmpresaId { get; set; }
    public string UserGeneraId { get; set; } = null!;
    public int? TipologiaId { get; set; }

    // Propiedades de navegación
    public ApplicationUser? AsesorProceso { get; set; }
    public ApplicationUser? AuditorProceso { get; set; }
    public Empresa Empresa { get; set; } = null!;
    public ApplicationUser UserGenerador { get; set; } = null!;
    public Tipologia? Tipologia { get; set; }
    public ICollection<ResultadoCertificacion> Resultados { get; set; } = [];
    public ICollection<Cuestionario> Cuestionarios { get; set; } = [];
    public ICollection<ProcesoArchivos> ProcesosArchivos { get; set; } = [];
}
