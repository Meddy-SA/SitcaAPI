using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sitca.Models
{
    public class CuestionarioItemHistory
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [StringLength(30)]
        public string Item { get; set; }
        
        [StringLength(20)]
        public string Type { get; set; }
        public int Result { get; set; }

        public int CuestionarioItemId { get; set; }
        public CuestionarioItem CuestionarioItem { get; set; }

    }
}
