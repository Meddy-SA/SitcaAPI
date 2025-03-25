using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Sitca.Models.Enums;

namespace Sitca.Models.DTOs;

/// <summary>
/// DTO para subir un nuevo archivo a un proceso
/// </summary>
public class SubirArchivoProcesoDTO
{
    /// <summary>
    /// ID del proceso de certificación al que se asociará el archivo
    /// </summary>
    [Required]
    public int ProcesoCertificacionId { get; set; }

    /// <summary>
    /// Nombre personalizado para el archivo (opcional)
    /// </summary>
    [MaxLength(100)]
    public string? Nombre { get; set; }

    /// <summary>
    /// Extensión del archivo (opcional)
    /// </summary>
    [MaxLength(5)]
    public string? Tipo { get; set; }

    /// <summary>
    /// Tipo de archivo según la clasificación del negocio
    /// </summary>
    public FileCompany TipoArchivo { get; set; } = FileCompany.Informativo;

    /// <summary>
    /// Archivo a subir
    /// </summary>
    public IFormFile? Archivo { get; set; } = null!;
}

/// <summary>
/// DTO para actualizar información de un archivo existente
/// </summary>
public class ActualizarArchivoProcesoDTO
{
    /// <summary>
    /// Nombre actualizado para el archivo
    /// </summary>
    [MaxLength(100)]
    public string? Nombre { get; set; }

    /// <summary>
    /// Tipo de archivo según la clasificación del negocio
    /// </summary>
    public FileCompany? TipoArchivo { get; set; }
}

/// <summary>
/// DTO para respuesta después de subir un archivo
/// </summary>
public class ArchivoSubidoDTO
{
    /// <summary>
    /// ID del archivo subido
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre del archivo
    /// </summary>
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Ruta donde se almacenó el archivo
    /// </summary>
    public string Ruta { get; set; } = null!;

    /// <summary>
    /// Tipo/extensión del archivo
    /// </summary>
    public string Tipo { get; set; } = null!;

    /// <summary>
    /// Tipo de archivo según la clasificación del negocio
    /// </summary>
    public FileCompany TipoArchivo { get; set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }
}

/// <summary>
/// DTO para filtrar archivos por proceso
/// </summary>
public class FiltrarArchivosProcesoDTO
{
    /// <summary>
    /// ID del proceso de certificación
    /// </summary>
    [Required]
    public int ProcesoCertificacionId { get; set; }

    /// <summary>
    /// Tipo de archivo según la clasificación del negocio (opcional)
    /// </summary>
    public FileCompany? TipoArchivo { get; set; }

    /// <summary>
    /// Texto para búsqueda por nombre (opcional)
    /// </summary>
    public string? TextoBusqueda { get; set; }
}
