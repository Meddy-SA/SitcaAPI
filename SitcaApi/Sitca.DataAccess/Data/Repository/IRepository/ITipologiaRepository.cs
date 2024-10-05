using Sitca.Models;
using Sitca.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface ITipologiaRepository : IRepository<Tipologia>
    {
        Task<List<CommonVm>> SelectList(string lang);
    }
}
