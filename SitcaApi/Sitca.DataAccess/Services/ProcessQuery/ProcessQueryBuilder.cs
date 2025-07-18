using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;
using static Utilities.Common.Constants;
using Localization = Utilities.Common.LocalizationUtilities;
using Roles = Utilities.Common.Constants.Roles;

namespace Sitca.DataAccess.Services.ProcessQuery;

public class ProcessQueryBuilder : IProcessQueryBuilder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ProcessQueryBuilder> _logger;

    public ProcessQueryBuilder(ApplicationDbContext db, ILogger<ProcessQueryBuilder> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProcesoCertificacion> BuildBaseQueryById(int id)
    {
        return await _db
            .ProcesoCertificacion.AsNoTracking()
            .Where(p => p.Id == id)
            .AsSplitQuery()
            .Include(p => p.Empresa)
            .ThenInclude(e => e.Pais)
            .Include(p => p.Empresa)
            .ThenInclude(e => e.Tipologias)
            .ThenInclude(t => t.Tipologia)
            .Include(p => p.Resultados)
            .ThenInclude(r => r.Distintivo)
            .Include(p => p.AsesorProceso)
            .Include(p => p.AuditorProceso)
            .Include(p => p.UserGenerador)
            .FirstOrDefaultAsync();
    }

    public IQueryable<ProcesoCertificacion> BuildBaseQuery(bool isRecertificacion)
    {
        return _db
            .ProcesoCertificacion.AsNoTracking()
            .Where(p => p.Recertificacion == isRecertificacion)
            .AsSplitQuery()
            .Include(p => p.Empresa)
            .ThenInclude(e => e.Pais)
            .Include(p => p.Empresa)
            .ThenInclude(e => e.Tipologias)
            .ThenInclude(t => t.Tipologia)
            .Include(p => p.Resultados.Where(r => r.Id == p.Id))
            .ThenInclude(r => r.Distintivo)
            .Include(p => p.AsesorProceso)
            .Include(p => p.AuditorProceso);
    }

    public IQueryable<ProcesoCertificacion> BuildRoleBasedQuery(
        ApplicationUser user,
        string role,
        bool isRecertification
    )
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(role))
            throw new ArgumentNullException(nameof(role));

        // Consulta base
        var query = BuildBaseQuery(isRecertification);

        // Aplicar filtros según el rol
        return role switch
        {
            Roles.Asesor => query.Where(p => p.AsesorId == user.Id),
            Roles.Auditor => query.Where(p => p.AuditorId == user.Id),
            Roles.CTC or Roles.Consultor or Roles.EmpresaAuditora
                or Roles.TecnicoPais or Roles.ATP =>
                query.Where(p => p.Empresa.PaisId == user.PaisId),
            Roles.Empresa => query.Where(p => p.EmpresaId == user.EmpresaId),
            _ => throw new ArgumentException($"Role {role} not supported", nameof(role)),
        };
    }

    public IQueryable<ProcesoCertificacion> BuildUnifiedQuery(
        ApplicationUser user,
        string role,
        bool isRecertification
    )
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // Para roles Admin
        if (role == Roles.Admin)
        {
            return BuildBaseQuery(isRecertification);
        }

        // Para roles específicos (Asesor, Auditor, CTC, etc.)
        if (role != Roles.Admin)
        {
            return BuildRoleBasedQuery(user, role, isRecertification);
        }

        // Por defecto, retorna una consulta vacía para evitar errores
        return _db.ProcesoCertificacion.Where(p => false);
    }

    public IQueryable<ProcesoCertificacion> ApplyFilters(
        IQueryable<ProcesoCertificacion> query,
        CompanyFilterDTO filter,
        int? distintiveId = null
    )
    {
        if (filter == null)
            return query;

        if (filter.CountryId > 0)
            query = query.Where(p => p.Empresa.PaisId == filter.CountryId);

        if (filter.StatusId.HasValue && filter.StatusId.Value != -1)
        {
            string statusPrefix = filter.StatusId.Value.ToString() + " - ";
            query = query.Where(p => p.Status.StartsWith(statusPrefix));
        }

        if (filter.TypologyId > 0)
            query = query.Where(p =>
                p.Empresa.Tipologias.Any(t => t.IdTipologia == filter.TypologyId)
            );

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(p => p.Empresa.Nombre.Contains(filter.Name));

        if (distintiveId.HasValue && distintiveId.Value > 0)
        {
            query = query.Where(p =>
                p.Resultados.Any(r => r.Aprobado && r.DistintivoId == distintiveId.Value)
            );
        }

        return query;
    }

    public async Task<BlockResult<ProcesoCertificacionVm>> BuildAndExecuteBlockProjection(
        IQueryable<ProcesoCertificacion> query,
        string language,
        int blockNumber,
        int blockSize
    )
    {
        // 0. Ordenar la consulta original antes de cualquier operación
        query = query.OrderBy(p => p.Id);

        // 1. Preparar la proyección pero sin ejecutarla aún
        var projection = query.Select(p => new
        {
            // Datos del proceso
            p.Id,
            p.EmpresaId,
            p.NumeroExpediente,
            p.FechaInicio,
            p.FechaFinalizacion,
            p.FechaVencimiento,
            p.Recertificacion,
            p.Status,

            // Datos de empresa
            NombreEmpresa = p.Empresa.Nombre,
            NombreRepresentante = p.Empresa.NombreRepresentante,
            EstadoEmpresa = p.Empresa.Estado,
            Activo = p.Empresa.Active,

            // País
            PaisId = p.Empresa.PaisId,
            PaisNombre = p.Empresa.Pais.Name,

            // Tipologías - Versión mejorada
            // Primero comprobamos si existe una tipología directa en el proceso
            TieneTipologiaDirecta = p.Tipologia != null,
            // Si existe, guardamos sus datos
            TipologiaDirectaId = p.Tipologia != null ? p.Tipologia.Id : 0,
            TipologiaDirectaNombre = p.Tipologia != null
                ? (language == "es" ? p.Tipologia.Name : p.Tipologia.NameEnglish)
                : null,
            // También guardamos las tipologías de la empresa (siempre)
            TipologiasEmpresa = p.Empresa.Tipologias.Select(t => new
            {
                Id = t.IdTipologia,
                Nombre = language == "es" ? t.Tipologia.Name : t.Tipologia.NameEnglish,
            }),

            // Fecha de revisión
            FechaRevision = p
                .Cuestionarios.Where(e => e.Prueba == false && !e.FechaFinalizado.HasValue)
                .Select(e => e.FechaRevisionAuditor)
                .FirstOrDefault(),

            // Asesor y Auditor
            AsesorId = p.AsesorId,
            AsesorNombre = p.AsesorId != null
                ? p.AsesorProceso.FirstName + " " + p.AsesorProceso.LastName
                : null,
            AuditorId = p.AuditorId,
            AuditorNombre = p.AuditorId != null
                ? p.AuditorProceso.FirstName + " " + p.AuditorProceso.LastName
                : null,

            // Distintivo
            UltimoResultado = p
                .Resultados.Select(r => new
                {
                    r.DistintivoId,
                    DistintivoNombre = Localization.GetDistintivoTranslation(
                        r.Distintivo.Name,
                        language
                    ),
                })
                .FirstOrDefault(),

            // Contar procesos activos por empresa
            TotalProcesosActivos = _db.ProcesoCertificacion
                .Where(proc => proc.EmpresaId == p.EmpresaId && proc.Empresa.Active)
                .Count(),
        });

        // 1.1 Contamos la cantidad de procesos por estado
        var totalPendiente = await projection.CountAsync(p =>
            p.EstadoEmpresa == ProcessStatusDecimal.Initial
            || p.EstadoEmpresa == ProcessStatusDecimal.ForConsulting
        );

        var totalProcesos = await projection.CountAsync(p =>
            p.EstadoEmpresa > ProcessStatusDecimal.ForConsulting
            && p.EstadoEmpresa < ProcessStatusDecimal.Completed
        );

        var totalFinalizados = await projection.CountAsync(p =>
            p.EstadoEmpresa == ProcessStatusDecimal.Completed
        );

        // 2. Aplicar paginación por bloques a la proyección
        var blockData = await projection.ToBlockResultAsync(blockNumber, blockSize);

        // 3. Mapear los resultados a nuestro modelo de vista
        var items = blockData
            .Items.Select(p =>
            {
                // Procesamos las tipologías según la lógica del negocio
                var tipologiasNombres = new List<string>();
                var tipologiasIds = new List<int>();

                // Si hay una tipología directa en el proceso, la usamos
                if (p.TieneTipologiaDirecta)
                {
                    tipologiasNombres.Add(p.TipologiaDirectaNombre);
                    tipologiasIds.Add(p.TipologiaDirectaId);
                }
                // Si no hay tipología directa, usamos las de la empresa
                else if (p.TipologiasEmpresa.Any())
                {
                    tipologiasNombres.AddRange(p.TipologiasEmpresa.Select(t => t.Nombre));
                    tipologiasIds.AddRange(p.TipologiasEmpresa.Select(t => t.Id));
                }

                return new ProcesoCertificacionVm
                {
                    Id = p.Id,
                    EmpresaId = p.EmpresaId,
                    NombreEmpresa = p.NombreEmpresa,
                    NumeroExpediente = p.NumeroExpediente,
                    Pais = p.PaisNombre,
                    PaisDto = new PaisDTO { Id = p.PaisId ?? 0, Nombre = p.PaisNombre },
                    Responsable = p.NombreRepresentante,
                    Status = p.Status,
                    StatusId = StatusConverter.ConvertStatusTextToInt(p.Status),
                    FechaInicio = p.FechaInicio,
                    FechaFinalizacion = p.FechaFinalizacion,
                    Recertificacion = p.Recertificacion,
                    Distintivo = p.UltimoResultado?.DistintivoNombre,
                    DistintivoId = p.UltimoResultado?.DistintivoId,
                    FechaVencimiento = p.FechaVencimiento?.ToString("yyyy-MM-dd"),
                    Tipologias = tipologiasNombres,
                    TipologiasIds = tipologiasIds,
                    Asesor =
                        p.AsesorId == null
                            ? null
                            : new Personnal { Id = p.AsesorId, Name = p.AsesorNombre },
                    Auditor =
                        p.AuditorId == null
                            ? null
                            : new Personnal { Id = p.AuditorId, Name = p.AuditorNombre },
                    FechaRevision = p.FechaRevision?.ToString("yyyy-MM-dd"),
                    Activo = p.Activo,
                    TotalProcesosActivos = p.TotalProcesosActivos,
                };
            })
            .ToList();

        // 4. Crear el resultado por bloques con los items mapeados
        return new BlockResult<ProcesoCertificacionVm>
        {
            Items = items,
            TotalCount = blockData.TotalCount,
            BlockSize = blockData.BlockSize,
            CurrentBlock = blockData.CurrentBlock,
            TotalBlocks = blockData.TotalBlocks,
            HasMoreItems = blockData.HasMoreItems,
            TotalPending = totalPendiente,
            TotalInProcess = totalProcesos,
            TotalCompleted = totalFinalizados,
        };
    }
}
