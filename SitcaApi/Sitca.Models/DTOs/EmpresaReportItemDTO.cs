namespace Sitca.Models.DTOs;

public class EmpresaReportItemDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public int PaisId { get; set; }
    public string Responsable { get; set; } = string.Empty;
    public string Tipologias { get; set; } = string.Empty; // Tipologías concatenadas
    public List<string> TipologiasList { get; set; } = new List<string>();
    public List<int> TipologiasIds { get; set; } = new List<int>();
    public string Estado { get; set; } = string.Empty;
    public int EstadoId { get; set; }
    public string Certificacion { get; set; } = string.Empty;

    // Distintivo principal (del proceso más reciente)
    public string? Distintivo { get; set; }
    public int? DistintivoId { get; set; }

    // Todos los distintivos de la empresa
    public List<string> Distintivos { get; set; } = new List<string>();
    public List<int> DistintivosIds { get; set; } = new List<int>();

    // Procesos en trámite
    public bool EnProceso { get; set; } // Si el proceso más reciente está en proceso
    public int ProcesosEnProceso { get; set; } // Número de procesos en trámite

    public int TotalProcesos { get; set; }

    public bool Activa { get; set; } = true;
    public DateTime? FechaVencimiento { get; set; } // Fecha de vencimiento del distintivo
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
}
