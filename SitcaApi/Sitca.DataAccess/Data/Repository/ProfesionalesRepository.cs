using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Models;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Data.Repository;

public class ProfesionalesRepository : IProfesionalesRepository
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ProfesionalesRepository> _logger;

    // Mapeo de roles a tipos de profesional
    private static readonly Dictionary<string, string> RoleMapping = new()
    {
        { "Auditor", "auditor" },
        { "Asesor", "asesor" },
        { "Asesor/Auditor", "auditor/asesor" },
    };

    public ProfesionalesRepository(ApplicationDbContext db, ILogger<ProfesionalesRepository> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ProfesionalesHabilitadosResponseDTO>> GetProfesionalesHabilitadosAsync(
        ProfesionalesHabilitadosFilterDTO filter
    )
    {
        try
        {
            filter ??= new ProfesionalesHabilitadosFilterDTO();

            // Consulta base - Usuarios con roles de profesionales
            var query = _db
                .Users.AsNoTracking()
                .Include(u => u.Pais)
                .Include(u => u.CompAuditora)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u =>
                    u.UserRoles.Any(ur =>
                        ur.Role.Name == "Auditor"
                        || ur.Role.Name == "Asesor"
                        || ur.Role.Name == "Asesor/Auditor"
                    )
                );

            // Aplicar filtros
            if (filter.SoloActivos == true)
            {
                query = query.Where(u => u.Active);
            }

            if (filter.SoloConCarnet == true)
            {
                query = query.Where(u => !string.IsNullOrEmpty(u.NumeroCarnet));
            }

            if (filter.PaisId.HasValue)
            {
                query = query.Where(u => u.PaisId == filter.PaisId.Value);
            }

            if (!string.IsNullOrEmpty(filter.TipoProfesional))
            {
                var roleNames = GetRoleNamesForTipo(filter.TipoProfesional);
                if (roleNames.Any())
                {
                    query = query.Where(u =>
                        u.UserRoles.Any(ur => roleNames.Contains(ur.Role.Name))
                    );
                }
            }

            // Ejecutar consulta
            var usuarios = await query
                .OrderBy(u => u.Pais != null ? u.Pais.Name : "Sin país")
                .ThenBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            // Agrupar por país y mapear a DTOs
            var profesionalesPorPais = usuarios
                .GroupBy(u => new
                {
                    PaisId = u.PaisId ?? 0,
                    PaisNombre = u.Pais?.Name ?? "Sin país asignado",
                })
                .Select(group => new ProfesionalesPorPaisDTO
                {
                    PaisId = group.Key.PaisId,
                    Pais = group.Key.PaisNombre,
                    Profesionales = group.Select(u => MapUserToProfesionalDTO(u)).ToList(),
                })
                .OrderBy(p => p.Pais)
                .ToList();

            // Calcular totales por país
            foreach (var paisGroup in profesionalesPorPais)
            {
                paisGroup.TotalProfesionales = paisGroup.Profesionales.Count;
                paisGroup.TotalAuditores = paisGroup.Profesionales.Count(p => p.Tipo == "auditor");
                paisGroup.TotalAsesores = paisGroup.Profesionales.Count(p => p.Tipo == "asesor");
                paisGroup.TotalAuditoresAsesores = paisGroup.Profesionales.Count(p =>
                    p.Tipo == "auditor/asesor"
                );
            }

            // Calcular totales generales
            var totalProfesionales = profesionalesPorPais.Sum(p => p.TotalProfesionales);
            var resumenPorTipo = new Dictionary<string, int>
            {
                { "auditor", profesionalesPorPais.Sum(p => p.TotalAuditores) },
                { "asesor", profesionalesPorPais.Sum(p => p.TotalAsesores) },
                { "auditor/asesor", profesionalesPorPais.Sum(p => p.TotalAuditoresAsesores) },
            };

            var response = new ProfesionalesHabilitadosResponseDTO
            {
                PaisesProfesionales = profesionalesPorPais,
                TotalPaises = profesionalesPorPais.Count,
                TotalProfesionales = totalProfesionales,
                ResumenPorTipo = resumenPorTipo,
            };

            return Result<ProfesionalesHabilitadosResponseDTO>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener profesionales habilitados con filtro {@Filter}",
                filter
            );
            return Result<ProfesionalesHabilitadosResponseDTO>.Failure(
                $"Error interno al obtener profesionales: {ex.Message}"
            );
        }
    }

    private static ProfesionalDTO MapUserToProfesionalDTO(ApplicationUser user)
    {
        // Determinar el tipo de profesional basado en los roles
        var userRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var tipoProfesional = GetTipoProfesional(userRoles);

        return new ProfesionalDTO
        {
            NumeroCarnet = user.NumeroCarnet ?? string.Empty,
            Nombre = user.FirstName ?? string.Empty,
            Apellido = user.LastName ?? string.Empty,
            Tipo = tipoProfesional,
            FechaExpiracion = user.VencimientoCarnet,
            CompaniaAuditora = user.CompAuditora?.Name,
            Codigo = user.Codigo ?? string.Empty,
            Activo = user.Active,
        };
    }

    private static string GetTipoProfesional(List<string> userRoles)
    {
        // Si tiene el rol combinado, devolver ese
        if (userRoles.Contains("Asesor/Auditor"))
        {
            return "auditor/asesor";
        }

        // Si tiene ambos roles por separado, también es auditor/asesor
        if (userRoles.Contains("Auditor") && userRoles.Contains("Asesor"))
        {
            return "auditor/asesor";
        }

        // Si solo tiene uno de los roles
        if (userRoles.Contains("Auditor"))
        {
            return "auditor";
        }

        if (userRoles.Contains("Asesor"))
        {
            return "asesor";
        }

        // Por defecto (no debería llegar aquí debido al filtro en la consulta)
        return "desconocido";
    }

    private List<string> GetRoleNamesForTipo(string tipoProfesional)
    {
        return tipoProfesional.ToLower() switch
        {
            "auditor" => new List<string> { "Auditor" },
            "asesor" => new List<string> { "Asesor" },
            "auditor/asesor" => new List<string> { "Asesor/Auditor", "Auditor", "Asesor" },
            _ => new List<string>(),
        };
    }
}

