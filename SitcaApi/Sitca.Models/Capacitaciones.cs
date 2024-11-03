using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models
{
  public class Capacitaciones
  {
    [Key]
    public int Id { get; set; }

    public DateTime FechaCarga { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(250)]
    public string Descripcion { get; set; } = null!;

    [StringLength(500)]
    public string Ruta { get; set; } = null!;

    [StringLength(10)]
    public string Tipo { get; set; } = null!;

    public bool Activo { get; set; }

    [ForeignKey("UsuarioCarga")]
    [StringLength(450)]
    public string UsuarioCargaId { get; set; } = null!;
    public ApplicationUser UsuarioCarga { get; set; } = null!;
  }
}
