using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sitca.Models
{
    public class SubtituloSeccion
    {
        [Key]
        public int Id { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(100)]
        public string NameEnglish { get; set; }

        [StringLength(5)]
        public string Orden { get; set; }
        [StringLength(10)]
        public string Nomenclatura { get; set; }
        public int SeccionModuloId { get; set; }
        public SeccionModulo SeccionModulo { get; set; }
        
    }
}
