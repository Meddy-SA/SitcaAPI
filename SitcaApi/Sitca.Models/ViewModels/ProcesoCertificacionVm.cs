using Sitca.Models.DTOs;

namespace Sitca.Models.ViewModels;

public class ProcesoCertificacionVm
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string NombreEmpresa { get; set; } = null!;
    public string NumeroExpediente { get; set; } = null!;
    public string Pais { get; set; } = null!;
    public PaisDTO PaisDto { get; set; } = null!;
    public List<string> Tipologias { get; set; } = [];
    public List<int>? TipologiasIds { get; set; }
    public string Responsable { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int StatusId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
    public bool Recertificacion { get; set; }
    public string? Distintivo { get; set; }
    public int? DistintivoId { get; set; }
    public string? FechaVencimiento { get; set; }
    public Personnal? Asesor { get; set; }
    public Personnal? Auditor { get; set; }
    public bool? Activo { get; set; }
    public string? FechaRevision { get; set; }
}
