using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models
{
  public class Archivo
  {
    [Key]
    public int Id { get; set; }

    public DateTime FechaCarga { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(50)]
    public string Ruta { get; set; } = null!;

    [StringLength(10)]
    public string Tipo { get; set; } = null!;

    [NotMapped]
    public string Base64Str { get; set; } = null!;

    public int? CuestionarioItemId { get; set; }
    public CuestionarioItem CuestionarioItem { get; set; } = null!;


    public int? EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;


    [ForeignKey("UsuarioCarga")]
    [StringLength(450)]
    public string UsuarioCargaId { get; set; } = null!;
    public ApplicationUser UsuarioCarga { get; set; } = null!;


    [ForeignKey("Usuario")]
    [StringLength(450)]
    public string UsuarioId { get; set; } = null!;
    public ApplicationUser Usuario { get; set; } = null!;

    public bool Activo { get; set; }
  }
}
