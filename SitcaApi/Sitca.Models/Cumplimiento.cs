using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sitca.Models
{
    public class Cumplimiento
    {
        [Key]
        public int Id { get; set; }
        

        public int PorcentajeMinimo { get; set; }
        public int PorcentajeMaximo { get; set; }

        public int ModuloId { get; set; }
        public Modulo Modulo { get; set; }

        public int DistintivoId { get; set; }
        public Distintivo Distintivo { get; set; }

        public int? TipologiaId { get; set; }
        public Tipologia Tipologia { get; set; }
    }
}
