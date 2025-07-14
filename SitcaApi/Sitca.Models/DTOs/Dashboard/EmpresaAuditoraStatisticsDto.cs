using System;
using System.Collections.Generic;

namespace Sitca.Models.DTOs.Dashboard
{
    public class EmpresaAuditoraStatisticsDto
    {
        public List<AuditorActivoDto> AuditoresActivos { get; set; }
        public List<AuditoriaProgramadaDto> AuditoriasProgamadas { get; set; }
        public List<AuditoriaCompletadaDto> AuditoriasCompletadas { get; set; }
        public EstadisticasEmpresaAuditoraDto Estadisticas { get; set; }
        public List<AlertaGestionDto> AlertasGestion { get; set; }
        public List<DistribucionTrabajoDto> DistribucionTrabajo { get; set; }
    }

    public class AuditorActivoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public List<string> Especialidades { get; set; }
        public List<string> Paises { get; set; }
        public bool CertificacionVigente { get; set; }
        public DateTime? FechaVencimientoCertificacion { get; set; }
        public int AuditoriasPendientes { get; set; }
        public decimal CalificacionPromedio { get; set; }
        public string EstadoDisponibilidad { get; set; }
    }

    public class AuditoriaProgramadaDto
    {
        public int Id { get; set; }
        public string Empresa { get; set; }
        public string Auditor { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }
        public string Nivel { get; set; }
        public int Duracion { get; set; }
        public string Estado { get; set; }
        public string Pais { get; set; }
    }

    public class AuditoriaCompletadaDto
    {
        public int Id { get; set; }
        public string Empresa { get; set; }
        public string Auditor { get; set; }
        public DateTime FechaCompletada { get; set; }
        public string Resultado { get; set; }
        public int Puntuacion { get; set; }
        public string Nivel { get; set; }
        public bool InformeEntregado { get; set; }
    }

    public class EstadisticasEmpresaAuditoraDto
    {
        public int TotalAuditores { get; set; }
        public int AuditoresActivos { get; set; }
        public int AuditoriasEsteDate { get; set; }
        public int AuditoriasCompletadasMes { get; set; }
        public decimal TasaAprobacion { get; set; }
        public int TiempoPromedioAuditoria { get; set; }
        public int CertificacionesVigentes { get; set; }
        public decimal IngresosMes { get; set; }
    }

    public class AlertaGestionDto
    {
        public string Tipo { get; set; }
        public string Mensaje { get; set; }
        public string Prioridad { get; set; }
    }

    public class DistribucionTrabajoDto
    {
        public string Auditor { get; set; }
        public int AuditoriasPendientes { get; set; }
        public int AuditoriasMes { get; set; }
        public string Disponibilidad { get; set; }
    }
}