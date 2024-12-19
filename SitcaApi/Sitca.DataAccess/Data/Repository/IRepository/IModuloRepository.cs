using System.Collections.Generic;
using System.Threading.Tasks;
using Sitca.Models;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface IModuloRepository : IRepository<Modulo>
    {
        List<Modulo> GetList(int? idTipologia);

        Modulo Details(int id);

        Task<bool> Edit(Modulo data);
    }
}
