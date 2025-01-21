using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Builders;
using Sitca.DataAccess.Data.Repository.Constants;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.DataAccess.Data.Repository.Specifications;
using Sitca.DataAccess.Extensions;
using Sitca.DataAccess.Middlewares;
using Sitca.DataAccess.Services.Notification;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;
using Utilities;
using ConstantRoles = Utilities.Common.Constants.Roles;

namespace Sitca.DataAccess.Data.Repository
{
    public class EmpresaRepository : Repository<Empresa>, IEmpresaRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly ILogger<EmpresaRepository> _logger;

        public EmpresaRepository(
            ApplicationDbContext db,
            INotificationService notificationService,
            ILogger<EmpresaRepository> logger
        )
            : base(db)
        {
            _db = db;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<int> GetCompanyStatusAsync(int CompanyId)
        {
            var proccess = await _db
                .Empresa.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == CompanyId);

            if (proccess == null)
            {
                return 0;
            }

            return proccess.Estado == null ? 0 : (int)proccess.Estado;
        }

        public async Task<bool> ActualizarDatos(
            EmpresaUpdateVm datos,
            ApplicationUser user,
            string role
        )
        {
            var empresaId = datos.Id;
            if (role == ConstantRoles.Empresa)
            {
                empresaId = user.EmpresaId ?? datos.Id;
            }

            var empresa = await _db
                .Empresa.Include("Tipologias")
                .FirstOrDefaultAsync(s => s.Id == empresaId);

            empresa.Direccion = datos.Direccion;
            empresa.IdNacional = datos.IdNacionalRepresentante;
            empresa.Nombre = datos.Nombre;
            empresa.NombreRepresentante = datos.Responsable;
            // TODO: Se puede cambiar el pais?
            if (datos.Pais != null && role == ConstantRoles.Empresa && empresa.Estado < 2)
            {
                empresa.PaisId = datos.Pais.id;
            }
            empresa.CargoRepresentante = datos.CargoRepresentante;
            empresa.Ciudad = datos.Ciudad;
            empresa.Telefono = datos.Telefono;
            empresa.WebSite = datos.Website;
            empresa.Email = datos.Email;

            if (datos.Tipologias.Any(s => s.isSelected))
            {
                empresa.Tipologias.Clear();
                await Context.SaveChangesAsync();
                var listaTipologias = new List<TipologiasEmpresa>();

                foreach (var item in datos.Tipologias.Where(s => s.isSelected))
                {
                    var TipologiaEmpresa = new TipologiasEmpresa
                    {
                        IdEmpresa = empresa.Id,
                        IdTipologia = item.id,
                    };

                    listaTipologias.Add(TipologiaEmpresa);
                }
                empresa.Tipologias = listaTipologias;
            }

            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Result<int>> SaveEmpresaAsync(RegisterDTO model)
        {
            try
            {
                // Crear y configurar la empresa
                var empresa = new Empresa
                {
                    Active = true,
                    IdPais = model.CountryId,
                    PaisId = model.CountryId,
                    Nombre = model.Empresa,
                    NombreRepresentante = model.Representante,
                    Email = model.Email,
                    CargoRepresentante = string.Empty,
                    Ciudad = string.Empty,
                    IdNacional = string.Empty,
                    Telefono = string.Empty,
                    Calle = string.Empty,
                    Numero = string.Empty,
                    Direccion = string.Empty,
                    Longitud = string.Empty,
                    Latitud = string.Empty,
                    WebSite = string.Empty,
                    ResultadoSugerido = string.Empty,
                    ResultadoActual = string.Empty,
                    EsHomologacion = false,
                    Estado = 0,
                    Tipologias = model
                        .Tipologias.Where(s => s.isSelected)
                        .Select(t => new TipologiasEmpresa { IdTipologia = t.id })
                        .ToList(),
                };

                await _db.Empresa.AddAsync(empresa);
                await _db.SaveChangesAsync();

                return Result<int>.Success(empresa.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar empresa: {Message}", ex.Message);
                return Result<int>.Failure($"Error al registrar empresa: {ex.Message}");
            }
        }

        public async Task<List<EmpresaVm>> GetCompanyListAsync(
            CompanyFilterDTO filter,
            string language = "es"
        )
        {
            try
            {
                var query = _db
                    .Empresa.AsNoTracking()
                    .Include(x => x.Pais)
                    .Include(x => x.Tipologias)
                    .ThenInclude(t => t.Tipologia)
                    .Include(x => x.Certificaciones)
                    .ThenInclude(c => c.Resultados)
                    .ThenInclude(r => r.Distintivo)
                    .Where(x => x.Nombre != null && !x.EsHomologacion);

                // Aplicar filtros
                Distintivo distintivo = null;
                if (filter.DistinctiveId > 0)
                {
                    distintivo = await _db
                        .Distintivo.AsNoTracking()
                        .Where(d => d.Id == filter.DistinctiveId)
                        .FirstOrDefaultAsync();
                }
                query = ApplyFilters(query, filter, distintivo);

                var companies = await query
                    .Select(x => new EmpresaVm
                    {
                        Id = x.Id,
                        Nombre = x.Nombre,
                        Certificacion = x
                            .Certificaciones.OrderByDescending(c => c.Id)
                            .Select(c => c.Id.ToString())
                            .FirstOrDefault(),
                        Pais = x.Pais.Name,
                        Recertificacion =
                            x.Certificaciones.Count > 1
                            || x.Certificaciones.Any(s => s.Recertificacion),
                        Status = ((int)x.Estado).ToLocalizedString(language),
                        Responsable = _db
                            .ApplicationUser.Where(s => s.EmpresaId == x.Id)
                            .Select(s => s.FirstName)
                            .FirstOrDefault(),
                        Tipologias = x
                            .Tipologias.Select(z =>
                                language == "es" ? z.Tipologia.Name : z.Tipologia.NameEnglish
                            )
                            .ToList(),
                        Distintivo = x.ResultadoActual,
                    })
                    .ToListAsync();
                return companies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company list with filter {@Filter}", filter);
                throw;
            }
        }

        private static IQueryable<Empresa> ApplyFilters(
            IQueryable<Empresa> query,
            CompanyFilterDTO filter,
            Distintivo distintivo = null
        )
        {
            if (filter.CountryId > 0)
                query = query.Where(x => x.PaisId == filter.CountryId);

            if (filter.StatusId.HasValue && filter.StatusId.Value != -1)
                query = query.Where(x => x.Estado == filter.StatusId);

            if (filter.TypologyId > 0)
                query = query.Where(x => x.Tipologias.Any(z => z.IdTipologia == filter.TypologyId));

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(s => s.Nombre.Contains(filter.Name));

            if (distintivo != null)
                query = query.Where(x => x.ResultadoActual == distintivo.Name);

            return query;
        }

        public List<EmpresaVm> GetListXVencerReporte(FiltroEmpresaReporteVm data)
        {
            bool? homologacion =
                data.homologacion == "-1" ? true
                : data.homologacion == "1" ? false
                : (bool?)null;

            var meses = data.meses ?? 1;
            var dueDate = DateTime.Now.AddMonths(meses);
            var empresas = _db
                .Empresa.Include(x => x.Tipologias)
                .Where(x =>
                    x.Nombre != null
                    && (x.PaisId == data.country || data.country == 0)
                    && (x.EsHomologacion == homologacion || homologacion == null)
                    //&& (x.Estado == 8)
                    && (x.ResultadoVencimiento != null && x.ResultadoVencimiento < dueDate)
                    && (
                        data.tipologia == 0
                        || data.tipologia == null
                        || (x.Tipologias.Any(z => z.IdTipologia == data.tipologia))
                    )
                );

            var result = empresas
                .Select(x => new EmpresaVm
                {
                    Nombre = x.Nombre,
                    Pais = x.Pais.Name,
                    Id = x.Id,
                    Status = x.Estado.ToString(),
                    Vencimiento = x.ResultadoVencimiento.ToStringArg(),
                    Responsable = _db
                        .ApplicationUser.FirstOrDefault(s => s.EmpresaId == x.Id)
                        .FirstName,
                    Certificacion = x.ResultadoActual,
                    Tipologias = x.Tipologias.Any()
                        ? x.Tipologias.Select(z => z.Tipologia.Name).ToList()
                        : null,
                })
                .ToList();

            foreach (var item in result)
            {
                item.Status = item.Status.ToDecimal().GetEstado(data.lang);
            }

            return result;
        }

        public List<EmpresaVm> GetListRenovacionReporte(FiltroEmpresaReporteVm data)
        {
            var empresas = _db
                .Empresa.Include(x => x.Tipologias)
                .Where(x =>
                    x.Nombre != null
                    && (x.PaisId == data.country || data.country == 0)
                    && (x.Estado < 8 && x.Estado > 0)
                    && (x.ResultadoVencimiento != null)
                    && (x.Certificaciones.Count > 1)
                    && (
                        data.tipologia == 0
                        || data.tipologia == null
                        || (x.Tipologias.Any(z => z.IdTipologia == data.tipologia))
                    )
                );

            var result = empresas
                .Select(x => new EmpresaVm
                {
                    Nombre = x.Nombre,
                    Pais = x.Pais.Name,
                    Id = x.Id,
                    Status = x.Estado.ToString(),
                    Vencimiento = x.ResultadoVencimiento.ToStringArg(),
                    Responsable = _db
                        .ApplicationUser.FirstOrDefault(s => s.EmpresaId == x.Id)
                        .FirstName,
                    Certificacion = x.ResultadoActual,
                    Tipologias = x.Tipologias.Any()
                        ? data.lang == "es"
                            ? x.Tipologias.Select(z => z.Tipologia.Name).ToList()
                            : x.Tipologias.Select(z => z.Tipologia.NameEnglish).ToList()
                        : null,
                })
                .ToList();

            foreach (var item in result)
            {
                item.Status = item.Status.ToDecimal().GetEstado(data.lang);
            }

            return result;
        }

        public List<EmpresaVm> GetListReporte(FiltroEmpresaReporteVm data)
        {
            bool? homologacion =
                data.homologacion == "-1" ? true
                : data.homologacion == "1" ? false
                : (bool?)null;

            var empresas = _db
                .Empresa.Include(x => x.Tipologias)
                .Where(x =>
                    x.Nombre != null
                    && (x.PaisId == data.country || data.country == 0)
                    && (x.Estado == data.estado || data.estado == -1)
                    && (x.EsHomologacion == homologacion || homologacion == null)
                    && (
                        data.tipologia == 0
                        || data.tipologia == null
                        || (x.Tipologias.Any(z => z.IdTipologia == data.tipologia))
                    )
                );

            var result = empresas
                .Select(x => new EmpresaVm
                {
                    Nombre = x.Nombre,
                    Pais = x.Pais.Name,
                    Id = x.Id,
                    Status = x.Estado.ToString(),
                    Responsable = _db
                        .ApplicationUser.FirstOrDefault(s => s.EmpresaId == x.Id)
                        .FirstName,
                    Certificacion =
                        x.Estado > 1
                            ? x.Estado == 8
                                ? !x
                                    .Certificaciones.OrderByDescending(x => x.Id)
                                    .First()
                                    .Resultados.First()
                                    .Aprobado
                                    ? "No Certificado"
                                    : data.lang == "es"
                                        ? x
                                            .Certificaciones.OrderByDescending(x => x.Id)
                                            .First()
                                            .Resultados.First()
                                            .Distintivo.Name
                                        : x
                                            .Certificaciones.OrderByDescending(x => x.Id)
                                            .First()
                                            .Resultados.First()
                                            .Distintivo.NameEnglish
                                : data.lang == "es"
                                    ? "En Proceso"
                                    : "In Process"
                            : data.lang == "es"
                                ? "No comenzada"
                                : "Not started",
                    Tipologias = x.Tipologias.Any()
                        ? data.lang == "es"
                            ? x.Tipologias.Select(z => z.Tipologia.Name).ToList()
                            : x.Tipologias.Select(z => z.Tipologia.NameEnglish).ToList()
                        : null,
                })
                .ToList();

            foreach (var item in result)
            {
                item.Status = item.Status.ToDecimal().GetEstado(data.lang);
            }

            if (data.certificacion != "Todas")
            {
                result = result
                    .Where(s => s.Certificacion.ToLower() == data.certificacion.ToLower())
                    .ToList();
            }

            return result;
        }

        public async Task<Result<List<EmpresaPersonalVm>>> GetListReportePersonalAsync(
            FiltroEmpresaReporteVm data
        )
        {
            try
            {
                // 1. Construir y ejecutar la consulta base
                var empresasQuery = _db
                    .Empresa.Include(z => z.Pais)
                    .Include(s => s.Certificaciones)
                    .AsNoTracking()
                    .Where(x => x.Nombre != null && x.Estado > 0);

                // Aplicar filtros
                empresasQuery = ApplyCountryFilter(empresasQuery, data.country ?? 0);
                empresasQuery = ApplyStatusFilter(empresasQuery, data.estado ?? -1);

                // 2. Cargar los datos necesarios en una sola consulta
                var empresas = await empresasQuery
                    .Select(e => new
                    {
                        Empresa = e,
                        UltimaCertificacion = e
                            .Certificaciones.OrderByDescending(c => c.Id)
                            .FirstOrDefault(),
                    })
                    .ToListAsync();

                // 3. Obtener todos los IDs de usuarios relacionados
                var userIds = empresas
                    .Where(e => e.UltimaCertificacion != null)
                    .SelectMany(e =>
                        new[]
                        {
                            e.UltimaCertificacion.AsesorId,
                            e.UltimaCertificacion.AuditorId,
                            e.UltimaCertificacion.UserGeneraId,
                        }
                    )
                    .Where(id => id != null)
                    .Distinct()
                    .ToList();

                var usuarios = await _db
                    .ApplicationUser.AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(
                        u => u.Id,
                        u => new CommonUserVm
                        {
                            id = u.Id,
                            email = u.Email,
                            fullName = $"{u.FirstName} {u.LastName}",
                            codigo = u.Codigo,
                        }
                    );

                // 5. Mapear los resultados
                var listaEmpresas = empresas
                    .Where(e => e.UltimaCertificacion != null)
                    .Select(e => new EmpresaPersonalVm
                    {
                        Nombre = e.Empresa.Nombre,
                        Pais = e.Empresa.Pais.Name,
                        Status = e.Empresa.Estado.GetEstado(data.lang),
                        Asesor =
                            e.UltimaCertificacion.AsesorId != null
                                ? usuarios.GetValueOrDefault(e.UltimaCertificacion.AsesorId)
                                : null,
                        Auditor =
                            e.UltimaCertificacion.AuditorId != null
                                ? usuarios.GetValueOrDefault(e.UltimaCertificacion.AuditorId)
                                : null,
                        TecnicoPais =
                            e.UltimaCertificacion.UserGeneraId != null
                                ? usuarios.GetValueOrDefault(e.UltimaCertificacion.UserGeneraId)
                                : null,
                    })
                    .ToList();

                return Result<List<EmpresaPersonalVm>>.Success(listaEmpresas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el reporte de personal de empresas");
                return Result<List<EmpresaPersonalVm>>.Failure(ex.Message);
            }
        }

        private static IQueryable<Empresa> ApplyCountryFilter(
            IQueryable<Empresa> query,
            int countryId
        )
        {
            return countryId == 0 ? query : query.Where(x => x.PaisId == countryId);
        }

        private static IQueryable<Empresa> ApplyStatusFilter(IQueryable<Empresa> query, int estado)
        {
            return estado == -1 ? query : query.Where(x => x.Estado == estado);
        }

        public async Task<List<EmpresaVm>> ListForRoleAsync(
            ApplicationUser user,
            string role,
            CompanyFilterDTO filter
        )
        {
            try
            {
                if (string.IsNullOrEmpty(role))
                    throw new ArgumentNullException(nameof(role));

                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                var query = _db
                    .ProcesoCertificacion.Include(x => x.Empresa)
                    .ThenInclude(e => e.Pais)
                    .Include(x => x.Empresa)
                    .ThenInclude(e => e.Tipologias)
                    .ThenInclude(t => t.Tipologia)
                    .Include(x => x.Resultados)
                    .ThenInclude(r => r.Distintivo)
                    .AsNoTracking();

                // Aplicar filtros según el rol
                query = role switch
                {
                    RoleConstants.Asesor => EmpresaSpecifications.ForAsesor(query, user.Id),
                    RoleConstants.Auditor => EmpresaSpecifications.ForAuditor(query, user.Id),
                    RoleConstants.CTC => EmpresaSpecifications.ForCTC(query, user.PaisId ?? 0),
                    RoleConstants.Consultor => EmpresaSpecifications.ForConsultor(
                        query,
                        user.PaisId ?? 0
                    ),
                    _ => throw new ArgumentException($"Role {role} not supported", nameof(role)),
                };

                // Aplicar filtros adicionales
                if (filter != null)
                {
                    query = ApplyCompanyFilters(query, filter);
                }

                var companies = await query
                    .Select(x => new EmpresaVm
                    {
                        Id = x.EmpresaId,
                        Status = ((int)x.Empresa.Estado).ToLocalizedString(user.Lenguage),
                        Pais = x.Empresa.Pais.Name,
                        Nombre = x.Empresa.Nombre,
                        Recertificacion = x.Recertificacion,
                        Tipologias = x.Empresa.Tipologias.Any()
                            ? GetTipologias(x.Empresa.Tipologias, user.Lenguage)
                            : null,
                        Responsable = x.Empresa.NombreRepresentante,
                        Distintivo =
                            x.Resultados.Where(r => r.Aprobado)
                                .OrderByDescending(r => r.Id)
                                .Select(r =>
                                    user.Lenguage == "es"
                                        ? r.Distintivo.Name
                                        : r.Distintivo.NameEnglish
                                )
                                .FirstOrDefault() ?? "Sin Distintivo",
                    })
                    .ToListAsync();

                return companies;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting company list for role with filter {@Filter}",
                    filter
                );
                throw;
            }
        }

        private static IQueryable<ProcesoCertificacion> ApplyCompanyFilters(
            IQueryable<ProcesoCertificacion> query,
            CompanyFilterDTO filter
        )
        {
            if (filter == null)
                return query;

            if (filter.CountryId > 0)
            {
                query = query.Where(x => x.Empresa.PaisId == filter.CountryId);
            }

            if (filter.TypologyId > 0)
                query = query.Where(x =>
                    x.Empresa.Tipologias.Any(z => z.IdTipologia == filter.TypologyId)
                );

            if (filter.DistinctiveId > 0)
            {
                query = query.Where(x =>
                    x.Resultados.OrderByDescending(r => r.Id)
                        .Any(r => r.Aprobado && r.DistintivoId == filter.DistinctiveId)
                );
            }

            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(x => x.Empresa.Nombre.Contains(filter.Name));
            }

            if (filter.StatusId.HasValue && filter.StatusId > 0)
            {
                query = query.Where(x => x.Empresa.Estado == filter.StatusId.Value);
            }

            return query;
        }

        private static List<string> GetTipologias(
            IEnumerable<TipologiasEmpresa> tipologias,
            string language
        ) =>
            language == "es"
                ? tipologias.Select(z => z.Tipologia.Name).ToList()
                : tipologias.Select(z => z.Tipologia.NameEnglish).ToList();

        public async Task<Result<bool>> SolicitaAuditoriaAsync(int idEmpresa)
        {
            try
            {
                var procesoCertificacion = await _db
                    .ProcesoCertificacion.OrderByDescending(s => s.Id)
                    .FirstOrDefaultAsync(s => s.EmpresaId == idEmpresa);

                if (procesoCertificacion == null)
                    return Result<bool>.Failure(
                        $"No se encontró proceso de certificación para la empresa {idEmpresa}"
                    );

                procesoCertificacion.FechaSolicitudAuditoria = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al solicitar auditoría para empresa {EmpresaId}",
                    idEmpresa
                );
                return Result<bool>.Failure($"Error al solicitar auditoría: {ex.Message}");
            }
        }

