using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models.ViewModels
{
  [NotMapped]
  public class EstadisticasVm
  {
    public IEnumerable<EstadisticaItemVm> EmpresasPorPais { get; set; } = [];
    public IEnumerable<EstadisticaItemVm> EmpresasPorTipologia { get; set; } = [];
  }

  [NotMapped]
  public class EmpresasCalificadas
  {
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Name { get; set; } = null!;
    public string FechaDictamen { get; set; } = null!;
    public string NumeroDictamen { get; set; } = null!;
    public string Distintivo { get; set; } = null!;
    public bool Aprobado { get; set; }
    public string Observaciones { get; set; } = null!;
    public CommonUserVm Asesor { get; set; } = null!;
    public CommonUserVm Auditor { get; set; } = null!;
    public CommonVm Tipologia { get; set; } = null!;
  }

  [NotMapped]
  public class EstadisticaItemVm
  {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Count { get; set; }
  }
}
