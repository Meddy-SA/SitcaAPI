using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data;
using Sitca.DataAccess.Data.Repository.Specifications;
using Sitca.DataAccess.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;
using Localization = Utilities.Common.LocalizationUtilities;
using Roles = Utilities.Common.Constants.Roles;

namespace Sitca.DataAccess.Services.CompanyQuery;

public class CompanyQueryBuilder : ICompanyQueryBuilder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<CompanyQueryBuilder> _logger;

    public CompanyQueryBuilder(ApplicationDbContext db, ILogger<CompanyQueryBuilder> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IQueryable<Empresa> BuildGeneralQuery(bool includeHomologacion)
    {
        return _db
            .Empresa.AsNoTracking()
            .Include(x => x.Pais)
            .Include(x => x.Tipologias)
            .ThenInclude(t => t.Tipologia)
            .Include(x => x.Certificaciones.OrderByDescending(c => c.Id).Take(1))
            .ThenInclude(c => c.Resultados.OrderByDescending(r => r.Id).Take(1))
            .ThenInclude(r => r.Distintivo)
            .Include(x => x.Certificaciones.OrderByDescending(c => c.Id).Take(1))
            .ThenInclude(c => c.AsesorProceso)
            .Include(x => x.Certificaciones.OrderByDescending(c => c.Id).Take(1))
            .ThenInclude(c => c.AuditorProceso)
            .Where(x => x.Nombre != null && (!x.EsHomologacion || includeHomologacion));
    }

    public IQueryable<Empresa> BuildRoleBasedQuery(ApplicationUser user, string role)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(role))
            throw new ArgumentNullException(nameof(role));

        // Consulta base sin includes
        var baseQuery = _db.ProcesoCertificacion.AsNoTracking();

        // Aplicar filtros según el rol
        var filteredQuery = role switch
        {
            Roles.Asesor => EmpresaSpecifications.ForAsesor(baseQuery, user.Id),
            Roles.Auditor => EmpresaSpecifications.ForAuditor(baseQuery, user.Id),
            Roles.CTC => EmpresaSpecifications.ForCTC(baseQuery, user.PaisId ?? 0),
            Roles.Consultor or Roles.EmpresaAuditora => EmpresaSpecifications.ForConsultor(
                baseQuery,
                user.PaisId ?? 0
            ),
            _ => throw new ArgumentException($"Role {role} not supported", nameof(role)),
        };

        // Aplicar includes después de filtrar
        var certifications = filteredQuery
            .Include(x => x.Empresa)
            .ThenInclude(e => e.Pais)
            .Include(x => x.Empresa)
            .ThenInclude(e => e.Tipologias)
            .ThenInclude(t => t.Tipologia)
            .Include(x => x.Resultados.OrderByDescending(r => r.Id).Take(1))
            .ThenInclude(r => r.Distintivo)
            .Include(x => x.AsesorProceso)
            .Include(x => x.AuditorProceso);

        // Obtener empresas de certificaciones
        var empresaIds = certifications.Select(x => x.EmpresaId).Distinct();

        return _db
            .Empresa.AsNoTracking()
            .Include(x => x.Pais)
            .Include(x => x.Tipologias)
            .ThenInclude(t => t.Tipologia)
            .Include(x => x.Certificaciones.OrderByDescending(c => c.Id).Take(1))
            .ThenInclude(c => c.Resultados.OrderByDescending(r => r.Id).Take(1))
            .ThenInclude(r => r.Distintivo)
            .Include(x => x.Certificaciones.OrderByDescending(c => c.Id).Take(1))
            .ThenInclude(c => c.AsesorProceso)
            .Include(x => x.Certificaciones.OrderByDescending(c => c.Id).Take(1))
            .ThenInclude(c => c.AuditorProceso)
            .Where(x => empresaIds.Contains(x.Id));
    }

    public IQueryable<Empresa> ApplyFilters(
        IQueryable<Empresa> query,
        CompanyFilterDTO filter,
        int? distintiveId = null
    )
    {
        if (filter == null)
            return query;

        if (filter.CountryId > 0)
            query = query.Where(x => x.PaisId == filter.CountryId);

        if (filter.StatusId.HasValue && filter.StatusId.Value != -1)
            query = query.Where(x => x.Estado == filter.StatusId);

        if (filter.TypologyId > 0)
            query = query.Where(x => x.Tipologias.Any(z => z.IdTipologia == filter.TypologyId));

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(s => s.Nombre.Contains(filter.Name));

        if (distintiveId.HasValue && distintiveId.Value > 0)
        {
            query = query.Where(x =>
                (
                    x.ResultadoActual != null
                    && x.ResultadoActual == GetDistintivoName(distintiveId.Value)
                )
                || x.Certificaciones.Any(c =>
                    c.Resultados.Any(r => r.Aprobado && r.DistintivoId == distintiveId.Value)
                )
            );
        }

        return query;
    }

    private string GetDistintivoName(int distintiveId)
    {
        return _db.Distintivo.Where(d => d.Id == distintiveId).Select(d => d.Name).FirstOrDefault();
    }

    public async Task<List<EmpresaVm>> BuildAndExecuteProjection(
        IQueryable<Empresa> query,
        string language
    )
    {
        // 1. Obtener solo los datos necesarios desde la base de datos
        var empresasData = await query
            .Select(x => new
            {
                x.Id,
                x.Nombre,
                x.NombreRepresentante,
                x.Estado,
                x.ResultadoActual,
                PaisName = x.Pais.Name,
                LatestCertificacion = x
                    .Certificaciones.OrderByDescending(c => c.Id)
                    .Take(1)
                    .Select(c => new
                    {
                        c.Id,
                        c.Recertificacion,
                        AsesorId = c.AsesorId,
                        AsesorName = c.AsesorId != null
                            ? c.AsesorProceso.FirstName + " " + c.AsesorProceso.LastName
                            : null,
                        AuditorId = c.AuditorId,
                        AuditorName = c.AuditorId != null
                            ? c.AuditorProceso.FirstName + " " + c.AuditorProceso.LastName
                            : null,
                    })
                    .FirstOrDefault(),
                CertificacionesCount = x.Certificaciones.Count,
                TieneCertificacionesRecertificadas = x.Certificaciones.Any(c => c.Recertificacion),
                Tipologias = x
                    .Tipologias.Select(t => new
                    {
                        Spanish = t.Tipologia.Name,
                        English = t.Tipologia.NameEnglish,
                    })
                    .ToList(),
            })
            .ToListAsync();

        // 2. Mapear a EmpresaVm en memoria
        return empresasData
            .Select(x => new EmpresaVm
            {
                Id = x.Id,
                Nombre = x.Nombre,
                Pais = x.PaisName,
                Responsable = x.NombreRepresentante,
                Status = ((int)x.Estado).ToLocalizedString(language),
                StatusId = x.Estado,
                Certificacion = x.LatestCertificacion?.Id.ToString(),
                Recertificacion =
                    x.CertificacionesCount > 1 || x.TieneCertificacionesRecertificadas,
                Distintivo = Localization.GetDistintivoTranslation(x.ResultadoActual, language),
                Tipologias = x
                    .Tipologias.Select(t => language == "es" ? t.Spanish : t.English)
                    .ToList(),
                Asesor =
                    x.LatestCertificacion?.AsesorId == null
                        ? null
                        : new Personnal
                        {
                            Id = x.LatestCertificacion.AsesorId,
                            Name = x.LatestCertificacion.AsesorName,
                        },
                Auditor =
                    x.LatestCertificacion?.AuditorId == null
                        ? null
                        : new Personnal
                        {
                            Id = x.LatestCertificacion.AuditorId,
                            Name = x.LatestCertificacion.AuditorName,
                        },
            })
            .ToList();
    }

    private static Personnal GetUsuarioVm(string id, ApplicationUser user)
    {
        if (id == null || user == null)
            return null;

        return new Personnal { Id = id, Name = $"{user.FirstName} {user.LastName}".Trim() };
    }
}