        public async Task<List<EstadisticaItemVm>> EnCertificacion(int idPais, string lenguage)
        {
            try
            {
                //Empresas que estan en asesoria
                var empresasPorTipologia = _db
                    .Tipologia.Include(s => s.Empresas)
                    .Select(x => new EstadisticaItemVm
                    {
                        Id = x.Id,
                        Name = lenguage == "es" ? x.Name : x.NameEnglish,
                        Count = x
                            .Empresas.Where(s =>
                                s.Empresa.Estado > 0
                                && s.Empresa.Estado < 4
                                && (s.Empresa.PaisId == idPais || idPais == 0)
                            )
                            .Count(),
                    });

                return await empresasPorTipologia.ToListAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<EmpresasCalificadas>> EvaluadasEnCtc(int idPais, string language)
        {
            try
            {
                var distintivos = await _db.Distintivo.ToListAsync();

                var query = await _db
                    .ProcesoCertificacion.Include(u => u.AuditorProceso)
                    .Include(z => z.AsesorProceso)
                    .Include(x => x.Empresa)
                    .ThenInclude(s => s.Tipologias)
                    .ThenInclude(s => s.Tipologia)
                    .Include(m => m.Resultados)
                    .Where(s => s.Status.Contains("8") && s.Empresa.PaisId == idPais)
                    .ToListAsync();

                var noCertificado = language == "es" ? "No Certificado" : "Not Certified";

                var result = query
                    .Select(x => new EmpresasCalificadas
                    {
                        Id = x.Id,
                        EmpresaId = x.EmpresaId,
                        Name = x.Empresa.Nombre,
                        Aprobado = x.Resultados.First().Aprobado,
                        Asesor =
                            x.AsesorProceso != null
                                ? new CommonUserVm
                                {
                                    fullName =
                                        x.AsesorProceso.FirstName + " " + x.AsesorProceso.LastName,
                                    codigo = x.AsesorProceso.Codigo,
                                    id = x.AsesorId,
                                }
                                : null,
                        Auditor =
                            x.AuditorProceso != null
                                ? new CommonUserVm
                                {
                                    fullName =
                                        x.AuditorProceso.FirstName
                                        + " "
                                        + x.AuditorProceso.LastName,
                                    codigo = x.AuditorProceso.Codigo,
                                    id = x.AuditorId,
                                }
                                : null,
                        Tipologia = new CommonVm
                        {
                            name =
                                language == "es"
                                    ? x.Empresa.Tipologias.First().Tipologia.Name
                                    : x.Empresa.Tipologias.First().Tipologia.NameEnglish,
                        },
                        Observaciones = x.Resultados.First().Observaciones,
                        FechaDictamen = x.FechaFinalizacion.Value.AddHours(-6).ToStringArg(),
                        Distintivo = x.Resultados.First().Aprobado
                            ? language == "es"
                                ? distintivos
                                    .FirstOrDefault(u => u.Id == x.Resultados.First().DistintivoId)
                                    .Name
                                : distintivos
                                    .FirstOrDefault(u => u.Id == x.Resultados.First().DistintivoId)
                                    .NameEnglish
                            : noCertificado,
                        NumeroDictamen = x.Resultados.First().NumeroDictamen,
                    })
                    .ToList();

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }

        public async Task<List<EstadisticaItemVm>> EstadisticasCtc(int idPais, string lang)
        {
            try
            {
                //Empresas que estan en asesoria
                var empresasPorTipologia = _db
                    .Tipologia.Include(s => s.Empresas)
                    .Select(x => new EstadisticaItemVm
                    {
                        Id = x.Id,
                        Name = lang == "es" ? x.Name : x.NameEnglish,
                        Count = x
                            .Empresas.Where(s =>
                                s.Empresa.Active
                                && s.Empresa.Estado > 0
                                && s.Empresa.Estado > 5
                                && s.Empresa.Estado < 8
                                && (s.Empresa.PaisId == idPais || idPais == 0)
                            )
                            .Count(),
                    });

                return await empresasPorTipologia.ToListAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<EmpresaUpdateVm> Data(int empresaId, ApplicationUser user)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            try
            {
                var empresa = await _db
                    .Empresa.AsNoTracking()
                    .Include(p => p.Pais)
                    .Include(t => t.Tipologias)
                    .Include(x => x.Archivos.Where(a => a.Activo))
                    .ThenInclude(c => c.UsuarioCarga)
                    .FirstOrDefaultAsync(s => s.Id == empresaId && s.Active);

                if (empresa == null)
                {
                    _logger.LogWarning("Empresa no encontrada o inactiva: {EmpresaId}", empresaId);
                    throw new NotFoundException(
                        $"La empresa {empresaId} no existe o está inactiva."
                    );
                }

                // Obtener tipologías
                var allTipologias = await _db.Tipologia.AsNoTracking().ToListAsync();

                // Construir el resultado usando un builder pattern
                var resultBuilder = new EmpresaUpdateBuilder(empresa, user)
                    .WithBasicInfo()
                    .WithLocation()
                    .WithContactInfo()
                    .WithTipologias(allTipologias)
                    .WithArchivos();

                var result = resultBuilder.Build();

                // Obtener y procesar certificaciones
                await EnrichWithCertificationsAsync(result, user);

                return result;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al obtener datos de empresa para usuario {UserId}",
                    user.Id
                );
                throw new BusinessException("Error al obtener datos de la empresa.", ex);
            }
        }

        private async Task EnrichWithCertificationsAsync(
            EmpresaUpdateVm result,
            ApplicationUser user
        )
        {
            result.Certificaciones = await GetCertificacionesAsync(user, result.Id);

            if (result.Certificaciones.Any())
            {
                result.CertificacionActual = ProcessCurrentCertification(result.Certificaciones);
                await CheckAndNotifyExpirationAsync(result.CertificacionActual, user);
            }
        }

        private async Task CheckAndNotifyExpirationAsync(
            CertificacionDetailsVm certification,
            ApplicationUser user
        )
        {
            if (
                certification.alertaVencimiento
                && !await _notificationService.HasBeenNotifiedAsync(user.Id, certification.Id)
            )
            {
                await _notificationService.SendExpirationNotificationAsync(user, certification);
            }
        }

        private static List<CommonVm> MapTipologias(
            List<Tipologia> allTipologias,
            ICollection<TipologiasEmpresa> empresaTipologias,
            string language
        )
        {
            return allTipologias
                .Select(x => new CommonVm
                {
                    name = language == "es" ? x.Name : x.NameEnglish,
                    id = x.Id,
                    isSelected = empresaTipologias.Any(z => z.IdTipologia == x.Id),
                })
                .ToList();
        }

        private static List<ArchivoVm> MapArchivos(ICollection<Archivo> archivos, string userId)
        {
            if (archivos == null)
                return null;

            return archivos
                .Where(s => s.Activo)
                .Select(z => new ArchivoVm
                {
                    Id = z.Id,
                    Nombre = z.Nombre,
                    Ruta = z.Ruta,
                    Tipo = z.Tipo,
                    Cargador = $"{z.UsuarioCarga.FirstName} {z.UsuarioCarga.LastName}",
                    FechaCarga = z.FechaCarga.ToUtc(),
                    Propio = z.UsuarioCargaId == userId,
                })
                .ToList();
        }

        private async Task<List<CertificacionDetailsVm>> GetCertificacionesAsync(
            ApplicationUser user,
            int companyId
        )
        {
            var noCertificado = user.Lenguage == "es" ? "No certificado" : "Not certified";

            return await _db
                .ProcesoCertificacion.Include(i => i.Resultados)
                .ThenInclude(it => it.Distintivo)
                .Include(x => x.AsesorProceso)
                .Include(x => x.AuditorProceso)
                .Include(x => x.UserGenerador)
                .Where(s => s.EmpresaId == companyId)
                .Select(x => new CertificacionDetailsVm
                {
                    Status = Utilities.Utilities.CambiarIdiomaEstado(x.Status, user.Lenguage),
                    FechaInicio = x.FechaInicio.ToUtc(),
                    FechaFin = x.FechaFinalizacion.ToUtc(),
                    Expediente = x.NumeroExpediente,
                    Recertificacion = x.Recertificacion,
                    Asesor = MapCommonUser(x.AsesorProceso),
                    Auditor = x.AuditorId != null ? MapCommonUser(x.AuditorProceso) : null,
                    Generador = MapCommonUser(x.UserGenerador),
                    Resultado = GetResultado(x.Resultados, user.Lenguage, noCertificado),
                    FechaVencimiento = x.FechaVencimiento.ToStringArg(),
                    Id = x.Id,
                })
                .ToListAsync();
        }

        private static CommonUserVm MapCommonUser(ApplicationUser user)
        {
            return new CommonUserVm
            {
                email = user.Email,
                fullName = $"{user.FirstName} {user.LastName}",
                id = user.Id,
                phone = user.PhoneNumber,
                codigo = user.NumeroCarnet,
            };
        }

        private static string GetResultado(
            ICollection<ResultadoCertificacion> resultados,
            string language,
            string noCertificado
        )
        {
            if (!resultados.Any())
                return string.Empty;

            var firstResult = resultados.First();
            return firstResult.Aprobado
                ? language == "es"
                    ? firstResult.Distintivo.Name
                    : firstResult.Distintivo.NameEnglish
                : noCertificado;
        }

        private static CertificacionDetailsVm ProcessCurrentCertification(
            List<CertificacionDetailsVm> certificaciones
        )
        {
            var currentCertification = certificaciones.OrderByDescending(s => s.Id).First();

            if (currentCertification.FechaVencimiento != null)
            {
                var dueDate = DateTime.Now.AddMonths(6);
                var vencimientoSello = currentCertification.FechaVencimiento.ToDateArg();
                if (vencimientoSello < dueDate)
                {
                    currentCertification.alertaVencimiento = true;
                }
            }

            return currentCertification;
        }

        public EstadisticasVm Estadisticas(string lang)
        {
            var empresasPorPais = _db.Pais.Select(x => new EstadisticaItemVm
            {
                Id = x.Id,
                Name =
                    (x.Name == "Republica Dominicana" && lang == "en")
                        ? "Dominican Republic"
                        : x.Name,
                Count = x.Empresas.Count(s => s.Active),
            });

            var empresasPorTipologia = _db.Tipologia.Select(x => new EstadisticaItemVm
            {
                Id = x.Id,
                Name = lang == "es" ? x.Name : x.NameEnglish,
                Count = x.Empresas.Count(),
            });

            var result = new EstadisticasVm
            {
                EmpresasPorPais = empresasPorPais,
                EmpresasPorTipologia = empresasPorTipologia,
            };

            return result;
        }

        public async Task<UrlResult> Delete(int id, int paisId, string role)
        {
            try
            {
                var empresa = _db.Empresa.Find(id);

                if (role != "Admin")
                {
                    if (empresa.PaisId != paisId)
                    {
                        return new UrlResult { Success = false, Message = "Error 403 Forbidden" };
                    }
                }

                var homologaciones = _db.Homologacion.Where(s => s.EmpresaId == id);
                var proceso = _db.ProcesoCertificacion.Where(s => s.EmpresaId == id);
                var cuestionarios = _db
                    .CuestionarioItem.Include(s => s.Archivos)
                    .Where(s => s.Cuestionario.IdEmpresa == id);

                foreach (var item in cuestionarios)
                {
                    if (item.Archivos.Any())
                    {
                        _db.Archivo.RemoveRange(item.Archivos);
                    }

                    var obs = _db.CuestionarioItemObservaciones.Where(s =>
                        s.CuestionarioItemId == item.Id
                    );
                    if (obs.Any())
                    {
                        _db.CuestionarioItemObservaciones.RemoveRange(obs);
                    }
                }
                await _db.SaveChangesAsync();

                _db.CuestionarioItem.RemoveRange(cuestionarios);
                _db.SaveChanges();

                var otrosarchivos = _db.Archivo.Where(s => s.EmpresaId == id);
                _db.Archivo.RemoveRange(otrosarchivos);
                await _db.SaveChangesAsync();

                var cuestionariosObj = _db.Cuestionario.Where(s => s.IdEmpresa == id);
                _db.Cuestionario.RemoveRange(cuestionariosObj);
                _db.SaveChanges();
                foreach (var item in proceso)
                {
                    var resultado = _db.ResultadoCertificacion.Where(s =>
                        s.CertificacionId == item.Id
                    );
                    _db.ResultadoCertificacion.RemoveRange(resultado);
                }
                await _db.SaveChangesAsync();

                _db.ProcesoCertificacion.RemoveRange(proceso);
                await _db.SaveChangesAsync();

                _db.Empresa.Remove(empresa);
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return new UrlResult
                {
                    Success = false,
                    Message = e.InnerException + " " + e.Message,
                };
            }

            return new UrlResult { Success = true };
        }

        public async Task<ResponseListadoExterno> GetCertificadasParaExterior(
            ListadoExternoFiltro filtro
        )
        {
            var countries = await _db.Pais.ToListAsync();
            var pais = 0;
            if (!string.IsNullOrEmpty(filtro.Pais))
            {
                var paisObj = countries.FirstOrDefault(s =>
                    s.Name.ToLower() == filtro.Pais.ToLower()
                );
                if (paisObj != null)
                {
                    pais = paisObj.Id;
                }
            }

            try
            {
                //empresas que estan en recertificacion teniendo un solo proceso (importadas) o empresas que tienen un proceso finalizado y con fechavencimiento valida
                var empresasImportadas = await _db
                    .Empresa.Include(s => s.Certificaciones)
                    .Include(s => s.Tipologias)
                    .ThenInclude(s => s.Tipologia)
                    .Where(s =>
                        (s.PaisId == pais || pais == 0)
                        && (
                            (
                                s.Certificaciones.Count == 1
                                && s.Certificaciones.Any(x => x.Recertificacion)
                            )
                            || s.Certificaciones.Any(s =>
                                s.FechaVencimiento != null && s.FechaVencimiento > DateTime.Now.Date
                            )
                        )
                    )
                    .ToListAsync();

                return new ResponseListadoExterno
                {
                    Success = true,
                    Data = empresasImportadas
                        .Select(x => new ListadoExterno
                        {
                            Id = x.Id.ToString(),
                            Nombre = x.Nombre,
                            Pais = x.Pais.Name,
                            Tipologia = x.Tipologias.First().Tipologia.Name,
                        })
                        .ToList(),
                };
            }
            catch (Exception e)
            {
                return new ResponseListadoExterno
                {
                    Success = false,
                    Message = e.InnerException.ToString(),
                };
            }
        }
    }
}
