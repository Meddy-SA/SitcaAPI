
namespace Sitca.Models.ViewModels
{
  public class ListadoExterno
  {
    public string Nombre { get; set; } = null!;
    public string Pais { get; set; } = null!;
    public string Id { get; set; } = null!;
    public string Tipologia { get; set; } = null!;
  }
  public class ListadoExternoFiltro
  {
    public string Pais { get; set; } = null!;
  }

  public class ResponseListadoExterno
  {
    public bool Success { get; set; }
    public List<ListadoExterno> Data { get; set; } = [];
    public string Message { get; set; } = null!;
  }


}
