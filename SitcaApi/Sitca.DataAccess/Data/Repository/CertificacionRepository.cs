using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.Constants;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.DataAccess.Services.Cuestionarios;
using Sitca.Models;
using Sitca.Models.Constants;
using Sitca.Models.DTOs;
using Sitca.Models.Enums;
using Sitca.Models.ViewModels;
using Utilities;
using static Utilities.Common.Constants;
using ConstantRoles = Utilities.Common.Constants.Roles;

namespace Sitca.DataAccess.Data.Repository
{
    class CertificacionRepository : Repository<ProcesoCertificacion>, ICertificacionRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly ICuestionarioReaperturaService _cuestionarioService;
        private readonly ILogger<CertificacionRepository> _logger;
        private const int ESTADO_AUDITORIA_EN_PROCESO = 5;

        public CertificacionRepository(
            ApplicationDbContext db,
            IConfiguration configuration,
            ICuestionarioReaperturaService cuestionarioService,
            ILogger<CertificacionRepository> logger
        )
            : base(db)
        {
            _db = db;
            _config = configuration;
            _cuestionarioService = cuestionarioService;
            _logger = logger;
        }

        public async Task<Result<bool>> UpdateNumeroExpAsync(CertificacionDetailsVm data)
        {
            if (data == null)
                return Result<bool>.Failure("Los datos de certificación son requeridos");

            if (string.IsNullOrWhiteSpace(data.Expediente))
                return Result<bool>.Failure("El número de expediente es requerido");

            var strategy = _db.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _db.Database.BeginTransactionAsync();
                    try
                    {
                        var certificacion = await _db.ProcesoCertificacion.FirstOrDefaultAsync(c =>
                            c.Id == data.Id
                        );

                        if (certificacion == null)
                            return Result<bool>.Failure(
                                $"No se encontró la certificación con ID {data.Id}"
                            );

                        certificacion.NumeroExpediente = data.Expediente;
                        await _db.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation(
                            "Número de expediente actualizado para certificación {CertificacionId}: {NumeroExpediente}",
                            data.Id,
                            data.Expediente
                        );

                        return Result<bool>.Success(true);
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
                    data.Id,
                    ex.Message
                );
                return Result<bool>.Failure(
                    $"Error al actualizar número de expediente: {ex.Message}"
                );
            }
        }

        public async Task<bool> SaveCalificacion(
            SaveCalificacionVm data,
            ApplicationUser appUser,
            string role
        )
        {
            //agregar tipologia
            try
            {
                var certificacion = await _db
                    .ProcesoCertificacion.Include(s => s.Empresa)
                    .FirstOrDefaultAsync(x => x.Id == data.idProceso);
                var empresa = certificacion.Empresa;
                certificacion.FechaFinalizacion = DateTime.UtcNow;

                if (data.aprobado)
                {
                    certificacion.FechaVencimiento = DateTime.UtcNow.AddYears(2);
                    empresa.ResultadoVencimiento = certificacion.FechaVencimiento;
                    var distintivo = await _db.Distintivo.FindAsync(data.distintivoId);
                    empresa.ResultadoActual =
                        appUser.Lenguage == "es" ? distintivo.Name : distintivo.NameEnglish;
                }

                var resultado = new ResultadoCertificacion
                {
                    Aprobado = data.aprobado,
                    DistintivoId = data.aprobado ? data.distintivoId : (int?)null,
                    CertificacionId = data.idProceso,
                    NumeroDictamen = data.Dictamen,
                    Observaciones = data.Observaciones,
                };
                _db.ResultadoCertificacion.Add(resultado);

                int toStatus = 8;
                var newStatus = new CertificacionStatusVm
                {
                    CertificacionId = data.idProceso,
                    Status = StatusConstants.GetLocalizedStatus(toStatus, "es"),
                };
                await ChangeStatus(newStatus, toStatus);

                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<Result<int>> AsignaAuditorAsync(
            AsignaAuditoriaVm data,
            string language = "es"
        )
        {
            // NOTE: Agregar tipologia
            try
            {
                var proceso = await _db.ProcesoCertificacion.FirstOrDefaultAsync(s =>
                    s.EmpresaId == data.EmpresaId && s.FechaFinalizacion == null
                );

                proceso.AuditorId = data.AuditorId;
                proceso.FechaFijadaAuditoria = data.Fecha.ToDateUniversal();
                proceso.FechaSolicitudAuditoria = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                int toStatus = 4;

                var nuevoEstado = new CertificacionStatusVm
                {
                    CertificacionId = proceso.Id,
                    Status = StatusConstants.GetLocalizedStatus(toStatus, language),
                };

                await ChangeStatus(nuevoEstado, toStatus);
                return Result<int>.Success(proceso.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al asignar auditor {AuditorId} a empresa {EmpresaId}",
                    data.AuditorId,
                    data.EmpresaId
                );
                return Result<int>.Failure("Error al asignar auditor");
            }
        }

        public async Task<Result<CuestionarioDetailsMinVm>> GenerarCuestionarioAsync(
            CuestionarioCreateVm data,
            string userGenerador,
            string role
        )
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            try
            {
                // Validar existencia de entidades relacionadas
                var empresa = await _db.Empresa.FindAsync(data.EmpresaId);
                if (empresa == null)
                    return Result<CuestionarioDetailsMinVm>.Failure("La empresa no existe");

                var proceso = await _db.ProcesoCertificacion.FindAsync(data.CertificacionId);
                if (proceso == null)
                    return Result<CuestionarioDetailsMinVm>.Failure("La certificación no existe");

                // Crear cuestionario
                var cuestionario = new Cuestionario
                {
                    FechaInicio = DateTime.UtcNow,
                    FechaGenerado = DateTime.UtcNow,
                    AsesorId = data.AsesorId,
                    IdEmpresa = data.EmpresaId,
                    TipologiaId = data.TipologiaId,
                    IdTipologia = data.TipologiaId,
                    Prueba = role == ConstantRoles.Asesor,
                    ProcesoCertificacionId = proceso.Id,
                    AuditorId = role == ConstantRoles.Auditor ? proceso.AuditorId : null,
                };

                await _db.Cuestionario.AddAsync(cuestionario);

                // Actualizar estado según rol
                switch (role)
                {
                    case ConstantRoles.Asesor:
                        empresa.Estado = ProcessStatus.ConsultancyUnderway;
                        proceso.TipologiaId = cuestionario.TipologiaId;
                        proceso.Status = StatusConstants.GetLocalizedStatus(
                            ProcessStatus.ConsultancyUnderway,
                            "es"
                        );
                        break;

                    case ConstantRoles.Auditor:
                        empresa.Estado = ProcessStatus.AuditingUnderway;
                        proceso.Status = StatusConstants.GetLocalizedStatus(
                            ProcessStatus.AuditingUnderway,
                            "es"
                        );
                        break;
                }

                // Guardar cambios
                await _db.SaveChangesAsync();

                var result = new CuestionarioDetailsMinVm
                {
                    Id = cuestionario.Id,
                    Asesor = new CommonUserVm { id = data.AsesorId, email = string.Empty },
                    Empresa = new CommonVm { id = data.EmpresaId },
                    Prueba = cuestionario.Prueba,
                    Tipologia = new CommonVm { id = data.TipologiaId },
                };

                return Result<CuestionarioDetailsMinVm>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error generando cuestionario para empresa {EmpresaId}",
                    data.EmpresaId
                );
                return Result<CuestionarioDetailsMinVm>.Failure(
                    $"Error generando cuestionario: {ex.Message}"
                );
            }
        }

        public async Task<Result<int>> ComenzarProcesoAsync(
            CertificacionVm data,
            string userGenerador
        )
        {
            try
            {
                if (data == null)
                    return Result<int>.Failure("Los datos de certificación son requeridos");

                if (string.IsNullOrEmpty(userGenerador))
                    return Result<int>.Failure("El usuario generador es requerido");

                const int toStatus = 1;

                // Crear una estrategia de ejecución
                var strategy = _db.Database.CreateExecutionStrategy();

                return await strategy.ExecuteAsync(async () =>
                {
                    // Validar si la empresa existe
                    var empresa = await _db.Empresa.FirstOrDefaultAsync(s =>
                        s.Id == data.EmpresaId
                    );
                    if (empresa == null)
                        return Result<int>.Failure($"La empresa con ID {data.EmpresaId} no existe");

                    // Verificar procesos existentes
                    var otrosProcesos = await _db.ProcesoCertificacion.AnyAsync(s =>
                        s.EmpresaId == data.EmpresaId
                    );

                    var status = StatusConstants.GetLocalizedStatus(toStatus, "es");

                    using var transaction = await _db.Database.BeginTransactionAsync();
                    try
                    {
                        var proceso = new ProcesoCertificacion
                        {
                            AsesorId = data.Asesor,
                            EmpresaId = data.EmpresaId,
                            Status = status,
                            FechaInicio = DateTime.UtcNow,
                            UserGeneraId = userGenerador,
                            Recertificacion = otrosProcesos,
                            NumeroExpediente = "",
                        };

                        _db.ProcesoCertificacion.Add(proceso);
                        await _db.SaveChangesAsync();

                        empresa.Estado = toStatus;
                        empresa.FechaAutoNotif = null;
                        await _db.SaveChangesAsync();

                        await transaction.CommitAsync();
                        return Result<int>.Success(proceso.Id);
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
                    "Error al comenzar proceso para empresa {IdEmpresa}. Detalles: {Details}",
                    data.EmpresaId,
                    ex.Message
                );
                return Result<int>.Failure(
                    $"Error al iniciar el proceso de certificación: {ex.Message}"
                );
            }
        }

        public async Task<Result<bool>> CambiarAuditorAsync(CambioAuditor data)
        {
            if (data == null)
                return Result<bool>.Failure("Los datos de cambio son requeridos");

            if (string.IsNullOrEmpty(data.userId))
                return Result<bool>.Failure("El ID del usuario es requerido");

            var strategy = _db.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _db.Database.BeginTransactionAsync();
                    try
                    {
                        var proceso = await _db.ProcesoCertificacion.FindAsync(data.idProceso);

                        if (proceso == null)
                            return Result<bool>.Failure(
                                $"No se encontró el proceso con ID {data.idProceso}"
                            );

                        // Obtener todos los cuestionarios que necesitan actualización
                        var cuestionarios = await _db
                            .Cuestionario.Where(s =>
                                s.ProcesoCertificacionId == proceso.Id
                                && !s.Prueba
                                && s.FechaFinalizado == null
                            )
                            .ToListAsync();

                        if (data.auditor)
                        {
                            // Actualizar Auditor
                            proceso.AuditorId = data.userId;
                            foreach (var cuestionario in cuestionarios)
                            {
                                cuestionario.AuditorId = proceso.AuditorId;
                            }

                            _logger.LogInformation(
                                "Cambiando auditor del proceso {ProcesoId} al usuario {UserId}",
                                data.idProceso,
                                data.userId
                            );
                        }
                        else
                        {
                            // Actualizar Asesor
                            proceso.AsesorId = data.userId;
                            foreach (var cuestionario in cuestionarios)
                            {
                                cuestionario.AsesorId = proceso.AsesorId;
                            }

                            _logger.LogInformation(
                                "Cambiando asesor del proceso {ProcesoId} al usuario {UserId}",
                                data.idProceso,
                                data.userId
                            );
                        }

                        // Guardar todos los cambios de una vez
                        await _db.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Result<bool>.Success(true);
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
                    "Error al cambiar {Rol} del proceso {ProcesoId} al usuario {UserId}. Error: {Error}",
                    data.auditor ? "auditor" : "asesor",
                    data.idProceso,
                    data.userId,
                    ex.Message
                );

                return Result<bool>.Failure(
                    $"Error al cambiar {(data.auditor ? "auditor" : "asesor")}: {ex.Message}"
                );
            }
        }

        public async Task<List<CuestionarioDetailsMinVm>> GetCuestionariosList(
            int idEmpresa,
            ApplicationUser user
        )
        {
            try
            {
                var cuestionarios = await _db
                    .Cuestionario.AsNoTracking()
                    .Include(s => s.Tipologia)
                    .Include(s => s.Certificacion)
                    .Where(s => s.IdEmpresa == idEmpresa)
                    .Select(s => new CuestionarioDetailsMinVm
                    {
                        Id = s.Id,
                        Prueba = s.Prueba,
                        IdCertificacion = s.ProcesoCertificacionId ?? 0,
                        TecnicoPaisId = s.TecnicoPaisId ?? null,
                        // Manejo seguro de la fecha de evaluación
                        FechaEvaluacion =
                            s.Certificacion != null && s.Certificacion.FechaFijadaAuditoria.HasValue
                                ? s.Certificacion.FechaFijadaAuditoria.Value.ToStringArg()
                                : s.FechaVisita.ToStringArg() ?? string.Empty,
                        // Manejo seguro de fechas
                        FechaFin = s.FechaFinalizado.HasValue
                            ? s.FechaFinalizado.Value.ToUtc()
                            : string.Empty,
                        FechaRevisionAuditor = s.FechaRevisionAuditor.HasValue
                            ? s.FechaRevisionAuditor.Value.ToUtc()
                            : string.Empty,
                        FechaInicio = s.FechaInicio.ToUtc(),

                        // Manejo seguro de la tipología
                        Tipologia = new CommonVm
                        {
                            name =
                                s.Tipologia != null
                                    ? (
                                        user.Lenguage == "es"
                                            ? (s.Tipologia.Name ?? "Sin nombre")
                                            : (s.Tipologia.NameEnglish ?? "No name")
                                    )
                                    : "Sin tipología",
                        },
                    })
                    .ToListAsync();

                return cuestionarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al obtener lista de cuestionarios para empresa {IdEmpresa}. Detalles: {Details}",
                    idEmpresa,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<CuestionarioNoCumpleVm> GetNoCumplimientos(
            int id,
            ApplicationUser user,
            string role
        )
        {
            var cuestionario = await _db
                .Cuestionario.Include("Tipologia")
                .FirstOrDefaultAsync(s => s.Id == id);
            var empresa = _db.Empresa.Find(cuestionario.IdEmpresa);

            var lenguage = user.Lenguage;

            var respuestas = _db.CuestionarioItem.Where(s => s.CuestionarioId == id).ToList();
            var proceso = await _db
                .ProcesoCertificacion.Include(x => x.AsesorProceso)
                .FirstOrDefaultAsync(x => x.Id == cuestionario.ProcesoCertificacionId);

            if (
                (role == "Asesor" && proceso.AsesorId != user.Id)
                || (role == "Auditor" && proceso.AuditorId != user.Id)
            )
            {
                return null;
            }

            var result = new CuestionarioNoCumpleVm
            {
                Id = cuestionario.Id,
                Prueba = cuestionario.Prueba,
                Expediente = proceso.NumeroExpediente,
                FechaFinalizacion = cuestionario.FechaFinalizado.ToStringArg(),
                Tipologia = new CommonVm
                {
                    id = cuestionario.IdTipologia,
                    name = cuestionario.Tipologia.Name,
                },
                Empresa = new CommonVm { id = cuestionario.IdEmpresa, name = empresa.Nombre },
                Asesor = new CommonUserVm
                {
                    codigo = proceso.AsesorProceso.Codigo,
                    fullName =
                        proceso.AsesorProceso.FirstName + " " + proceso.AsesorProceso.LastName,
                },
            };

            var bioseguridad = _config["Settings:bioseguridad"] == "true";

            var pregs = await _db
                .Pregunta.Where(s =>
                    (s.TipologiaId == proceso.TipologiaId || s.TipologiaId == null)
                    && (bioseguridad || s.ModuloId < 11)
                )
                .OrderBy(z => z.Modulo.Orden)
                .ThenBy(y => y.SeccionModuloId)
                .ThenBy(s => s.SubtituloSeccionId)
                .Select(k => new CuestionarioItemVm
                {
                    Id = k.Id,
                    NoAplica = k.NoAplica,
                    Text = lenguage == "es" ? k.Texto : k.Text,
                    Order = Int32.Parse(k.Orden),
                    Nomenclatura = k.Nomenclatura,
                    Obligatoria = k.Obligatoria,
                })
                .ToListAsync();

            foreach (var pregunta in pregs)
            {
                if (respuestas.Any(s => s.PreguntaId == pregunta.Id))
                {
                    var resp = respuestas.First(s => s.PreguntaId == pregunta.Id);
                    pregunta.Result = resp.Resultado;
                    pregunta.IdRespuesta = resp.Id;
                }
            }

            result.Preguntas = pregs;
            return result;
        }

        /// <summary>
        /// Obtiene un cuestionario detallado con su información asociada, incluyendo módulos, preguntas y respuestas.
        /// Valida el acceso del usuario, construye la estructura base del cuestionario, carga los módulos y preguntas,
        /// procesa las respuestas existentes y calcula los resultados finales.
        /// </summary>
        /// <param name="id">Identificador único del cuestionario</param>
        /// <param name="user">Usuario que realiza la solicitud</param>
        /// <param name="role">Rol del usuario en el sistema</param>
        /// <returns>Retorna un objeto CuestionarioDetailsVm con toda la información del cuestionario o null si el usuario no tiene acceso</returns>
        /// <exception cref="Exception">Se propaga cualquier error no controlado durante el proceso</exception>
        public async Task<CuestionarioDetailsVm> GetCuestionario(
            int id,
            ApplicationUser user,
            string role
        )
        {
            try
            {
                // 1. Validación inicial
                var (cuestionario, empresa, proceso) = await GetInitialData(id);
                if (!ValidateUserAccess(proceso, user, role))
                    return null;

                // 2. Construcción del resultado base
                var result = await BuildBaseCuestionario(cuestionario, empresa, proceso, user);

                // 3. Carga de módulos y preguntas
                await LoadModulosYPreguntas(result, cuestionario, user);

                // 4. Procesamiento de respuestas
                await ProcessResponses(result, id);

                // 5. Cálculo de resultados
                await CalculateResults(result, cuestionario, user);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuestionario {Id}", id);
                throw;
            }
        }

        private async Task<(
            Cuestionario cuestionario,
            Empresa empresa,
            ProcesoCertificacion proceso
        )> GetInitialData(int id)
        {
            try
            {
                var cuestionario = await _db
                    .Cuestionario.AsNoTracking()
                    .Include(c => c.Tipologia)
                    .FirstOrDefaultAsync(s => s.Id == id);

                var empresa = await _db.Empresa.FindAsync(cuestionario.IdEmpresa);
                var proceso = await _db.ProcesoCertificacion.FindAsync(
                    cuestionario.ProcesoCertificacionId
                );

                return (cuestionario, empresa, proceso);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos iniciales del cuestionario {Id}", id);
                throw new Exception("Error al cargar datos iniciales del cuestionario", ex);
            }
        }

        private bool ValidateUserAccess(
            ProcesoCertificacion proceso,
            ApplicationUser user,
            string role
        )
        {
            if (proceso == null || user == null)
                return false;

            return role switch
            {
                ConstantRoles.Asesor => proceso.AsesorId == user.Id,
                ConstantRoles.Auditor => proceso.AuditorId == user.Id,
                ConstantRoles.Admin
                or ConstantRoles.TecnicoPais
                or ConstantRoles.CTC
                or ConstantRoles.Consultor => true,
                _ => false,
            };
        }

        private async Task<CuestionarioDetailsVm> BuildBaseCuestionario(
            Cuestionario cuestionario,
            Empresa empresa,
            ProcesoCertificacion proceso,
            ApplicationUser user
        )
        {
            try
            {
                var result = new CuestionarioDetailsVm
                {
                    Id = cuestionario.Id,
                    Prueba = cuestionario.Prueba,
                    Pais = empresa.PaisId.GetValueOrDefault(),
                    Expediente = proceso.NumeroExpediente,
                    Recertificacion = proceso.Recertificacion,
                    FechaFinalizacion = cuestionario.FechaFinalizado.ToStringArg(),
                    FechaRevisionAuditor = cuestionario.FechaRevisionAuditor?.ToStringArg() ?? null,
                    TecnicoPaisId = cuestionario.TecnicoPaisId ?? null,
                    Tipologia = new CommonVm
                    {
                        id = cuestionario.IdTipologia,
                        name = cuestionario.Tipologia.Name,
                    },
                    Empresa = new CommonVm { id = cuestionario.IdEmpresa, name = empresa.Nombre },
                    Modulos = new List<ModulosVm>(),
                };

                // Cargar datos del auditor/asesor en una sola consulta

                var idSearch = cuestionario.Prueba ? proceso.AsesorId : proceso.AuditorId;

                string role = cuestionario.Prueba ? "Asesor" : "Auditor";

                var users = await _db
                    .ApplicationUser.AsNoTracking()
                    .Where(a => a.Id == idSearch)
                    .Select(a => new
                    {
                        a.FirstName,
                        a.LastName,
                        a.NumeroCarnet,
                        a.Id,
                        a.Email,
                        a.PhoneNumber,
                    })
                    .FirstOrDefaultAsync();

                if (users != null)
                {
                    var userVm = new CommonUserVm
                    {
                        fullName = $"{users.FirstName} {users.LastName}".Trim(),
                        codigo = users.NumeroCarnet,
                        id = users.Id,
                        email = users.Email,
                        phone = users.PhoneNumber,
                    };

                    if (cuestionario.Prueba)
                        result.Asesor = userVm;
                    else
                        result.Auditor = userVm;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al construir cuestionario base {Id}", cuestionario.Id);
                throw new Exception("Error al construir datos base del cuestionario", ex);
            }
        }

        private async Task LoadModulosYPreguntas(
            CuestionarioDetailsVm result,
            Cuestionario cuestionario,
            ApplicationUser user
        )
        {
            try
            {
                // Optimización: Carga todos los módulos y sus relaciones en una sola consulta
                var modulos = await _db
                    .Modulo.AsNoTracking()
                    .Include(m => m.Secciones)
                    .ThenInclude(s => s.SubtituloSeccion)
                    .Where(m => m.TipologiaId == cuestionario.IdTipologia || m.TipologiaId == null)
                    .OrderBy(m => m.Orden)
                    .ToListAsync();

                if (_config.GetValue<bool>("Settings:bioseguridad"))
                {
                    modulos = modulos.Where(s => s.Id < 11).OrderBy(m => m.Orden).ToList();
                }

                foreach (var modulo in modulos)
                {
                    var moduloVm = await BuildModuloVm(modulo, cuestionario, user);
                    result.Modulos.Add(moduloVm);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al cargar módulos y preguntas del cuestionario {Id}",
                    cuestionario.Id
                );
                throw new Exception("Error al cargar módulos y preguntas", ex);
            }
        }

        private async Task<ModulosVm> BuildModuloVm(
            Modulo modulo,
            Cuestionario cuestionario,
            ApplicationUser user
        )
        {
            try
            {
                var moduloVm = new ModulosVm
                {
                    Nomenclatura = modulo.Nomenclatura,
                    Orden = modulo.Orden,
                    Nombre = user.Lenguage == "es" ? modulo.Nombre : modulo.EnglishName,
                    Id = modulo.Id,
                    Items = new List<CuestionarioItemVm>(),
                };

                // Si el módulo tiene secciones, procesar cada una
                if (modulo.Secciones?.Any() == true)
                {
                    await ProcessModuleSections(
                        moduloVm.Items,
                        modulo.Secciones,
                        cuestionario.IdTipologia,
                        modulo.Id,
                        user.Lenguage
                    );
                }
                else
                {
                    // Si no tiene secciones, obtener preguntas directamente del módulo
                    var preguntas = await GetPreguntas(modulo.Id, null, null, user.Lenguage);
                    if (preguntas.Any())
                    {
                        moduloVm.Items.AddRange(preguntas);
                    }
                }

                return moduloVm;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al construir ModuloVm para módulo {ModuloId}",
                    modulo.Id
                );
                throw new Exception($"Error al construir módulo {modulo.Id}", ex);
            }
        }

        private async Task ProcessModuleSections(
            List<CuestionarioItemVm> items,
            IEnumerable<SeccionModulo> secciones,
            int tipologiaId,
            int moduloId,
            string language
        )
        {
            foreach (
                var seccion in secciones
                    .Where(s => s.TipologiaId == tipologiaId || s.TipologiaId == null)
                    .OrderBy(x => x.Orden)
            )
            {
                // Agregar la sección como un item
                items.Add(
                    new CuestionarioItemVm
                    {
                        Nomenclatura = seccion.Nomenclatura,
                        Order = int.Parse(seccion.Orden),
                        Text = language == "en" ? seccion.NameEnglish : seccion.Name,
                        Type = "seccion",
                    }
                );

                // Procesar subtítulos si existen
                if (seccion.SubtituloSeccion?.Any() == true)
                {
                    await ProcessSectionSubtitles(
                        items,
                        seccion.SubtituloSeccion,
                        moduloId,
                        seccion.Id,
                        language
                    );
                }

                // Obtener preguntas de la sección sin subtítulo
                var preguntasSeccion = await GetPreguntas(moduloId, seccion.Id, null, language);
                if (preguntasSeccion.Any())
                {
                    items.AddRange(preguntasSeccion);
                }
            }
        }

        private async Task ProcessSectionSubtitles(
            List<CuestionarioItemVm> items,
            IEnumerable<SubtituloSeccion> subtitulos,
            int moduloId,
            int seccionId,
            string language
        )
        {
            foreach (var subtitulo in subtitulos)
            {
                // Agregar el subtítulo como un item
                items.Add(
                    new CuestionarioItemVm
                    {
                        Nomenclatura = subtitulo.Nomenclatura,
                        Order = int.Parse(subtitulo.Orden),
                        Text = language == "en" ? subtitulo.NameEnglish : subtitulo.Name,
                        Type = "subtitulo",
                    }
                );

                // Obtener preguntas del subtítulo
                var preguntasSubtitulo = await GetPreguntas(
                    moduloId,
                    seccionId,
                    subtitulo.Id,
                    language
                );
                if (preguntasSubtitulo.Any())
                {
                    items.AddRange(preguntasSubtitulo);
                }
            }
        }

        private async Task ProcessResponses(CuestionarioDetailsVm result, int cuestionarioId)
        {
            try
            {
                if (result?.Modulos == null)
                {
                    throw new ArgumentException(
                        "El resultado o sus módulos son nulos",
                        nameof(result)
                    );
                }

                var respuestas = await _db
                    .CuestionarioItem.AsNoTracking()
                    .Include(c => c.Archivos)
                    .Where(c => c.CuestionarioId == cuestionarioId)
                    .ToListAsync();

                foreach (var modulo in result.Modulos)
                {
                    var preguntas = modulo.Items.Where(s => s.Type == "pregunta");
                    foreach (var pregunta in preguntas)
                    {
                        var respuesta = respuestas.FirstOrDefault(r => r.PreguntaId == pregunta.Id);
                        if (respuesta != null)
                        {
                            pregunta.Result = respuesta.Resultado;
                            pregunta.IdRespuesta = respuesta.Id;
                            pregunta.TieneArchivos = respuesta.Archivos.Any(a => a.Activo);
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Error al procesar respuestas del cuestionario {Id}. Detalles: {Details}",
                    cuestionarioId,
                    ex.ToString()
                );
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al procesar respuestas del cuestionario {Id}",
                    cuestionarioId
                );
                throw;
            }
        }

        private async Task CalculateResults(
            CuestionarioDetailsVm result,
            Cuestionario cuestionario,
            ApplicationUser user
        )
        {
            try
            {
                var cumplimientos = await _db
                    .Cumplimiento.Include(c => c.Distintivo)
                    .Where(c => c.TipologiaId == cuestionario.IdTipologia || c.TipologiaId == null)
                    .ToListAsync();

                foreach (var modulo in result.Modulos)
                {
                    CalculateModuleResults(modulo, cumplimientos, user.Lenguage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al calcular resultados del cuestionario {Id}",
                    cuestionario.Id
                );
                throw new Exception("Error al calcular resultados", ex);
            }
        }

        private void CalculateModuleResults(
            ModulosVm modulo,
            List<Cumplimiento> cumplimientos,
            string language
        )
        {
            modulo.Resultados = new ResultadosModuloVm
            {
                TotalObligatorias = modulo.Items.Count(s =>
                    s.Type == "pregunta" && s.Obligatoria && s.Result < 2
                ),
                TotalComplementarias = modulo.Items.Count(s =>
                    s.Type == "pregunta" && !s.Obligatoria && s.Result < 2
                ),
                ObligCumple = modulo.Items.Count(s =>
                    s.Type == "pregunta" && s.Obligatoria && s.Result == 1
                ),
                ComplementCumple = modulo.Items.Count(s =>
                    s.Type == "pregunta" && !s.Obligatoria && s.Result == 1
                ),
            };

            CalculatePercentages(modulo.Resultados);
            DetermineModuleResult(modulo, cumplimientos, language);
        }

        private void CalculatePercentages(ResultadosModuloVm resultados)
        {
            resultados.PorcComplementCumple =
                resultados.TotalComplementarias > 0
                    ? resultados.ComplementCumple * 100 / resultados.TotalComplementarias
                    : 0;

            resultados.PorcObligCumple =
                resultados.TotalObligatorias > 0
                    ? resultados.ObligCumple * 100 / resultados.TotalObligatorias
                    : 0;
        }

        private void DetermineModuleResult(
            ModulosVm modulo,
            List<Cumplimiento> cumplimientos,
            string language
        )
        {
            var defaultResult = language == "es" ? "No certificado" : "Not certified";
            modulo.Resultados.ResultModulo = defaultResult;

            if (modulo.Id >= 11)
            {
                modulo.Resultados.ResultModulo = "-";
                return;
            }

            if (modulo.Resultados.TotalObligatorias == modulo.Resultados.ObligCumple)
            {
                var cumplimiento = cumplimientos.FirstOrDefault(c =>
                    c.ModuloId == modulo.Id
                    && modulo.Resultados.PorcComplementCumple > c.PorcentajeMinimo
                    && modulo.Resultados.PorcComplementCumple < (c.PorcentajeMaximo + 1)
                );

                if (cumplimiento?.Distintivo != null)
                {
                    modulo.Resultados.ResultModulo =
                        language == "es"
                            ? cumplimiento.Distintivo.Name
                            : cumplimiento.Distintivo.NameEnglish;
                }
            }
        }

        public async Task<List<CuestionarioItemVm>> GetPreguntas(
            int idModulo,
            int? idSeccion,
            int? idSubtitulo,
            string lang = "es"
        )
        {
            IQueryable<Pregunta> query = _db.Pregunta.AsNoTracking();

            if (idSubtitulo.HasValue && idSubtitulo.Value > 0)
            {
                query = query.Where(s => s.SubtituloSeccionId == idSubtitulo);
            }
            else if (idSeccion.HasValue && idSeccion.Value > 0)
            {
                query = query.Where(s =>
                    s.SeccionModuloId == idSeccion && s.SubtituloSeccionId == null
                );
            }
            else
            {
                query = query.Where(s =>
                    s.ModuloId == idModulo
                    && s.SeccionModuloId == null
                    && s.SubtituloSeccionId == null
                );
            }

            return await query
                .Select(x => new CuestionarioItemVm
                {
                    Id = x.Id,
                    Nomenclatura = x.Nomenclatura,
                    Text = lang == "es" ? x.Texto : x.Text,
                    Order = int.Parse(x.Orden),
                    Result = 0,
                    Type = "pregunta",
                    NoAplica = x.NoAplica,
                    Obligatoria = x.Obligatoria,
                    IdRespuesta = 0,
                })
                .ToListAsync();
        }

        public async Task<bool> ChangeStatus(CertificacionStatusVm data, int status)
        {
            var certificacion = await _db
                .ProcesoCertificacion.Include("Empresa")
                .FirstOrDefaultAsync(s => s.Id == data.CertificacionId);
            certificacion.Status = data.Status;
            certificacion.Empresa.Estado = status;

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<List<HistorialVm>> GetHistory(int idCuestionario)
        {
            var cuestionario = await _db
                .Cuestionario.Include("Items")
                .FirstOrDefaultAsync(s => s.Id == idCuestionario);

            var resultados = new List<HistorialVm>();

            var archivos = await _db
                .Archivo.Where(s => s.CuestionarioItem.CuestionarioId == idCuestionario && s.Activo)
                .ToListAsync();

            var TotalPreguntas = _db.Pregunta.Count(s =>
                (s.TipologiaId == cuestionario.TipologiaId || s.TipologiaId == null)
                && (!s.Nomenclatura.StartsWith("mb-"))
            );
            var TotalPorcentaje = 0;
            foreach (var item in cuestionario.Items.GroupBy(s => s.FechaActualizado))
            {
                var archivosCount = archivos.Count(s => s.FechaCarga.Date == item.Key);
                var currentPorcentaje = item.Count() * 100 / TotalPreguntas;
                TotalPorcentaje += currentPorcentaje;
                var fila = new HistorialVm
                {
                    Fecha = item.Key.ToStringArg(),
                    Cantidad = item.Count(),
                    Archivos = archivosCount,
                    Porcentaje = currentPorcentaje,
                    PorcentajeAcumulado = TotalPorcentaje,
                };

                resultados.Add(fila);
            }

            return resultados;
        }

        public async Task<int> SavePregunta(
            CuestionarioItemVm obj,
            ApplicationUser appUser,
            string role
        )
        {
            var itemCuestionario = await _db.CuestionarioItem.FirstOrDefaultAsync(s =>
                s.CuestionarioId == obj.CuestionarioId && s.PreguntaId == obj.Id
            );
            if (itemCuestionario != null)
            {
                //ya existe, solo editar el result
                itemCuestionario.Resultado = obj.Result ?? 0;
                await _db.SaveChangesAsync();

                return itemCuestionario.Id;
            }

            var nuevoItem = new CuestionarioItem
            {
                Obligatorio = obj.Obligatoria,
                Nomenclatura = obj.Nomenclatura,
                PreguntaId = obj.Id,
                Texto = obj.Text,
                Resultado = obj.Result ?? 0,
                CuestionarioId = obj.CuestionarioId,
                FechaActualizado = DateTime.Now.Date,
            };

            _db.CuestionarioItem.Add(nuevoItem);
            await _db.SaveChangesAsync();

            return nuevoItem.Id;
        }

        public Task<bool> IsCuestionarioCompleto(CuestionarioDetailsVm data)
        {
            bool isCompleto = data
                .Modulos.SelectMany(x => x.Items)
                .Where(s => s.Type == "pregunta")
                .All(s => s.Result != null && s.Result != 0);

            return Task.FromResult(isCompleto);
        }

        public async Task<Result<int>> FinCuestionario(
            int idCuestionario,
            ApplicationUser appUser,
            string role
        )
        {
            try
            {
                var cuestionario = await _db.Cuestionario.FirstOrDefaultAsync(s =>
                    s.Id == idCuestionario
                );

                if (cuestionario == null)
                    return Result<int>.Failure("Cuestionario no encontrado");

                int toStatus = 0;
                switch (role)
                {
                    case ConstantRoles.Asesor:
                        toStatus = 3; // Asesoría finalizada
                        cuestionario.FechaFinalizado = DateTime.UtcNow;
                        cuestionario.Resultado = 1;
                        break;
                    case ConstantRoles.Auditor:
                        if (cuestionario.FechaRevisionAuditor.HasValue)
                            return Result<int>.Failure(
                                "El cuestionario ya fue revisado por un auditor"
                            );
                        cuestionario.FechaRevisionAuditor = DateTime.UtcNow;
                        var resultadoSugerido = await SaveResultadoSugerido(
                            idCuestionario,
                            appUser,
                            role
                        );
                        if (!resultadoSugerido)
                            return Result<int>.Failure("Error al guardar el resultado sugerido");
                        break;
                    case ConstantRoles.TecnicoPais:
                        if (!cuestionario.FechaRevisionAuditor.HasValue)
                            return Result<int>.Failure(
                                "El cuestionario debe ser revisado por un auditor primero"
                            );

                        if (!string.IsNullOrEmpty(cuestionario.TecnicoPaisId))
                            return Result<int>.Failure(
                                "El cuestionario ya fue finalizado por un técnico país"
                            );
                        toStatus = 6; // Auditoría finalizada
                        cuestionario.TecnicoPaisId = appUser.Id;
                        cuestionario.FechaFinalizado = DateTime.UtcNow;
                        cuestionario.Resultado = 1;
                        break;
                    default:
                        return Result<int>.Failure("Rol no autorizado");
                }

                await _db.SaveChangesAsync();

                if (toStatus > 0)
                {
                    var status = new CertificacionStatusVm
                    {
                        CertificacionId = cuestionario.ProcesoCertificacionId ?? 0,
                        Status = StatusConstants.GetLocalizedStatus(toStatus, "es"),
                    };
                    var changeStatusResult = await ChangeStatus(status, toStatus);
                    if (!changeStatusResult) // Asumiendo que ChangeStatus devuelve bool
                        return Result<int>.Failure("Error al cambiar el estado de la certificación");
                }
                return Result<int>.Success(cuestionario.ProcesoCertificacionId ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al finalizar cuestionario {IdCuestionario} por {Role}",
                    idCuestionario,
                    role
                );
                return Result<int>.Failure($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<Result<bool>> CanFinalizeCuestionario(int idCuestionario, string role)
        {
            try
            {
                if (string.IsNullOrEmpty(role))
                    return Result<bool>.Failure("Rol no especificado");

                if (role != "TecnicoPais")
                    return Result<bool>.Success(true); // Otros roles pueden finalizar sin validación

                var cuestionario = await _db
                    .Cuestionario.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == idCuestionario);

                if (cuestionario == null)
                    return Result<bool>.Failure("Cuestionario no encontrado");

                if (cuestionario.FechaFinalizado.HasValue)
                    return Result<bool>.Failure("El cuestionario ya está finalizado");

                if (!cuestionario.FechaRevisionAuditor.HasValue)
                    return Result<bool>.Failure(
                        "El cuestionario debe ser revisado por un auditor primero"
                    );

                if (!string.IsNullOrEmpty(cuestionario.TecnicoPaisId))
                    return Result<bool>.Failure(
                        "El cuestionario ya fue finalizado por un técnico país"
                    );

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al verificar si se puede finalizar el cuestionario {IdCuestionario} por {Role}",
                    idCuestionario,
                    role
                );
                return Result<bool>.Failure($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<bool> SaveResultadoSugerido(
            int idCuestionario,
            ApplicationUser appUser,
            string role
        )
        {
            var cuestionario = await _db.Cuestionario.FirstOrDefaultAsync(s =>
                s.Id == idCuestionario
            );
            var empresa = _db.Empresa.Find(cuestionario.IdEmpresa);

            try
            {
                var resultados = await GetCuestionario(idCuestionario, appUser, role);

                if (resultados.Modulos.Any(s => s.Resultados.ResultModulo == "No Certificado"))
                {
                    var noCertificado =
                        appUser.Lenguage == "es" ? "No Certificado" : "Not Certified";
                    empresa.ResultadoSugerido = noCertificado;
                    await _db.SaveChangesAsync();
                    return true;
                }

                var distintivos = _db.Distintivo.ToList();
                var listaDistintivosPorModulo = new List<Distintivo>();

                //no tener en cuenta el modulo bioseguridad
                foreach (var item in resultados.Modulos.Where(s => s.Id < 11))
                {
                    listaDistintivosPorModulo.Add(
                        distintivos.First(s =>
                            s.Name == item.Resultados.ResultModulo
                            || s.NameEnglish == item.Resultados.ResultModulo
                        )
                    );
                }

                empresa.ResultadoSugerido =
                    appUser.Lenguage == "es"
                        ? listaDistintivosPorModulo
                            .OrderByDescending(s => s.Importancia)
                            .First()
                            .Name
                        : listaDistintivosPorModulo
                            .OrderByDescending(s => s.Importancia)
                            .First()
                            .NameEnglish;
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                var a = e;
                return false;
            }
        }

        public async Task<bool> CalcularResultado(int idCuestionario)
        {
            //var Cuestionario = await _db.Cuestionario.Include("CuestionarioItem").FirstOrDefaultAsync(s => s.Id == idCuestionario);

            var cuestionario = await GetCuestionario(idCuestionario, null, "");

            foreach (var modulo in cuestionario.Modulos)
            {
                var totalOblig = modulo.Items.Count(s => s.Type == "pregunta" && s.Obligatoria);
                var obligatoriasCumplidas = modulo.Items.Count(s =>
                    s.Type == "pregunta" && s.Obligatoria && s.Result == 1
                );

                var totalComplement = modulo.Items.Count(s =>
                    s.Type == "pregunta" && !s.Obligatoria
                );
                var complementCumplidas = modulo.Items.Count(s =>
                    s.Type == "pregunta" && !s.Obligatoria && s.Result == 1
                );

                decimal porcentajeOblig = Math.Floor(
                    (decimal)(obligatoriasCumplidas * 100 / totalOblig)
                );
                decimal porcentajeComplement = Math.Floor(
                    (decimal)(complementCumplidas * 100 / totalComplement)
                );
            }
            return true;
        }

        /// <summary>
        /// Gets the list of certification statuses in the specified language
        /// </summary>
        /// <param name="lang">Language code ("es" for Spanish, "en" for English)</param>
        /// <returns>A list of certification statuses with their IDs and localized names</returns>
        /// <exception cref="ArgumentException">Thrown when language code is invalid</exception>
        public Task<List<CommonVm>> GetStatusList(string lang)
        {
            if (string.IsNullOrWhiteSpace(lang))
            {
                throw new ArgumentException("Language code cannot be null or empty", nameof(lang));
            }

            lang = lang.ToLowerInvariant();
            if (!IsValidLanguage(lang))
            {
                throw new ArgumentException($"Invalid language code: {lang}", nameof(lang));
            }

            var statuses = Enum.GetValues<CertificationStatus>()
                .Select(status => new CommonVm
                {
                    id = (int)status,
                    name = StatusLocalizations.GetDescription(status, lang),
                })
                .ToList();

            return Task.FromResult(statuses);
        }

        /// <summary>
        /// Retrieves a list of active distintivos (badges/distinctions) in the specified language
        /// </summary>
        /// <param name="lang">Language code ("es" for Spanish, "en" for English)</param>
        /// <returns>A list of distintivos with their IDs and localized names</returns>
        /// <exception cref="ArgumentException">Thrown when language code is invalid</exception>
        public async Task<List<CommonVm>> GetDistintivos(string lang)
        {
            // Validate language parameter
            if (string.IsNullOrWhiteSpace(lang))
            {
                throw new ArgumentException("Language code cannot be null or empty", nameof(lang));
            }

            // Normalize language code
            lang = lang.ToLowerInvariant();
            if (!IsValidLanguage(lang))
            {
                throw new ArgumentException($"Invalid language code: {lang}", nameof(lang));
            }

            try
            {
                var distintivos = await _db
                    .Distintivo.AsNoTracking()
                    .Where(s => s.Activo)
                    .OrderBy(s => s.Importancia)
                    .Select(x => new CommonVm
                    {
                        id = x.Id,
                        name = lang == LanguageCodes.Spanish ? x.Name : x.NameEnglish,
                    })
                    .ToListAsync();

                return distintivos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving distintivos for language {Lang}", lang);
                throw;
            }
        }

        private static bool IsValidLanguage(string lang) =>
            lang == LanguageCodes.Spanish || lang == LanguageCodes.English;

        /// <summary>
        /// Guarda o actualiza las observaciones para una respuesta específica
        /// </summary>
        /// <param name="user">Usuario que realiza la acción</param>
        /// <param name="data">Datos de la observación</param>
        /// <returns>True si la operación fue exitosa, False en caso contrario</returns>
        public async Task<Result<string>> SaveObservaciones(
            ApplicationUser user,
            ObservacionesDTO data
        )
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(data);

            try
            {
                if (string.IsNullOrWhiteSpace(data.Observaciones))
                {
                    throw new ArgumentException(
                        "Las observaciones no pueden estar vacías",
                        nameof(data)
                    );
                }
                var observacion = await _db.CuestionarioItemObservaciones.FirstOrDefaultAsync(s =>
                    s.CuestionarioItemId == data.IdRespuesta
                );

                if (observacion != null)
                {
                    observacion.Observaciones = data.Observaciones;
                }
                else
                {
                    observacion = new CuestionarioItemObservaciones
                    {
                        CuestionarioItemId = data.IdRespuesta,
                        Date = DateTime.UtcNow,
                        Observaciones = data.Observaciones,
                        UsuarioCargaId = user.Id,
                    };
                    await _db.CuestionarioItemObservaciones.AddAsync(observacion);
                }

                await _db.SaveChangesAsync();
                return Result<string>.Success(data.Observaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al guardar observaciones para respuesta {IdRespuesta}",
                    data.IdRespuesta
                );
                return Result<string>.Failure(ex.Message);
            }
        }

        public async Task<ObservacionesDTO> GetObservaciones(int idRespuesta)
        {
            return await _db
                .CuestionarioItemObservaciones.AsNoTracking()
                .Where(s => s.CuestionarioItemId == idRespuesta)
                .Select(o => new ObservacionesDTO
                {
                    IdRespuesta = idRespuesta,
                    Observaciones = o.Observaciones,
                })
                .FirstOrDefaultAsync();
        }

        public async Task<RegistroHallazgos> ReporteHallazgos(
            int CuestionarioId,
            ApplicationUser user,
            string role
        )
        {
            var cuestionario = await _db.Cuestionario.FirstOrDefaultAsync(s =>
                s.Id == CuestionarioId
            );
            var empresa = await _db.Empresa.FindAsync(cuestionario.IdEmpresa);
            var auditorDB = await _db.Users.FindAsync(cuestionario.AuditorId);
            ApplicationUser appUser = (ApplicationUser)auditorDB;

            var result = new RegistroHallazgos
            {
                Empresa = empresa.Nombre,
                Generador = appUser.FirstName + " " + appUser.LastName,
                HallazgosItems = new List<HallazgosDTO>(),
            };

            var cuestionariosItems = _db
                .CuestionarioItem.Include(s => s.CuestionarioItemObservaciones)
                .Where(s => s.CuestionarioId == CuestionarioId && s.Resultado == -1);

            foreach (var item in cuestionariosItems)
            {
                var referencia = item.Nomenclatura;
                var orden = new Version("1.1");
                if (item.Nomenclatura.Contains("mb"))
                {
                    orden = new Version(item.Nomenclatura.Replace("mb-", ""));
                }
                else
                {
                    orden = new Version(item.Nomenclatura);
                }

                var hallazgo = new HallazgosDTO
                {
                    Descripcion = item.CuestionarioItemObservaciones.Any()
                        ? item.CuestionarioItemObservaciones.First().Observaciones
                        : "",
                    Obligatorio = item.Obligatorio ? "Si" : "No",
                    Referencia = item.Nomenclatura,
                    ReferenciaOrden = orden,
                    //Modulo = item.Nomenclatura.Contains("mb")? "BIO": (orden.Major - 3).ToString(),
                    Modulo = item.Nomenclatura.Contains("mb")
                        ? "BIO"
                        : (orden.Major - 3).ToString(),
                };
                result.HallazgosItems.Add(hallazgo);
            }

            if (result.HallazgosItems.Any())
            {
                result.HallazgosItems = result
                    .HallazgosItems.OrderBy(s => s.ReferenciaOrden)
                    .ToList();
            }

            return result;
        }

        public async Task<List<ObservacionesDTO>> GetListObservaciones(IEnumerable<int> ItemIds)
        {
            var items = await _db
                .CuestionarioItemObservaciones.AsNoTracking()
                .Where(s => ItemIds.Contains(s.CuestionarioItemId))
                .Select(x => new ObservacionesDTO
                {
                    Observaciones = x.Observaciones,
                    IdRespuesta = x.CuestionarioItemId,
                })
                .ToListAsync();

            return items;
        }

        public async Task<Result<bool>> ConvertirARecertificacionAsync(
            ApplicationUser user,
            EmpresaVm data
        )
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                if (data == null)
                    throw new ArgumentNullException(nameof(data));

                if (!int.TryParse(data.Certificacion, out int certificacionId))
                    return Result<bool>.Failure("ID de certificación inválido");

                var proceso = await _db.ProcesoCertificacion.FirstOrDefaultAsync(s =>
                    s.Id == certificacionId && s.EmpresaId == data.Id
                );

                if (proceso == null)
                    return Result<bool>.Failure(
                        $"No se encontró el proceso de certificación {certificacionId} para la empresa {data.Id}"
                    );

                // Crear una estrategia de ejecución para manejar reintentos
                var strategy = _db.Database.CreateExecutionStrategy();

                return await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _db.Database.BeginTransactionAsync();
                    try
                    {
                        proceso.Recertificacion = true;

                        await _db.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation(
                            "Proceso {ProcesoId} convertido a recertificación por usuario {UserId}",
                            certificacionId,
                            user.Id
                        );

                        return Result<bool>.Success(true);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(
                            ex,
                            "Error al convertir proceso {ProcesoId} a recertificación",
                            certificacionId
                        );
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al convertir proceso a recertificación para empresa {EmpresaId}",
                    data.Id
                );
                return Result<bool>.Failure($"Error al convertir a recertificación: {ex.Message}");
            }
        }

        public async Task<Result<bool>> ReAbrirCuestionario(
            ApplicationUser user,
            int cuestionarioId
        )
        {
            return await _cuestionarioService.EjecutarReapertura(cuestionarioId, user);
        }
    }
}
