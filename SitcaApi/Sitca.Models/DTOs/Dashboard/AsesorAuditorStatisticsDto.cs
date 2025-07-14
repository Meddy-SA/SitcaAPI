namespace Sitca.Models.DTOs.Dashboard
{
    public class AsesorAuditorStatisticsDto
    {
        public int ProcesosAsignados { get; set; }
        public int CertificacionesPendientes { get; set; }
        public List<AuditoriaEsteDate> AuditoriasEsteDate { get; set; } = new();
        public EstadisticasPersonalesDto EstadisticasPersonales { get; set; } = new();
    }

    public class AuditoriaEsteDate
    {
        public int EmpresaId { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public DateTime FechaAuditoria { get; set; }
        public string Tipo { get; set; } = string.Empty;
    }

    public class EstadisticasPersonalesDto
    {
        public int CertificacionesCompletadas { get; set; }
        public int PromedioTiempoCertificacion { get; set; }
        public double SatisfaccionClientes { get; set; }
    }
}