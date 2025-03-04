namespace Sitca.Models.DTOs;

/// <summary>
/// DTO para mostrar información básica de una empresa
/// </summary>
public class EmpresaBasicaDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string NombreRepresentante { get; set; } = string.Empty;
    public string? CargoRepresentante { get; set; }
    public string? IdNacional { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? WebSite { get; set; }

    public string ResultadoActual { get; set; } = string.Empty;
    public DateTime? ResultadoVencimiento { get; set; }
    public PaisDTO Pais { get; set; } = null!;
    public decimal? Estado { get; set; }
    public TipologiaDTO[] Tipologias { get; set; } = [];
    public bool EsHomologacion { get; set; }
    public bool Activo { get; set; }
}
