using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sitca.Models
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string User { get; set; }

        [StringLength(300)]
        public string Observaciones { get; set; }
    }
}
