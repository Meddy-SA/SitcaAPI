using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Sitca.Models;
using Sitca.Models.ViewModels;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface IUsersRepository: IRepository<ApplicationUser>
    {
        Task<bool> SetLanguageAsync(string lang, string user);
        Task<List<UsersListVm>> GetUsersAsync(string query,int pais, string role);

        Task<UsersListVm> GetUserById(string id);

        Task<List<UsersListVm>> GetPersonal(int pais, int EmpresaAuditoraId);
    }
}
