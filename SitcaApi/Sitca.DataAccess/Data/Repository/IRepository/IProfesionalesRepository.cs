using System.Threading.Tasks;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Data.Repository.IRepository;

public interface IProfesionalesRepository
{
    Task<Result<ProfesionalesHabilitadosResponseDTO>> GetProfesionalesHabilitadosAsync(
        ProfesionalesHabilitadosFilterDTO filter
    );
}

