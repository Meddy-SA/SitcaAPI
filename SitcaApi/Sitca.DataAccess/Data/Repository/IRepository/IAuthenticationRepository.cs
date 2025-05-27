using System.Collections.Generic;
using System.Threading.Tasks;
using Sitca.Models;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Data.Repository.IRepository;

public interface IAuthenticationRepository
{
    Task<Result<Pais>> GetCountry(int paisId);
    Task<Result<List<Pais>>> GetCountries();
}
