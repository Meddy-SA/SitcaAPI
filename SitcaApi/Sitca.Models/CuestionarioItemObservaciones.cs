using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sitca.Models
{
    public class CuestionarioItemObservaciones
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime Date { get; set; }

        [StringLength(1000)]
        public string Observaciones { get; set; }

        public int CuestionarioItemId { get; set; }
        public CuestionarioItem CuestionarioItem { get; set; }

        public string UsuarioCargaId { get; set; }
    }
}
