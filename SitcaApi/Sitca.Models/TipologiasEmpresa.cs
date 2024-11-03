
namespace Sitca.Models
{
  public class TipologiasEmpresa
  {
    public int IdEmpresa { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public int IdTipologia { get; set; }
    public Tipologia Tipologia { get; set; } = null!;
  }
}
