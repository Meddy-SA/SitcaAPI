using Sitca.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface ICompañiasAuditorasRepository : IRepository<CompAuditoras>
    {
        Task<bool> Save(CompAuditoras data);
    }
}
