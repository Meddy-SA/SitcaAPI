using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Sitca.Models
{
    public class ProcesoCertificacion
    {
        [Key]
        public int Id { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }

        public DateTime? FechaSolicitudAuditoria { get; set; }

        public DateTime? FechaFijadaAuditoria { get; set; }

        public bool Recertificacion { get; set; }

        [StringLength(40)]
        public string NumeroExpediente { get; set; }

        [StringLength(30)]
        public string Status { get; set; }

        [ForeignKey("AsesorProceso")]
        [StringLength(450)]
        public string AsesorId { get; set; }
        public ApplicationUser AsesorProceso { get; set; }


        public int? TipologiaId { get; set; }
        public Tipologia Tipologia { get; set; }



        [ForeignKey("AuditorProceso")]
        [StringLength(450)]
        public string AuditorId { get; set; }
        public ApplicationUser AuditorProceso { get; set; }        

       
        public int EmpresaId { get; set; }        
        public Empresa Empresa { get; set; }


        [ForeignKey("UserGenerador")]
        [StringLength(450)]
        public string UserGeneraId { get; set; }
        public ApplicationUser UserGenerador { get; set; }


        //[ForeignKey("Resultado")]
        //public int? ResultadoId { get; set; }
        //public ResultadoCertificacion Resultado { get; set; }

        public ICollection<ResultadoCertificacion> Resultados { get; set; }

        public DateTime? FechaVencimiento { get; set; }

    }
}
