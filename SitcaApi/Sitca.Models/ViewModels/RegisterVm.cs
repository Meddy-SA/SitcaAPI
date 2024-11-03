using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models.ViewModels
{
  [NotMapped]
  public class RegisterVm
  {
    public string email { get; set; } = null!;
    public string empresa { get; set; } = null!;
    public string password { get; set; } = null!;
    public int country { get; set; }
    public string representante { get; set; } = null!;
    public string language { get; set; } = null!;
    public List<CommonVm> tipologias { get; set; } = [];
  }

  [NotMapped]
  public class RegisterStaffVm
  {
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Password { get; set; } = null!;
    public int Country { get; set; }
    public string Role { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Codigo { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string NumeroCarnet { get; set; } = null!;
    public DateTime VencimientoCarnet { get; set; }
    public string FechaIngreso { get; set; } = null!;
    public string HojaDeVida { get; set; } = null!;
    public string DocumentoAcreditacion { get; set; } = null!;
    public string Departamento { get; set; } = null!;
    public string Ciudad { get; set; } = null!;
    public string DocumentoIdentidad { get; set; } = null!;
    public string Profesion { get; set; } = null!;
    public string Nacionalidad { get; set; } = null!;

    public int CompAuditoraId { get; set; }
    public bool Active { get; set; }

    public bool Notificaciones { get; set; }
  }

}
