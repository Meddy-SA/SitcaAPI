using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models
{
  public class Empresa
  {
    [Key]
    public int Id { get; set; }
    [StringLength(200)]
    public string Nombre { get; set; } = null!;

    [StringLength(150)]
    public string NombreRepresentante { get; set; } = null!;

    [StringLength(60)]
    public string CargoRepresentante { get; set; } = null!;

    [StringLength(50)]
    public string Ciudad { get; set; } = null!;

    [StringLength(15)]
    public string IdNacional { get; set; } = null!;

    [StringLength(15)]
    public string Telefono { get; set; } = null!;

    [StringLength(150)]
    public string Calle { get; set; } = null!;
    [StringLength(60)]
    public string Numero { get; set; } = null!;
    [StringLength(150)]
    public string Direccion { get; set; } = null!;
    [StringLength(20)]
    public string Longitud { get; set; } = null!;
    [StringLength(20)]
    public string Latitud { get; set; } = null!;

    [StringLength(50)]
    public string Email { get; set; } = null!;

    [StringLength(100)]
    public string WebSite { get; set; } = null!;

    [Required]
    public int IdPais { get; set; }
    [Required]
    public bool Active { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Estado { get; set; }

    public int? PaisId { get; set; }

    public string ResultadoSugerido { get; set; } = null!;
    public string ResultadoActual { get; set; } = null!;

    public DateTime? ResultadoVencimiento { get; set; }

    public bool EsHomologacion { get; set; }

    public Pais Pais { get; set; } = null!;

    public DateTime? FechaAutoNotif { get; set; }

    public ICollection<TipologiasEmpresa> Tipologias { get; set; } = [];
    public ICollection<Archivo> Archivos { get; set; } = [];
    public ICollection<ProcesoCertificacion> Certificaciones { get; set; } = [];
    public ICollection<Homologacion> Homologaciones { get; set; } = [];

  }
}
