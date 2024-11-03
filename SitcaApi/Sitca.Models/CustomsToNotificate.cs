using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models
{
  public class CustomsToNotificate
  {
    [Key]
    public int Id { get; set; }
    [ForeignKey("CustomUser")]
    public int CustomId { get; set; }
    public int NotificacionId { get; set; }

    public Notificacion Notificacion { get; set; } = null!;
    public NotificationCustomUsers CustomUser { get; set; } = null!;
  }
}
