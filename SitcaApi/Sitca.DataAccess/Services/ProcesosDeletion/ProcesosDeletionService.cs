using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data;
using Sitca.Models;
using Sitca.Models.DTOs;
using Utilities.Common;

namespace Sitca.DataAccess.Services.ProcesosDeletion;

public class ProcesosDeletionService : IProcesosDeletionService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ProcesosDeletionService> _logger;

    public ProcesosDeletionService(ApplicationDbContext db, ILogger<ProcesosDeletionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<ProcesosDeletionInfo>> CanDeleteProcesoAsync(
        int procesoId,
        int paisId,
        string userRole
    )
    {
        try
        {
            var proceso = await _db
                .ProcesoCertificacion.AsNoTracking()
                .Include(p => p.Empresa)
                .FirstOrDefaultAsync(p => p.Id == procesoId);

            if (proceso == null)
            {
                return Result<ProcesosDeletionInfo>.Failure(
                    $"Proceso con ID {procesoId} no encontrado"
                );
            }

            // Verificar permisos
            if (!HasDeletePermission(proceso, paisId, userRole))
            {
                return Result<ProcesosDeletionInfo>.Failure(
                    "No tiene permisos para eliminar este proceso"
                );
            }

            // Contar dependencias
            var info = new ProcesosDeletionInfo
            {
                ProcesoId = procesoId,
                ProcesoExpediente = proceso.NumeroExpediente,
                TotalCuestionarios = await _db.Cuestionario.CountAsync(c =>
                    c.ProcesoCertificacionId == procesoId
                ),
                TotalResultados = await _db.ResultadoCertificacion.CountAsync(r =>
                    r.CertificacionId == procesoId
                ),
                TotalArchivos = await _db.ProcesoArchivos.CountAsync(a =>
                    a.ProcesoCertificacionId == procesoId
                ),
                TotalHomologaciones = await _db.Homologacion.CountAsync(h =>
                    h.CertificacionId == procesoId
                ),
            };

            // Determinar si se puede eliminar
            info.CanDelete = true;
            info.Dependencies = new List<string>();

            if (info.TotalCuestionarios > 0)
                info.Dependencies.Add($"{info.TotalCuestionarios} cuestionario(s)");

            if (info.TotalResultados > 0)
                info.Dependencies.Add($"{info.TotalResultados} resultado(s) de certificación");

            if (info.TotalArchivos > 0)
                info.Dependencies.Add($"{info.TotalArchivos} archivo(s) de proceso");

            if (info.TotalHomologaciones > 0)
                info.Dependencies.Add($"{info.TotalHomologaciones} homologación(es)");

            return Result<ProcesosDeletionInfo>.Success(info);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al verificar si se puede eliminar el proceso {ProcesoId}",
                procesoId
            );
            return Result<ProcesosDeletionInfo>.Failure(
                $"Error al verificar dependencias: {ex.Message}"
            );
        }
    }

    public async Task<Result<ProcesosDeletionResult>> DeleteProcesoWithRelatedEntitiesAsync(
        int procesoId,
        int paisId,
        string userRole
    )
    {
        var result = new ProcesosDeletionResult { ProcesoId = procesoId, Success = false };

        try
        {
            var proceso = await _db
                .ProcesoCertificacion.Include(p => p.Empresa)
                .FirstOrDefaultAsync(p => p.Id == procesoId);

            if (proceso == null)
            {
                return Result<ProcesosDeletionResult>.Failure(
                    $"Proceso con ID {procesoId} no encontrado"
                );
            }

            result.ProcesoExpediente = proceso.NumeroExpediente;

            // Verificar permisos
            if (!HasDeletePermission(proceso, paisId, userRole))
            {
                return Result<ProcesosDeletionResult>.Failure(
                    "No tiene permisos para eliminar este proceso"
                );
            }

            // Usar transacción para garantizar integridad
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // 1. Eliminar cuestionarios y sus dependencias
                    await DeleteCuestionariosAsync(procesoId, result);

                    // 2. Eliminar resultados de certificación
                    await DeleteResultadosCertificacionAsync(procesoId, result);

                    // 3. Eliminar archivos del proceso
                    await DeleteProcesoArchivosAsync(procesoId, result);

                    // 4. Eliminar homologaciones
                    await DeleteHomologacionesAsync(procesoId, result);

                    // 5. Finalmente eliminar el proceso
                    _db.ProcesoCertificacion.Remove(proceso);
                    await _db.SaveChangesAsync();

                    await transaction.CommitAsync();

                    result.Success = true;
                    result.DeletedEntities.Add($"Proceso: {proceso.NumeroExpediente}");

                    _logger.LogInformation(
                        "Proceso {ProcesoId} eliminado exitosamente con todas sus dependencias",
                        procesoId
                    );

                    return Result<ProcesosDeletionResult>.Success(result);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result.Errors.Add(ex.Message);
                    _logger.LogError(ex, "Error al eliminar proceso {ProcesoId}", procesoId);
                    return Result<ProcesosDeletionResult>.Failure(
                        $"Error durante la eliminación: {ex.Message}"
                    );
                }
            });
        }
        catch (Exception ex)
        {
            result.Errors.Add(ex.Message);
            _logger.LogError(ex, "Error general al eliminar proceso {ProcesoId}", procesoId);
            return Result<ProcesosDeletionResult>.Failure(
                $"Error al eliminar proceso: {ex.Message}"
            );
        }
    }

    private async Task DeleteCuestionariosAsync(int procesoId, ProcesosDeletionResult result)
    {
        // Obtener IDs de cuestionarios para evitar problemas de tracking
        var cuestionarioIds = await _db
            .Cuestionario.AsNoTracking()
            .Where(c => c.ProcesoCertificacionId == procesoId)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var cuestionarioId in cuestionarioIds)
        {
            // Eliminar items del cuestionario usando IDs
            var itemIds = await _db
                .CuestionarioItem.AsNoTracking()
                .Where(ci => ci.CuestionarioId == cuestionarioId)
                .Select(ci => ci.Id)
                .ToListAsync();

            if (itemIds.Any())
            {
                // Eliminar observaciones primero
                var observaciones = await _db
                    .CuestionarioItemObservaciones
                    .Where(cio => itemIds.Contains(cio.CuestionarioItemId))
                    .ToListAsync();

                _db.CuestionarioItemObservaciones.RemoveRange(observaciones);

                // Eliminar historial de items
                var itemsHistory = await _db
                    .CuestionarioItemHistories
                    .Where(cih => itemIds.Contains(cih.CuestionarioItemId))
                    .ToListAsync();

                _db.CuestionarioItemHistories.RemoveRange(itemsHistory);

                // Eliminar items
                var items = await _db
                    .CuestionarioItem
                    .Where(ci => ci.CuestionarioId == cuestionarioId)
                    .ToListAsync();

                _db.CuestionarioItem.RemoveRange(items);
            }
        }

        // Eliminar cuestionarios
        var cuestionarios = await _db
            .Cuestionario
            .Where(c => c.ProcesoCertificacionId == procesoId)
            .ToListAsync();

        _db.Cuestionario.RemoveRange(cuestionarios);
        
        // Guardar cambios para limpiar el tracking context
        await _db.SaveChangesAsync();
        
        result.QuestionnairesDeleted = cuestionarios.Count;
        result.DeletedEntities.Add($"{cuestionarios.Count} cuestionario(s) con sus items");
    }

    private async Task DeleteResultadosCertificacionAsync(
        int procesoId,
        ProcesosDeletionResult result
    )
    {
        var resultados = await _db
            .ResultadoCertificacion.Where(r => r.CertificacionId == procesoId)
            .ToListAsync();

        _db.ResultadoCertificacion.RemoveRange(resultados);
        await _db.SaveChangesAsync();
        
        result.ResultsDeleted = resultados.Count;
        result.DeletedEntities.Add($"{resultados.Count} resultado(s) de certificación");
    }

    private async Task DeleteProcesoArchivosAsync(int procesoId, ProcesosDeletionResult result)
    {
        var archivos = await _db
            .ProcesoArchivos.Where(a => a.ProcesoCertificacionId == procesoId)
            .ToListAsync();

        _db.ProcesoArchivos.RemoveRange(archivos);
        await _db.SaveChangesAsync();
        
        result.FilesDeleted = archivos.Count;
        result.DeletedEntities.Add($"{archivos.Count} archivo(s) de proceso");
    }

    private async Task DeleteHomologacionesAsync(int procesoId, ProcesosDeletionResult result)
    {
        var homologaciones = await _db
            .Homologacion.Where(h => h.CertificacionId == procesoId)
            .ToListAsync();

        _db.Homologacion.RemoveRange(homologaciones);
        await _db.SaveChangesAsync();
        
        result.HomologacionesDeleted = homologaciones.Count;
        if (homologaciones.Count > 0)
        {
            result.DeletedEntities.Add($"{homologaciones.Count} homologación(es)");
        }
    }

    private bool HasDeletePermission(ProcesoCertificacion proceso, int paisId, string userRole)
    {
        // Admin puede eliminar cualquier proceso
        if (userRole == Constants.Roles.Admin)
            return true;

        // TecnicoPais solo puede eliminar procesos de empresas de su país
        if (userRole == Constants.Roles.TecnicoPais)
            return proceso.Empresa.PaisId == paisId;

        return false;
    }
}

