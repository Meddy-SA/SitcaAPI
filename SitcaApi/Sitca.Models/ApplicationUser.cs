using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace Sitca.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? EmpresaId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public int? PaisId { get; set; }
        public virtual Pais Pais { get; set; } = null!;
        public string Codigo { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public string NumeroCarnet { get; set; } = null!;
        public string FechaIngreso { get; set; } = null!;
        public string HojaDeVida { get; set; } = null!;
        public string DocumentoAcreditacion { get; set; } = null!;
        public string Departamento { get; set; } = null!;
        public string Ciudad { get; set; } = null!;
        public string DocumentoIdentidad { get; set; } = null!;
        public string Profesion { get; set; } = null!;
        public string Nacionalidad { get; set; } = null!;
        public int? CompAuditoraId { get; set; }
        public string Discriminator { get; set; } = "ApplicationUser";

        [JsonIgnore]
        public virtual CompAuditoras? CompAuditora { get; set; } = null!;
        public bool Active { get; set; }
        public bool Notificaciones { get; set; }
        public string Lenguage { get; set; } = null!;
        public DateTime? VencimientoCarnet { get; set; }
        public DateTime? AvisoVencimientoCarnet { get; set; }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

        public ApplicationUser()
        {
            UserRoles = new HashSet<ApplicationUserRole>();
        }
    }
}
