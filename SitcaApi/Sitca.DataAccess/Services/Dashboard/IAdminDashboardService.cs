using System.Threading.Tasks;
using Sitca.Models.DTOs;
using Sitca.Models.DTOs.Dashboard;

namespace Sitca.DataAccess.Services.Dashboard
{
    public interface IAdminDashboardService
    {
        Task<Result<AdminStatisticsDto>> GetStatisticsAsync(string userId);
        Task<Result<SystemStatusDto>> GetSystemStatusAsync();
    }
}