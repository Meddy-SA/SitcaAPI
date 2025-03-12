namespace Sitca.Models.DTOs;

/// <summary>
/// DTO para mostrar información básica de los cuestionarios
/// </summary>
public class CuestionarioBasicoDTO
{
    public int Id { get; set; }
    public bool Prueba { get; set; } = true;
    public DateTime? FechaRevision { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaEvaluacion { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
}
