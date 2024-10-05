using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sitca.Models
{
    public class NotificationGroups
    {
        [Key]
        public int Id { get; set; }
        public int NotificationId { get; set; }

        [StringLength(60)]
        public string RoleId { get; set; }

        public Notificacion Notification { get; set; }
    }
}
