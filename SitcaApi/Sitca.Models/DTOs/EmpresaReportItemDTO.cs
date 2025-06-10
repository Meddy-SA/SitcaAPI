namespace Sitca.Models.DTOs;

public class EmpresaReportItemDTO
{
    // Datos de la empresa
    public int EmpresaId { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public int PaisId { get; set; }
    public string Responsable { get; set; } = string.Empty;
    public string Tipologias { get; set; } = string.Empty; // Tipologías concatenadas
    public List<string> TipologiasList { get; set; } = new List<string>();
    public List<int> TipologiasIds { get; set; } = new List<int>();
    public bool EmpresaActiva { get; set; } = true;
    
    // Datos del proceso de certificación
    public int ProcesoId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int EstadoId { get; set; }
    public string TipoCertificacion { get; set; } = string.Empty; // Certificación o Homologación
    public DateTime? FechaInicioProceso { get; set; }
    public DateTime? FechaFinProceso { get; set; }
    public string NumeroExpediente { get; set; } = string.Empty;
    
    // Distintivo del proceso
    public string? Distintivo { get; set; }
    public int? DistintivoId { get; set; }
    public bool EnProceso { get; set; } // Si el proceso está en trámite
    public DateTime? FechaVencimientoDistintivo { get; set; }
    
    // Auditoría
    public DateTime? FechaAuditoria { get; set; }
    public DateTime? FechaAuditoriaFin { get; set; }
}

public class EmpresaReportResponseDTO
{
    public List<EmpresaReportItemDTO> Items { get; set; } = new List<EmpresaReportItemDTO>();
    public int TotalCount { get; set; }
    public int TotalActive { get; set; }
    public int TotalInactive { get; set; }

    // Contadores por distintivo
    public int TotalEnProceso { get; set; }
    public Dictionary<int, int> DistintivosCount { get; set; } = new Dictionary<int, int>();
    public Dictionary<string, int> DistintivosNameCount { get; set; } =
        new Dictionary<string, int>();

    public int CurrentBlock { get; set; }
    public int TotalBlocks { get; set; }
    public int BlockSize { get; set; }
    public int totalUniqueCompanies { get; set; }
}
