using System;
using Sitca.Models;
using System.Collections.Generic;
using System.Text;
using Sitca.Models.ViewModels;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface IEmpresaRepository : IRepository<Empresa>
    {
        Task<ResponseListadoExterno> GetCertificadasParaExterior(ListadoExternoFiltro filtro);

        Task<UrlResult> Delete(int id,int paisId, string role);
        int SaveEmpresa(RegisterVm model);
        List<EmpresaVm> GetList(int? idEstado,int idPais, int idTipologia, string q, string leng );

        List<EmpresaVm> GetListReporte(FiltroEmpresaReporteVm data);
   
        Task<List<EmpresasCalificadas>> EvaluadasEnCtc(int idPais, string language);

        List<EmpresaVm> GetListXVencerReporte(FiltroEmpresaReporteVm data);
        List<EmpresaVm> GetListRenovacionReporte(FiltroEmpresaReporteVm data);        

        List<EmpresaPersonalVm> GetListReportePersonal(FiltroEmpresaReporteVm data);

        EstadisticasVm Estadisticas(string lang);

        Task<List<EstadisticaItemVm>> EnCertificacion(int idPais, string lenguage);

        Task<List<EstadisticaItemVm>> EstadisticasCtc(int idPais, string lang);

        Task<EmpresaUpdateVm>  Data(int empresaId, string userId);

        Task<List<EmpresaVm>> ListForRole(ApplicationUser user, string role);
        bool ActualizarDatos(EmpresaUpdateVm datos, string user, string role);

        Task<bool> SolicitaAuditoria(int idEmpresa);
    }
}
