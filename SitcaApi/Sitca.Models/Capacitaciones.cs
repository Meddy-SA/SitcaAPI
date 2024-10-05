using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Sitca.Models
{
    public class Capacitaciones
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaCarga { get; set; }

        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(250)]
        public string Descripcion { get; set; }

        [StringLength(500)]
        public string Ruta { get; set; }

        [StringLength(10)]
        public string Tipo { get; set; }

        public bool Activo { get; set; }

        [ForeignKey("UsuarioCarga")]
        [StringLength(450)]
        public string UsuarioCargaId { get; set; }
        public ApplicationUser UsuarioCarga { get; set; }
    }
}
