using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface IHomologacionRepository : IRepository<Homologacion>
    {
        Task<int> Create(HomologacionDTO datos, ApplicationUser userId);

        Task<List<HomologacionDTO>> List(int country);

        Task<HomologacionDTO> Details(ApplicationUser appUser, string role, int id);

        Task<bool> Update(ApplicationUser appUser, string role, HomologacionDTO datos);

        Task<Result<HomologacionBloqueoDto>> ToggleBloqueoEdicionAsync(int id);
    }
}
