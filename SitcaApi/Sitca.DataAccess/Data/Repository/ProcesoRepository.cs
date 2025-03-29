using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.Constants;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.DataAccess.Extensions;
using Sitca.DataAccess.Services.ProcessQuery;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.Mappers;
using Sitca.Models.ViewModels;
using static Utilities.Common.Constants;

namespace Sitca.DataAccess.Data.Repository;

public class ProcesoRepository : Repository<ProcesoCertificacion>, IProcesoRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IProcessQueryBuilder _queryBuilder;
    private readonly IConfiguration _config;
    private readonly ILogger<ProcesoRepository> _logger;

    public ProcesoRepository(
        ApplicationDbContext db,
        IProcessQueryBuilder queryBuilder,
        IConfiguration configuration,
        ILogger<ProcesoRepository> logger
    )
        : base(db)
    {
        _db = db;
        _queryBuilder = queryBuilder;
        _config = configuration;
        _logger = logger;
    }

    public async Task<Result<ProcesoCertificacionDTO>> GetProcesoForIdAsync(int id, string userId)
    {
        try
        {
            // Carga el proceso junto con todas sus relaciones necesarias en una sola consulta
            var proceso = await _db
                .ProcesoCertificacion.AsNoTracking()
                .Include(p => p.Empresa)
                .ThenInclude(e => e.Pais)
                .Include(p => p.Tipologia)
                .Include(p => p.AsesorProceso)
                .Include(p => p.AuditorProceso)
                .Include(p => p.UserGenerador)
                .Include(p => p.ProcesosArchivos.Where(a => a.Enabled))
                .ThenInclude(a => a.UserCreate)
                .Include(p => p.Cuestionarios)
                .FirstOrDefaultAsync(p => p.Id == id && p.Enabled != false);

            // Si no se encuentra el proceso, devuelve un error
            if (proceso == null)
            {
                _logger.LogWarning(
                    "No se encontró el proceso de certificación con ID: {ProcesoId}",
                    id
                );
                return Result<ProcesoCertificacionDTO>.Failure(
                    $"No se encontró el proceso de certificación con ID: {id}"
                );
            }

            // Mapea el proceso a DTO
            var procesoDto = proceso.ToDto(userId);

            // Registra el acceso exitoso en los logs
            _logger.LogInformation(
                "Proceso de certificación obtenido exitosamente: {ProcesoId}",
                id
            );

            // Devuelve el resultado exitoso
            return Result<ProcesoCertificacionDTO>.Success(procesoDto);
        }
        catch (Exception ex)
        {
            // Registra cualquier error que ocurra
            _logger.LogError(ex, "Error al obtener el proceso de certificación: {ProcesoId}", id);
            return Result<ProcesoCertificacionDTO>.Failure(
                $"Error al obtener el proceso de certificación: {ex.Message}"
            );
        }
    }

    public async Task<Result<ExpedienteDTO>> UpdateCaseNumberAsync(
        ExpedienteDTO expediente,
        string userId
    )
    {
        if (expediente == null)
            return Result<ExpedienteDTO>.Failure("Los datos de certificación son requeridos");

        if (string.IsNullOrWhiteSpace(expediente.Expediente))
            return Result<ExpedienteDTO>.Failure("El número de expediente es requerido");

        var strategy = _db.Database.CreateExecutionStrategy();

        try
        {
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    var certificacion = await _db.ProcesoCertificacion.FirstOrDefaultAsync(c =>
                        c.Id == expediente.Id
                    );

                    if (certificacion == null)
                        return Result<ExpedienteDTO>.Failure(
                            $"No se encontró la certificación con ID {expediente.Id}"
                        );

                    certificacion.NumeroExpediente = expediente.Expediente;
                    certificacion.UpdatedBy = userId;
                    certificacion.UpdatedAt = DateTime.UtcNow;

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation(
                        "Número de expediente actualizado para certificación {CertificacionId}: {NumeroExpediente}",
                        expediente.Id,
                        expediente.Expediente
                    );

                    return Result<ExpedienteDTO>.Success(expediente);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al actualizar número de expediente para certificación {CertificacionId}. Error: {Error}",
                expediente.Id,
                ex.Message
            );
            return Result<ExpedienteDTO>.Failure(
                $"Error al actualizar número de expediente: {ex.Message}"
            );
        }
    }

    public async Task<BlockResult<ProcesoCertificacionVm>> GetProcessesBlockAsync(
        ApplicationUser user,
        string role,
        CompanyFilterDTO filter,
        string language = "es"
    )
    {
        try
        {
            // Construir la consulta unificada basada en el rol
            var query = _queryBuilder.BuildUnifiedQuery(user, role, filter.IsRecetification);

            // Aplicar filtros adicionales
            query = _queryBuilder.ApplyFilters(query, filter, filter.DistinctiveId);

            // Ejecutar la proyección paginada
            return await _queryBuilder.BuildAndExecuteBlockProjection(
                query,
                language ?? "es",
                filter.BlockNumber,
                filter.BlockSize
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting process list with filter {@Filter} for role {Role}",
                filter,
                role
            );
            throw;
        }
    }

    public async Task<Result<ProcesoCertificacionDTO>> CrearRecertificacionAsync(
        int empresaId,
        string userId
    )
    {
        try
        {
            // Verificar si existe algún proceso habilitado en estado 8 (Finalizado)
            var procesoAnterior = await _db
                .ProcesoCertificacion.Include(p => p.Empresa)
                .Include(p => p.Tipologia)
                .FirstOrDefaultAsync(p =>
                    p.EmpresaId == empresaId
                    && p.Status == ProcessStatusText.Spanish.Completed
                    && p.Recertificacion
                    && p.Enabled
                );

            if (procesoAnterior == null)
            {
                _logger.LogWarning(
                    "No se encontró proceso finalizado y habilitado para la empresa ID: {EmpresaId}",
                    empresaId
                );
                return Result<ProcesoCertificacionDTO>.Failure(
                    $"No existe un proceso finalizado para la empresa con ID: {empresaId}"
                );
            }

            // Estrategia de ejecución para la transacción
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // Crear nuevo proceso como recertificación
                    var nuevoProceso = new ProcesoCertificacion
                    {
                        EmpresaId = empresaId,
                        FechaInicio = DateTime.UtcNow,
                        Recertificacion = true,
                        NumeroExpediente = "",
                        Status = ProcessStatusText.Spanish.Initial,
                        UserGeneraId = userId,
                        TipologiaId = procesoAnterior.TipologiaId,
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow,
                        Enabled = true,
                    };

                    _db.ProcesoCertificacion.Add(nuevoProceso);
                    await _db.SaveChangesAsync();

                    // Actualizar estado de la empresa
                    await ActualizarEstadoEmpresaAsync(
                        empresaId,
                        ProcessStatusText.Spanish.Initial
                    );

                    await transaction.CommitAsync();

                    _logger.LogInformation(
                        "Recertificación creada exitosamente para empresa {EmpresaId}, nuevo proceso ID: {ProcesoId}",
                        empresaId,
                        nuevoProceso.Id
                    );

                    // Mapear a DTO y devolver
                    var procesoDto = nuevoProceso.ToDto(userId);
                    return Result<ProcesoCertificacionDTO>.Success(procesoDto);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(
                        ex,
                        "Error al crear recertificación para empresa {EmpresaId}: {Error}",
                        empresaId,
                        ex.Message
                    );
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error no controlado al crear recertificación para empresa {EmpresaId}: {Error}",
                empresaId,
                ex.Message
            );
            return Result<ProcesoCertificacionDTO>.Failure(
                $"Error al crear recertificación: {ex.Message}"
            );
        }
    }

    public async Task<Result<ProcessStartedVm>> ComenzarProcesoAsesoriaAsync(
        ProcessStartedVm process,
        ApplicationUser userApp
    )
    {
        try
        {
            if (process == null)
                return Result<ProcessStartedVm>.Failure(
                    "Los datos de certificación son requeridos"
                );

            const int toStatus = ProcessStatus.ForConsulting;

            // Crear una estrategia de ejecución
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                var processDB = await _db
                    .ProcesoCertificacion.Include(p => p.Empresa)
                    .FirstOrDefaultAsync(p => p.Id == process.Id);

                if (processDB == null)
                    return Result<ProcessStartedVm>.Failure(
                        $"El proceso con ID {process.Id} no existe"
                    );

                if (processDB.Empresa == null)
                    return Result<ProcessStartedVm>.Failure(
                        $"La Empresa asociada al proceso {process.Id} no existe"
                    );

                var status = StatusConstants.GetLocalizedStatus(toStatus, "es");

                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // Actualizar proceso
                    processDB.UserGeneraId = userApp.Id;
                    processDB.UpdatedBy = userApp.Id;
                    processDB.UpdatedAt = DateTime.UtcNow;
                    processDB.Status = status;
                    processDB.AsesorId = process.AdviserId;

                    _db.ProcesoCertificacion.Update(processDB);

                    // Actualizar empresa
                    processDB.Empresa.Estado = toStatus;
                    processDB.Empresa.FechaAutoNotif = null;

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    process.GeneratorId = userApp.Id;
                    process.NewStatus = toStatus;
                    process.GeneratorName = $"{userApp.FirstName} {userApp.LastName}";

                    return Result<ProcessStartedVm>.Success(process);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al comenzar proceso con ID {ProcesoId}. Detalles: {Details}",
                process.Id,
                ex.Message
            );
            return Result<ProcessStartedVm>.Failure(
                $"Error al iniciar el proceso de certificación: {ex.Message}"
            );
        }
    }

    public async Task<Result<AsignaAuditoriaVm>> AsignarAuditorAsync(
        AsignaAuditoriaVm process,
        string user
    )
    {
        try
        {
            const int toStatus = ProcessStatus.ForAuditing;

            // Crear una estrategia de ejecución
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                var processDB = await _db
                    .ProcesoCertificacion.Include(p => p.Empresa)
                    .FirstOrDefaultAsync(p => p.Id == process.ProcesoId);

                if (processDB == null)
                    return Result<AsignaAuditoriaVm>.Failure(
                        $"El proceso con ID {process.ProcesoId} no existe"
                    );

                if (processDB.Empresa == null)
                    return Result<AsignaAuditoriaVm>.Failure(
                        $"La Empresa asociada al proceso {process.ProcesoId} no existe"
                    );

                var status = StatusConstants.GetLocalizedStatus(toStatus, "es");

                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // Actualizar proceso
                    processDB.AuditorId = process.AuditorId;
                    processDB.FechaFijadaAuditoria = process.Fecha.ToUniversal();
                    processDB.FechaSolicitudAuditoria = DateTime.UtcNow;

                    processDB.UpdatedBy = user;
                    processDB.UpdatedAt = DateTime.UtcNow;
                    processDB.Status = status;

                    _db.ProcesoCertificacion.Update(processDB);

                    // Actualizar empresa
                    processDB.Empresa.Estado = toStatus;
                    processDB.Empresa.FechaAutoNotif = null;

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Result<AsignaAuditoriaVm>.Success(process);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al asignar auditor {AuditorId} a proceso {ProcesoId}",
                process.AuditorId,
                process.ProcesoId
            );
            return Result<AsignaAuditoriaVm>.Failure("Error al asignar auditor");
        }
    }

    private async Task ActualizarEstadoEmpresaAsync(int empresaId, string nuevoEstado)
    {
        // Obtener el valor numérico del estado desde la cadena (por ejemplo, "0 - Inicial" -> 0)
        var estadoNumerico = int.Parse(nuevoEstado.Split(' ')[0]);

        var empresa = await _db.Empresa.FindAsync(empresaId);
        if (empresa != null)
        {
            // Solo actualizar si el nuevo estado es mayor que el actual
            if (!empresa.Estado.HasValue)
            {
                empresa.Estado = estadoNumerico;
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Estado de empresa {EmpresaId} actualizado a {NuevoEstado}",
                    empresaId,
                    nuevoEstado
                );
            }
        }
    }

    private static DateTime? ToDateUniversal(string fecha)
    {
        if (!string.IsNullOrEmpty(fecha))
        {
            try
            {
                var terminos = fecha.Split('-');
                return new DateTime(
                    Int32.Parse(terminos[0]),
                    Int32.Parse(terminos[1]),
                    Int32.Parse(terminos[2])
                );
            }
            catch (Exception)
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }
}
