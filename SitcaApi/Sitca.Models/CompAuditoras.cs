namespace Sitca.Models;

public class CompAuditoras
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string Representante { get; set; } = "-";
    public string NumeroCertificado { get; set; } = string.Empty;
    public DateTime? FechaInicioConcesion { get; set; }
    public DateTime? FechaFinConcesion { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool Status { get; set; }
    public bool Special { get; set; }
    public int PaisId { get; set; }
    public Pais? Pais { get; set; } = default!;
}

