using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data;
using Sitca.DataAccess.Data.Repository.Constants;
using Sitca.Models;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Services.Cuestionarios;

public class CuestionarioReaperturaService : ICuestionarioReaperturaService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<CuestionarioReaperturaService> _logger;
    private const int ESTADO_AUDITORIA_EN_PROCESO = 5;

    public CuestionarioReaperturaService(
        ApplicationDbContext db,
        ILogger<CuestionarioReaperturaService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<bool>> EjecutarReapertura(int cuestionarioId, ApplicationUser user)
    {
        try
        {
            if (user == null)
            {
                _logger.LogWarning("Intento de reabrir cuestionario sin usuario especificado");
                return Result<bool>.Failure("Usuario no especificado");
            }

            var strategy = _db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(() =>
                ProcesarReapertura(cuestionarioId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al reabrir cuestionario {CuestionarioId}", cuestionarioId);
            return Result<bool>.Failure("Error inesperado al procesar la solicitud");
        }
    }

    private async Task<Result<bool>> ProcesarReapertura(int cuestionarioId)
    {
        var cuestionario = await ObtenerCuestionarioParaReapertura(cuestionarioId);
        if (cuestionario == null)
        {
            return Result<bool>.Failure("Cuestionario no encontrado o no está en un estado válido para reapertura");
        }

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await AplicarCambiosReapertura(cuestionario);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al reabrir cuestionario {CuestionarioId}", cuestionarioId);
            return Result<bool>.Failure($"Error al reabrir cuestionario: {ex.Message}");
        }
    }

    private async Task AplicarCambiosReapertura(Cuestionario cuestionario)
    {
        ResetearEstadoCuestionario(cuestionario);
        ActualizarEstadoCertificacion(cuestionario);
        await ActualizarEstadoEmpresa(cuestionario.Certificacion.EmpresaId);
    }

    private async Task<Cuestionario> ObtenerCuestionarioParaReapertura(int cuestionarioId)
    {
        return await _db.Cuestionario
            .Include(s => s.Certificacion)
            .FirstOrDefaultAsync(s =>
                s.Id == cuestionarioId &&
                (s.Certificacion.Status.StartsWith("6 - ") ||
                 s.Certificacion.Status.StartsWith("7 - ")));
    }

    private void ResetearEstadoCuestionario(Cuestionario cuestionario)
    {
        cuestionario.Resultado = 0;
        cuestionario.FechaFinalizado = null;
        cuestionario.FechaRevisionAuditor = null;
        cuestionario.TecnicoPaisId = null;
        cuestionario.Certificacion.FechaFinalizacion = null;
    }

    private void ActualizarEstadoCertificacion(Cuestionario cuestionario)
    {
        var audEnProceso = StatusConstants.GetLocalizedStatus(ESTADO_AUDITORIA_EN_PROCESO, "es");
        var audEnProcesoEn = StatusConstants.GetLocalizedStatus(ESTADO_AUDITORIA_EN_PROCESO, "en");
        var audFinalizada = StatusConstants.GetLocalizedStatus(6, "es");
        var enRevison = StatusConstants.GetLocalizedStatus(7, "es");

        cuestionario.Certificacion.Status =
            (cuestionario.Certificacion.Status == audFinalizada ||
             cuestionario.Certificacion.Status == enRevison)
                ? audEnProceso
                : audEnProcesoEn;
    }

    private async Task ActualizarEstadoEmpresa(int empresaId)
    {
        var empresa = await _db.Empresa.FirstOrDefaultAsync(s => s.Id == empresaId)
            ?? throw new InvalidOperationException($"Empresa {empresaId} no encontrada");

        empresa.Estado = ESTADO_AUDITORIA_EN_PROCESO;
    }
}
