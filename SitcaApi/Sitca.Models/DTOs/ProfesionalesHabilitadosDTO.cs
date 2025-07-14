namespace Sitca.Models.DTOs;

public class ProfesionalDTO
{
    public string NumeroCarnet { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // auditor, asesor, auditor/asesor
    public DateTime? FechaExpiracion { get; set; }
    public string? CompaniaAuditora { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

public class ProfesionalesPorPaisDTO
{
    public string Pais { get; set; } = string.Empty;
    public int PaisId { get; set; }
    public List<ProfesionalDTO> Profesionales { get; set; } = new List<ProfesionalDTO>();
    public int TotalProfesionales { get; set; }
    public int TotalAuditores { get; set; }
    public int TotalAsesores { get; set; }
    public int TotalAuditoresAsesores { get; set; }
}

public class ProfesionalesHabilitadosResponseDTO
{
    public List<ProfesionalesPorPaisDTO> PaisesProfesionales { get; set; } = new List<ProfesionalesPorPaisDTO>();
    public int TotalPaises { get; set; }
    public int TotalProfesionales { get; set; }
    public Dictionary<string, int> ResumenPorTipo { get; set; } = new Dictionary<string, int>();
}

public class ProfesionalesHabilitadosFilterDTO
{
    public int? PaisId { get; set; }
    public string? TipoProfesional { get; set; } // auditor, asesor, auditor/asesor
    public bool? SoloActivos { get; set; } = true;
    public bool? SoloConCarnet { get; set; } = true;
    public string Language { get; set; } = "es";
}