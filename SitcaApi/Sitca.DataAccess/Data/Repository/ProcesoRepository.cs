using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.Mappers;

namespace Sitca.DataAccess.Data.Repository;

public class ProcesoRepository : Repository<ProcesoCertificacion>, IProcesoRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<ProcesoRepository> _logger;

    public ProcesoRepository(
        ApplicationDbContext db,
        IConfiguration configuration,
        ILogger<ProcesoRepository> logger
    )
        : base(db)
    {
        _db = db;
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

    private async Task ActualizarEstadoEmpresaAsync(int empresaId, string nuevoEstado)
    {
        // Obtener el valor numérico del estado desde la cadena (por ejemplo, "0 - Inicial" -> 0)
        var estadoNumerico = int.Parse(nuevoEstado.Split(' ')[0]);

        var empresa = await _db.Empresa.FindAsync(empresaId);
        if (empresa != null)
        {
            // Solo actualizar si el nuevo estado es mayor que el actual
            if (!empresa.Estado.HasValue || estadoNumerico > empresa.Estado.Value)
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
}
