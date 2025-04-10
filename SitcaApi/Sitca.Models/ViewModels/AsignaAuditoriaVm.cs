using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models.ViewModels;

[NotMapped]
public class AsignaAuditoriaVm
{
    public int ProcesoId { get; set; }
    public string AuditorId { get; set; } = null!;
    public string Fecha { get; set; } = null!;
    public int TipologiaId { get; set; }
    public int NewStatus { get; set; } = 0;
}
