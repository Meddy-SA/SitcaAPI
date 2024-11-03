namespace Sitca.Models.ViewModels;

public class AsignaAuditoriaVm
{
  public string AuditorId { get; set; } = null!;
  public string Fecha { get; set; } = null!;
  public int TipologiaId { get; set; }
  public int EmpresaId { get; set; }
}

