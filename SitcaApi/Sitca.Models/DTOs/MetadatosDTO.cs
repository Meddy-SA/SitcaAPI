using Sitca.Models.ViewModels;

namespace Sitca.Models.DTOs;

/// <summary>
/// DTO que contiene todos los metadatos necesarios para la gestión de empresas
/// </summary>
public class MetadatosDTO
{
    /// <summary>
    /// Lista de países disponibles
    /// </summary>
    public List<CommonVm> Paises { get; set; } = new List<CommonVm>();

    /// <summary>
    /// Lista de tipologías de empresa
    /// </summary>
    public List<CommonVm> Tipologias { get; set; } = new List<CommonVm>();

    /// <summary>
    /// Lista de distintivos disponibles
    /// </summary>
    public List<CommonVm> Distintivos { get; set; } = new List<CommonVm>();

    /// <summary>
    /// Lista de estados de certificación
    /// </summary>
    public List<CommonVm> Estados { get; set; } = new List<CommonVm>();
}
