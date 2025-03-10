using System.Text.Json.Serialization;

namespace Sitca.Models.DTOs;

/// <summary>
/// Modelo para la respuesta de la API Python de procesamiento de imágenes
/// </summary>
public class PythonApiResponse
{
    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje descriptivo del resultado de la operación
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = null!;

    /// <summary>
    /// Datos adicionales devueltos por la API (si los hay)
    /// </summary>
    [JsonPropertyName("data")]
    public object Data { get; set; } = null!;

    /// <summary>
    /// Timestamp de la respuesta
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}
