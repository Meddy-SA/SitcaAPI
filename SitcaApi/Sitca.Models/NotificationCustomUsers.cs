using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sitca.Models
{
    public class NotificationCustomUsers
    {
        [Key]
        public int Id { get; set; }

        public bool Global { get; set; }

        [StringLength(50)]
        public string Email { get; set; }

        [StringLength(150)]
        public string Name { get; set; }
        public int PaisId { get; set; }
    }
}
