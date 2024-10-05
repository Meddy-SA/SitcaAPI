using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sitca.Models
{
    public class CompAuditoras
    {
        [Key]
        public int Id { get; set; }

        [StringLength(120)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Direccion { get; set; }



        [StringLength(200)]
        public string Representante { get; set; }

        //numero certificado reconocimiento
        [StringLength(30)]
        public string NumeroCertificado { get; set; }

        public DateTime? FechaInicioConcesion { get; set; }

        public DateTime? FechaFinConcesion { get; set; }

        [StringLength(100)]
        public string Tipo { get; set; }

        [StringLength(120)]
        public string Email { get; set; }

        [StringLength(20)]
        public string Telefono { get; set; }

        public bool Status { get; set; }

        public bool Special { get; set; }

        public int PaisId { get; set; }

        public Pais Pais { get; set; }

    }
}
