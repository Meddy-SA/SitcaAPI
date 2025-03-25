namespace Sitca.Models.DTOs;

/// <summary>
/// DTO para mostrar información básica de una tipología
/// </summary>
public class TipologiaDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string NombreIngles { get; set; } = string.Empty;
}
