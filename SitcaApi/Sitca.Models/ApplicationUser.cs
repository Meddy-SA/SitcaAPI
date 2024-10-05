using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models
{
    public class ApplicationUser: IdentityUser
    {
        public int? EmpresaId { get; set; }

        [StringLength(60)]
        public string FirstName { get; set; }

        [StringLength(60)]
        public string LastName { get; set; }
        
        public int? PaisId { get; set; }

        [StringLength(60)]
        public string Codigo { get; set; }

        //Nuevos Campos
        [StringLength(200)]
        public string Direccion { get; set; }

        [StringLength(20)]
        public string NumeroCarnet { get; set; }

        [StringLength(20)]
        public string FechaIngreso { get; set; }

        [StringLength(60)]
        public string HojaDeVida { get; set; }

        [StringLength(60)]
        public string DocumentoAcreditacion { get; set; }

        //[StringLength(20)]
        //public string Telefono { get; set; }

        [StringLength(120)]
        public string Departamento { get; set; }
        [StringLength(120)]
        public string Ciudad { get; set; }

        [StringLength(30)]
        public string DocumentoIdentidad { get; set; }

        [StringLength(120)]
        public string Profesion { get; set; }

        [StringLength(60)]
        public string Nacionalidad { get; set; }


        [ForeignKey("CompAuditora")]
        public int? CompAuditoraId { get; set; }
        public CompAuditoras CompAuditora { get; set; }

        public bool Active { get; set; }

        public bool Notificaciones { get; set; }

        [StringLength(3)]
        public string Lenguage { get; set; }

        public DateTime? VencimientoCarnet { get; set; }

        public DateTime? AvisoVencimientoCarnet { get; set; }
    }
}
