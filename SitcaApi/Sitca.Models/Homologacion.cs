using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Sitca.Models
{
    public class Homologacion
    {
        [Key]
        public int Id { get; set; }

        [StringLength(70)]
        public string Distintivo { get; set; }

        [StringLength(70)]
        public string DistintivoExterno { get; set; }

        public bool? EnProcesoSiccs { get; set; }

        [StringLength(1000)]
        public string DatosProceso { get; set; }

        public DateTime? FechaCreacion { get; set; }

        public DateTime? FechaUltimaEdicion { get; set; }

        public DateTime FechaOtorgamiento { get; set; }

        public DateTime FechaVencimiento { get; set; }

        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; }

        public ProcesoCertificacion Certificacion { get; set; }

        [ForeignKey("Certificacion")]
        public int CertificacionId { get; set; }
    }
}
