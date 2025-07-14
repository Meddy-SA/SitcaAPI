using System.Threading.Tasks;
using Sitca.Models.DTOs;
using Sitca.Models.DTOs.Dashboard;

namespace Sitca.DataAccess.Services.Dashboard
{
    public interface IRoleDashboardService
    {
        Task<Result<AtpStatisticsDto>> GetAtpStatisticsAsync(string userId);
        Task<Result<AsesorAuditorStatisticsDto>> GetAsesorAuditorStatisticsAsync(string userId);
        Task<Result<EmpresaStatisticsDto>> GetEmpresaStatisticsAsync(string userId);
        Task<Result<ConsultorStatisticsDto>> GetConsultorStatisticsAsync(string userId);
        Task<Result<CtcStatisticsDto>> GetCtcStatisticsAsync(string userId);
        Task<Result<EmpresaAuditoraStatisticsDto>> GetEmpresaAuditoraStatisticsAsync(string userId);
    }
}