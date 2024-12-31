using Sitca.Models.DTOs;
using Sitca.Models;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface ICompañiasAuditorasRepository : IRepository<CompAuditoras>
    {
        Task<Result<CompAuditoras>> SaveAsync(CompAuditoras data);
    }
}
