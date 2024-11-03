namespace Sitca.Models.ViewModels
{
  public class UsersVm
  {
  }

  public class UsersListVm
  {
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string EmailConfirmed { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int PaisId { get; set; }
    public string Rol { get; set; } = null!;
    public string Pais { get; set; } = null!;
    public string Codigo { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string NumeroCarnet { get; set; } = null!;
    public string FechaIngreso { get; set; } = null!;
    public string HojaDeVida { get; set; } = null!;
    public string DocumentoAcreditacion { get; set; } = null!;
    public string Departamento { get; set; } = null!;
    public string VencimientoCarnet { get; set; } = null!;

    public DateTime AvisoVencimientoCarnet { get; set; }

    public string Ciudad { get; set; } = null!;
    public string DocumentoIdentidad { get; set; } = null!;
    public string Profesion { get; set; } = null!;
    public string Nacionalidad { get; set; } = null!;
    public string RutaPdf { get; set; } = null!;
    public int? CompAuditoraId { get; set; }
    public bool CanDeactivate { get; set; }
    public bool Active { get; set; }
    public bool Notificaciones { get; set; }
    public string Lang { get; set; } = null!;
  }

  public class NotificacionVm
  {
    public Notificacion Data { get; set; } = null!;
    public List<UsersListVm> Users { get; set; } = [];
  }

  public class NotificacionSigleVm
  {
    public Notificacion Data { get; set; } = null!;
    public UsersListVm User { get; set; } = null!;
  }
}
