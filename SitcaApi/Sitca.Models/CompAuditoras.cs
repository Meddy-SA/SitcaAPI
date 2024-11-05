namespace Sitca.Models;

public class CompAuditoras
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public string Direccion { get; set; } = null!;
  public string Representante { get; set; } = null!;
  public string NumeroCertificado { get; set; } = null!;
  public DateTime? FechaInicioConcesion { get; set; }
  public DateTime? FechaFinConcesion { get; set; }
  public string Tipo { get; set; } = null!;
  public string Email { get; set; } = null!;
  public string Telefono { get; set; } = null!;
  public bool Status { get; set; }
  public bool Special { get; set; }
  public int PaisId { get; set; }
  public Pais Pais { get; set; } = null!;
}

