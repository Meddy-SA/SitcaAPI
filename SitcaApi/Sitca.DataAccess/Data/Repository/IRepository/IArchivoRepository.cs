using Sitca.Models;
using Sitca.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface IArchivoRepository : IRepository<Archivo>
    {
        Task<bool> SaveFileData(Archivo data);

        

        Task<List<Archivo>> GetList(ArchivoFilterVm data, ApplicationUser user, string role);

        Task<bool> DeleteFile(int data, ApplicationUser user, string role);
    }
}
