using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
  public class CompAuditoras
  {
    [Key]
    public int Id { get; set; }

    [StringLength(120)]
    public string Name { get; set; } = null!;

    [StringLength(200)]
    public string Direccion { get; set; } = null!;


    [StringLength(200)]
    public string Representante { get; set; } = null!;

    //numero certificado reconocimiento
    [StringLength(30)]
    public string NumeroCertificado { get; set; } = null!;

    public DateTime? FechaInicioConcesion { get; set; }

    public DateTime? FechaFinConcesion { get; set; }

    [StringLength(100)]
    public string Tipo { get; set; } = null!;

    [StringLength(120)]
    public string Email { get; set; } = null!;

    [StringLength(20)]
    public string Telefono { get; set; } = null!;

    public bool Status { get; set; }

    public bool Special { get; set; }

    public int PaisId { get; set; }

    public Pais Pais { get; set; } = null!;

  }
}
