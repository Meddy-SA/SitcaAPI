using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sitca.Models
{
    public class Pais
    {
        [Key]
        public int Id { get; set; }
        [StringLength(30)]
        public string Name { get; set; }
        [Required]
        public bool Active { get; set; }

        public ICollection<Empresa> Empresas { get; set; }
    }
}
