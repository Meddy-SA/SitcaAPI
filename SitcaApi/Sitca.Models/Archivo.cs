using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Sitca.Models
{
    public class Archivo
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime FechaCarga { get; set; }

        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(50)]
        public string Ruta { get; set; }

        [StringLength(10)]
        public string Tipo { get; set; }

        [NotMapped]
        public string Base64Str { get; set; }

        public int? CuestionarioItemId { get; set; }
        public CuestionarioItem CuestionarioItem { get; set; }


        public int? EmpresaId { get; set; }
        public Empresa Empresa { get; set; }


        [ForeignKey("UsuarioCarga")]
        [StringLength(450)]
        public string UsuarioCargaId { get; set; }
        public ApplicationUser UsuarioCarga { get; set; }


        [ForeignKey("Usuario")]
        [StringLength(450)]
        public string UsuarioId { get; set; }
        public ApplicationUser Usuario { get; set; }


        public bool Activo { get; set; }
    }
}
