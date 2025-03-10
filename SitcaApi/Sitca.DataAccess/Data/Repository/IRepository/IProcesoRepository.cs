using System.Threading.Tasks;
using Sitca.Models;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Data.Repository.IRepository;

public interface IProcesoRepository : IRepository<ProcesoCertificacion>
{
    Task<Result<ProcesoCertificacionDTO>> GetProcesoForIdAsync(int id, string userId);
}
