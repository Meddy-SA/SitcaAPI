using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
  public class NotificationGroups
  {
    [Key]
    public int Id { get; set; }
    public int NotificationId { get; set; }

    [StringLength(60)]
    public string RoleId { get; set; } = null!;

    public Notificacion Notification { get; set; } = null!;
  }
}
