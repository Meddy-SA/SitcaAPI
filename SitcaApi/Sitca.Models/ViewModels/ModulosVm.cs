using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models.ViewModels
{
  [NotMapped]
  public class ModulosVm
  {
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Transversal { get; set; }
    public string Nomenclatura { get; set; } = null!;
    public int Orden { get; set; }
    public string Tipologia { get; set; } = null!;

    public List<CuestionarioItemVm> Items { get; set; } = [];

    public ResultadosModuloVm Resultados { get; set; } = null!;
  }

  [NotMapped]
  public class ResultadosModuloVm
  {
    public int TotalObligatorias { get; set; }
    public int TotalComplementarias { get; set; }

    public int ObligCumple { get; set; }
    public int ComplementCumple { get; set; }

    public decimal PorcObligCumple { get; set; }

    public decimal PorcComplementCumple { get; set; }

    public string ResultModulo { get; set; } = null!;
  }
}
