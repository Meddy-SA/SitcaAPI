using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data;
using Sitca.Models;
using Sitca.Models.DTOs;
using Utilities.Common;

namespace Sitca.DataAccess.Services.EmpresaDeletion;

public class EmpresaDeletionService : IEmpresaDeletionService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<EmpresaDeletionService> _logger;

    public EmpresaDeletionService(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        ILogger<EmpresaDeletionService> logger
    )
    {
        _db = db;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<EmpresaDeletionInfo>> CanDeleteEmpresaAsync(
        int empresaId,
        int paisId,
        string userRole
    )
    {
        try
        {
            var empresa = await _db
                .Empresa.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == empresaId);

            if (empresa == null)
            {
                return Result<EmpresaDeletionInfo>.Failure(
                    $"Empresa con ID {empresaId} no encontrada"
                );
            }

            // Verificar permisos
            if (!HasDeletePermission(empresa, paisId, userRole))
            {
                return Result<EmpresaDeletionInfo>.Failure(
                    "No tiene permisos para eliminar esta empresa"
                );
            }

            // Contar dependencias
            var info = new EmpresaDeletionInfo
            {
                EmpresaId = empresaId,
                EmpresaNombre = empresa.Nombre,
                TotalProcesos = await _db.ProcesoCertificacion.CountAsync(p =>
                    p.EmpresaId == empresaId
                ),
                TotalCuestionarios = await _db.Cuestionario.CountAsync(c =>
                    c.IdEmpresa == empresaId
                ),
                TotalArchivos = await _db.Archivo.CountAsync(a => a.EmpresaId == empresaId),
                TotalUsuarios = await _db.Users.CountAsync(u => u.EmpresaId == empresaId),
                TotalHomologaciones = await _db.Homologacion.CountAsync(h =>
                    h.EmpresaId == empresaId
                ),
            };

            // Determinar si se puede eliminar
            info.CanDelete = true;
            info.Dependencies = new List<string>();

            if (info.TotalProcesos > 0)
                info.Dependencies.Add($"{info.TotalProcesos} proceso(s) de certificación");

            if (info.TotalCuestionarios > 0)
                info.Dependencies.Add($"{info.TotalCuestionarios} cuestionario(s)");

            if (info.TotalArchivos > 0)
                info.Dependencies.Add($"{info.TotalArchivos} archivo(s)");

            if (info.TotalUsuarios > 0)
                info.Dependencies.Add($"{info.TotalUsuarios} usuario(s)");

            if (info.TotalHomologaciones > 0)
                info.Dependencies.Add($"{info.TotalHomologaciones} homologación(es)");

            return Result<EmpresaDeletionInfo>.Success(info);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al verificar si se puede eliminar la empresa {EmpresaId}",
                empresaId
            );
            return Result<EmpresaDeletionInfo>.Failure(
                $"Error al verificar dependencias: {ex.Message}"
            );
        }
    }

    public async Task<Result<EmpresaDeletionResult>> DeleteEmpresaWithRelatedEntitiesAsync(
        int empresaId,
        int paisId,
        string userRole
    )
    {
        var result = new EmpresaDeletionResult { EmpresaId = empresaId, Success = false };

        try
        {
            var empresa = await _db.Empresa.FirstOrDefaultAsync(e => e.Id == empresaId);

            if (empresa == null)
            {
                return Result<EmpresaDeletionResult>.Failure(
                    $"Empresa con ID {empresaId} no encontrada"
                );
            }

            result.EmpresaNombre = empresa.Nombre;

            // Verificar permisos
            if (!HasDeletePermission(empresa, paisId, userRole))
            {
                return Result<EmpresaDeletionResult>.Failure(
                    "No tiene permisos para eliminar esta empresa"
                );
            }

            // Usar transacción para garantizar integridad
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // 1. Eliminar cuestionarios y sus items
                    await DeleteCuestionariosAsync(empresaId, result);

                    // 2. Eliminar procesos de certificación y sus dependencias
                    await DeleteProcesoCertificacionAsync(empresaId, result);

                    // 3. Eliminar archivos
                    await DeleteArchivosAsync(empresaId, result);

                    // 4. Eliminar homologaciones
                    await DeleteHomologacionesAsync(empresaId, result);

                    // 5. Eliminar tipologías asociadas
                    await DeleteTipologiasEmpresaAsync(empresaId, result);

                    // 6. Limpiar referencias de auditoría antes de eliminar usuarios
                    await ClearAuditReferencesAsync(empresaId, result);

                    // 7. Eliminar usuarios asociados (al final, después de eliminar todas las referencias)
                    await DeleteAssociatedUsersAsync(empresaId, result);

                    // 8. Finalmente eliminar la empresa
                    _db.Empresa.Remove(empresa);
                    await _db.SaveChangesAsync();

                    await transaction.CommitAsync();

                    result.Success = true;
                    result.DeletedEntities.Add($"Empresa: {empresa.Nombre}");

                    _logger.LogInformation(
                        "Empresa {EmpresaId} eliminada exitosamente con todas sus dependencias",
                        empresaId
                    );

                    return Result<EmpresaDeletionResult>.Success(result);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result.Errors.Add(ex.Message);
                    _logger.LogError(ex, "Error al eliminar empresa {EmpresaId}", empresaId);
                    return Result<EmpresaDeletionResult>.Failure(
                        $"Error durante la eliminación: {ex.Message}"
                    );
                }
            });
        }
        catch (Exception ex)
        {
            result.Errors.Add(ex.Message);
            _logger.LogError(ex, "Error general al eliminar empresa {EmpresaId}", empresaId);
            return Result<EmpresaDeletionResult>.Failure(
                $"Error al eliminar empresa: {ex.Message}"
            );
        }
    }

    private async Task DeleteAssociatedUsersAsync(int empresaId, EmpresaDeletionResult result)
    {
        var users = await _db.Users.Where(u => u.EmpresaId == empresaId).ToListAsync();

        foreach (var user in users)
        {
            var deleteResult = await _userManager.DeleteAsync(user);
            if (deleteResult.Succeeded)
            {
                result.UsersDeleted++;
                result.DeletedEntities.Add($"Usuario: {user.Email}");
            }
            else
            {
                var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
                result.Errors.Add($"Error eliminando usuario {user.Email}: {errors}");
                _logger.LogWarning("Error eliminando usuario {UserId}: {Errors}", user.Id, errors);
            }
        }
    }

    private async Task DeleteCuestionariosAsync(int empresaId, EmpresaDeletionResult result)
    {
        // Obtener IDs de cuestionarios para evitar problemas de tracking
        var cuestionarioIds = await _db
            .Cuestionario.AsNoTracking()
            .Where(c => c.IdEmpresa == empresaId)
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
            .Where(c => c.IdEmpresa == empresaId)
            .ToListAsync();

        _db.Cuestionario.RemoveRange(cuestionarios);
        
        // Guardar cambios para limpiar el tracking context
        await _db.SaveChangesAsync();
        
        result.QuestionnairesDeleted = cuestionarios.Count;
        result.DeletedEntities.Add($"{cuestionarios.Count} cuestionario(s) con sus items");
    }

    private async Task DeleteProcesoCertificacionAsync(int empresaId, EmpresaDeletionResult result)
    {
        // Obtener IDs de procesos para evitar problemas de tracking
        var procesoIds = await _db
            .ProcesoCertificacion.AsNoTracking()
            .Where(p => p.EmpresaId == empresaId)
            .Select(p => p.Id)
            .ToListAsync();

        foreach (var procesoId in procesoIds)
        {
            // Eliminar resultados de certificación
            var resultados = await _db
                .ResultadoCertificacion.Where(r => r.CertificacionId == procesoId)
                .ToListAsync();

            _db.ResultadoCertificacion.RemoveRange(resultados);

            // Eliminar archivos del proceso
            var procesosArchivos = await _db
                .ProcesoArchivos.Where(pa => pa.ProcesoCertificacionId == procesoId)
                .ToListAsync();

            _db.ProcesoArchivos.RemoveRange(procesosArchivos);
        }

        // Eliminar procesos
        var procesos = await _db
            .ProcesoCertificacion.Where(p => p.EmpresaId == empresaId)
            .ToListAsync();

        _db.ProcesoCertificacion.RemoveRange(procesos);
        
        // Guardar cambios para limpiar el tracking context
        await _db.SaveChangesAsync();
        
        result.ProcessesDeleted = procesos.Count;
        result.DeletedEntities.Add($"{procesos.Count} proceso(s) de certificación");
    }

    private async Task DeleteArchivosAsync(int empresaId, EmpresaDeletionResult result)
    {
        var archivos = await _db.Archivo.Where(a => a.EmpresaId == empresaId).ToListAsync();

        _db.Archivo.RemoveRange(archivos);
        await _db.SaveChangesAsync();
        
        result.FilesDeleted = archivos.Count;
        result.DeletedEntities.Add($"{archivos.Count} archivo(s)");
    }

    private async Task DeleteHomologacionesAsync(int empresaId, EmpresaDeletionResult result)
    {
        var homologaciones = await _db
            .Homologacion.Where(h => h.EmpresaId == empresaId)
            .ToListAsync();

        _db.Homologacion.RemoveRange(homologaciones);
        await _db.SaveChangesAsync();
        
        result.HomologacionesDeleted = homologaciones.Count;
        if (homologaciones.Count > 0)
        {
            result.DeletedEntities.Add($"{homologaciones.Count} homologación(es)");
        }
    }

    private async Task DeleteTipologiasEmpresaAsync(int empresaId, EmpresaDeletionResult result)
    {
        // TipologiasEmpresa se eliminará automáticamente por la relación de cascada
        // configurada en el modelo, pero vamos a contar cuántas había
        var tipologiasCount = await _db
            .Empresa.Where(e => e.Id == empresaId)
            .SelectMany(e => e.Tipologias)
            .CountAsync();

        if (tipologiasCount > 0)
        {
            result.DeletedEntities.Add($"{tipologiasCount} tipología(s) asociada(s)");
        }
    }

    private bool HasDeletePermission(Empresa empresa, int paisId, string userRole)
    {
        // Admin puede eliminar cualquier empresa
        if (userRole == Constants.Roles.Admin)
            return true;

        // TecnicoPais solo puede eliminar empresas de su país
        if (userRole == Constants.Roles.TecnicoPais)
            return empresa.PaisId == paisId;

        return false;
    }

    private async Task ClearAuditReferencesAsync(int empresaId, EmpresaDeletionResult result)
    {
        // Obtener IDs de usuarios asociados a la empresa
        var userIds = await _db.Users
            .Where(u => u.EmpresaId == empresaId)
            .Select(u => u.Id)
            .ToListAsync();

        if (!userIds.Any()) return;

        var totalReferencesCleared = 0;

        // Limpiar referencias en CrossCountryAuditRequest si existen para esta empresa
        var auditRequests = await _db.CrossCountryAuditRequests
            .Where(car => userIds.Contains(car.CreatedBy) || 
                         (!string.IsNullOrEmpty(car.UpdatedBy) && userIds.Contains(car.UpdatedBy)))
            .ToListAsync();

        foreach (var auditRequest in auditRequests)
        {
            if (userIds.Contains(auditRequest.CreatedBy))
                auditRequest.CreatedBy = null;
            if (!string.IsNullOrEmpty(auditRequest.UpdatedBy) && userIds.Contains(auditRequest.UpdatedBy))
                auditRequest.UpdatedBy = null;
        }

        totalReferencesCleared += auditRequests.Count;

        if (totalReferencesCleared > 0)
        {
            await _db.SaveChangesAsync();
            result.DeletedEntities.Add($"Referencias de auditoría limpiadas en {totalReferencesCleared} entidad(es)");
        }
    }
}

