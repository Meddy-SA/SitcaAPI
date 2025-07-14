using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sitca.Models.DTOs;
using Sitca.Models.DTOs.Dashboard;

namespace Sitca.DataAccess.Services.Dashboard
{
    public interface IActivityService
    {
        Task<Result<List<RecentActivityDto>>> GetRecentActivitiesAsync(string userId, int limit = 10, int offset = 0);
        Task<Result<List<RecentActivityDto>>> GetActivitiesByTypeAsync(string activityType, int limit = 10);
        Task<Result<List<RecentActivityDto>>> GetActivitiesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Result<List<RecentActivityDto>>> GetActivitiesByRoleAsync(string userId, string role, int limit, int days);
        Task<Result<List<NotificationDto>>> GetNotificationsByRoleAsync(string userId, string role);
    }
}