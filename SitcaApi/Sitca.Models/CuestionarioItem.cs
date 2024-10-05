using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
    public class CuestionarioItem
    {
        [Key]
        public int Id { get; set; }

        [StringLength(2500)]
        public string Texto { get; set; }
        [StringLength(30)]
        public string Nomenclatura { get; set; }
        
        public bool ResultadoAuditor { get; set; } //si, no, no Aplica
        public int Resultado { get; set; }

        public bool Obligatorio{ get; set; }

        public int CuestionarioId { get; set; }
        public Cuestionario Cuestionario { get; set; }

        public int PreguntaId { get; set; }
        public Pregunta Pregunta { get; set; }
        public DateTime? FechaActualizado { get; set; }

        public IEnumerable<Archivo> Archivos { get; set; }

        public IEnumerable<CuestionarioItemObservaciones> CuestionarioItemObservaciones { get; set; }
    }
}
