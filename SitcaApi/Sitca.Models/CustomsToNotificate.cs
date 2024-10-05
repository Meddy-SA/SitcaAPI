using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Sitca.Models
{
    public class CustomsToNotificate
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("CustomUser")]
        public int CustomId  { get; set; }

        public int NotificacionId { get; set; }

        public Notificacion Notificacion { get; set; }

        public NotificationCustomUsers CustomUser { get; set; }


    }
}
