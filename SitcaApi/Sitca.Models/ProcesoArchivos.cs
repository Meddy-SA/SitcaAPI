using Sitca.Models.Enums;

namespace Sitca.Models;

public class ProcesoArchivos : AuditableEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Ruta { get; set; } = null!;
    public string Tipo { get; set; } = null!;
    public FileCompany? FileTypesCompany { get; set; } = FileCompany.Informativo;
    public int ProcesoCertificacionId { get; set; }
    public ProcesoCertificacion ProcesoCertificacion { get; set; } = null!;
    public long? FileSize { get; set; }

    // Propiedad de transporte (no mapeada)
    public string? Base64Str { get; set; }
}
