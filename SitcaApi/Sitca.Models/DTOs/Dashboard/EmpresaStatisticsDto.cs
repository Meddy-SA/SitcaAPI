using System;
using System.Collections.Generic;

namespace Sitca.Models.DTOs.Dashboard
{
    public class EmpresaStatisticsDto
    {
        public CertificacionActualDto CertificacionActual { get; set; }
        public List<TareaPendienteDto> TareasPendientes { get; set; }
        public List<DocumentoRecienteDto> DocumentosRecientes { get; set; }
        public List<ProximaActividadDto> ProximasActividades { get; set; }
        public EstadisticasGeneralesDto EstadisticasGenerales { get; set; }
    }

    public class CertificacionActualDto
    {
        public int Id { get; set; }
        public string Nivel { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string NumeroCertificado { get; set; }
        public int Puntuacion { get; set; }
        public int DiasVigencia { get; set; }
    }

    public class TareaPendienteDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; }
        public string Prioridad { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string AsignadoPor { get; set; }
        public int Progreso { get; set; }
        public List<string> Requisitos { get; set; }
    }

    public class DocumentoRecienteDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Tipo { get; set; }
        public DateTime FechaSubida { get; set; }
        public string Estado { get; set; }
        public string Url { get; set; }
    }

    public class ProximaActividadDto
    {
        public string Tipo { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public string Auditor { get; set; }
    }

    public class EstadisticasGeneralesDto
    {
        public int ProcesoCompletado { get; set; }
        public int DocumentosAprobados { get; set; }
        public int CapacitacionesCompletadas { get; set; }
        public DateTime? ProximaEvaluacion { get; set; }
    }
}