using Sitca.Models.Enums;

namespace Sitca.Models.DTOs;

/// <summary>
/// DTO para mostrar información básica de un archivo de proceso
/// </summary>
public class ProcesoArchivoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public FileCompany? TipoArchivo { get; set; }
    public string? NombreTipoArchivo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? CreadoPor { get; set; }
    public string? NombreCreador { get; set; }
}
