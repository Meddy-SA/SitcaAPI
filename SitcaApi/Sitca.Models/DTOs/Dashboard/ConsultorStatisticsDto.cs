using System;
using System.Collections.Generic;

namespace Sitca.Models.DTOs.Dashboard
{
    public class ConsultorStatisticsDto
    {
        public List<ProyectoActivoDto> ProyectosActivos { get; set; }
        public List<ReunionProgramadaDto> ReunionesProgramadas { get; set; }
        public List<InformePendienteDto> InformesPendientes { get; set; }
        public EstadisticasConsultorDto Estadisticas { get; set; }
    }

    public class ProyectoActivoDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Cliente { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public string Tipo { get; set; }
        public string Prioridad { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int PorcentajeCompletado { get; set; }
        public decimal Presupuesto { get; set; }
        public string ProximoHito { get; set; }
        public DateTime? FechaProximoHito { get; set; }
    }

    public class ReunionProgramadaDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Cliente { get; set; }
        public int ProyectoId { get; set; }
        public DateTime FechaHora { get; set; }
        public int Duracion { get; set; }
        public string Ubicacion { get; set; }
        public string Tipo { get; set; }
        public string Estado { get; set; }
        public List<string> Agenda { get; set; }
    }

    public class InformePendienteDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public int ProyectoId { get; set; }
        public string Cliente { get; set; }
        public string Tipo { get; set; }
        public string Estado { get; set; }
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