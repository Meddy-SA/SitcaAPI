using System.Collections.Generic;
using System.Threading.Tasks;
using Sitca.Models.DTOs;
using Sitca.Models.DTOs.Dashboard;

namespace Sitca.DataAccess.Services.Dashboard
{
    public interface IDashboardService
    {
        Task<Result<AdminStatisticsDto>> GetAdminStatisticsAsync(string userId);
        Task<Result<List<RecentActivityDto>>> GetRecentActivitiesAsync(string userId, int limit = 10, int offset = 0);
        Task<Result<SystemStatusDto>> GetSystemStatusAsync();
        Task<Result<AtpStatisticsDto>> GetAtpStatisticsAsync(string userId);
        Task<Result<AsesorAuditorStatisticsDto>> GetAsesorAuditorStatisticsAsync(string userId);
        Task<Result<EmpresaStatisticsDto>> GetEmpresaStatisticsAsync(string userId);
        Task<Result<ConsultorStatisticsDto>> GetConsultorStatisticsAsync(string userId);
        Task<Result<CtcStatisticsDto>> GetCtcStatisticsAsync(string userId);
        Task<Result<EmpresaAuditoraStatisticsDto>> GetEmpresaAuditoraStatisticsAsync(string userId);
        Task<Result<List<RecentActivityDto>>> GetActivitiesByRoleAsync(string userId, string role, int limit, int days);
        Task<Result<List<NotificationDto>>> GetNotificationsByRoleAsync(string userId, string role);
    }
}