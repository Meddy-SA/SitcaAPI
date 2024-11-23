using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
  public interface ICertificacionRepository : IRepository<ProcesoCertificacion>
  {
    Task<Result<bool>> ReAbrirCuestionario(ApplicationUser user, int cuestionarioId);
    Task<bool> ConvertirARecertificacion(ApplicationUser user, EmpresaVm data);
    Task<List<ObservacionesDTO>> GetListObservaciones(IEnumerable<int> ItemIds);
    Task<RegistroHallazgos> ReporteHallazgos(int CuestionarioId, ApplicationUser user, string role);
    Task<ObservacionesDTO> GetObservaciones(int idRespuesta);
    Task<Result<string>> SaveObservaciones(ApplicationUser user, ObservacionesDTO data);
    Task<int> ComenzarProceso(CertificacionVm data, string userGenerador);
    Task<CuestionarioDetailsVm> GetCuestionario(int id, ApplicationUser user, string role);
    Task<CuestionarioNoCumpleVm> GetNoCumplimientos(int id, ApplicationUser user, string role);
    Task<CuestionarioDetailsMinVm> GenerarCuestionario(CuestionarioCreateVm data, string userGenerador, string role);
    Task<int> SavePregunta(CuestionarioItemVm obj, ApplicationUser appUser, string role);
    Task<Result<int>> FinCuestionario(int idCuestionario, ApplicationUser appUser, string role);
    Task<Result<bool>> CanFinalizeCuestionario(int idCuestionario, string role);
    Task<List<CuestionarioDetailsMinVm>> GetCuestionariosList(int idEmpresa, ApplicationUser appUser);
    Task<int> AsignaAuditor(AsignaAuditoriaVm data, ApplicationUser appUser, string role);
    Task<bool> ChangeStatus(CertificacionStatusVm data, int status);
    Task<bool> SaveResultadoSugerido(int idCuestionario, ApplicationUser appUser, string role);
    Task<bool> IsCuestionarioCompleto(CuestionarioDetailsVm data);
    Task<bool> CambiarAuditor(CambioAuditor data);
    Task<bool> UpdateNumeroExp(CertificacionDetailsVm data);
    Task<List<HistorialVm>> GetHistory(int idCuestionario);
    Task<List<CommonVm>> GetStatusList(string lang);
    Task<bool> SaveCalificacion(SaveCalificacionVm data, ApplicationUser appUser, string role);
    Task<List<CommonVm>> GetDistintivos(string lang);
  }
}
