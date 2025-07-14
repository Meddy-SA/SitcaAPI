using System;
using System.Collections.Generic;

namespace Sitca.Models.DTOs.Dashboard
{
    public class CtcStatisticsDto
    {
        public List<EvaluacionPendienteDto> EvaluacionesPendientes { get; set; }
        public List<CertificacionRecienteDto> CertificacionesRecientes { get; set; }
        public EstadisticasRegionalesDto EstadisticasRegionales { get; set; }
        public List<AlertaImportanteDto> AlertasImportantes { get; set; }
        public List<TendenciaCertificacionDto> TendenciasCertificacion { get; set; }
    }

    public class EvaluacionPendienteDto
    {
        public int Id { get; set; }
        public string NombreEmpresa { get; set; }
        public string Pais { get; set; }
        public string TipoEvaluacion { get; set; }
        public string Estado { get; set; }
        public DateTime FechaEnvio { get; set; }
        public string Evaluador { get; set; }
        public string Nivel { get; set; }
        public string Prioridad { get; set; }
    }

    public class CertificacionRecienteDto
    {
        public int Id { get; set; }
        public string NombreEmpresa { get; set; }
        public string Pais { get; set; }
        public string Nivel { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string NumeroCertificado { get; set; }
        public string Estado { get; set; }
    }

    public class EstadisticasRegionalesDto
    {
        public int TotalCertificaciones { get; set; }
        public Dictionary<string, int> CertificacionesPorPais { get; set; }
        public Dictionary<string, int> CertificacionesPorNivel { get; set; }
        public int EvaluacionesPendientes { get; set; }
        public int CertificacionesPorVencer { get; set; }
        public int NuevasCertificacionesMes { get; set; }
        public decimal TasaAprobacion { get; set; }
    }

    public class AlertaImportanteDto
    {
        public string Tipo { get; set; }
        public string Mensaje { get; set; }
        public string Prioridad { get; set; }
    }

    public class TendenciaCertificacionDto
    {
        public string Mes { get; set; }
        public int Nuevas { get; set; }
        public int Renovadas { get; set; }
        public int Suspendidas { get; set; }
    }
}