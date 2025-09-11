namespace Sitca.Models.DTOs.Dashboard
{
    public class ConsultorStatisticsDto
    {
        public List<ProyectoActivoDto> ProyectosActivos { get; set; } = [];
        public List<ReunionProgramadaDto> ReunionesProgramadas { get; set; } = [];
        public List<InformePendienteDto> InformesPendientes { get; set; } = [];
        public EstadisticasConsultorDto Estadisticas { get; set; } = default!;
    }

    public class ProyectoActivoDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = null!;
        public string Cliente { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public string Prioridad { get; set; } = null!;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int PorcentajeCompletado { get; set; }
        public decimal Presupuesto { get; set; }
        public string ProximoHito { get; set; } = null!;
        public DateTime? FechaProximoHito { get; set; }
    }

    public class ReunionProgramadaDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = null!;
        public string Cliente { get; set; } = null!;
        public int ProyectoId { get; set; }
        public DateTime FechaHora { get; set; }
        public int Duracion { get; set; }
        public string Ubicacion { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public List<string> Agenda { get; set; } = [];
    }

    public class InformePendienteDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = null!;
        public int ProyectoId { get; set; }
        public string Cliente { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public DateTime FechaVencimiento { get; set; }
        public int PorcentajeCompletado { get; set; }
    }

    public class EstadisticasConsultorDto
    {
        public int TotalProyectos { get; set; }
        public int ConsultoriasActivas { get; set; }
        public int ProyectosCompletados { get; set; }
        public int InformesPendientes { get; set; }
        public decimal FacturacionMensual { get; set; }
        public int HorasFacturadas { get; set; }
        public decimal ClientesSatisfechos { get; set; }
    }
}

