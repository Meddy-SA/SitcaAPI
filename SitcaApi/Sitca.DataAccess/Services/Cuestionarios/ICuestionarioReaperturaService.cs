using System.Threading.Tasks;
using Sitca.Models;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Services.Cuestionarios;

public interface ICuestionarioReaperturaService
{
  Task<Result<bool>> EjecutarReapertura(int cuestionarioId, ApplicationUser user);
}
