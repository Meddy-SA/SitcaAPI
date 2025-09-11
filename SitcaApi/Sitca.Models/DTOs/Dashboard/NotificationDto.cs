namespace Sitca.Models.DTOs.Dashboard
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = null!;
        public string Titulo { get; set; } = null!;
        public string Mensaje { get; set; } = null!;
        public string Prioridad { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }
        public bool Leida { get; set; }
        public List<string> AccionesDisponibles { get; set; } = [];
    }
}

