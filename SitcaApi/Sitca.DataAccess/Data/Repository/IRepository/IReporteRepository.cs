using System.Threading.Tasks;
using Sitca.Models;
using Sitca.Models.ViewModels;

namespace Sitca.DataAccess.Data.Repository.IRepository;

public interface IReporteRepository : IRepository<Cuestionario>
{
    /// <summary>
    /// Comprueba si se puede generar un reporte de certificación
    /// </summary>
    /// <param name="cuestionarioId">ID del cuestionario</param>
    /// <returns>True si el reporte puede ser generado</returns>
    Task<bool> ReporteCertificacion(int cuestionarioId);

    /// <summary>
    /// Genera el reporte completo de certificación para un cuestionario específico
    /// </summary>
    /// <param name="cuestionarioId">ID del cuestionario</param>
    /// <param name="user">Usuario actual</param>
    /// <param name="role">Rol del usuario</param>
    /// <returns>Bytes del archivo PDF generado</returns>
    Task<byte[]> GenerarReporteCertificacion(int cuestionarioId, ApplicationUser user, string role);

    /// <summary>
    /// Procesa los datos del cuestionario y prepara toda la información para generar el reporte
    /// </summary>
    /// <param name="cuestionarioId">ID del cuestionario</param>
    /// <param name="user">Usuario actual</param>
    /// <param name="role">Rol del usuario</param>
    /// <returns>Datos completos procesados para el reporte</returns>
    Task<CuestionarioDetailsVm> PrepararDatosCertificacion(
        int cuestionarioId,
        ApplicationUser user,
        string role
    );
}
