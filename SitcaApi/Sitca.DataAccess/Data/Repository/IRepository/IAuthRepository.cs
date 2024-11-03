using System.Threading.Tasks;
using Sitca.Models;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Data.Repository.IRepository;

public interface IAuthRepository
{
  Task<AuthResult> LoginAsync(LoginDTO login);
  Task<AuthResult> RegisterAsync(RegisterDTO register);
  Task<AuthResult> RenewTokenAsync(string userId);
}
