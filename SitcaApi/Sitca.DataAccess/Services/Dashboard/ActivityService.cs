using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.DTOs.Dashboard;

namespace Sitca.DataAccess.Services.Dashboard
{
    public class ActivityService : IActivityService
    {
        private readonly ApplicationDbContext _db;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ActivityService> _logger;

        public ActivityService(
            ApplicationDbContext db,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ILogger<ActivityService> logger)
        {
            _db = db;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Result<List<RecentActivityDto>>> GetRecentActivitiesAsync(string userId, int limit = 10, int offset = 0)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result<List<RecentActivityDto>>.Failure("Usuario no encontrado");
                }

                var activities = new List<RecentActivityDto>();

                // Get activities based on user's country if applicable
                var empresaQuery = _db.Empresa.Where(e => e.Active);
                var procesosQuery = _db.ProcesoCertificacion.Where(p => p.Enabled);

                if (user.PaisId.HasValue)
                {
                    empresaQuery = empresaQuery.Where(e => e.IdPais == user.PaisId);
                    procesosQuery = procesosQuery.Include(p => p.Empresa)
                        .Where(p => p.Empresa.IdPais == user.PaisId);
                }

                // Fetch different types of activities
                var registeredCompanies = await GetRecentCompanyRegistrations(empresaQuery, limit / 3);
                var completedCertifications = await GetRecentCertificationCompletions(procesosQuery, limit / 3);
                var scheduledAudits = await GetUpcomingAudits(procesosQuery, limit / 3);

                activities.AddRange(registeredCompanies);
                activities.AddRange(completedCertifications);
                activities.AddRange(scheduledAudits);

                // Order by timestamp and apply pagination
                var paginatedActivities = activities
                    .OrderByDescending(a => a.Timestamp)
                    .Skip(offset)
                    .Take(limit)
                    .ToList();

                return Result<List<RecentActivityDto>>.Success(paginatedActivities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activities for user {UserId}", userId);
                return Result<List<RecentActivityDto>>.Failure("Error al obtener actividades recientes");
            }
        }

        public async Task<Result<List<RecentActivityDto>>> GetActivitiesByTypeAsync(string activityType, int limit = 10)
        {
            try
            {
                var activities = new List<RecentActivityDto>();

                switch (activityType.ToLower())
                {
                    case "company_registered":
                        activities = await GetRecentCompanyRegistrations(_db.Empresa.Where(e => e.Active), limit);
                        break;
                    case "certification_completed":
                        activities = await GetRecentCertificationCompletions(_db.ProcesoCertificacion.Where(p => p.Enabled), limit);
                        break;
                    case "audit_scheduled":
                        activities = await GetUpcomingAudits(_db.ProcesoCertificacion.Where(p => p.Enabled), limit);
                        break;
                    default:
                        return Result<List<RecentActivityDto>>.Failure($"Tipo de actividad '{activityType}' no válido");
                }

                return Result<List<RecentActivityDto>>.Success(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activities by type {ActivityType}", activityType);
                return Result<List<RecentActivityDto>>.Failure("Error al obtener actividades por tipo");
            }
        }

        public async Task<Result<List<RecentActivityDto>>> GetActivitiesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var activities = new List<RecentActivityDto>();

                var procesosQuery = _db.ProcesoCertificacion
                    .Where(p => p.Enabled && 
                           ((p.CreatedAt.HasValue && p.CreatedAt.Value >= startDate && p.CreatedAt.Value <= endDate) ||
                            (p.FechaFinalizacion.HasValue && p.FechaFinalizacion.Value >= startDate && p.FechaFinalizacion.Value <= endDate)));

                var completedCertifications = await GetRecentCertificationCompletions(procesosQuery, 100);
                activities.AddRange(completedCertifications);

                var orderedActivities = activities
                    .OrderByDescending(a => a.Timestamp)
                    .ToList();

                return Result<List<RecentActivityDto>>.Success(orderedActivities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activities by date range");
                return Result<List<RecentActivityDto>>.Failure("Error al obtener actividades por rango de fecha");
            }
        }

