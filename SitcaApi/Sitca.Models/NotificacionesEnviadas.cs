using System.ComponentModel.DataAnnotations;

namespace Sitca.Models;

public class NotificacionesEnviadas
{
  [Key]
  public int Id { get; set; }
  [StringLength(450)]
  public string UserId { get; set; } = null!;
  public int CertificacionId { get; set; }
  public DateTime FechaNotificacion { get; set; }
}
