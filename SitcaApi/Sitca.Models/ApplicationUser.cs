using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models
{
  public class ApplicationUser : IdentityUser
  {
    public int? EmpresaId { get; set; }

    [StringLength(60)]
    public string FirstName { get; set; } = null!;

    [StringLength(60)]
    public string LastName { get; set; } = null!;

    public int? PaisId { get; set; }

    [StringLength(60)]
    public string Codigo { get; set; } = null!;

    //Nuevos Campos
    [StringLength(200)]
    public string Direccion { get; set; } = null!;

    [StringLength(20)]
    public string NumeroCarnet { get; set; } = null!;

    [StringLength(20)]
    public string FechaIngreso { get; set; } = null!;

    [StringLength(60)]
    public string HojaDeVida { get; set; } = null!;

    [StringLength(60)]
    public string DocumentoAcreditacion { get; set; } = null!;

    //[StringLength(20)]
    //public string Telefono { get; set; }

    [StringLength(120)]
    public string Departamento { get; set; } = null!;
    [StringLength(120)]
    public string Ciudad { get; set; } = null!;

    [StringLength(30)]
    public string DocumentoIdentidad { get; set; } = null!;

    [StringLength(120)]
    public string Profesion { get; set; } = null!;

    [StringLength(60)]
    public string Nacionalidad { get; set; } = null!;


    [ForeignKey("CompAuditora")]
    public int? CompAuditoraId { get; set; }
    public CompAuditoras CompAuditora { get; set; } = null!;

    public bool Active { get; set; }

    public bool Notificaciones { get; set; }

    [StringLength(3)]
    public string Lenguage { get; set; } = null!;

    public DateTime? VencimientoCarnet { get; set; }

    public DateTime? AvisoVencimientoCarnet { get; set; }
  }
}
