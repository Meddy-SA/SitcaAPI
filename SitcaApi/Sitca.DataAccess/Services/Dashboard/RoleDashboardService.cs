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
using Sitca.Models.ViewModels;
using Utilities.Common;
using static Utilities.Common.Constants;

namespace Sitca.DataAccess.Services.Dashboard
{
    public class RoleDashboardService : IRoleDashboardService
    {
        private readonly ApplicationDbContext _db;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RoleDashboardService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleDashboardService(
            ApplicationDbContext db,
            IUnitOfWork unitOfWork,
            ILogger<RoleDashboardService> logger,
            UserManager<ApplicationUser> userManager
        )
        {
            _db = db;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<Result<AtpStatisticsDto>> GetAtpStatisticsAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result<AtpStatisticsDto>.Failure("Usuario no encontrado");
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains(Constants.Roles.ATP))
                {
                    return Result<AtpStatisticsDto>.Failure("Usuario no tiene rol de ATP");
                }

                // ATP users typically support companies in their country
                var empresasAsignadas = await GetAtpAssignedCompanies(user);
                var actividadesRecientes = await GetAtpRecentActivities(user);

                var atpStats = new AtpStatisticsDto
                {
                    TareasAsignadas = empresasAsignadas.Count,
                    TareasCompletadas = empresasAsignadas.Count(e =>
                        e.EstadoProceso == "Completado"
                    ),
                    TareasPendientes = empresasAsignadas.Count(e =>
                        e.EstadoProceso != "Completado"
                    ),
                    EmpresasAsignadas = empresasAsignadas,
                    ActividadesRecientes = actividadesRecientes,
                };

                return Result<AtpStatisticsDto>.Success(atpStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ATP statistics for user {UserId}", userId);
                return Result<AtpStatisticsDto>.Failure("Error al obtener estadísticas ATP");
            }
        }

        public async Task<Result<AsesorAuditorStatisticsDto>> GetAsesorAuditorStatisticsAsync(
            string userId
        )
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result<AsesorAuditorStatisticsDto>.Failure("Usuario no encontrado");
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var isAuditor = userRoles.Contains(Constants.Roles.Auditor);
                var isAsesor = userRoles.Contains(Constants.Roles.Asesor);
                var isAsesorAuditor = userRoles.Contains(Constants.Roles.AsesorAuditor);

                // Si tiene rol combinado, es ambos
                if (isAsesorAuditor)
                {
                    isAuditor = true;
                    isAsesor = true;
                }

                if (!isAuditor && !isAsesor)
                {
                    return Result<AsesorAuditorStatisticsDto>.Failure(
                        "Usuario no tiene rol de Asesor o Auditor"
                    );
                }

                var statistics = await CalculateAsesorAuditorStatistics(
                    userId,
                    isAuditor,
                    isAsesor
                );

