using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
    public class Cuestionario
    {
        [Key]
        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaGenerado { get; set; }

        public DateTime? FechaVisita { get; set; }

        public DateTime? FechaFinalizado { get; set; }
        public bool Prueba { get; set; }

        #region FK-Tipologia
        public Tipologia Tipologia { get; set; }
        public int IdTipologia { get; set; }

        public int? TipologiaId { get; set; }
        #endregion


        public ICollection<CuestionarioItem> Items { get; set; }

        #region FK-Proceso-Certificacion
        public ProcesoCertificacion Certificacion { get; set; }
        public int? ProcesoCertificacionId { get; set; }
        #endregion



        public string AsesorId { get; set; }
        public string AuditorId { get; set; }

        public int Resultado { get; set; }
    }
}
