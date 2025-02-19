using System.Collections.Generic;
using System.Threading.Tasks;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface IUsersRepository : IRepository<ApplicationUser>
    {
        Task<bool> SetLanguageAsync(string lang, string user);

        Task<Result<List<UsersListVm>>> GetUsersAsync(string query, int paisId, string role);

        Task<UsersListVm> GetUserById(string id);

        Task<Result<List<CompAuditoraListVm>>> GetCompaniesAsync(int paisId);

        Task<Result<List<UsersListVm>>> GetPersonalAsync(int paisId, int empresaAuditoraId);

        Task<Result<CompAuditoras>> AssignUserToCompanyAsync(int companyId, string[] userIds);

        Task<Result<bool>> UnassignUsersFromCompanyAsync(int companyId, string[] userIds);
    }
}
