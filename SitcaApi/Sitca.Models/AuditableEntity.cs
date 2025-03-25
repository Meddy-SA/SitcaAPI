using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models;

/// <summary>
/// Clase base para entidades que requieren auditoría
/// </summary>
public abstract class AuditableEntity
{
    /// <summary>
    /// Indica si la entidad está habilitada
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Identificador del usuario que creó la entidad
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Fecha de creación de la entidad
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Identificador del usuario que actualizó la entidad
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Fecha de última actualización de la entidad
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // Propiedades de navegación
    /// <summary>
    /// Usuario que creó la entidad
    /// </summary>
    [ForeignKey(nameof(CreatedBy))]
    public ApplicationUser? UserCreate { get; set; }
}
