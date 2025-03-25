using System.Linq;
using System.Threading.Tasks;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;

namespace Sitca.DataAccess.Services.ProcessQuery;

public interface IProcessQueryBuilder
{
    IQueryable<ProcesoCertificacion> BuildBaseQuery(bool isRecertification);
    IQueryable<ProcesoCertificacion> BuildRoleBasedQuery(
        ApplicationUser user,
        string role,
        bool isRecertification
    );
    IQueryable<ProcesoCertificacion> ApplyFilters(
        IQueryable<ProcesoCertificacion> query,
        CompanyFilterDTO filter,
        int? distintiveId = null
    );
    Task<BlockResult<ProcesoCertificacionVm>> BuildAndExecuteBlockProjection(
        IQueryable<ProcesoCertificacion> query,
        string language,
        int blockNumber,
        int blockSize
    );
    IQueryable<ProcesoCertificacion> BuildUnifiedQuery(
        ApplicationUser user,
        string role,
        bool isRecertification
    );
}
