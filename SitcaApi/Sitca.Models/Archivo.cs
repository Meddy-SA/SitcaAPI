using Sitca.Models.Enums;

namespace Sitca.Models;

public class Archivo
{
  public int Id { get; set; }
  public DateTime FechaCarga { get; set; }
  public string Nombre { get; set; } = null!;
  public string Ruta { get; set; } = null!;
  public string Tipo { get; set; } = null!;
  public FileCompany? FileTypesCompany { get; set; } = FileCompany.Informativo;
  public bool Activo { get; set; }

  // Claves foráneas
  public string UsuarioCargaId { get; set; } = null!;
  public string? UsuarioId { get; set; }
  public int? CuestionarioItemId { get; set; }
  public int? EmpresaId { get; set; }

  // Propiedades de navegación
  public ApplicationUser UsuarioCarga { get; set; } = null!;
  public ApplicationUser? Usuario { get; set; } = null!;
  public CuestionarioItem? CuestionarioItem { get; set; }
  public Empresa? Empresa { get; set; }

  // Propiedad de transporte (no mapeada)
  public string? Base64Str { get; set; }
}

