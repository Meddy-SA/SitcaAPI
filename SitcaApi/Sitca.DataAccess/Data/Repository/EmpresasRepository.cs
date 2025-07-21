using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.DataAccess.Middlewares;
using Sitca.DataAccess.Services.CompanyQuery;
using Sitca.DataAccess.Services.Notification;
using Sitca.DataAccess.Services.ProcessQuery;
using Sitca.Models;
using Sitca.Models.Constants;
using Sitca.Models.DTOs;
using Sitca.Models.Enums;
using Sitca.Models.Mappers;
using Sitca.Models.ViewModels;
using static Utilities.Common.Constants;
using Localization = Utilities.Common.LocalizationUtilities;
using Rol = Utilities.Common.Constants.Roles;

namespace Sitca.DataAccess.Data.Repository;

public class EmpresasRepository : Repository<Empresa>, IEmpresasRepository
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notificationService;
    private readonly ICompanyQueryBuilder _queryBuilder;
    private readonly ILogger<EmpresasRepository> _logger;

    public EmpresasRepository(
        ApplicationDbContext db,
        INotificationService notificationService,
        ICompanyQueryBuilder queryBuilder,
        ILogger<EmpresasRepository> logger
    )
        : base(db)
    {
        _db = db;
        _notificationService = notificationService;
        _queryBuilder = queryBuilder;
        _logger = logger;
    }

    private static bool IsValidLanguage(string lang) =>
        lang == LanguageCodes.Spanish || lang == LanguageCodes.English;

    /// <summary>
    /// Obtiene todos los metadatos necesarios para la gestión de empresas
    /// </summary>
    /// <param name="language">Código de idioma ("es" para español, "en" para inglés)</param>
    /// <returns>Objeto que contiene listas de países, tipologías, distintivos y estados</returns>
    public async Task<Result<MetadatosDTO>> GetMetadataAsync(string language)
    {
        try
        {
            // Validar y normalizar el código de idioma
            if (string.IsNullOrWhiteSpace(language))
            {
                language = "es"; // Establecer español como idioma por defecto
            }
            else
            {
                language = language.ToLowerInvariant();
                if (!IsValidLanguage(language))
                {
                    return Result<MetadatosDTO>.Failure($"Código de idioma no válido: {language}");
                }
            }

            // Crear el objeto de respuesta
            var metadata = new MetadatosDTO();

            // Obtener la lista de países activos
            metadata.Paises = await _db
                .Pais.AsNoTracking()
                .Where(p => p.Active)
                .OrderBy(p => p.Name)
                .Select(p => new CommonVm
                {
                    id = p.Id,
                    name = p.Name, // Los países no tienen traducción en el modelo
                    isSelected = false,
                })
                .ToListAsync();

            // Obtener la lista de tipologías activas
            metadata.Tipologias = await _db
                .Tipologia.AsNoTracking()
                .Where(t => t.Active)
                .OrderBy(t => language == "es" ? t.Name : t.NameEnglish)
                .Select(t => new CommonVm
                {
                    id = t.Id,
                    name = language == "es" ? t.Name : t.NameEnglish,
                    isSelected = false,
                })
                .ToListAsync();

            // Obtener la lista de distintivos activos
            metadata.Distintivos = await _db
                .Distintivo.AsNoTracking()
                .Where(d => d.Activo)
                .OrderBy(d => d.Importancia)
                .Select(d => new CommonVm
                {
                    id = d.Id,
                    name = language == "es" ? d.Name : d.NameEnglish,
                    isSelected = false,
                })
                .ToListAsync();

            // Obtener la lista de estados de certificación
            metadata.Estados = Enum.GetValues<CertificationStatus>()
                .Select(status => new CommonVm
                {
                    id = (int)status,
                    name = StatusLocalizations.GetDescription(status, language),
                    isSelected = false,
                })
                .ToList();

            return Result<MetadatosDTO>.Success(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener metadatos para el idioma {Language}", language);
            return Result<MetadatosDTO>.Failure(
                $"Error interno al recuperar los metadatos: {ex.Message}"
            );
        }
    }

    /// <summary>
    /// Actualiza los datos básicos de una empresa aplicando validaciones de seguridad según el rol del usuario.
    /// </summary>
    /// <param name="datosEmpresa">DTO con los datos de la empresa a actualizar</param>
    /// <param name="user">Usuario que realiza la operación</param>
    /// <param name="role">Rol del usuario</param>
    /// <returns>Resultado de la operación con detalles de éxito o error</returns>
    public async Task<Result<bool>> ActualizarDatosEmpresaAsync(
        EmpresaBasicaDTO datosEmpresa,
        ApplicationUser user,
        string role
    )
    {
        try
        {
            // Validaciones de entrada
            if (datosEmpresa == null)
                return Result<bool>.Failure("Los datos de empresa no pueden ser nulos");

            if (string.IsNullOrEmpty(datosEmpresa.Nombre))
                return Result<bool>.Failure("El nombre de la empresa es obligatorio");

            if (string.IsNullOrEmpty(datosEmpresa.NombreRepresentante))
                return Result<bool>.Failure("El nombre del representante es obligatorio");

            // Determinar el ID de empresa según el rol
            var empresaId =
                role == Rol.Empresa ? user.EmpresaId ?? datosEmpresa.Id : datosEmpresa.Id;

            // Buscar la empresa en base de datos
            var empresa = await _db
                .Empresa.Include(e => e.Tipologias)
                .FirstOrDefaultAsync(e => e.Id == empresaId);

            if (empresa == null)
                return Result<bool>.Failure($"No se encontró la empresa con ID {empresaId}");

            // Crear una estrategia de ejecución para manejar reintentos en caso de fallos de conexión
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // Actualizar datos básicos
                    ActualizarDatosBasicos(empresa, datosEmpresa);

                    // Actualizar país si se cumplen las condiciones
                    if (ActualizarPais(empresa, datosEmpresa.Pais, role))
                    {
                        _logger.LogInformation(
                            "Actualizado país de empresa {EmpresaId} a {PaisId}",
                            empresa.Id,
                            datosEmpresa.Pais.Id
                        );
                    }

                    // Actualizar tipologías si es necesario
                    await ActualizarTipologiasAsync(empresa, datosEmpresa.Tipologias);
                    
                    // Actualizar tipología en el proceso de certificación si se cumplen las condiciones
                    if (datosEmpresa.Tipologias != null && datosEmpresa.Tipologias.Length > 0)
                    {
                        var tipologiaId = datosEmpresa.Tipologias.FirstOrDefault()?.Id;
                        if (tipologiaId.HasValue && tipologiaId.Value > 0)
                        {
                            await ActualizarTipologiaProcesoCertificacionAsync(empresa.Id, tipologiaId.Value, role);
                        }
                    }

                    // Guardar cambios
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation(
                        "Empresa {EmpresaId} actualizada exitosamente por {UserId}",
                        empresa.Id,
                        user.Id
                    );

                    return Result<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al actualizar empresa {EmpresaId}", empresa.Id);
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error no controlado al actualizar empresa {EmpresaId}",
                datosEmpresa.Id
            );
            return Result<bool>.Failure($"Error al actualizar la empresa: {ex.Message}");
        }
    }

    public async Task<Result<List<ProcesoArchivoDTO>>> GetFilesByCompanyAsync(int empresaId)
    {
        try
        {
            var archivos = await _db
                .Archivo.AsNoTracking()
                .Where(a => a.EmpresaId == empresaId && a.Activo)
                .Include(a => a.UsuarioCarga)
                .ToListAsync();

            var archivosDto = archivos.Select(a => a.ToDto()).ToList();
            return Result<List<ProcesoArchivoDTO>>.Success(archivosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error no controlado al obtener archivos de la empresa {EmpresaId}",
                empresaId
            );
            return Result<List<ProcesoArchivoDTO>>.Failure(
                $"Error al obtener archivos de la empresa: {ex.Message}"
            );
        }
    }

    /// <summary>
    /// Obtiene un bloque de procesos de certificación filtrado según los criterios especificados.
    /// </summary>
    /// <param name="filtro">Objeto que contiene los criterios de filtrado.</param>
    /// <param name="blockNumber">Número del bloque a recuperar.</param>
    /// <param name="blockSize">Tamaño del bloque.</param>
    /// <returns>Bloque de procesos de certificación que cumplen con los criterios de filtrado.</returns>
    /// <exception cref="ArgumentNullException">Se lanza cuando el filtro es null.</exception>
    /// <exception cref="DatabaseException">Se lanza cuando ocurre un error en la base de datos.</exception>
    public async Task<BlockResult<ProcesoCertificacionVm>> GetProcesosCompaniesBlockAsync(
        FilterCompanyDTO filtro
    )
    {
        try
        {
            // 1. Validar parámetros
            ArgumentNullException.ThrowIfNull(filtro, nameof(filtro));

            // 2. Construir la consulta base de procesos de certificación
            var query = _db.ProcesoCertificacion.AsNoTracking();

            // 3. Aplicar filtros según la empresa asociada
            query = ApplyEmpresaFilters(query, filtro);

            // 4. Aplicar filtros específicos de proceso si es necesario
            // Por ejemplo: query = ApplyProcesoFilters(query, otrosFiltros);

            // 5. Aplicar proyección y ejecutar la consulta con paginación por bloques
            return await BuildAndExecuteBlockProjection(
                query,
                filtro.Lang ?? "es",
                filtro.BlockNumber,
                filtro.BlockSize
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error obteniendo bloque de procesos con filtro {@Filter}, blockNumber: {BlockNumber}, blockSize: {BlockSize}",
                filtro,
                filtro.BlockNumber,
                filtro.BlockSize
            );

            throw new DatabaseException(
                "Error al obtener el bloque de procesos de certificación",
                ex
            );
        }
    }

    /// <summary>
    /// Aplica filtros relacionados con la empresa a la consulta de procesos de certificación.
    /// </summary>
    /// <param name="query">Consulta base a la que aplicar los filtros.</param>
    /// <param name="filtro">Criterios de filtrado.</param>
    /// <returns>Consulta con los filtros aplicados.</returns>
    private IQueryable<ProcesoCertificacion> ApplyEmpresaFilters(
        IQueryable<ProcesoCertificacion> query,
        FilterCompanyDTO filtro
    )
    {
        // Convertir el filtro de homologación
        var homologacion = filtro.Homologacion switch
        {
            "-1" => true,
            "1" => false,
            _ => null as bool?,
        };

        // Aplicar filtros relacionados con la empresa
        if (filtro.Country > 0)
        {
            query = query.Where(p => p.Empresa.PaisId == filtro.Country);
        }

        if (filtro.Estado != -1)
        {
            string statusPrefix = filtro.Estado.Value.ToString() + " - ";
            query = query.Where(p => p.Status.StartsWith(statusPrefix));
        }

        if (homologacion.HasValue)
        {
            query = query.Where(p => p.Empresa.EsHomologacion == homologacion.Value);
        }

        if (filtro.Tipologia > 0)
        {
            query = query.Where(p =>
                p.Empresa.Tipologias.Any(t => t.IdTipologia == filtro.Tipologia)
                || p.TipologiaId == filtro.Tipologia
            );
        }

        if (filtro.Activo.HasValue)
        {
            query = query.Where(p => p.Empresa.Active == filtro.Activo.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Name))
        {
            query = query.Where(p => p.Empresa.Nombre.ToLower().Contains(filtro.Name.ToLower()));
        }

        return query;
    }

    /// <summary>
    /// Construye y ejecuta la proyección por bloques para obtener procesos de certificación paginados.
    /// </summary>
    /// <param name="query">Consulta base a la que aplicar la proyección.</param>
    /// <param name="language">Idioma para la localización de los resultados.</param>
    /// <param name="blockNumber">Número del bloque a recuperar.</param>
    /// <param name="blockSize">Tamaño del bloque.</param>
    /// <returns>Resultado por bloques con los procesos proyectados.</returns>
    private async Task<BlockResult<ProcesoCertificacionVm>> BuildAndExecuteBlockProjection(
        IQueryable<ProcesoCertificacion> query,
        string language,
        int blockNumber,
        int blockSize
    )
    {
        // 0. Ordenar la consulta original antes de cualquier operación
        query = query.OrderBy(p => p.Id);

        // 1. Incluir todas las relaciones necesarias
        query = query
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

        // 2. Preparar la proyección pero sin ejecutarla aún
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
            // También guardamos las tipologías de la empresa
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
        });

        // 3. Contar la cantidad de procesos por estado
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

        // 4. Aplicar paginación por bloques a la proyección
        var blockData = await projection.ToBlockResultAsync(blockNumber, blockSize);

        // 5. Mapear los resultados a nuestro modelo de vista
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
                };
            })
            .ToList();

        // 6. Crear el resultado por bloques con los items mapeados
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

    /// <summary>
    /// Actualiza los datos básicos de la empresa
    /// </summary>
    private void ActualizarDatosBasicos(Empresa empresa, EmpresaBasicaDTO datos)
    {
        empresa.Nombre = datos.Nombre;
        empresa.NombreRepresentante = datos.NombreRepresentante;
        empresa.CargoRepresentante = datos.CargoRepresentante ?? empresa.CargoRepresentante;
        empresa.IdNacional = datos.IdNacional ?? empresa.IdNacional;
        empresa.Direccion = datos.Direccion ?? empresa.Direccion;
        empresa.Ciudad = datos.Ciudad ?? empresa.Ciudad;
        empresa.Telefono = datos.Telefono;
        empresa.WebSite = datos.WebSite ?? empresa.WebSite;
        empresa.Email = datos.Email;
    }

    /// <summary>
    /// Actualiza el país de la empresa si se cumplen las condiciones
    /// </summary>
    /// <returns>True si se actualizó el país, False en caso contrario</returns>
    private bool ActualizarPais(Empresa empresa, PaisDTO paisDTO, string role)
    {
        // Solo empresas pueden actualizar su país y solo si su estado es menor a 2
        if (paisDTO != null && role == Rol.Empresa && empresa.Estado < 2)
        {
            empresa.PaisId = paisDTO.Id;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Actualiza las tipologías de la empresa
    /// </summary>
    /// <param name="empresa">Entidad empresa a actualizar</param>
    /// <param name="tipologiasDTO">Array de tipologías desde el DTO</param>
    private async Task ActualizarTipologiasAsync(Empresa empresa, TipologiaDTO[] tipologiasDTO)
    {
        // Limpiar tipologías existentes
        empresa.Tipologias.Clear();
        
        // Si no hay nuevas tipologías, solo limpiar
        if (tipologiasDTO == null || tipologiasDTO.Length == 0)
            return;

        // Agregar nuevas tipologías seleccionadas
        foreach (var tipologia in tipologiasDTO)
        {
            empresa.Tipologias.Add(new TipologiasEmpresa
            {
                IdEmpresa = empresa.Id,
                IdTipologia = tipologia.Id
            });
        }
    }

    /// <summary>
    /// Actualiza la tipología en el proceso de certificación activo si se cumplen las condiciones
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="tipologiaId">ID de la tipología a asignar</param>
    /// <param name="role">Rol del usuario</param>
    private async Task ActualizarTipologiaProcesoCertificacionAsync(int empresaId, int tipologiaId, string role)
    {
        // Verificar que el rol sea TecnicoPais o Admin
        if (role != Rol.TecnicoPais && role != Rol.Admin)
        {
            _logger.LogInformation(
                "Rol {Role} no autorizado para actualizar tipología en proceso de certificación",
                role
            );
            return;
        }

        // Buscar el proceso de certificación activo más reciente de la empresa
        var procesoCertificacion = await _db.ProcesoCertificacion
            .Where(p => p.EmpresaId == empresaId)
            .OrderByDescending(p => p.FechaInicio)
            .FirstOrDefaultAsync();

        if (procesoCertificacion == null)
        {
            _logger.LogInformation(
                "No se encontró proceso de certificación para la empresa {EmpresaId}",
                empresaId
            );
            return;
        }

        // Verificar que el estado sea "0 - Inicial" o "1 - Para Asesorar"
        bool estadoPermitido = procesoCertificacion.Status.StartsWith("0 - ") || 
                              procesoCertificacion.Status.StartsWith("1 - ");

        if (!estadoPermitido)
        {
            _logger.LogInformation(
                "Estado del proceso {ProcesoId} no permite actualización de tipología. Estado actual: {Status}",
                procesoCertificacion.Id,
                procesoCertificacion.Status
            );
            return;
        }

        // Actualizar la tipología
        procesoCertificacion.TipologiaId = tipologiaId;
        
        _logger.LogInformation(
            "Actualizada tipología del proceso {ProcesoId} a {TipologiaId} por usuario con rol {Role}",
            procesoCertificacion.Id,
            tipologiaId,
            role
        );
    }
}
