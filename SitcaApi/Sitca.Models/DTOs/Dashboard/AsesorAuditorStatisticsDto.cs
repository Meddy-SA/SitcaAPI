using System;
using System.Collections.Generic;

namespace Sitca.Models.DTOs.Dashboard
{
    public class AsesorAuditorStatisticsDto
    {
        // KPIs principales
        public int TotalEmpresasAsignadas { get; set; }
        public int EmpresasEnProceso { get; set; }
        public int EmpresasCompletadas { get; set; }
        public int AuditoriasProgramadas { get; set; }

        // Información del rol
        public bool IsAuditor { get; set; }
        public bool IsAsesor { get; set; }

        // Lista de empresas asignadas (detalle)
        public List<EmpresaAsignadaAsesorDto> EmpresasAsignadas { get; set; } = new();

        // Próximas auditorías con detalle
        public List<ProximaAuditoriaDto> ProximasAuditorias { get; set; } = new();

        // Distribución por estado
        public List<EstadoDistribucionDto> DistribucionPorEstado { get; set; } = new();

        // Actividades recientes filtradas por el usuario
        public List<RecentActivityDto> ActividadesRecientes { get; set; } = new();

        // Métricas de rendimiento
        public MetricasRendimientoDto Metricas { get; set; } = new();
    }

    public class EmpresaAsignadaAsesorDto
    {
        public int EmpresaId { get; set; }
        public int ProcesoId { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public string Tipologia { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string EstadoProceso { get; set; } = string.Empty;
        public string EstadoNumerico { get; set; } = string.Empty;
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaProximaAuditoria { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string Prioridad { get; set; } = "media";
        public bool Recertificacion { get; set; }
    }

    public class ProximaAuditoriaDto
    {
        public int ProcesoId { get; set; }
        public int EmpresaId { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public string Tipologia { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public DateTime FechaAuditoria { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int DiasRestantes { get; set; }
    }

    public class EstadoDistribucionDto
    {
        public string Estado { get; set; } = string.Empty;
        public string EstadoCorto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class MetricasRendimientoDto
    {
        public int CertificacionesCompletadas { get; set; }
        public int CertificacionesEsteAnio { get; set; }
        public int PromedioTiempoCertificacion { get; set; }
        public int ProcesoMasAntiguo { get; set; }
    }
}
