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

    Task<List<UsersListVm>> GetPersonal(int paisId, int empresaAuditoraId);
  }
}
