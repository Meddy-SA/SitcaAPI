using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
  public class BackupCuestionario
  {
    [Key]
    public int Id { get; set; }

    public int CuestionarioId { get; set; }

    public string CuestionarioCompleto { get; set; } = null!;
  }
}
