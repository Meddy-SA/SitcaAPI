using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models.ViewModels
{
  [NotMapped]
  public class HomologacionDTO
  {
    public int id { get; set; }
    public int empresaId { get; set; }
    public string nombre { get; set; } = null!;
    public Tipologia tipologia { get; set; } = null!;
    public string datosProceso { get; set; } = null!;
    public SelloItc selloItc { get; set; } = null!;
    public CommonVm distintivoSiccs { get; set; } = null!;
    public DateTime fechaOtorgamiento { get; set; }
    public DateTime fechaVencimiento { get; set; }
    public List<ArchivoVm> archivos { get; set; } = [];
    public bool enProceso { get; set; }
  }

  [NotMapped]
  public class SelloItc
  {
    public string name { get; set; } = null!;
  }

}
