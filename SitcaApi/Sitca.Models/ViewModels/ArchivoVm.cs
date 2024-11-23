using Sitca.Models.Enums;

namespace Sitca.Models.ViewModels;

public class DatosCapacitacion
{
  public string nombre { get; set; } = null!;
  public string descripcion { get; set; } = null!;
  public string tipo { get; set; } = null!;
  public string url { get; set; } = null!;
}

public class ArchivoVm
{
  public int Id { get; set; }
  public string Ruta { get; set; } = null!;
  public string Nombre { get; set; } = null!;
  public string Tipo { get; set; } = null!;
  public string FechaCarga { get; set; } = null!;
  public string Cargador { get; set; } = null!;
  public FileCompany? FileTypesCompany { get; set; } = FileCompany.Informativo;
  public string? NameType { get; set; }

  public bool Propio { get; set; }
}

public class ArchivoFilterVm
{
  public int? idPregunta { get; set; }
  public int? idCuestionario { get; set; }
  public string type { get; set; } = null!;
}

