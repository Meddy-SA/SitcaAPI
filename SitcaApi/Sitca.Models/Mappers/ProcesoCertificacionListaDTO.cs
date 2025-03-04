namespace Sitca.Models.DTOs;

/// <summary>
/// DTO ligero para listados de procesos de certificación
/// </summary>
public class ProcesoCertificacionListaDTO
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

    /// <summary>
    /// Fecha de inicio del proceso
    /// </summary>
    public DateTime FechaInicio { get; set; }

    /// <summary>
    /// Fecha de vencimiento, si aplica
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>
    /// Indica si es un proceso de recertificación
    /// </summary>
    public bool Recertificacion { get; set; }

    /// <summary>
    /// Nombre de la empresa
    /// </summary>
    public string NombreEmpresa { get; set; } = string.Empty;

    /// <summary>
    /// ID de la empresa
    /// </summary>
    public int EmpresaId { get; set; }

    /// <summary>
    /// Cantidad de archivos asociados
    /// </summary>
    public int CantidadArchivos { get; set; }

    /// <summary>
    /// Nombre del asesor asignado
    /// </summary>
    public string? NombreAsesor { get; set; }

    /// <summary>
    /// Nombre del auditor asignado
    /// </summary>
    public string? NombreAuditor { get; set; }

    /// <summary>
    /// País de la empresa
    /// </summary>
    public string NombrePais { get; set; } = string.Empty;

    /// <summary>
    /// ID del país
    /// </summary>
    public int PaisId { get; set; }

    /// <summary>
    /// Nombre de la tipología
    /// </summary>
    public string? NombreTipologia { get; set; }

    /// <summary>
    /// Fecha de la última modificación
    /// </summary>
    public DateTime? UltimaActualizacion { get; set; }
}

/// <summary>
/// Clase para respuestas paginadas de procesos de certificación
/// </summary>
public class ProcesoCertificacionPaginadoDTO
{
    /// <summary>
    /// Lista de procesos de certificación
    /// </summary>
    public List<ProcesoCertificacionListaDTO> Items { get; set; } =
        new List<ProcesoCertificacionListaDTO>();

    /// <summary>
    /// Número total de registros
    /// </summary>
    public int TotalRegistros { get; set; }

    /// <summary>
    /// Número de página actual
    /// </summary>
    public int PaginaActual { get; set; }

    /// <summary>
    /// Tamaño de página
    /// </summary>
    public int TamañoPagina { get; set; }

    /// <summary>
    /// Número total de páginas
    /// </summary>
    public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / TamañoPagina);
}
