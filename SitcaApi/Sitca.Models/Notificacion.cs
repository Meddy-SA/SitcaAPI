using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
  public class Notificacion
  {
    [Key]
    public int Id { get; set; }

    [StringLength(150)]
    public string TituloInterno { get; set; } = null!;
    [StringLength(150)]
    public string TituloParaEmpresa { get; set; } = null!;
    public int Status { get; set; }

    [StringLength(200)]
    public string TextoInterno { get; set; } = null!;
    [StringLength(200)]
    public string TextoParaEmpresa { get; set; } = null!;

    public int Pais { get; set; }

    [StringLength(150)]
    public string TituloInternoEn { get; set; } = null!;

    [StringLength(150)]
    public string TituloParaEmpresaEn { get; set; } = null!;

    [StringLength(200)]
    public string TextoInternoEn { get; set; } = null!;
    [StringLength(200)]
    public string TextoParaEmpresaEn { get; set; } = null!;

    public ICollection<NotificationGroups> NotificationGroups { get; set; } = [];
  }
}
