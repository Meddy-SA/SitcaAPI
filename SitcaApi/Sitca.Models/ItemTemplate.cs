using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sitca.Models
{
    public class ItemTemplate
    {
        [Key]
        public int Id { get; set; }

        [StringLength(30)]
        public string BarCode { get; set; }

        [StringLength(250)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }
}