                return Result<AsesorAuditorStatisticsDto>.Success(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving Asesor/Auditor statistics for user {UserId}",
                    userId
                );
                return Result<AsesorAuditorStatisticsDto>.Failure(
                    "Error al obtener estadísticas de Asesor/Auditor"
                );
            }
        }

        private async Task<List<EmpresaAsignadaDto>> GetAtpAssignedCompanies(ApplicationUser user)
        {
            // ATP typically handles companies in their country that need technical assistance
            var query = _db
                .ProcesoCertificacion.Include(p => p.Empresa)
                .Where(p => p.Enabled && p.Empresa.Active);

            if (user.PaisId.HasValue)
            {
                query = query.Where(p => p.Empresa.IdPais == user.PaisId);
            }

            // Get companies in process that might need ATP assistance
            var empresasAsignadas = await query
                .Where(p => p.Status == "En Proceso" || p.Status == "Pendiente")
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .Select(p => new EmpresaAsignadaDto
                {
                    EmpresaId = p.EmpresaId,
                    NombreEmpresa = p.Empresa.Nombre,
                    EstadoProceso = p.Status,
                    FechaAsignacion = p.CreatedAt ?? DateTime.Now,
                    Prioridad = DeterminePriority(p),
                })
                .ToListAsync();

            return empresasAsignadas;
        }

        private async Task<List<ActividadRecienteDto>> GetAtpRecentActivities(ApplicationUser user)
        {
            var activities = new List<ActividadRecienteDto>();

            // Get recent certification process updates
            var recentUpdates = await _db
                .ProcesoCertificacion.Include(p => p.Empresa)
                .Where(p =>
                    p.Enabled
                    && p.UpdatedAt.HasValue
                    && p.UpdatedAt.Value >= DateTime.Now.AddDays(-7)
                )
                .OrderByDescending(p => p.UpdatedAt)
                .Take(5)
                .ToListAsync();

            foreach (var update in recentUpdates)
            {
                activities.Add(
                    new ActividadRecienteDto
                    {
                        Tipo = "proceso_actualizado",
                        Descripcion =
                            $"Proceso de {update.Empresa.Nombre} actualizado a estado: {update.Status}",
                        Timestamp = update.UpdatedAt.Value,
                    }
                );
            }

            return activities.OrderByDescending(a => a.Timestamp).ToList();
        }

        private async Task<AsesorAuditorStatisticsDto> CalculateAsesorAuditorStatistics(
            string userId,
            bool isAuditor,
            bool isAsesor
        )
        {
            // 1. Query base filtrada por AsesorId o AuditorId
            var query = _db
                .ProcesoCertificacion
                .Include(p => p.Empresa)
                .ThenInclude(e => e.Pais)
                .Include(p => p.Tipologia)
                .Where(p => p.Enabled);

            // Filtrar por AsesorId y/o AuditorId según el rol
            if (isAuditor && isAsesor)
            {
                // Si tiene ambos roles, buscar por cualquiera de los dos
                query = query.Where(p => p.AuditorId == userId || p.AsesorId == userId);
            }
            else if (isAuditor)
            {
                query = query.Where(p => p.AuditorId == userId);
            }
            else if (isAsesor)
            {
                query = query.Where(p => p.AsesorId == userId);
            }

            var procesos = await query.ToListAsync();

            // 2. Definir estados según rol
            string[] estadosEnProceso;
            string[] estadosCompletados;

            if (isAuditor && isAsesor)
            {
                // Si tiene ambos roles, considerar todos los estados
                estadosEnProceso = new[]
                {
                    ProcessStatusText.Spanish.ForConsulting,
                    ProcessStatusText.Spanish.ConsultancyUnderway,
                    ProcessStatusText.Spanish.ForAuditing,
                    ProcessStatusText.Spanish.AuditingUnderway,
                };
                estadosCompletados = new[]
                {
                    ProcessStatusText.Spanish.ConsultancyCompleted,
                    ProcessStatusText.Spanish.AuditCompleted,
                    ProcessStatusText.Spanish.Completed,
                };
            }
            else if (isAsesor)
            {
                estadosEnProceso = new[]
                {
                    ProcessStatusText.Spanish.ForConsulting,
                    ProcessStatusText.Spanish.ConsultancyUnderway,
                };
                estadosCompletados = new[]
                {
                    ProcessStatusText.Spanish.ConsultancyCompleted,
                    ProcessStatusText.Spanish.Completed,
                };
            }
            else
            {
                estadosEnProceso = new[]
                {
                    ProcessStatusText.Spanish.ForAuditing,
                    ProcessStatusText.Spanish.AuditingUnderway,
                };
                estadosCompletados = new[]
                {
                    ProcessStatusText.Spanish.AuditCompleted,
                    ProcessStatusText.Spanish.Completed,
                };
            }

            // 3. Calcular KPIs
            var empresasEnProceso = procesos.Count(p =>
                estadosEnProceso.Any(e => p.Status != null && p.Status.Contains(e))
            );
            var empresasCompletadas = procesos.Count(p =>
                estadosCompletados.Any(e => p.Status != null && p.Status.Contains(e))
            );

            // 4. Construir lista de empresas asignadas
            var empresasAsignadas = procesos
                .OrderByDescending(p =>
                    DeterminePriority(p) == "alta" ? 2 : DeterminePriority(p) == "media" ? 1 : 0
                )
                .ThenBy(p => p.FechaFijadaAuditoria ?? DateTime.MaxValue)
                .Take(20)
                .Select(p => new EmpresaAsignadaAsesorDto
                {
                    EmpresaId = p.EmpresaId,
                    ProcesoId = p.Id,
                    NombreEmpresa = p.Empresa?.Nombre ?? "N/A",
                    Tipologia = p.Tipologia?.Name ?? "N/A",
                    Pais = p.Empresa?.Pais?.Name ?? "N/A",
                    EstadoProceso = p.Status ?? "N/A",
                    EstadoNumerico = ExtractStatusNumber(p.Status),
                    FechaInicio = p.FechaInicio,
                    FechaProximaAuditoria = p.FechaFijadaAuditoria,
                    FechaVencimiento = p.FechaVencimiento,
                    Prioridad = DeterminePriority(p),
                    Recertificacion = p.Recertificacion,
                })
                .ToList();

            // 5. Próximas auditorías (30 días)
            var proximasAuditorias = procesos
                .Where(p =>
                    p.FechaFijadaAuditoria.HasValue
                    && p.FechaFijadaAuditoria > DateTime.Now
                    && p.FechaFijadaAuditoria <= DateTime.Now.AddDays(30)
                )
                .OrderBy(p => p.FechaFijadaAuditoria)
                .Take(10)
                .Select(p => new ProximaAuditoriaDto
                {
                    ProcesoId = p.Id,
                    EmpresaId = p.EmpresaId,
                    NombreEmpresa = p.Empresa?.Nombre ?? "N/A",
                    Tipologia = p.Tipologia?.Name ?? "N/A",
                    Pais = p.Empresa?.Pais?.Name ?? "N/A",
                    FechaAuditoria = p.FechaFijadaAuditoria!.Value,
                    Tipo = p.Recertificacion ? "recertificacion" : "auditoria_inicial",
                    DiasRestantes = (int)(p.FechaFijadaAuditoria!.Value - DateTime.Now).TotalDays,
                })
                .ToList();

            // 6. Distribución por estado
            var distribucion = procesos
                .Where(p => !string.IsNullOrEmpty(p.Status))
                .GroupBy(p => p.Status)
                .Select(g => new EstadoDistribucionDto
                {
                    Estado = g.Key!,
                    EstadoCorto = ExtractStatusShortName(g.Key),
                    Cantidad = g.Count(),
                    Color = GetStatusColor(g.Key),
                })
                .OrderBy(e => ExtractStatusNumber(e.Estado))
                .ToList();

            // 7. Actividades recientes
            var actividadesRecientes = await GetActivitiesForUserAsync(
                userId,
                isAuditor,
                isAsesor,
                10
            );

            // 8. Métricas de rendimiento
            var metricas = CalculateMetrics(procesos, estadosCompletados);

            return new AsesorAuditorStatisticsDto
            {
                TotalEmpresasAsignadas = procesos.Count,
                EmpresasEnProceso = empresasEnProceso,
                EmpresasCompletadas = empresasCompletadas,
                AuditoriasProgramadas = proximasAuditorias.Count,
                IsAuditor = isAuditor,
                IsAsesor = isAsesor,
                EmpresasAsignadas = empresasAsignadas,
                ProximasAuditorias = proximasAuditorias,
                DistribucionPorEstado = distribucion,
                ActividadesRecientes = actividadesRecientes,
                Metricas = metricas,
            };
        }

        private static string ExtractStatusNumber(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return "0";
            var parts = status.Split('-');
            return parts.Length > 0 ? parts[0].Trim() : "0";
        }

        private static string ExtractStatusShortName(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return "";
            var parts = status.Split('-');
            return parts.Length > 1 ? parts[1].Trim() : status;
        }

        private static string GetStatusColor(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return "default";
            if (status.Contains("Inicial"))
                return "info";
            if (status.Contains("Para "))
                return "primary";
            if (status.Contains("en Proceso"))
                return "warning";
            if (status.Contains("Finalizada") || status.Contains("Finalizado"))
                return "success";
            if (status.Contains("revisión"))
                return "accent";
            return "default";
        }

        private MetricasRendimientoDto CalculateMetrics(
            List<ProcesoCertificacion> procesos,
            string[] estadosCompletados
        )
        {
            var completados = procesos.Where(p =>
                estadosCompletados.Any(e => p.Status != null && p.Status.Contains(e))
            );

            var completadosConFechas = completados
                .Where(p => p.FechaFinalizacion.HasValue)
                .ToList();

            var promedioTiempo =
                completadosConFechas.Count != 0
                    ? (int)
                        completadosConFechas.Average(p =>
                            (p.FechaFinalizacion!.Value - p.FechaInicio).TotalDays
                        )
                    : 0;

            var procesoActivo = procesos
                .Where(p =>
                    p.Status == null
                    || !estadosCompletados.Any(e => p.Status.Contains(e))
                )
                .OrderBy(p => p.FechaInicio)
                .FirstOrDefault();

            var procesoMasAntiguo =
                procesoActivo != null
                    ? (int)(DateTime.Now - procesoActivo.FechaInicio).TotalDays
                    : 0;

            var inicioAnio = new DateTime(DateTime.Now.Year, 1, 1);
            var completadosEsteAnio = completadosConFechas.Count(p =>
                p.FechaFinalizacion >= inicioAnio
            );

            return new MetricasRendimientoDto
            {
                CertificacionesCompletadas = completados.Count(),
                CertificacionesEsteAnio = completadosEsteAnio,
                PromedioTiempoCertificacion = promedioTiempo,
                ProcesoMasAntiguo = procesoMasAntiguo,
            };
        }

        private async Task<List<RecentActivityDto>> GetActivitiesForUserAsync(
            string userId,
            bool isAuditor,
            bool isAsesor,
            int limit
        )
        {
            var query = _db
                .ProcesoCertificacion.Include(p => p.Empresa)
                .Where(p =>
                    p.Enabled && p.UpdatedAt.HasValue && p.UpdatedAt >= DateTime.Now.AddDays(-30)
                );

            if (isAuditor)
                query = query.Where(p => p.AuditorId == userId);
            else if (isAsesor)
                query = query.Where(p => p.AsesorId == userId);

            var updates = await query.OrderByDescending(p => p.UpdatedAt).Take(limit).ToListAsync();

            return updates
                .Select(p => new RecentActivityDto
                {
                    Id = p.Id,
                    Title = DetermineActivityTitle(p.Status),
                    Description = $"{p.Empresa?.Nombre ?? "N/A"} - {ExtractStatusShortName(p.Status)}",
                    Timestamp = p.UpdatedAt ?? DateTime.Now,
                    Type = DetermineActivityType(p.Status),
                    Icon = DetermineActivityIcon(p.Status),
                    Priority = DeterminePriority(p) == "alta" ? "warning" : "info",
                    CompanyId = p.EmpresaId,
                    CompanyName = p.Empresa?.Nombre,
                    ProcesoCertificacionId = p.Id,
                })
                .ToList();
        }

        private static string DetermineActivityTitle(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return "Proceso actualizado";
            if (status.Contains("Finalizada") || status.Contains("Finalizado"))
                return "Proceso completado";
            if (status.Contains("en Proceso"))
                return "Proceso en curso";
            if (status.Contains("Para "))
                return "Proceso asignado";
            return "Proceso actualizado";
        }

        private static string DetermineActivityType(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return "process_update";
            if (status.Contains("Finalizada") || status.Contains("Finalizado"))
                return "certification_completed";
            if (status.Contains("en Proceso"))
                return "audit_in_progress";
            if (status.Contains("Para "))
                return "audit_scheduled";
            return "process_update";
        }

        private static string DetermineActivityIcon(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return "update";
            if (status.Contains("Finalizada") || status.Contains("Finalizado"))
                return "check_circle";
            if (status.Contains("en Proceso"))
                return "pending";
            if (status.Contains("Para "))
                return "assignment";
            return "update";
        }

        private string DeterminePriority(ProcesoCertificacion proceso)
        {
            if (
                proceso.FechaVencimiento.HasValue
                && proceso.FechaVencimiento.Value <= DateTime.Now.AddDays(30)
            )
                return "alta";

            if (
                proceso.Status == "Pendiente"
                && proceso.CreatedAt.HasValue
                && proceso.CreatedAt.Value <= DateTime.Now.AddDays(-15)
            )
                return "alta";

            if (proceso.Status == "En Proceso")
                return "media";

            return "baja";
        }

        private async Task<int> CalculateAverageCertificationTime(
            IQueryable<ProcesoCertificacion> query,
            int completedCount,
            string completed
        )
        {
            if (completedCount == 0)
                return 0;

            var completedProcesses = await query
                .Where(p =>
                    (p.Status == completed) && p.FechaFinalizacion.HasValue && p.FechaInicio != null
                )
                .Select(p => new
                {
                    Duration = (p.FechaFinalizacion.Value - p.FechaInicio).TotalDays,
                })
                .ToListAsync();

            if (!completedProcesses.Any())
                return 0;

            return (int)completedProcesses.Average(p => p.Duration);
        }

        private async Task<double> CalculateCustomerSatisfaction(string userId)
        {
            // This would normally calculate from actual feedback/ratings
            // For now, return a placeholder value
            return await Task.FromResult(4.5);
        }

        public async Task<Result<EmpresaStatisticsDto>> GetEmpresaStatisticsAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result<EmpresaStatisticsDto>.Failure("Usuario no encontrado");
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains(Constants.Roles.Empresa))
                {
                    return Result<EmpresaStatisticsDto>.Failure("Usuario no tiene rol de Empresa");
                }

                if (!user.EmpresaId.HasValue)
                {
                    return Result<EmpresaStatisticsDto>.Failure(
                        "Usuario no tiene empresa asignada"
                    );
                }

                // Get empresa data using the same method as MiEmpresa endpoint
                var empresaData = await _unitOfWork.Empresa.Data(user.EmpresaId.Value, user);
                if (empresaData == null)
                {
                    return Result<EmpresaStatisticsDto>.Failure(
                        "No se pudo obtener información de la empresa"
                    );
                }

                // Build statistics from real empresa data
                var empresaStats = new EmpresaStatisticsDto
                {
                    CertificacionActual = BuildCertificacionActual(empresaData),
                    TareasPendientes = await BuildTareasPendientes(empresaData),
                    DocumentosRecientes = BuildDocumentosRecientes(empresaData, userId),
                    ProximasActividades = await BuildProximasActividades(empresaData),
                    EstadisticasGenerales = BuildEstadisticasGenerales(empresaData),
                };

                return Result<EmpresaStatisticsDto>.Success(empresaStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving Empresa statistics for user {UserId}",
                    userId
                );
                return Result<EmpresaStatisticsDto>.Failure(
                    "Error al obtener estadísticas de Empresa"
                );
            }
        }

        public async Task<Result<ConsultorStatisticsDto>> GetConsultorStatisticsAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result<ConsultorStatisticsDto>.Failure("Usuario no encontrado");
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains(Constants.Roles.Consultor))
                {
                    return Result<ConsultorStatisticsDto>.Failure(
                        "Usuario no tiene rol de Consultor"
                    );
                }

                // For now, return placeholder data
                var consultorStats = new ConsultorStatisticsDto
                {
                    ProyectosActivos = new List<ProyectoActivoDto>(),
                    ReunionesProgramadas = new List<ReunionProgramadaDto>(),
                    InformesPendientes = new List<InformePendienteDto>(),
                    Estadisticas = new EstadisticasConsultorDto
                    {
                        TotalProyectos = 3,
                        ConsultoriasActivas = 2,
                        ProyectosCompletados = 8,
                        InformesPendientes = 2,
                        FacturacionMensual = 25000,
                        HorasFacturadas = 120,
                        ClientesSatisfechos = 4.8m,
                    },
                };

                return Result<ConsultorStatisticsDto>.Success(consultorStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving Consultor statistics for user {UserId}",
                    userId
                );
                return Result<ConsultorStatisticsDto>.Failure(
                    "Error al obtener estadísticas de Consultor"
                );
            }
        }

        public async Task<Result<CtcStatisticsDto>> GetCtcStatisticsAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result<CtcStatisticsDto>.Failure("Usuario no encontrado");
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains(Constants.Roles.CTC))
                {
                    return Result<CtcStatisticsDto>.Failure("Usuario no tiene rol de CTC");
                }

                // For now, return placeholder data
                var ctcStats = new CtcStatisticsDto
                {
                    EvaluacionesPendientes = new List<EvaluacionPendienteDto>(),
                    CertificacionesRecientes = new List<CertificacionRecienteDto>(),
                    EstadisticasRegionales = new EstadisticasRegionalesDto
                    {
                        TotalCertificaciones = 847,
                        CertificacionesPorPais = new Dictionary<string, int>
                        {
                            { "Guatemala", 186 },
                            { "Costa Rica", 203 },
                            { "Panamá", 142 },
                            { "Honduras", 98 },
                            { "El Salvador", 127 },
                            { "Belice", 45 },
                            { "Nicaragua", 46 },
                        },
                        CertificacionesPorNivel = new Dictionary<string, int>
                        {
                            { "azul", 421 },
                            { "rojo", 298 },
                            { "verde", 128 },
                        },
                        EvaluacionesPendientes = 23,
                        CertificacionesPorVencer = 15,
                        NuevasCertificacionesMes = 12,
                        TasaAprobacion = 87.5m,
                    },
                    AlertasImportantes = new List<AlertaImportanteDto>(),
                    TendenciasCertificacion = new List<TendenciaCertificacionDto>(),
                };

                return Result<CtcStatisticsDto>.Success(ctcStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving CTC statistics for user {UserId}", userId);
                return Result<CtcStatisticsDto>.Failure("Error al obtener estadísticas de CTC");
            }
        }

        public async Task<Result<EmpresaAuditoraStatisticsDto>> GetEmpresaAuditoraStatisticsAsync(
            string userId
        )
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result<EmpresaAuditoraStatisticsDto>.Failure("Usuario no encontrado");
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains(Constants.Roles.EmpresaAuditora))
                {
                    return Result<EmpresaAuditoraStatisticsDto>.Failure(
                        "Usuario no tiene rol de Empresa Auditora"
                    );
                }

                // Get statistics for country (same logic as TécnicoPais)
                var empresasPorPais = await GetEmpresasPorPaisAsync(user);
                var empresasPorTipologia = await GetEmpresasPorTipologiaAsync(user);

                // Get user statistics from ProcesoCertificacion
                var userStatistics = await GetEmpresaAuditoraUserStatisticsAsync(userId);

                // Get recent activities for the country
                var actividadesRecientes = await GetRecentActivitiesForCountryAsync(user);

                // Build final statistics
                var empresaAuditoraStats = new EmpresaAuditoraStatisticsDto
                {
                    AuditoresActivos = new List<AuditorActivoDto>(),
                    AuditoriasProgamadas = await GetAuditoriasProgamadasAsync(userId),
                    AuditoriasCompletadas = await GetAuditoriasCompletadasAsync(userId),
                    Estadisticas = userStatistics,
                    AlertasGestion = new List<AlertaGestionDto>(),
                    DistribucionTrabajo = new List<DistribucionTrabajoDto>(),
                    EmpresasPorPais = empresasPorPais,
                    EmpresasPorTipologia = empresasPorTipologia,
                    ActividadesRecientes = actividadesRecientes,
                };

                return Result<EmpresaAuditoraStatisticsDto>.Success(empresaAuditoraStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving Empresa Auditora statistics for user {UserId}",
                    userId
                );
                return Result<EmpresaAuditoraStatisticsDto>.Failure(
                    "Error al obtener estadísticas de Empresa Auditora"
                );
            }
        }

        private async Task<List<CountryStatDto>> GetEmpresasPorPaisAsync(ApplicationUser user)
        {
            var empresaQuery = _db.Empresa.Where(e => e.Active);

            // Filter by user's country
            if (user.PaisId.HasValue)
            {
                empresaQuery = empresaQuery.Where(e => e.IdPais == user.PaisId);
            }

            return await empresaQuery
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
        }

        private async Task<List<TypologyStatDto>> GetEmpresasPorTipologiaAsync(ApplicationUser user)
        {
            var empresaQuery = _db.Empresa.Where(e => e.Active);

            // Filter by user's country
            if (user.PaisId.HasValue)
            {
                empresaQuery = empresaQuery.Where(e => e.IdPais == user.PaisId);
            }

            return await empresaQuery
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
        }

        private async Task<EstadisticasEmpresaAuditoraDto> GetEmpresaAuditoraUserStatisticsAsync(
            string userId
        )
        {
            var procesosQuery = _db.ProcesoCertificacion.Where(p => p.Enabled && p.AuditorId == userId);

            var totalAuditorias = await procesosQuery.CountAsync();

            var auditoriasProgamadas = await procesosQuery
                .Where(p => p.FechaFijadaAuditoria.HasValue && p.FechaFijadaAuditoria > DateTime.Now)
                .CountAsync();

            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var auditoriasCompletadasMes = await procesosQuery
                .Where(p =>
                    p.FechaFinalizacion.HasValue
                    && p.FechaFinalizacion >= startOfMonth
                    && (p.Status == ProcessStatusText.Spanish.AuditCompleted
                        || p.Status == ProcessStatusText.Spanish.Completed)
                )
                .CountAsync();

            var totalCompletadas = await procesosQuery
                .Where(p =>
                    p.FechaFinalizacion.HasValue
                    && (p.Status == ProcessStatusText.Spanish.AuditCompleted
                        || p.Status == ProcessStatusText.Spanish.Completed)
                )
                .CountAsync();

            var tasaAprobacion = totalCompletadas > 0
                ? (decimal)totalCompletadas / totalAuditorias * 100
                : 0;

            var tiempoPromedio = await CalculateAverageCertificationTime(
                procesosQuery,
                totalCompletadas,
                ProcessStatusText.Spanish.AuditCompleted
            );

            return new EstadisticasEmpresaAuditoraDto
            {
                TotalAuditores = 0, // Would need separate query for auditors under this empresa auditora
                AuditoresActivos = 0,
                AuditoriasEsteDate = auditoriasProgamadas,
                AuditoriasCompletadasMes = auditoriasCompletadasMes,
                TasaAprobacion = tasaAprobacion,
                TiempoPromedioAuditoria = tiempoPromedio,
                CertificacionesVigentes = totalCompletadas,
                IngresosMes = 0, // Would need financial data
            };
        }

        private async Task<List<AuditoriaProgramadaDto>> GetAuditoriasProgamadasAsync(string userId)
        {
            return await _db
                .ProcesoCertificacion.Where(p =>
                    p.Enabled
                    && p.AuditorId == userId
                    && p.FechaFijadaAuditoria.HasValue
                    && p.FechaFijadaAuditoria > DateTime.Now
                )
                .Include(p => p.Empresa)
                .Include(p => p.AuditorProceso)
                .OrderBy(p => p.FechaFijadaAuditoria)
                .Take(10)
                .Select(p => new AuditoriaProgramadaDto
                {
                    Id = p.Id,
                    Empresa = p.Empresa.Nombre,
                    Auditor = p.AuditorProceso != null
                        ? $"{p.AuditorProceso.FirstName} {p.AuditorProceso.LastName}"
                        : "Sin asignar",
                    Fecha = p.FechaFijadaAuditoria.Value,
                    Tipo = p.Recertificacion ? "Recertificación" : "Auditoría inicial",
                    Nivel = p.Empresa.ResultadoActual ?? "N/A",
                    Duracion = 0,
                    Estado = p.Status,
                    Pais = p.Empresa.Pais != null ? p.Empresa.Pais.Name : "N/A",
                })
                .ToListAsync();
        }

        private async Task<List<AuditoriaCompletadaDto>> GetAuditoriasCompletadasAsync(string userId)
        {
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            return await _db
                .ProcesoCertificacion.Where(p =>
                    p.Enabled
                    && p.AuditorId == userId
                    && p.FechaFinalizacion.HasValue
                    && p.FechaFinalizacion >= startOfMonth
                    && (p.Status == ProcessStatusText.Spanish.AuditCompleted
                        || p.Status == ProcessStatusText.Spanish.Completed)
                )
                .Include(p => p.Empresa)
                .Include(p => p.AuditorProceso)
                .OrderByDescending(p => p.FechaFinalizacion)
                .Take(10)
                .Select(p => new AuditoriaCompletadaDto
                {
                    Id = p.Id,
                    Empresa = p.Empresa.Nombre,
                    Auditor = p.AuditorProceso != null
                        ? $"{p.AuditorProceso.FirstName} {p.AuditorProceso.LastName}"
                        : "N/A",
                    FechaCompletada = p.FechaFinalizacion.Value,
                    Resultado = p.Empresa.ResultadoActual ?? "N/A",
                    Puntuacion = 0,
                    Nivel = p.Empresa.ResultadoActual ?? "N/A",
                    InformeEntregado = !string.IsNullOrEmpty(p.NumeroExpediente),
                })
                .ToListAsync();
        }

        private async Task<List<RecentActivityDto>> GetRecentActivitiesForCountryAsync(
            ApplicationUser user
        )
        {
            var activities = new List<RecentActivityDto>();

            var empresaQuery = _db.Empresa.Where(e => e.Active);
            var procesosQuery = _db.ProcesoCertificacion.Where(p => p.Enabled);

            if (user.PaisId.HasValue)
            {
                empresaQuery = empresaQuery.Where(e => e.IdPais == user.PaisId);
                procesosQuery = procesosQuery.Include(p => p.Empresa).Where(p => p.Empresa.IdPais == user.PaisId);
            }

            // Recent company registrations
            var registeredCompanies = await empresaQuery
                .OrderByDescending(e => e.Id)
                .Take(3)
                .Select(e => new RecentActivityDto
                {
                    Id = e.Id,
                    Title = "Nueva empresa registrada",
                    Description = $"{e.Nombre} se registró para certificación",
                    Timestamp = DateTime.Now.AddDays(-e.Id % 30),
                    Type = "company_registered",
                    Icon = "business",
                    Priority = "info",
                    CompanyId = e.Id,
                    CompanyName = e.Nombre,
                })
                .ToListAsync();

            // Recent certification completions
            var completedCertifications = await procesosQuery
                .Where(p =>
                    (p.Status == ProcessStatusText.Spanish.AuditCompleted
                        || p.Status == ProcessStatusText.Spanish.Completed)
                    && p.FechaFinalizacion.HasValue
                )
                .Include(p => p.Empresa)
                .OrderByDescending(p => p.FechaFinalizacion)
                .Take(3)
                .Select(p => new RecentActivityDto
                {
                    Id = p.Id,
                    Title = "Auditoría completada",
                    Description = $"{p.Empresa.Nombre} completó auditoría",
                    Timestamp = p.FechaFinalizacion.Value,
                    Type = "audit_completed",
                    Icon = "verified",
                    Priority = "success",
                    CompanyId = p.EmpresaId,
                    CompanyName = p.Empresa.Nombre,
                    Distintivo = p.Empresa.ResultadoActual,
                })
                .ToListAsync();

            // Upcoming audits
            var scheduledAudits = await procesosQuery
                .Where(p => p.FechaFijadaAuditoria.HasValue && p.FechaFijadaAuditoria > DateTime.Now)
                .Include(p => p.Empresa)
                .OrderBy(p => p.FechaFijadaAuditoria)
                .Take(3)
                .Select(p => new RecentActivityDto
                {
                    Id = p.Id,
                    Title = "Auditoría reprogramada",
                    Description = $"Auditoría de {p.Empresa.Nombre} movida al próximo mes",
                    Timestamp = p.UpdatedAt ?? p.CreatedAt ?? DateTime.Now,
                    Type = "audit_scheduled",
                    Icon = "event",
                    Priority = "warning",
                    CompanyId = p.EmpresaId,
                    CompanyName = p.Empresa.Nombre,
                    AuditDate = p.FechaFijadaAuditoria,
                })
                .ToListAsync();

            activities.AddRange(registeredCompanies);
            activities.AddRange(completedCertifications);
            activities.AddRange(scheduledAudits);

            return activities.OrderByDescending(a => a.Timestamp).Take(10).ToList();
        }

        // Helper methods for building empresa statistics from real data
        private CertificacionActualDto BuildCertificacionActual(EmpresaUpdateVm empresaData)
        {
            if (empresaData.CertificacionActual != null)
            {
                var cert = empresaData.CertificacionActual;
                return new CertificacionActualDto
                {
                    Id = cert.Id,
                    Nivel = DetermineNivelFromResultado(cert.Resultado),
                    Estado = DetermineEstadoFromStatus(cert.Status),
                    FechaEmision = DateTime.TryParse(cert.FechaInicio, out var fechaInicio)
                        ? fechaInicio
                        : DateTime.Now,
                    FechaVencimiento = DateTime.TryParse(cert.FechaVencimiento, out var fechaVenc)
                        ? fechaVenc
                        : DateTime.Now.AddYears(2),
                    NumeroCertificado = cert.Expediente ?? "N/A",
                    Puntuacion = 0, // CertificacionDetailsVm doesn't have this property
                    DiasVigencia = DateTime.TryParse(cert.FechaVencimiento, out var fv)
                        ? (int)(fv - DateTime.Now).TotalDays
                        : 0,
                };
            }

            // Return default if no current certification
            return new CertificacionActualDto
            {
                Id = 0,
                Nivel = "inicial",
                Estado = "pending",
                FechaEmision = DateTime.Now,
                FechaVencimiento = DateTime.Now.AddYears(2),
                NumeroCertificado = "En proceso",
                Puntuacion = 0,
                DiasVigencia = 0,
            };
        }

        private async Task<List<TareaPendienteDto>> BuildTareasPendientes(
            EmpresaUpdateVm empresaData
        )
        {
            var tareas = new List<TareaPendienteDto>();

            // Get current certification process for this company
            var proceso = await _db
                .ProcesoCertificacion.Where(p => p.EmpresaId == empresaData.Id && p.Enabled)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (proceso != null)
            {
                // Add tasks based on current process status
                switch (proceso.Status)
                {
                    case var status when status.Contains(ProcessStatusText.Spanish.Initial):
                        tareas.Add(
                            new TareaPendienteDto
                            {
                                Id = 1,
                                Titulo = "Asesoría pendiente",
                                Descripcion = "Su proceso está esperando asignación de asesor",
                                FechaVencimiento = DateTime.Now.AddMonths(6),
                                Prioridad = "media",
                                Tipo = "asesoria",
                            }
                        );
                        break;
                    case var status
                        when status.Contains(ProcessStatusText.Spanish.AuditCompletedNoNumber):
                        tareas.Add(
                            new TareaPendienteDto
                            {
                                Id = 2,
                                Titulo = "Auditoría Pendiente",
                                Descripcion = "Su proceso está esperando asignación de auditor",
                                FechaVencimiento = DateTime.Now.AddMonths(6),
                                Prioridad = "alta",
                                Tipo = "auditoria",
                            }
                        );
                        break;
                }
            }

            return tareas;
        }

        private List<DocumentoRecienteDto> BuildDocumentosRecientes(
            EmpresaUpdateVm empresaData,
            string userId
        )
        {
            var documentos = new List<DocumentoRecienteDto>();

            if (empresaData.Archivos != null && empresaData.Archivos.Any())
            {
                documentos = empresaData
                    .Archivos.OrderByDescending(a =>
                        DateTime.TryParse(a.FechaCarga, out var fecha) ? fecha : DateTime.MinValue
                    )
                    .Where(f => f.Cargador == userId)
                    .Take(5)
                    .Select(a => new DocumentoRecienteDto
                    {
                        Id = a.Id,
                        Titulo = a.Nombre,
                        Tipo = a.Tipo ?? "documento",
                        FechaSubida = DateTime.TryParse(a.FechaCarga, out var fecha)
                            ? fecha
                            : DateTime.Now,
                        Estado = "ok", // Default since ArchivoVm doesn't have approval status
                        Url = a.Ruta,
                    })
                    .ToList();
            }

            return documentos;
        }

        private async Task<List<ProximaActividadDto>> BuildProximasActividades(
            EmpresaUpdateVm empresaData
        )
        {
            var actividades = new List<ProximaActividadDto>();

            // Get current certification process
            var proceso = await _db
                .ProcesoCertificacion.Where(p => p.EmpresaId == empresaData.Id && p.Enabled)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (proceso != null)
            {
                // Add activities based on process dates
                if (
                    proceso.FechaFijadaAuditoria.HasValue
                    && proceso.FechaFijadaAuditoria > DateTime.Now
                )
                {
                    actividades.Add(
                        new ProximaActividadDto
                        {
                            Tipo = "auditoria",
                            Descripcion = "Auditoría de certificación programada",
                            Fecha = proceso.FechaFijadaAuditoria.Value,
                            Auditor = proceso.AuditorId ?? "Por asignar",
                        }
                    );
                }

                if (proceso.FechaVencimiento.HasValue && proceso.FechaVencimiento > DateTime.Now)
                {
                    var diasParaVencer = (proceso.FechaVencimiento.Value - DateTime.Now).TotalDays;
                    if (diasParaVencer <= 90) // Alert if expiring within 3 months
                    {
                        actividades.Add(
                            new ProximaActividadDto
                            {
                                Tipo = "renovacion",
                                Descripcion =
                                    $"Su certificación vence en {diasParaVencer:F0} días - Iniciar proceso de renovación",
                                Fecha = proceso.FechaVencimiento.Value.AddDays(-30), // Start renewal 30 days before
                                Auditor = "N/A",
                            }
                        );
                    }
                }
            }

            return actividades.OrderBy(a => a.Fecha).ToList();
        }

        private EstadisticasGeneralesDto BuildEstadisticasGenerales(EmpresaUpdateVm empresaData)
        {
            // Calculate process completion percentage based on status
            int procesoCompletado = CalculateProcessCompletion(empresaData.Estado);

            int documentosAprobados = empresaData.Archivos?.Count ?? 0; // ArchivoVm doesn't have Aprobado property
            int totalCertificaciones = empresaData.Certificaciones?.Count ?? 0;

            return new EstadisticasGeneralesDto
            {
                ProcesoCompletado = procesoCompletado,
                DocumentosAprobados = documentosAprobados,
                CapacitacionesCompletadas = totalCertificaciones, // Using certifications as training proxy
                ProximaEvaluacion = CalculateNextEvaluation(empresaData),
            };
        }

        private string DetermineNivelFromResultado(string resultado)
        {
            if (string.IsNullOrEmpty(resultado))
                return "inicial";

            if (resultado.Contains("Verde") || resultado.Contains("Green"))
                return "verde";
            if (resultado.Contains("Azul") || resultado.Contains("Blue"))
                return "azul";
            if (resultado.Contains("Rojo") || resultado.Contains("Red"))
                return "rojo";

            return "inicial";
        }

        private string DetermineEstadoFromStatus(string status)
        {
            if (string.IsNullOrEmpty(status))
                return "pending";

            return status.ToLower();
        }

        private int CalculateProcessCompletion(decimal estado)
        {
            // Map estado decimal to percentage
            // Based on ProcessStatus constants: 0=Initial, 1=ForConsulting, etc.
            return (int)((estado / 8.0m) * 100); // 8 is max status (Completed)
        }

        private DateTime CalculateNextEvaluation(EmpresaUpdateVm empresaData)
        {
            if (
                !string.IsNullOrEmpty(empresaData.CertificacionActual?.FechaVencimiento)
                && DateTime.TryParse(
                    empresaData.CertificacionActual.FechaVencimiento,
                    out var fechaVenc
                )
            )
            {
                // Next evaluation typically 6 months before expiration
                return fechaVenc.AddMonths(-6);
            }

            // Default to 180 days from now
            return DateTime.Now.AddDays(180);
        }
    }
}
