namespace Sitca.Models.DTOs;

public class EmpresaReportFilterDTO
{
    // Filtros seleccionables
    public List<int>? CountryIds { get; set; }
    public List<int>? TypologyIds { get; set; }
    public List<int>? StatusIds { get; set; }
    public List<string>? CertificationTypes { get; set; } // Certificación/Homologación
    public List<int>? DistintivoIds { get; set; } // Distintivo verde, rojo, azul, en proceso.

    // Idioma para localización de textos
    public string Language { get; set; } = "es";

    // Paginación por bloques
    public int BlockNumber { get; set; } = 1;
    public int BlockSize { get; set; } = 20;
}