        private async Task<List<RecentActivityDto>> GetRecentCompanyRegistrations(IQueryable<Models.Empresa> query, int limit)
        {
            return await query
                .Include(e => e.Certificaciones)
                .OrderByDescending(e => e.Id) // Using ID as proxy for creation order since CreatedAt doesn't exist
                .Take(limit)
                .Select(e => new RecentActivityDto
                {
                    Id = e.Id,
                    Title = "Nueva empresa registrada",
                    Description = $"{e.Nombre} se registró para certificación",
                    Timestamp = DateTime.Now.AddDays(-e.Id % 30), // Placeholder timestamp based on ID
                    Type = "company_registered",
                    Icon = "business",
                    Priority = "info",
                    CompanyId = e.Id,
                    CompanyName = e.Nombre,
                    ProcesoCertificacionId = e.Certificaciones
                        .Where(c => c.Enabled)
                        .OrderByDescending(c => c.Id)
                        .Select(c => (int?)c.Id)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        private async Task<List<RecentActivityDto>> GetRecentCertificationCompletions(IQueryable<Models.ProcesoCertificacion> query, int limit)
        {
            return await query
                .Where(p => (p.Status == "Completado" || p.Status == "Certificado") && p.FechaFinalizacion.HasValue)
                .Include(p => p.Empresa)
                .OrderByDescending(p => p.FechaFinalizacion)
                .Take(limit)
                .Select(p => new RecentActivityDto
                {
                    Id = p.Id,
                    Title = "Certificación completada",
                    Description = $"{p.Empresa.Nombre} obtuvo certificación",
                    Timestamp = p.FechaFinalizacion.Value,
                    Type = "certification_completed",
                    Icon = "verified",
                    Priority = "success",
                    CompanyId = p.EmpresaId,
                    CompanyName = p.Empresa.Nombre,
                    ProcesoCertificacionId = p.Id,
                    Distintivo = p.Empresa.ResultadoActual
                })
                .ToListAsync();
        }

        private async Task<List<RecentActivityDto>> GetUpcomingAudits(IQueryable<Models.ProcesoCertificacion> query, int limit)
        {
            return await query
                .Where(p => p.FechaFijadaAuditoria.HasValue && p.FechaFijadaAuditoria > DateTime.Now)
                .Include(p => p.Empresa)
                .OrderBy(p => p.FechaFijadaAuditoria)
                .Take(limit)
                .Select(p => new RecentActivityDto
                {
                    Id = p.Id,
                    Title = "Auditoría programada",
                    Description = $"Auditoría para {p.Empresa.Nombre} programada",
                    Timestamp = p.UpdatedAt ?? p.CreatedAt ?? DateTime.Now,
                    Type = "audit_scheduled",
                    Icon = "event",
                    Priority = "warning",
                    CompanyId = p.EmpresaId,
                    CompanyName = p.Empresa.Nombre,
                    ProcesoCertificacionId = p.Id,
                    AuditDate = p.FechaFijadaAuditoria
                })
                .ToListAsync();
        }

        public async Task<Result<List<RecentActivityDto>>> GetActivitiesByRoleAsync(string userId, string role, int limit, int days)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result<List<RecentActivityDto>>.Failure("Usuario no encontrado");
                }

                var startDate = DateTime.Now.AddDays(-days);
                var activities = new List<RecentActivityDto>();

                switch (role.ToLower())
                {
                    case "empresa":
                        activities = await GetEmpresaActivities(user, startDate, limit);
                        break;
                    case "consultor":
                    case "asesor":
                        activities = await GetConsultorActivities(user, startDate, limit);
                        break;
                    case "auditor":
                        activities = await GetAuditorActivities(user, startDate, limit);
                        break;
                    case "ctc":
                        activities = await GetCtcActivities(user, startDate, limit);
                        break;
                    default:
                        activities = await GetGeneralActivities(user, startDate, limit);
                        break;
                }

                return Result<List<RecentActivityDto>>.Success(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activities by role {Role} for user {UserId}", role, userId);
                return Result<List<RecentActivityDto>>.Failure("Error al obtener actividades por rol");
            }
        }

        public async Task<Result<List<NotificationDto>>> GetNotificationsByRoleAsync(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result<List<NotificationDto>>.Failure("Usuario no encontrado");
                }

                var notifications = new List<NotificationDto>();

                switch (role.ToLower())
                {
                    case "empresa":
                        notifications = await GetEmpresaNotifications(user);
                        break;
                    case "consultor":
                    case "asesor":
                        notifications = await GetConsultorNotifications(user);
                        break;
                    case "auditor":
                        notifications = await GetAuditorNotifications(user);
                        break;
                    case "ctc":
                        notifications = await GetCtcNotifications(user);
                        break;
                    default:
                        notifications = await GetGeneralNotifications(user);
                        break;
                }

                return Result<List<NotificationDto>>.Success(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications by role {Role} for user {UserId}", role, userId);
                return Result<List<NotificationDto>>.Failure("Error al obtener notificaciones por rol");
            }
        }

        private async Task<List<RecentActivityDto>> GetEmpresaActivities(ApplicationUser user, DateTime startDate, int limit)
        {
            // For now, return placeholder data
            return await Task.FromResult(new List<RecentActivityDto>
            {
                new RecentActivityDto
                {
                    Id = 1,
                    Title = "Documento subido",
                    Description = "Manual de procedimientos actualizado",
                    Timestamp = DateTime.Now.AddHours(-2),
                    Type = "document_uploaded",
                    Icon = "description",
                    Priority = "info"
                }
            });
        }

