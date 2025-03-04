using System.ComponentModel.DataAnnotations;

namespace Sitca.Models.DTOs;

/// <summary>
/// DTO para crear un nuevo proceso de certificación
/// </summary>
public class CrearProcesoCertificacionDTO
{
    /// <summary>
    /// ID de la empresa para la que se crea el proceso
    /// </summary>
    [Required(ErrorMessage = "El ID de la empresa es obligatorio")]
    public int EmpresaId { get; set; }

    /// <summary>
    /// ID del asesor asignado (opcional inicialmente)
    /// </summary>
    public string? AsesorId { get; set; }

    /// <summary>
    /// ID de la tipología (opcional inicialmente)
    /// </summary>
    public int? TipologiaId { get; set; }

    /// <summary>
    /// Número de expediente (opcional inicialmente)
    /// </summary>
    [MaxLength(40)]
    public string? NumeroExpediente { get; set; }

    /// <summary>
    /// Indica si es una recertificación
    /// </summary>
    public bool Recertificacion { get; set; }

    /// <summary>
    /// Cantidad (opcional)
    /// </summary>
    public int? Cantidad { get; set; }
}

/// <summary>
/// DTO para actualizar un proceso de certificación existente
/// </summary>
public class ActualizarProcesoCertificacionDTO
{
    /// <summary>
    /// ID del asesor asignado
    /// </summary>
    public string? AsesorId { get; set; }

    /// <summary>
    /// ID del auditor asignado
    /// </summary>
    public string? AuditorId { get; set; }

    /// <summary>
    /// ID de la tipología
    /// </summary>
    public int? TipologiaId { get; set; }

    /// <summary>
    /// Número de expediente
    /// </summary>
    [MaxLength(40)]
    public string? NumeroExpediente { get; set; }

    /// <summary>
    /// Estado del proceso
    /// </summary>
    [MaxLength(30)]
    public string? Status { get; set; }

    /// <summary>
    /// Fecha de solicitud de auditoría
    /// </summary>
    public DateTime? FechaSolicitudAuditoria { get; set; }

    /// <summary>
    /// Fecha fijada para la auditoría
    /// </summary>
    public DateTime? FechaFijadaAuditoria { get; set; }

    /// <summary>
    /// Fecha de finalización
    /// </summary>
    public DateTime? FechaFinalizacion { get; set; }

    /// <summary>
    /// Fecha de vencimiento
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>
    /// Cantidad
    /// </summary>
    public int? Cantidad { get; set; }
}

/// <summary>
/// DTO para filtrar procesos de certificación
/// </summary>
public class FiltrarProcesoCertificacionDTO
{
    /// <summary>
    /// Texto para búsqueda (número de expediente, nombre de empresa, etc.)
    /// </summary>
    public string? TextoBusqueda { get; set; }

    /// <summary>
    /// ID de la empresa
    /// </summary>
    public int? EmpresaId { get; set; }

    /// <summary>
    /// ID del país
    /// </summary>
    public int? PaisId { get; set; }

    /// <summary>
    /// ID de la tipología
    /// </summary>
    public int? TipologiaId { get; set; }

    /// <summary>
    /// ID del asesor
    /// </summary>
    public string? AsesorId { get; set; }

    /// <summary>
    /// ID del auditor
    /// </summary>
    public string? AuditorId { get; set; }

    /// <summary>
    /// Estado del proceso
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Indica si es una recertificación
    /// </summary>
    public bool? Recertificacion { get; set; }

    /// <summary>
    /// Fecha de inicio desde
    /// </summary>
    public DateTime? FechaInicioDesde { get; set; }

    /// <summary>
    /// Fecha de inicio hasta
    /// </summary>
    public DateTime? FechaInicioHasta { get; set; }

    /// <summary>
    /// Fecha de vencimiento desde
    /// </summary>
    public DateTime? FechaVencimientoDesde { get; set; }

    /// <summary>
    /// Fecha de vencimiento hasta
    /// </summary>
    public DateTime? FechaVencimientoHasta { get; set; }

    /// <summary>
    /// Número de página (para paginación)
    /// </summary>
    public int Pagina { get; set; } = 1;

    /// <summary>
    /// Tamaño de página (para paginación)
    /// </summary>
    public int TamañoPagina { get; set; } = 10;

    /// <summary>
    /// Campo por el cual ordenar
    /// </summary>
    public string OrdenarPor { get; set; } = "FechaInicio";

    /// <summary>
    /// Dirección de ordenamiento (ascendente/descendente)
    /// </summary>
    public bool OrdenAscendente { get; set; } = false;
}
