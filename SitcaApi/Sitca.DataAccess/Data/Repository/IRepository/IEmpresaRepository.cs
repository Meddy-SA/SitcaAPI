using System.Collections.Generic;
using System.Threading.Tasks;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;

namespace Sitca.DataAccess.Data.Repository.IRepository;

public interface IEmpresaRepository : IRepository<Empresa>
{
    Task<int> GetCompanyStatusAsync(int CompanyId);

    Task<ResponseListadoExterno> GetCertificadasParaExterior(ListadoExternoFiltro filtro);

    Task<UrlResult> Delete(int id, int paisId, string role);

    Task<Result<int>> SaveEmpresaAsync(RegisterDTO model);

    Task<List<EmpresaVm>> GetCompanyListAsync(CompanyFilterDTO filter, string language);

    Task<List<EmpresaVm>> GetListReporteAsync(FilterCompanyDTO filtro);

    List<EmpresaVm> GetListReporte(FiltroEmpresaReporteVm data); // WARN: Borrar si funciona GetListReporteAsync

    Task<List<EmpresasCalificadas>> EvaluadasEnCtc(int idPais, string language);

    List<EmpresaVm> GetListXVencerReporte(FiltroEmpresaReporteVm data);

    List<EmpresaVm> GetListRenovacionReporte(FiltroEmpresaReporteVm data);

    Task<Result<List<EmpresaPersonalVm>>> GetListReportePersonalAsync(FiltroEmpresaReporteVm data);

    EstadisticasVm Estadisticas(string lang);

    Task<List<EstadisticaItemVm>> EnCertificacion(int idPais, string lenguage);

    Task<List<EstadisticaItemVm>> EstadisticasCtc(int idPais, string lang);

    Task<EmpresaUpdateVm> Data(int companyId, ApplicationUser user);

    Task<List<EmpresaVm>> ListForRoleAsync(
        ApplicationUser user,
        string role,
        CompanyFilterDTO filter
    );

    Task<bool> ActualizarDatos(EmpresaUpdateVm datos, ApplicationUser user, string role);

    Task<Result<bool>> SolicitaAuditoriaAsync(int idEmpresa);
}
