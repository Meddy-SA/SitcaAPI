using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.DataAccess.Services.CompanyQuery;
using Sitca.DataAccess.Services.Notification;
using Sitca.Models;
using Sitca.Models.Constants;
using Sitca.Models.DTOs;
using Sitca.Models.Enums;
using Sitca.Models.Mappers;
using Sitca.Models.ViewModels;
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
                    await ActualizarTipologiasAsync(empresa);

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
    private async Task ActualizarTipologiasAsync(Empresa empresa)
    {
        if (empresa.Tipologias == null || !empresa.Tipologias.Any())
            return;

        // Limpiar tipologías existentes
        empresa.Tipologias.Clear();
        await _db.SaveChangesAsync();

        // Agregar nuevas tipologías seleccionadas
        var nuevasTipologias = empresa
            .Tipologias.Select(t => new TipologiasEmpresa
            {
                IdEmpresa = empresa.Id,
                IdTipologia = t.IdTipologia,
            })
            .ToList();

        empresa.Tipologias = nuevasTipologias;
    }
}
