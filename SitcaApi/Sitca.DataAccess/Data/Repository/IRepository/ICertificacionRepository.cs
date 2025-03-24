using System.Collections.Generic;
using System.Threading.Tasks;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface ICertificacionRepository : IRepository<ProcesoCertificacion>
    {
        Task<Result<bool>> ReAbrirCuestionario(ApplicationUser user, int cuestionarioId);
        Task<Result<bool>> ConvertirARecertificacionAsync(ApplicationUser user, EmpresaVm data);
        Task<Result<int>> NuevaRecertificacion(int empresaId, ApplicationUser user);
        Task<List<ObservacionesDTO>> GetListObservaciones(IEnumerable<int> ItemIds);
        Task<RegistroHallazgos> ReporteHallazgos(
            int CuestionarioId,
            ApplicationUser user,
            string role
        );
        Task<ObservacionesDTO> GetObservaciones(int idRespuesta);
        Task<Result<string>> SaveObservaciones(ApplicationUser user, ObservacionesDTO data);
        Task<Result<int>> ComenzarProcesoAsync(CertificacionVm data, string userGenerador);
        Task<CuestionarioDetailsVm> GetCuestionario(int id, ApplicationUser user, string role);
        Task<CuestionarioNoCumpleVm> GetNoCumplimientos(int id, ApplicationUser user, string role);
        Task<Result<CuestionarioDetailsMinVm>> GenerarCuestionarioAsync(
            CuestionarioCreateVm data,
            string userGenerador,
            string role
        );
        Task<int> SavePregunta(CuestionarioItemVm obj, ApplicationUser appUser, string role);
        Task<Result<int>> FinCuestionario(int idCuestionario, ApplicationUser appUser, string role);
        Task<Result<bool>> CanFinalizeCuestionario(int idCuestionario, string role);
        Task<List<CuestionarioDetailsMinVm>> GetCuestionariosList(
            int idEmpresa,
            ApplicationUser appUser
        );
        Task<bool> ChangeStatus(CertificacionStatusVm data, int status);
        Task<bool> SaveResultadoSugerido(int idCuestionario, ApplicationUser appUser, string role);
        Task<bool> IsCuestionarioCompleto(CuestionarioDetailsVm data);
        Task<Result<bool>> CambiarAuditorAsync(CambioAuditor data);
        Task<Result<List<HistorialVm>>> GetHistory(int idCuestionario);
        Task<List<CommonVm>> GetStatusList(string lang);
        Task<Result<bool>> SaveCalificacion(
            SaveCalificacionVm data,
            ApplicationUser appUser,
            string role
        );
        Task<List<CommonVm>> GetDistintivos(string lang);
    }
}
