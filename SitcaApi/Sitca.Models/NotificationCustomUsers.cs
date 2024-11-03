using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
  public class NotificationCustomUsers
  {
    [Key]
    public int Id { get; set; }

    public bool Global { get; set; }

    [StringLength(50)]
    public string Email { get; set; } = null!;

    [StringLength(150)]
    public string Name { get; set; } = null!;
    public int PaisId { get; set; }
  }
}
