using System.Threading.Tasks;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;

namespace Sitca.DataAccess.Data.Repository.IRepository;

public interface IProcesoRepository : IRepository<ProcesoCertificacion>
{
    Task<Result<ProcesoCertificacionDTO>> GetProcesoForIdAsync(int id, string userId);

    Task<Result<ExpedienteDTO>> UpdateCaseNumberAsync(ExpedienteDTO expediente, string userId);

    Task<BlockResult<ProcesoCertificacionVm>> GetProcessesBlockAsync(
        ApplicationUser user,
        string role,
        CompanyFilterDTO filter,
        string language = "es"
    );

    Task<Result<ProcesoCertificacionDTO>> CrearRecertificacionAsync(int empresaId, string userId);
}
