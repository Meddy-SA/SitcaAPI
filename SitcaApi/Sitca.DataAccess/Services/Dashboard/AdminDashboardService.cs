using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.DTOs.Dashboard;
using Utilities.Common;
using static Utilities.Common.Constants;

namespace Sitca.DataAccess.Services.Dashboard
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ApplicationDbContext _db;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminDashboardService> _logger;
        private readonly IMemoryCache _cache;
        private const string ADMIN_STATS_CACHE_KEY = "admin_statistics";
        private const string SYSTEM_STATUS_CACHE_KEY = "system_status";
        private const int CACHE_DURATION_MINUTES = 5;

        public AdminDashboardService(
            ApplicationDbContext db,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminDashboardService> logger,
            IMemoryCache cache
        )
        {
            _db = db;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
            _cache = cache;
        }

        public async Task<Result<AdminStatisticsDto>> GetStatisticsAsync(string userId)
        {
            try
            {
                var cacheKey = $"{ADMIN_STATS_CACHE_KEY}_{userId}";

                if (_cache.TryGetValue(cacheKey, out AdminStatisticsDto cachedStats))
                {
                    _logger.LogInformation("Returning cached admin statistics");
                    return Result<AdminStatisticsDto>.Success(cachedStats);
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result<AdminStatisticsDto>.Failure("Usuario no encontrado");
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var stats = await CalculateAdminStatisticsAsync(user, userRoles);

                _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));

                return Result<AdminStatisticsDto>.Success(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin statistics for user {UserId}", userId);
                return Result<AdminStatisticsDto>.Failure(
                    "Error al obtener estadísticas administrativas"
                );
            }
        }

        public async Task<Result<SystemStatusDto>> GetSystemStatusAsync()
        {
            try
            {
                if (_cache.TryGetValue(SYSTEM_STATUS_CACHE_KEY, out SystemStatusDto cachedStatus))
                {
                    return Result<SystemStatusDto>.Success(cachedStatus);
                }

                var systemStatus = await CalculateSystemStatusAsync();

                _cache.Set(SYSTEM_STATUS_CACHE_KEY, systemStatus, TimeSpan.FromMinutes(1));

                return Result<SystemStatusDto>.Success(systemStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system status");
                return Result<SystemStatusDto>.Failure("Error al obtener estado del sistema");
            }
        }

        private async Task<AdminStatisticsDto> CalculateAdminStatisticsAsync(
            ApplicationUser user,
            IList<string> userRoles
        )
        {
            var empresaQuery = _db.Empresa.Where(e => e.Active);
            var procesosQuery = _db.ProcesoCertificacion.Where(p => p.Enabled);

            // Filter by country based on role
            if (!userRoles.Contains(Constants.Roles.Admin))
            {
                // This users can only see data from their country
                if (user.PaisId.HasValue)
                {
                    empresaQuery = empresaQuery.Where(e => e.IdPais == user.PaisId);
                    procesosQuery = procesosQuery
                        .Include(p => p.Empresa)
                        .Where(p => p.Empresa.IdPais == user.PaisId);
                }
            }
            // Admin users see all countries (no filtering needed)

            const string initialStatus = ProcessStatusText.Spanish.Initial;
            const string consultingStatus = ProcessStatusText.Spanish.ForConsulting;
            const string completedStatus = ProcessStatusText.Spanish.Completed;

            var adminStats = new AdminStatisticsDto
            {
                TotalEmpresas = await empresaQuery.CountAsync(),
                TotalCompleted = await procesosQuery
                    .Where(p => p.Status == completedStatus)
                    .CountAsync(),
                TotalInProcess = await procesosQuery
                    .Where(p =>
                        p.Status != initialStatus
                        && p.Status != consultingStatus
                        && p.Status != completedStatus
                    )
                    .CountAsync(),
                TotalPending = await procesosQuery
                    .Where(p => p.Status == initialStatus || p.Status == consultingStatus)
                    .CountAsync(),
            };

            // Apply same filtering logic for EmpresasPorPais
            var empresasPorPaisQuery = _db.Empresa.Where(e => e.Active);

            if (
                userRoles.Contains(Constants.Roles.TecnicoPais)
                && !userRoles.Contains(Constants.Roles.Admin)
            )
            {
                // TecnicoPais users can only see data from their country
                if (user.PaisId.HasValue)
                {
                    empresasPorPaisQuery = empresasPorPaisQuery.Where(e => e.IdPais == user.PaisId);
                }
            }

            adminStats.EmpresasPorPais = await empresasPorPaisQuery
                .Include(e => e.Pais)
                .Where(e => e.Pais != null && e.Pais.Active)
                .GroupBy(e => new { CountryId = e.IdPais, e.Pais.Name })
                .Select(g => new CountryStatDto
                {
                    Name = g.Key.Name,
                    Count = g.Count(),
                    CountryId = g.Key.CountryId,
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            // Apply same filtering logic for EmpresasPorTipologia
            var empresasPorTipologiaQuery = _db.Empresa.Where(e => e.Active);

            if (!userRoles.Contains(Constants.Roles.Admin))
            {
                // This users can only see data from their country
                if (user.PaisId.HasValue)
                {
                    empresasPorTipologiaQuery = empresasPorTipologiaQuery.Where(e =>
                        e.IdPais == user.PaisId
                    );
                }
            }

            adminStats.EmpresasPorTipologia = await empresasPorTipologiaQuery
                .SelectMany(e =>
                    e.Tipologias.Where(te => te.Tipologia.Active).Select(te => te.Tipologia)
                )
                .GroupBy(t => new { t.Id, t.Name })
                .Select(g => new TypologyStatDto
                {
                    Name = g.Key.Name,
                    Count = g.Count(),
                    TipologiaId = g.Key.Id,
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return adminStats;
        }

        private async Task<SystemStatusDto> CalculateSystemStatusAsync()
        {
            var systemStatus = new SystemStatusDto
            {
                ApiStatus = new ApiStatusDto
                {
                    Status = "online",
                    ResponseTime = await MeasureResponseTimeAsync(),
                    LastCheck = DateTime.UtcNow,
                },
                DatabaseStatus = await GetDatabaseStatusAsync(),
                LastBackup = await GetLastBackupInfoAsync(),
                ServerHealth = await GetServerHealthAsync(),
            };

            return systemStatus;
        }

        private async Task<string> MeasureResponseTimeAsync()
        {
            var startTime = DateTime.UtcNow;
            await _db.Empresa.Take(1).ToListAsync();
            var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            return $"{responseTime:F0}ms";
        }

        private async Task<DatabaseStatusDto> GetDatabaseStatusAsync()
        {
            try
            {
                var connectionCount = await _db.Empresa.CountAsync() > 0 ? 1 : 0;

                return new DatabaseStatusDto
                {
                    Status = "connected",
                    ConnectionCount = connectionCount,
                    LastCheck = DateTime.UtcNow,
                };
            }
            catch
            {
                return new DatabaseStatusDto
                {
                    Status = "disconnected",
                    ConnectionCount = 0,
                    LastCheck = DateTime.UtcNow,
                };
            }
        }

        private async Task<LastBackupDto> GetLastBackupInfoAsync()
        {
            // This would normally check actual backup logs or status
            return await Task.FromResult(
                new LastBackupDto
                {
                    Timestamp = DateTime.UtcNow.AddHours(-2),
                    Status = "success",
                    Size = "2.4GB",
                }
            );
        }

        private async Task<ServerHealthDto> GetServerHealthAsync()
        {
            // This would normally check actual server metrics
            return await Task.FromResult(
                new ServerHealthDto
                {
                    CpuUsage = 23.5,
                    MemoryUsage = 67.2,
                    DiskUsage = 45.8,
                }
            );
        }
    }
}
