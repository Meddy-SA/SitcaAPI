namespace Sitca.Models;

public class Capacitaciones
{
  public int Id { get; set; }
  public DateTime FechaCarga { get; set; }
  public string Nombre { get; set; } = null!;
  public string Descripcion { get; set; } = null!;
  public string Ruta { get; set; } = null!;
  public string Tipo { get; set; } = null!;
  public bool Activo { get; set; }
  public string UsuarioCargaId { get; set; } = null!;
  public ApplicationUser UsuarioCarga { get; set; } = null!;
}

