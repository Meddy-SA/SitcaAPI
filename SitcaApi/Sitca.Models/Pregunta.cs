using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Sitca.Models
{
    public class Pregunta
    {
        [Key]
        public int Id { get; set; }

        [StringLength(2500)]
        public string Texto { get; set; }
        [StringLength(2500)]
        public string Text { get; set; }
        public bool NoAplica { get; set; }
        public bool Obligatoria { get; set; }
        public bool Status { get; set; }

        [StringLength(20)]
        public string Nomenclatura { get; set; }
        [StringLength(5)]
        public string Orden { get; set; }

        public int? TipologiaId { get; set; }
        public Tipologia Tipologia { get; set; }

        public int ModuloId { get; set; }
        public Modulo Modulo { get; set; }

        public int? SeccionModuloId { get; set; }
        public SeccionModulo SeccionModulo { get; set; }

        public int? SubtituloSeccionId { get; set; }
        public SubtituloSeccion SubtituloSeccion { get; set; }

    }
}
