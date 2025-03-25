namespace Sitca.DataAccess.Services.Url;

/// <summary>
/// Interfaz para el servicio de gestión de URLs
/// </summary>
public interface IUrlService
{
    /// <summary>
    /// Genera la URL base para los recursos de la aplicación
    /// </summary>
    /// <returns>URL base como string</returns>
    string GenerateBaseUrl();
}
