namespace Sitca.Models.DTOs;

/// <summary>
/// DTO para mostrar información básica de un usuario
/// </summary>
public class UsuarioBasicoDTO
{
    public string Id { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Codigo { get; set; }
}
