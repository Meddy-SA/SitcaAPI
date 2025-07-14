using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Services.Dashboard;
using Sitca.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.DTOs.Dashboard;
using Utilities.Common;

namespace Sitca.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            UserManager<ApplicationUser> userManager,
            ILogger<DashboardController> logger
        )
        {
            _dashboardService = dashboardService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Get admin statistics for dashboard
        /// </summary>
        /// <returns>Admin statistics including companies by country and typology</returns>
        [HttpGet("admin-statistics")]
        [ProducesResponseType(typeof(AdminStatisticsDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<Result<AdminStatisticsDto>>> GetAdminStatistics()
        {
            _logger.LogInformation("Getting admin statistics");

            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _dashboardService.GetAdminStatisticsAsync(appUser.Id);
            return this.HandleResponse(result);
        }

        /// <summary>
        /// Get recent activities across the system
        /// </summary>
        /// <param name="limit">Maximum number of activities to return (default: 10)</param>
        /// <param name="offset">Number of activities to skip for pagination (default: 0)</param>
        /// <returns>List of recent activities</returns>
        [HttpGet("recent-activities")]
        [ProducesResponseType(typeof(List<RecentActivityDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<Result<List<RecentActivityDto>>>> GetRecentActivities(
            [FromQuery] int limit = 10,
            [FromQuery] int offset = 0
        )
        {
            _logger.LogInformation(
                "Getting recent activities with limit: {Limit}, offset: {Offset}",
                limit,
                offset
            );

            if (limit <= 0 || limit > 100)
            {
                return this.HandleResponse(
                    Result<List<RecentActivityDto>>.Failure("Límite debe estar entre 1 y 100")
                );
            }

            if (offset < 0)
            {
                return this.HandleResponse(
                    Result<List<RecentActivityDto>>.Failure("Offset no puede ser negativo")
                );
            }

            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _dashboardService.GetRecentActivitiesAsync(
                appUser.Id,
                limit,
                offset
            );
            return this.HandleResponse(result);
        }

        /// <summary>
        /// Get system status information
        /// </summary>
        /// <returns>System health and status metrics</returns>
        [HttpGet("system-status")]
        [Authorize(Roles = $"{Constants.Roles.Admin},{Constants.Roles.TecnicoPais}")]
        [ProducesResponseType(typeof(SystemStatusDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<Result<SystemStatusDto>>> GetSystemStatus()
        {
            _logger.LogInformation("Getting system status");

            var result = await _dashboardService.GetSystemStatusAsync();
            return this.HandleResponse(result);
        }

        /// <summary>
        /// Get ATP (Technical Support) specific statistics
        /// </summary>
        /// <returns>ATP dashboard statistics</returns>
        [HttpGet("atp-statistics")]
        [Authorize(Roles = Constants.Roles.ATP)]
        [ProducesResponseType(typeof(AtpStatisticsDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<Result<AtpStatisticsDto>>> GetAtpStatistics()
        {
            _logger.LogInformation("Getting ATP statistics");

            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _dashboardService.GetAtpStatisticsAsync(appUser.Id);
            return this.HandleResponse(result);
        }

        /// <summary>
        /// Get Asesor/Auditor specific statistics
        /// </summary>
        /// <returns>Asesor/Auditor dashboard statistics</returns>
        [HttpGet("asesor-auditor-statistics")]
        [Authorize(Roles = $"{Constants.Roles.Asesor},{Constants.Roles.Auditor}")]
        [ProducesResponseType(typeof(AsesorAuditorStatisticsDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<
            ActionResult<Result<AsesorAuditorStatisticsDto>>
        > GetAsesorAuditorStatistics()
        {
            _logger.LogInformation("Getting Asesor/Auditor statistics");

            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _dashboardService.GetAsesorAuditorStatisticsAsync(appUser.Id);
            return this.HandleResponse(result);
        }

        /// <summary>
        /// Get activities by specific type
        /// </summary>
        /// <param name="type">Type of activity (company_registered, certification_completed, audit_scheduled)</param>
        /// <param name="limit">Maximum number of activities to return</param>
        /// <returns>List of activities of the specified type</returns>
        [HttpGet("activities/type/{type}")]
        [ProducesResponseType(typeof(List<RecentActivityDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<Result<List<RecentActivityDto>>>> GetActivitiesByType(
            string type,
            [FromQuery] int limit = 10
        )
        {
            _logger.LogInformation("Getting activities by type: {Type}", type);

            if (string.IsNullOrWhiteSpace(type))
            {
                return this.HandleResponse(
                    Result<List<RecentActivityDto>>.Failure("Tipo de actividad requerido")
                );
            }

            if (limit <= 0 || limit > 100)
            {
                return this.HandleResponse(
                    Result<List<RecentActivityDto>>.Failure("Límite debe estar entre 1 y 100")
                );
            }

            // This would need to be implemented in the activity service
            return this.HandleResponse(
                Result<List<RecentActivityDto>>.Failure("Funcionalidad en desarrollo")
            );
        }

        /// <summary>
        /// Get activities within a date range
        /// </summary>
        /// <param name="startDate">Start date for filtering activities</param>
        /// <param name="endDate">End date for filtering activities</param>
        /// <returns>List of activities within the date range</returns>
        [HttpGet("activities/date-range")]
        [ProducesResponseType(typeof(List<RecentActivityDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<Result<List<RecentActivityDto>>>> GetActivitiesByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate
        )
        {
            _logger.LogInformation(
                "Getting activities by date range: {StartDate} to {EndDate}",
                startDate,
                endDate
            );

            if (startDate >= endDate)
            {
                return this.HandleResponse(
                    Result<List<RecentActivityDto>>.Failure(
                        "Fecha inicio debe ser menor que fecha fin"
                    )
                );
            }

            if ((endDate - startDate).TotalDays > 365)
            {
                return this.HandleResponse(
                    Result<List<RecentActivityDto>>.Failure(
                        "Rango de fechas no puede exceder 365 días"
                    )
                );
            }

            // This would need to be implemented in the activity service
            return this.HandleResponse(
                Result<List<RecentActivityDto>>.Failure("Funcionalidad en desarrollo")
            );
        }

        /// <summary>
        /// Get empresa specific statistics for dashboard
        /// </summary>
        /// <returns>Empresa statistics including certification status and pending tasks</returns>
        [HttpGet("empresa-statistics")]
        [Authorize(Roles = Constants.Roles.Empresa)]
        [ProducesResponseType(typeof(EmpresaStatisticsDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<Result<EmpresaStatisticsDto>>> GetEmpresaStatistics()
        {
            _logger.LogInformation("Getting empresa statistics");

            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _dashboardService.GetEmpresaStatisticsAsync(appUser.Id);
            return this.HandleResponse(result);
        }

        /// <summary>
        /// Get consultor specific statistics for dashboard
        /// </summary>
        /// <returns>Consultor statistics including active projects and scheduled meetings</returns>
        [HttpGet("consultor-statistics")]
        [Authorize(Roles = Constants.Roles.Asesor)]
        [ProducesResponseType(typeof(ConsultorStatisticsDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<Result<ConsultorStatisticsDto>>> GetConsultorStatistics()
        {
            _logger.LogInformation("Getting consultor statistics");

            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _dashboardService.GetConsultorStatisticsAsync(appUser.Id);
            return this.HandleResponse(result);
        }

        /// <summary>
        /// Get CTC specific statistics for dashboard
        /// </summary>
        /// <returns>CTC statistics including regional data and pending evaluations</returns>
        [HttpGet("ctc-statistics")]
        [Authorize(Roles = Constants.Roles.CTC)]
        [ProducesResponseType(typeof(CtcStatisticsDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<Result<CtcStatisticsDto>>> GetCtcStatistics()
        {
            _logger.LogInformation("Getting CTC statistics");

            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _dashboardService.GetCtcStatisticsAsync(appUser.Id);
            return this.HandleResponse(result);
        }

        /// <summary>
        /// Get empresa auditora specific statistics for dashboard
        /// </summary>
        /// <returns>Empresa auditora statistics including auditor management and scheduled audits</returns>
        [HttpGet("empresa-auditora-statistics")]
        [Authorize(Roles = Constants.Roles.EmpresaAuditora)]
        [ProducesResponseType(typeof(EmpresaAuditoraStatisticsDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<
            ActionResult<Result<EmpresaAuditoraStatisticsDto>>
        > GetEmpresaAuditoraStatistics()
        {
            _logger.LogInformation("Getting empresa auditora statistics");

            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _dashboardService.GetEmpresaAuditoraStatisticsAsync(appUser.Id);
            return this.HandleResponse(result);
        }

        /// <summary>
        /// Get activities specific to user role
        /// </summary>
        /// <param name="role">Role to filter activities for</param>
        /// <param name="limit">Maximum number of activities to return</param>
        /// <param name="days">Days back to filter activities</param>
        /// <returns>List of role-specific activities</returns>
        [HttpGet("activities/{role}")]
        [ProducesResponseType(typeof(List<RecentActivityDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<Result<List<RecentActivityDto>>>> GetActivitiesByRole(
            string role,
            [FromQuery] int limit = 10,
            [FromQuery] int days = 7
        )
        {
            _logger.LogInformation(
                "Getting activities for role: {Role} with limit: {Limit}, days: {Days}",
                role,
                limit,
                days
            );

            if (string.IsNullOrWhiteSpace(role))
            {
                return this.HandleResponse(
                    Result<List<RecentActivityDto>>.Failure("Rol requerido")
                );
            }

            if (limit <= 0 || limit > 100)
            {
                return this.HandleResponse(
                    Result<List<RecentActivityDto>>.Failure("Límite debe estar entre 1 y 100")
                );
            }

            if (days <= 0 || days > 365)
            {
                return this.HandleResponse(
                    Result<List<RecentActivityDto>>.Failure("Días debe estar entre 1 y 365")
                );
            }

            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _dashboardService.GetActivitiesByRoleAsync(
                appUser.Id,
                role,
                limit,
                days
            );
            return this.HandleResponse(result);
        }

        /// <summary>
        /// Get notifications specific to user role
        /// </summary>
        /// <param name="role">Role to filter notifications for</param>
        /// <returns>List of role-specific notifications</returns>
        [HttpGet("notifications/{role}")]
        [ProducesResponseType(typeof(List<NotificationDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<Result<List<NotificationDto>>>> GetNotificationsByRole(
            string role
        )
        {
            _logger.LogInformation("Getting notifications for role: {Role}", role);

            if (string.IsNullOrWhiteSpace(role))
            {
                return this.HandleResponse(Result<List<NotificationDto>>.Failure("Rol requerido"));
            }

            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _dashboardService.GetNotificationsByRoleAsync(appUser.Id, role);
            return this.HandleResponse(result);
        }
    }
}
