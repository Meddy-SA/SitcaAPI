namespace Sitca.Models.DTOs.Dashboard
{
    public class AtpStatisticsDto
    {
        public int TareasAsignadas { get; set; }
        public int TareasCompletadas { get; set; }
        public int TareasPendientes { get; set; }
        public List<EmpresaAsignadaDto> EmpresasAsignadas { get; set; } = new();
        public List<ActividadRecienteDto> ActividadesRecientes { get; set; } = new();
    }

    public class EmpresaAsignadaDto
    {
        public int EmpresaId { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public string EstadoProceso { get; set; } = string.Empty;
        public DateTime FechaAsignacion { get; set; }
        public string Prioridad { get; set; } = string.Empty;
    }

    public class ActividadRecienteDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}