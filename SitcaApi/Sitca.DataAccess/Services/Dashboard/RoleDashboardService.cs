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
            var query = _db.ProcesoCertificacion.Where(p => p.Enabled);

            if (isAuditor)
            {
                query = query.Where(p => p.AuditorId == userId);
            }
            else if (isAsesor)
            {
                query = query.Where(p => p.AsesorId == userId);
            }

            string inProcess = isAsesor
                ? ProcessStatusText.Spanish.ForConsulting
                : ProcessStatusText.Spanish.AuditingUnderway;
            string proccessFinish = isAsesor
                ? ProcessStatusText.Spanish.ConsultancyCompleted
                : ProcessStatusText.Spanish.AuditCompleted;

            var procesosAsignados = await query.CountAsync();
            var certificacionesPendientes = await query
                .Where(p => p.Status == inProcess)
                .CountAsync();
            var certificacionesCompletadas = await query
                .Where(p => p.Status == proccessFinish)
                .CountAsync();

            var auditoriasProximas = await query
                .Where(p =>
                    p.FechaFijadaAuditoria.HasValue
                    && p.FechaFijadaAuditoria > DateTime.Now
                    && p.FechaFijadaAuditoria <= DateTime.Now.AddDays(30)
                )
                .Include(p => p.Empresa)
                .OrderBy(p => p.FechaFijadaAuditoria)
                .Take(5)
                .Select(p => new AuditoriaEsteDate
                {
                    EmpresaId = p.EmpresaId,
                    NombreEmpresa = p.Empresa.Nombre,
                    FechaAuditoria = p.FechaFijadaAuditoria.Value,
                    Tipo = p.Recertificacion ? "recertificacion" : "auditoria_inicial",
                })
                .ToListAsync();

            var promedioTiempo = await CalculateAverageCertificationTime(
                query,
                certificacionesCompletadas,
                proccessFinish
            );

            return new AsesorAuditorStatisticsDto
            {
                ProcesosAsignados = procesosAsignados,
                CertificacionesPendientes = certificacionesPendientes,
                AuditoriasEsteDate = auditoriasProximas,
                EstadisticasPersonales = new EstadisticasPersonalesDto
                {
                    CertificacionesCompletadas = certificacionesCompletadas,
                    PromedioTiempoCertificacion = promedioTiempo,
                    SatisfaccionClientes = await CalculateCustomerSatisfaction(userId),
                },
            };
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

                // For now, return placeholder data
                var empresaAuditoraStats = new EmpresaAuditoraStatisticsDto
                {
                    AuditoresActivos = new List<AuditorActivoDto>(),
                    AuditoriasProgamadas = new List<AuditoriaProgramadaDto>(),
                    AuditoriasCompletadas = new List<AuditoriaCompletadaDto>(),
                    Estadisticas = new EstadisticasEmpresaAuditoraDto
                    {
                        TotalAuditores = 15,
                        AuditoresActivos = 12,
                        AuditoriasEsteDate = 8,
                        AuditoriasCompletadasMes = 23,
                        TasaAprobacion = 89.5m,
                        TiempoPromedioAuditoria = 6,
                        CertificacionesVigentes = 14,
                        IngresosMes = 45000,
                    },
                    AlertasGestion = new List<AlertaGestionDto>(),
                    DistribucionTrabajo = new List<DistribucionTrabajoDto>(),
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
