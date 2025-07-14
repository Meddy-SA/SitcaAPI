using System.Collections.Generic;
using System.Threading.Tasks;
using Sitca.Models.DTOs;
using Sitca.Models.DTOs.Dashboard;

namespace Sitca.DataAccess.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly IActivityService _activityService;
        private readonly IRoleDashboardService _roleDashboardService;

        public DashboardService(
            IAdminDashboardService adminDashboardService,
            IActivityService activityService,
            IRoleDashboardService roleDashboardService)
        {
            _adminDashboardService = adminDashboardService;
            _activityService = activityService;
            _roleDashboardService = roleDashboardService;
        }

        public async Task<Result<AdminStatisticsDto>> GetAdminStatisticsAsync(string userId)
        {
            return await _adminDashboardService.GetStatisticsAsync(userId);
        }

        public async Task<Result<List<RecentActivityDto>>> GetRecentActivitiesAsync(string userId, int limit = 10, int offset = 0)
        {
            return await _activityService.GetRecentActivitiesAsync(userId, limit, offset);
        }

        public async Task<Result<SystemStatusDto>> GetSystemStatusAsync()
        {
            return await _adminDashboardService.GetSystemStatusAsync();
        }

        public async Task<Result<AtpStatisticsDto>> GetAtpStatisticsAsync(string userId)
        {
            return await _roleDashboardService.GetAtpStatisticsAsync(userId);
        }

        public async Task<Result<AsesorAuditorStatisticsDto>> GetAsesorAuditorStatisticsAsync(string userId)
        {
            return await _roleDashboardService.GetAsesorAuditorStatisticsAsync(userId);
        }

        public async Task<Result<EmpresaStatisticsDto>> GetEmpresaStatisticsAsync(string userId)
        {
            return await _roleDashboardService.GetEmpresaStatisticsAsync(userId);
        }

        public async Task<Result<ConsultorStatisticsDto>> GetConsultorStatisticsAsync(string userId)
        {
            return await _roleDashboardService.GetConsultorStatisticsAsync(userId);
        }

        public async Task<Result<CtcStatisticsDto>> GetCtcStatisticsAsync(string userId)
        {
            return await _roleDashboardService.GetCtcStatisticsAsync(userId);
        }

        public async Task<Result<EmpresaAuditoraStatisticsDto>> GetEmpresaAuditoraStatisticsAsync(string userId)
        {
            return await _roleDashboardService.GetEmpresaAuditoraStatisticsAsync(userId);
        }

        public async Task<Result<List<RecentActivityDto>>> GetActivitiesByRoleAsync(string userId, string role, int limit, int days)
        {
            return await _activityService.GetActivitiesByRoleAsync(userId, role, limit, days);
        }

        public async Task<Result<List<NotificationDto>>> GetNotificationsByRoleAsync(string userId, string role)
        {
            return await _activityService.GetNotificationsByRoleAsync(userId, role);
        }
    }
}