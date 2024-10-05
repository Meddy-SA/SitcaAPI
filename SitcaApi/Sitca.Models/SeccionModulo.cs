using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sitca.Models
{
    public class SeccionModulo
    {
        [Key]
        public int Id { get; set; }
        [StringLength(500)]
        public string Name { get; set; }
        [StringLength(500)]
        public string NameEnglish { get; set; }

        [StringLength(5)]
        public string Orden { get; set; }
        [StringLength(10)]
        public string Nomenclatura { get; set; }
        public int ModuloId { get; set; }
        public Modulo Modulo { get; set; }


        public int? TipologiaId { get; set; }
        public Tipologia Tipologia { get; set; }

        public ICollection<SubtituloSeccion> SubtituloSeccion { get; set; }

    }
}
