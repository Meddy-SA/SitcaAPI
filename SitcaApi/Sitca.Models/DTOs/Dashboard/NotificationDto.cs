using System;
using System.Collections.Generic;

namespace Sitca.Models.DTOs.Dashboard
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public string Prioridad { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Leida { get; set; }
        public List<string> AccionesDisponibles { get; set; }
    }
}