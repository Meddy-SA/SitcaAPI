using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Sitca.Models;
using Sitca.Models.ViewModels;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface IModuloRepository : IRepository<Modulo>
    {
        List<Modulo> GetList(int? idTipologia);

        Modulo Details(int id);

        Task<bool> Edit(Modulo data);
    }
}