        private async Task<List<RecentActivityDto>> GetConsultorActivities(ApplicationUser user, DateTime startDate, int limit)
        {
            return await Task.FromResult(new List<RecentActivityDto>
            {
                new RecentActivityDto
                {
                    Id = 1,
                    Title = "Reunión programada",
                    Description = "Reunión con cliente para revisión de avances",
                    Timestamp = DateTime.Now.AddHours(-1),
                    Type = "meeting_scheduled",
                    Icon = "event",
                    Priority = "info"
                }
            });
        }

        private async Task<List<RecentActivityDto>> GetAuditorActivities(ApplicationUser user, DateTime startDate, int limit)
        {
            return await Task.FromResult(new List<RecentActivityDto>
            {
                new RecentActivityDto
                {
                    Id = 1,
                    Title = "Auditoría completada",
                    Description = "Auditoría realizada exitosamente",
                    Timestamp = DateTime.Now.AddHours(-3),
                    Type = "audit_completed",
                    Icon = "verified",
                    Priority = "success"
                }
            });
        }

        private async Task<List<RecentActivityDto>> GetCtcActivities(ApplicationUser user, DateTime startDate, int limit)
        {
            return await Task.FromResult(new List<RecentActivityDto>
            {
                new RecentActivityDto
                {
                    Id = 1,
                    Title = "Evaluación aprobada",
                    Description = "Certificación aprobada para nueva empresa",
                    Timestamp = DateTime.Now.AddHours(-4),
                    Type = "evaluation_approved",
                    Icon = "verified",
                    Priority = "success"
                }
            });
        }

        private async Task<List<RecentActivityDto>> GetGeneralActivities(ApplicationUser user, DateTime startDate, int limit)
        {
            return await GetRecentActivitiesAsync(user.Id, limit, 0).ContinueWith(t => t.Result.Value ?? new List<RecentActivityDto>());
        }

        private async Task<List<NotificationDto>> GetEmpresaNotifications(ApplicationUser user)
        {
            return await Task.FromResult(new List<NotificationDto>
            {
                new NotificationDto
                {
                    Id = 1,
                    Tipo = "task_due",
                    Titulo = "Tarea por vencer",
                    Mensaje = "El manual de procedimientos debe entregarse en 2 días",
                    Prioridad = "alta",
                    FechaCreacion = DateTime.Now.AddHours(-1),
                    Leida = false,
                    AccionesDisponibles = new List<string> { "mark_read", "view_task", "extend_deadline" }
                }
            });
        }

        private async Task<List<NotificationDto>> GetConsultorNotifications(ApplicationUser user)
        {
            return await Task.FromResult(new List<NotificationDto>
            {
                new NotificationDto
                {
                    Id = 1,
                    Tipo = "meeting_reminder",
                    Titulo = "Recordatorio de reunión",
                    Mensaje = "Reunión con cliente en 1 hora",
                    Prioridad = "media",
                    FechaCreacion = DateTime.Now.AddMinutes(-30),
                    Leida = false,
                    AccionesDisponibles = new List<string> { "mark_read", "view_meeting", "reschedule" }
                }
            });
        }

        private async Task<List<NotificationDto>> GetAuditorNotifications(ApplicationUser user)
        {
            return await Task.FromResult(new List<NotificationDto>
            {
                new NotificationDto
                {
                    Id = 1,
                    Tipo = "audit_assigned",
                    Titulo = "Nueva auditoría asignada",
                    Mensaje = "Se le ha asignado una nueva auditoría para la próxima semana",
                    Prioridad = "alta",
                    FechaCreacion = DateTime.Now.AddHours(-2),
                    Leida = false,
                    AccionesDisponibles = new List<string> { "mark_read", "view_audit", "accept", "decline" }
                }
            });
        }

        private async Task<List<NotificationDto>> GetCtcNotifications(ApplicationUser user)
        {
            return await Task.FromResult(new List<NotificationDto>
            {
                new NotificationDto
                {
                    Id = 1,
                    Tipo = "evaluation_pending",
                    Titulo = "Evaluación pendiente",
                    Mensaje = "3 evaluaciones pendientes de revisión",
                    Prioridad = "media",
                    FechaCreacion = DateTime.Now.AddHours(-3),
                    Leida = false,
                    AccionesDisponibles = new List<string> { "mark_read", "view_evaluations", "prioritize" }
                }
            });
        }

        private async Task<List<NotificationDto>> GetGeneralNotifications(ApplicationUser user)
        {
            return await Task.FromResult(new List<NotificationDto>
            {
                new NotificationDto
                {
                    Id = 1,
                    Tipo = "system_update",
                    Titulo = "Actualización del sistema",
                    Mensaje = "Nueva versión del sistema disponible",
                    Prioridad = "baja",
                    FechaCreacion = DateTime.Now.AddHours(-5),
                    Leida = false,
                    AccionesDisponibles = new List<string> { "mark_read", "view_changelog" }
                }
            });
        }
    }
}