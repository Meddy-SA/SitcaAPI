
namespace Sitca.Models.ViewModels
{
  public class RegistroHallazgos
  {
    public string Empresa { get; set; } = null!;
    public string Generador { get; set; } = null!;
    public string Language { get; set; } = null!;
    public string RutaPdf { get; set; } = null!;
    public List<HallazgosDTO> HallazgosItems { get; set; } = [];
  }
  public class HallazgosDTO
  {
    public string Modulo { get; set; } = null!;
    public string Referencia { get; set; } = null!;
    public Version ReferenciaOrden { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public string Obligatorio { get; set; } = null!;
  }
}
