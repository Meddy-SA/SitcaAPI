using Sitca.Models;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface ICapacitacionesRepository : IRepository<Capacitaciones>
    {
        Task<bool> DeleteFile(int id);

        Task<bool> SaveCapacitacion(Capacitaciones data);
    }
}
