using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sitca.Models
{
    public class BackupCuestionario
    {
        [Key]
        public int Id { get; set; }

        public int CuestionarioId { get; set; }

        public string CuestionarioCompleto { get; set; }
    }
}
