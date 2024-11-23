using Sitca.Models;
using System.Collections.Generic;
using Sitca.Models.ViewModels;
using System.Threading.Tasks;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Data.Repository.IRepository;
public interface IEmpresaRepository : IRepository<Empresa>
{
  Task<int> GetCompanyStatusAsync(int CompanyId);

  Task<ResponseListadoExterno> GetCertificadasParaExterior(ListadoExternoFiltro filtro);

  Task<UrlResult> Delete(int id, int paisId, string role);

  int SaveEmpresa(RegisterVm model);

  Task<List<EmpresaVm>> GetCompanyListAsync(CompanyFilterDTO filter, string language);

  List<EmpresaVm> GetListReporte(FiltroEmpresaReporteVm data);

  Task<List<EmpresasCalificadas>> EvaluadasEnCtc(int idPais, string language);

  List<EmpresaVm> GetListXVencerReporte(FiltroEmpresaReporteVm data);

  List<EmpresaVm> GetListRenovacionReporte(FiltroEmpresaReporteVm data);

  List<EmpresaPersonalVm> GetListReportePersonal(FiltroEmpresaReporteVm data);

  EstadisticasVm Estadisticas(string lang);

  Task<List<EstadisticaItemVm>> EnCertificacion(int idPais, string lenguage);

  Task<List<EstadisticaItemVm>> EstadisticasCtc(int idPais, string lang);

  Task<EmpresaUpdateVm> Data(int companyId, ApplicationUser user);

  Task<List<EmpresaVm>> ListForRoleAsync(ApplicationUser user, string role, CompanyFilterDTO filter);

  Task<bool> ActualizarDatos(EmpresaUpdateVm datos, ApplicationUser user, string role);

  Task<Result<bool>> SolicitaAuditoriaAsync(int idEmpresa);
}
