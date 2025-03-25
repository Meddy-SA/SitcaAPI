using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.Constants;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.DataAccess.Middlewares;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;
using Utilities;
using StatusPro = Utilities.Common.Constants;

namespace Sitca.DataAccess.Data.Repository
{
    public class HomologacionRepository : Repository<Homologacion>, IHomologacionRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<HomologacionRepository> _logger;

        public HomologacionRepository(
            ApplicationDbContext db,
            ILogger<HomologacionRepository> logger
        )
            : base(db)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Result<HomologacionBloqueoDto>> ToggleBloqueoEdicionAsync(int id)
        {
            try
            {
                var homologacion = await _db.Homologacion.FindAsync(id);

                if (homologacion == null)
                {
                    _logger.LogWarning("Homologación no encontrada: {Id}", id);
                    return Result<HomologacionBloqueoDto>.Failure(
                        $"Homologación {id} no encontrada"
                    );
                }

                homologacion.EnProcesoSiccs = !homologacion.EnProcesoSiccs;
                homologacion.FechaUltimaEdicion = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                return Result<HomologacionBloqueoDto>.Success(
                    new HomologacionBloqueoDto
                    {
                        EstaBloqueado = homologacion.EnProcesoSiccs ?? false,
                        EmpresaId = homologacion.EmpresaId,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al modificar el estado de bloqueo de la homologación {Id}",
                    id
                );
                return Result<HomologacionBloqueoDto>.Failure(
                    $"Error al modificar el estado de bloqueo: {ex.Message}"
                );
            }
        }

        public async Task<int> Create(HomologacionDTO datos, ApplicationUser user)
        {
            // Definir la estrategia de ejecución para manejar errores transitorios en SQL Server
            var executionStrategy = _db.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    var tipologia = await _db.Tipologia.FindAsync(datos.tipologia.Id);
                    if (tipologia == null)
                        throw new ArgumentException(
                            $"No se encontró la tipología con ID {datos.tipologia.Id}"
                        );

                    var distintivoSiccs = await _db.Distintivo.FindAsync(datos.distintivoSiccs.id);
                    if (distintivoSiccs == null)
                        throw new ArgumentException(
                            $"No se encontró el distintivo SICCS con ID {datos.distintivoSiccs.id}"
                        );

                    #region ----CREA EMPRESA----
                    var empresa = new Empresa
                    {
                        Active = true,
                        EsHomologacion = true,
                        Nombre = datos.nombre,
                        PaisId = user.PaisId,
                        IdPais = user.PaisId ?? 6,
                        Estado = 8,
                        ResultadoVencimiento = datos.fechaVencimiento,
                        // Inicialización de campos obligatorios con valores por defecto
                        NombreRepresentante = string.Empty,
                        CargoRepresentante = string.Empty,
                        Ciudad = string.Empty,
                        IdNacional = string.Empty,
                        Telefono = string.Empty,
                        Calle = string.Empty,
                        Numero = string.Empty,
                        Direccion = string.Empty,
                        Longitud = string.Empty,
                        Latitud = string.Empty,
                        Email = string.Empty,
                        WebSite = string.Empty,
                        ResultadoSugerido = string.Empty,
                        ResultadoActual = string.Empty,
                    };
                    await _db.Empresa.AddAsync(empresa);
                    await _db.SaveChangesAsync();
                    #endregion

                    #region ----AGREGA TIPOLOGIA----
                    var tipologiaEmpresa = new TipologiasEmpresa
                    {
                        IdEmpresa = empresa.Id,
                        IdTipologia = tipologia.Id,
                    };
                    await _db.Set<TipologiasEmpresa>().AddAsync(tipologiaEmpresa);
                    await _db.SaveChangesAsync();

                    #endregion

                    #region ----AGREGA CERTIFICACION----
                    var proceso = new ProcesoCertificacion
                    {
                        FechaFinalizacion = datos.fechaVencimiento,
                        FechaInicio = datos.fechaOtorgamiento,
                        TipologiaId = tipologia.Id,
                        EmpresaId = empresa.Id,
                        FechaVencimiento = datos.fechaVencimiento,
                        UserGeneraId = user.Id,
                        Status = StatusConstants.GetLocalizedStatus(
                            StatusPro.ProcessStatus.Completed,
                            "es"
                        ),
                        NumeroExpediente = string.Empty,
                    };
                    await _db.ProcesoCertificacion.AddAsync(proceso);

                    // Guardar para obtener el ID del proceso
                    await _db.SaveChangesAsync();
                    #endregion

                    #region ----AGREGA DISTINTIVO SICCS----
                    var obs = string.IsNullOrEmpty(datos.datosProceso)
                        ? string.Empty
                        : datos.datosProceso;
                    var resultado = new ResultadoCertificacion
                    {
                        Aprobado = true,
                        CertificacionId = proceso.Id,
                        DistintivoId = distintivoSiccs.Id,
                        Observaciones = obs,
                        NumeroDictamen = string.Empty,
                    };
                    await _db.ResultadoCertificacion.AddAsync(resultado);
                    await _db.SaveChangesAsync();
                    #endregion

                    #region ----CREA HOMOLOGACION----
                    var homologacion = new Homologacion
                    {
                        CertificacionId = proceso.Id,
                        FechaCreacion = DateTime.UtcNow,
                        DistintivoExterno = datos.selloItc.name,
                        EnProcesoSiccs = false,
                        DatosProceso = obs,
                        FechaVencimiento = datos.fechaVencimiento,
                        FechaOtorgamiento = datos.fechaOtorgamiento,
                        Distintivo =
                            user.Lenguage == "es"
                                ? distintivoSiccs.Name
                                : distintivoSiccs.NameEnglish,
                        EmpresaId = empresa.Id,
                    };
                    await _db.Homologacion.AddAsync(homologacion);

                    // Guardar todos los cambios pendientes de una vez
                    await _db.SaveChangesAsync();
                    #endregion

                    await transaction.CommitAsync();
                    return empresa.Id;
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    // Loguear el error completo con información detallada
                    _logger.LogError(e, "Error al crear homologación: {Message}", e.Message);

                    // Lanzar una excepción personalizada con información útil
                    throw new DatabaseException("Error al crear la homologación", e);
                }
            });
        }

        public async Task<HomologacionDTO> Details(ApplicationUser appUser, string role, int id)
        {
            var item = await _db
                .Homologacion.Include(s => s.Empresa)
                .ThenInclude(s => s.Tipologias)
                .ThenInclude(s => s.Tipologia)
                .Include(s => s.Empresa)
                .ThenInclude(s => s.Archivos)
                .ThenInclude(s => s.UsuarioCarga)
                .FirstOrDefaultAsync(s =>
                    s.Id == id && (s.Empresa.PaisId == appUser.PaisId || role == "Admin")
                );

            var tipologia = item.Empresa.Tipologias.First().Tipologia;

            var result = new HomologacionDTO
            {
                id = item.Id,
                empresaId = item.EmpresaId,
                fechaOtorgamiento = item.FechaOtorgamiento,
                fechaVencimiento = item.FechaVencimiento,
                nombre = item.Empresa.Nombre,
                enProceso = item.EnProcesoSiccs == true,
                tipologia = new Tipologia
                {
                    Id = tipologia.Id,
                    Name = appUser.Lenguage == "es" ? tipologia?.Name : tipologia?.NameEnglish,
                    NameEnglish = tipologia?.NameEnglish,
                },
                datosProceso = item.DatosProceso,
                distintivoSiccs = new CommonVm { name = item.Distintivo },
                selloItc = new SelloItc { name = item.DistintivoExterno },
                archivos =
                    item.Empresa.Archivos != null
                        ? item
                            .Empresa.Archivos.Where(s => s.Activo)
                            .Select(z => new ArchivoVm
                            {
                                Id = z.Id,
                                Nombre = z.Nombre,
                                Ruta = z.Ruta,
                                Tipo = z.Tipo,
                                Cargador = z.UsuarioCarga.FirstName + " " + z.UsuarioCarga.LastName,
                                FechaCarga = z.FechaCarga.ToUtc(),
                                Propio = true,
                            })
                            .ToList()
                        : null,
            };

            return result;
        }

        public async Task<List<HomologacionDTO>> List(int country)
        {
            var homologaciones = await _db
                .Homologacion.AsNoTracking()
                .Include(s => s.Empresa)
                .ThenInclude(s => s.Tipologias)
                .Where(s => s.Empresa.PaisId == country)
                .ToListAsync();

            var tipologias = await _db.Tipologia.ToListAsync();

            return homologaciones
                .Select(s => new HomologacionDTO
                {
                    id = s.Id,
                    empresaId = s.EmpresaId,
                    fechaOtorgamiento = s.FechaOtorgamiento,
                    fechaVencimiento = s.FechaVencimiento,
                    nombre = s.Empresa.Nombre,
                    tipologia = tipologias.First(x =>
                        x.Id == s.Empresa.Tipologias.First().IdTipologia
                    ),
                    datosProceso = s.DatosProceso,
                    distintivoSiccs = new CommonVm { name = s.Distintivo },
                    selloItc = new SelloItc { name = s.DistintivoExterno },
                })
                .ToList();
        }

        public async Task<bool> Update(ApplicationUser appUser, string role, HomologacionDTO datos)
        {
            var distintivosSiccs = await _db.Distintivo.ToListAsync();

            var item = await _db
                .Homologacion.Include(s => s.Empresa)
                .ThenInclude(s => s.Tipologias)
                .ThenInclude(s => s.Tipologia)
                .Include(s => s.Certificacion)
                .ThenInclude(s => s.Resultados)
                .FirstOrDefaultAsync(s =>
                    s.Id == datos.id && (s.Empresa.PaisId == appUser.PaisId || role == "Admin")
                );

            if (item.EnProcesoSiccs == true)
            {
                return false;
            }

            item.Empresa.Nombre = datos.nombre;
            item.FechaUltimaEdicion = DateTime.UtcNow;
            item.DatosProceso = datos.datosProceso;
            var certificacionActual = item
                .Empresa.Certificaciones.OrderByDescending(s => s.Id)
                .First();

            if (item.FechaOtorgamiento != datos.fechaOtorgamiento)
            {
                item.FechaOtorgamiento = datos.fechaOtorgamiento;
                certificacionActual.FechaInicio = datos.fechaOtorgamiento;
            }

            if (item.FechaVencimiento != datos.fechaVencimiento)
            {
                item.FechaVencimiento = datos.fechaVencimiento;
                certificacionActual.FechaVencimiento = datos.fechaVencimiento;
            }

            _db.SaveChanges();

            var tipologia = item.Empresa.Tipologias.First().Tipologia;
            if (datos.selloItc.name != item.DistintivoExterno)
            {
                //hay que modificar el distintivo
                item.DistintivoExterno = datos.selloItc.name;
                item.Distintivo = datos.distintivoSiccs.name;

                item.Empresa.ResultadoActual = datos.distintivoSiccs.name;
                var resultadoActual = item
                    .Empresa.Certificaciones.OrderByDescending(s => s.Id)
                    .First()
                    .Resultados.FirstOrDefault();

                var nuevoDistintivo = distintivosSiccs.First(s =>
                    s.Name == datos.distintivoSiccs.name
                    || s.NameEnglish == datos.distintivoSiccs.name
                );

                resultadoActual.DistintivoId = nuevoDistintivo.Id;

                _db.SaveChanges();
            }

            //actualizar la tipologia
            if (datos.tipologia.Id != tipologia.Id)
            {
                item.Empresa.Tipologias.Clear();
                await _db.SaveChangesAsync();

                #region ----AGREGA TIPOLOGIA----

                var listaTipologias = new List<TipologiasEmpresa>();

                var TipologiaEmpresa = new TipologiasEmpresa
                {
                    IdEmpresa = item.EmpresaId,
                    IdTipologia = datos.tipologia.Id,
                };

                listaTipologias.Add(TipologiaEmpresa);

                item.Empresa.Tipologias = listaTipologias;

                _db.SaveChanges();

                #endregion
            }
            return true;
        }
    }
}
