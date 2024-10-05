using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sitca.Models
{
    public class Distintivo
    {
        [Key]
        public int Id { get; set; }

        [StringLength(40)]
        public string Name { get; set; }

        [StringLength(40)]
        public string NameEnglish { get; set; }

        public int? Importancia { get; set; }

        [StringLength(60)]
        public string File { get; set; }
        public bool Activo { get; set; }
    }
}
