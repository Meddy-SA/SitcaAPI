namespace Sitca.Models.DTOs;

/// <summary>
/// DTO para mostrar información básica de un Proceso de Certificación
/// </summary>
public class ProcesoCertificacionDTO
{
    /// <summary>
    /// Identificador único del proceso
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Número de expediente
    /// </summary>
    public string NumeroExpediente { get; set; } = string.Empty;

    /// <summary>
    /// Estado actual del proceso
    /// </summary>
    public string Status { get; set; } = string.Empty;
    public byte StatusId { get; set; } = 0;

    /// <summary>
    /// Fechas relevantes del proceso
    /// </summary>
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
    public DateTime? FechaSolicitudAuditoria { get; set; }
    public DateTime? FechaFijadaAuditoria { get; set; }
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>
    /// Indica si es una recertificación
    /// </summary>
    public bool Recertificacion { get; set; }

    /// <summary>
    /// Cantidad de elementos (opcional)
    /// </summary>
    public int? Cantidad { get; set; }

    /// <summary>
    /// Información sobre la tipología
    /// </summary>
    public TipologiaDTO? Tipologia { get; set; }

    /// <summary>
    /// Información de la empresa asociada
    /// </summary>
    public EmpresaBasicaDTO Empresa { get; set; } = null!;

    /// <summary>
    /// Información sobre el asesor asignado
    /// </summary>
    public UsuarioBasicoDTO? Asesor { get; set; }

    /// <summary>
    /// Información sobre el auditor asignado
    /// </summary>
    public UsuarioBasicoDTO? Auditor { get; set; }

    /// <summary>
    /// Usuario que generó el proceso
    /// </summary>
    public UsuarioBasicoDTO? UserGenerador { get; set; }

    /// <summary>
    /// Colección de archivos asociados al proceso
    /// </summary>
    public ICollection<ProcesoArchivoDTO> Archivos { get; set; } = new List<ProcesoArchivoDTO>();

    /// <summary>
    /// Colección de cuestionarios asociados al proceso
    /// </summary>
    public ICollection<CuestionarioBasicoDTO> Cuestionarios { get; set; } =
        new List<CuestionarioBasicoDTO>();

    /// <summary>
    /// Cantidad total de procesos de certificación para la empresa
    /// </summary>
    public int TotalProcesos { get; set; }

    /// <summary>
    /// Indica si este proceso es el último (más reciente) para la empresa
    /// </summary>
    public bool EsUltimoProceso { get; set; }

    /// <summary>
    /// Información de auditoría
    /// </summary>
    public string? CreadoPor { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public string? ActualizadoPor { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}
