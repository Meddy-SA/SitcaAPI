using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.Constants;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Services.EmpresaDeletion;
using Sitca.DataAccess.Services.Notification;
using Sitca.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;
using Utilities.Common;

namespace Sitca.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmpresasController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IConfiguration _config;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<EmpresasController> _logger;
    private readonly IEmpresaDeletionService _empresaDeletionService;

    public EmpresasController(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IConfiguration config,
        UserManager<ApplicationUser> userManager,
        ILogger<EmpresasController> logger,
        IEmpresaDeletionService empresaDeletionService
    )
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _config = config;
        _userManager = userManager;
        _logger = logger;
        _empresaDeletionService = empresaDeletionService;
    }

    /// <summary>
    /// Obtiene la lista de empresas (por país si se especifica)
    /// </summary>
    [Authorize]
    [HttpPost("procesos")]
    [ProducesResponseType(
        StatusCodes.Status200OK,
        Type = typeof(Result<BlockResult<ProcesoCertificacionVm>>)
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<BlockResult<ProcesoCertificacionVm>>>> GetProcessesList(
        CompanyFilterDTO filter
    )
    {
        try
        {
            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            // Usar el valor predeterminado si es null
            filter ??= new CompanyFilterDTO();

            // Validar y ajustar los parámetros de paginación por bloques
            if (filter.BlockSize <= 0)
                filter.BlockSize = 100;
            if (filter.BlockNumber <= 0)
                filter.BlockNumber = 1;

            // Limitar el tamaño máximo de bloque a un valor razonable
            filter.BlockSize = Math.Min(filter.BlockSize, 500);

            var processes = await _unitOfWork.Proceso.GetProcessesBlockAsync(
                appUser,
                role,
                filter,
                appUser.Lenguage
            );

            return this.HandleResponse(
                Result<BlockResult<ProcesoCertificacionVm>>.Success(processes)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener lista de procesos de certificación con filtro {@Filter}",
                filter
            );
            return StatusCode(
                500,
                Result<List<ProcesoCertificacionVm>>.Failure("Error interno del servidor")
            );
        }
    }

    [Authorize]
    [HttpPost("procesos/listado")]
    [ProducesResponseType(
        StatusCodes.Status200OK,
        Type = typeof(Result<BlockResult<ProcesoCertificacionVm>>)
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<
        ActionResult<Result<BlockResult<ProcesoCertificacionVm>>>
    > GetProcessesCompanies(FilterCompanyDTO filtro)
    {
        try
        {
            // 1. Validar usuario actual
            var currentUser = await this.GetCurrentUserAsync(_userManager);
            if (currentUser == null)
            {
                return Unauthorized(
                    Result<BlockResult<ProcesoCertificacionVm>>.Failure("Usuario no autorizado")
                );
            }

            // 2. Validar y preparar el filtro
            filtro ??= new FilterCompanyDTO { Lang = currentUser.Lenguage };

            // 3. Aplicar restricciones según rol
            if (!User.IsInRole(Constants.Roles.Admin))
            {
                filtro = filtro.WithCountry(currentUser.PaisId ?? 0);
            }

            // 4. Validar filtro
            if (!filtro.IsValid())
            {
                return BadRequest(
                    Result<BlockResult<ProcesoCertificacionVm>>.Failure(
                        "Criterios de filtrado inválidos"
                    )
                );
            }

            // 5. Asegurar que el idioma está configurado
            if (string.IsNullOrWhiteSpace(filtro.Lang))
            {
                filtro = filtro.WithLanguage(currentUser.Lenguage);
            }

            // 6. Validar y ajustar los parámetros de paginación por bloques
            filtro = filtro.WithBlockSize(
                filtro.BlockSize <= 0 ? 100 : Math.Min(filtro.BlockSize, 500)
            );
            filtro = filtro.WithBlockNumber(filtro.BlockNumber <= 0 ? 1 : filtro.BlockNumber);

            var processes = await _unitOfWork.Empresas.GetProcesosCompaniesBlockAsync(filtro);

            return this.HandleResponse(
                Result<BlockResult<ProcesoCertificacionVm>>.Success(processes)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener lista de procesos de certificación con filtro {@Filter}",
                filtro
            );
            return StatusCode(
                500,
                Result<List<ProcesoCertificacionVm>>.Failure("Error interno del servidor")
            );
        }
    }

    // <summary>
    /// Obtiene una empresa específica por su ID
    /// </summary>
    /// NOTE: Editar al modelo de Mappers.
    [Authorize(Roles = AuthorizationPolicies.Empresa.Details)]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<EmpresaUpdateVm>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<EmpresaUpdateVm>>> GetById(int id)
    {
        try
        {
            const int toStatus = 7;
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var empresa = await _unitOfWork.Empresa.Data(id, appUser);
            if (empresa == null)
                return NotFound(
                    Result<EmpresaUpdateVm>.Failure($"No se encontró la empresa con ID {id}")
                );

            // Si el usuario es CTC y la empresa está en un estado anterior, actualizar el estado
            if (
                User.IsInRole(Constants.Roles.CTC)
                && empresa.CertificacionActual != null
                && empresa.Estado < toStatus
            )
            {
                var status = new CertificacionStatusVm
                {
                    CertificacionId = empresa.CertificacionActual.Id,
                    Status = StatusConstants.GetLocalizedStatus(toStatus, "es"),
                };
                await _unitOfWork.ProcesoCertificacion.ChangeStatus(status, toStatus);

                empresa.Estado = toStatus;
                empresa.CertificacionActual.Status = StatusConstants.GetLocalizedStatus(
                    toStatus,
                    appUser.Lenguage
                );
            }

            return this.HandleResponse(Result<EmpresaUpdateVm>.Success(empresa));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalles de empresa: {EmpresaId}", id);
            return StatusCode(500, Result<EmpresaUpdateVm>.Failure("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Obtiene los metadatos para filtrar por listado de empresas.
    /// </summary>
    [Authorize]
    [HttpGet("metadata")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<MetadatosDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<MetadatosDTO>>> GetMetadatos()
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _unitOfWork.Empresas.GetMetadataAsync(appUser.Lenguage);
            return this.HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener metadatos de empresas");
            return StatusCode(500, Result<MetadatosDTO>.Failure("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Obtiene el último proceso de certificación de la empresa del usuario actual
    /// </summary>
    [Authorize(Roles = AuthorizationPolicies.Empresa.View)]
    [HttpGet("mi-empresa/ultimo-proceso")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<ProcesoCertificacionDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<ProcesoCertificacionDTO>>> GetUltimoProceso()
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            if (!appUser.EmpresaId.HasValue)
                return NotFound(
                    Result<EmpresaUpdateVm>.Failure("Usuario no asociado a ninguna empresa")
                );

            var resultado = await _unitOfWork.Proceso.GetUltimoProcesoByEmpresaIdAsync(
                appUser.EmpresaId.Value,
                appUser.Id
            );

            if (!resultado.IsSuccess)
                return NotFound(Result<ProcesoCertificacionDTO>.Failure(resultado.Error));

            return this.HandleResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener último proceso de la empresa del usuario");
            return StatusCode(500, Result<EmpresaUpdateVm>.Failure("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Obtiene el estado de la empresa del usuario actual
    /// </summary>
    [Authorize(Roles = AuthorizationPolicies.Empresa.View)]
    [HttpGet("mi-empresa/estado")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<int>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<int>>> GetEstadoMiEmpresa()
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            if (!appUser.EmpresaId.HasValue)
                return Ok(Result<int>.Success(0));

            var estado = await _unitOfWork.Empresa.GetCompanyStatusAsync(appUser.EmpresaId.Value);
            return Ok(Result<int>.Success(estado));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estado de mi empresa");
            return StatusCode(500, Result<int>.Failure("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Actualiza los datos de una empresa
    /// </summary>
    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<bool>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<bool>>> Update(int id, [FromBody] EmpresaBasicaDTO datos)
    {
        try
        {
            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            // Verificar que el ID en la ruta coincide con el ID en los datos
            if (datos.Id != id)
            {
                return BadRequest(
                    Result<bool>.Failure("El ID en la ruta no coincide con el ID en los datos")
                );
            }

            // Verificar que el usuario tiene acceso a esta empresa
            if (role == Constants.Roles.Empresa && appUser.EmpresaId != id)
            {
                return Forbid();
            }

            var res = await _unitOfWork.Empresas.ActualizarDatosEmpresaAsync(datos, appUser, role);

            return this.HandleResponse(res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar empresa: {EmpresaId}", id);
            return StatusCode(500, Result<bool>.Failure("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Obtiene información sobre las dependencias de una empresa antes de eliminarla
    /// </summary>
    [Authorize(Roles = AuthorizationPolicies.Empresa.AdminTecnico)]
    [HttpGet("{id}/deletion-info")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<EmpresaDeletionInfo>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<EmpresaDeletionInfo>>> GetDeletionInfo(int id)
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role))
                return BadRequest(
                    Result<EmpresaDeletionInfo>.Failure("No se encontró el rol del usuario")
                );

            var result = await _empresaDeletionService.CanDeleteEmpresaAsync(
                id,
                appUser.PaisId.GetValueOrDefault(),
                role
            );

            return this.HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener información de eliminación para empresa: {EmpresaId}",
                id
            );
            return StatusCode(
                500,
                Result<EmpresaDeletionInfo>.Failure("Error interno del servidor")
            );
        }
    }

    /// <summary>
    /// Elimina una empresa y todas sus entidades relacionadas
    /// </summary>
    [Authorize(Roles = AuthorizationPolicies.Empresa.AdminTecnico)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<EmpresaDeletionResult>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<EmpresaDeletionResult>>> Delete(int id)
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role))
                return BadRequest(
                    Result<EmpresaDeletionResult>.Failure("No se encontró el rol del usuario")
                );

            var result = await _empresaDeletionService.DeleteEmpresaWithRelatedEntitiesAsync(
                id,
                appUser.PaisId.GetValueOrDefault(),
                role
            );

            return this.HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar empresa: {EmpresaId}", id);
            return StatusCode(
                500,
                Result<EmpresaDeletionResult>.Failure("Error interno del servidor")
            );
        }
    }

    /// <summary>
    /// Busca empresas con filtros específicos
    /// </summary>
    [Authorize(Roles = AuthorizationPolicies.Empresa.ListCompany)]
    [HttpPost("buscar")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<List<EmpresaVm>>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<List<EmpresaVm>>>> Search(
        [FromBody] CompanyFilterDTO filter
    )
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            // Si no es admin, solo puede ver empresas de su país
            if (!User.IsInRole("Admin"))
            {
                filter.CountryId = appUser.PaisId ?? 0;
            }

            var companies = await _unitOfWork.Empresa.GetCompanyListAsync(filter, appUser.Lenguage);
            return Ok(Result<List<EmpresaVm>>.Success(companies));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar empresas con filtro {@Filter}", filter);
            return StatusCode(500, Result<List<EmpresaVm>>.Failure("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Busca empresas según el rol del usuario
    /// </summary>
    [Authorize]
    [HttpPost("buscar-por-rol")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<List<EmpresaVm>>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<List<EmpresaVm>>>> SearchByRole(
        [FromBody] CompanyFilterDTO filter
    )
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role))
                return BadRequest(
                    Result<List<EmpresaVm>>.Failure("No se encontró el rol del usuario")
                );

            var empresas = await _unitOfWork.Empresa.ListForRoleAsync(appUser, role, filter);
            return Ok(Result<List<EmpresaVm>>.Success(empresas));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argumento inválido al buscar empresas por rol");
            return BadRequest(Result<List<EmpresaVm>>.Failure(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar empresas por rol");
            return StatusCode(500, Result<List<EmpresaVm>>.Failure("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Obtiene todos los archivos relacionados a una empresa.
    /// </summary>
    [Authorize]
    [HttpGet("archivos/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<List<ProcesoArchivoDTO>>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<List<ProcesoArchivoDTO>>>> GetFilesForCompany(int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest(
                    Result<List<ProcesoArchivoDTO>>.Failure(
                        "El ID de la empresa debe ser mayor que cero"
                    )
                );

            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var archivos = await _unitOfWork.Empresas.GetFilesByCompanyAsync(id);
            return this.HandleResponse(archivos);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argumento inválido al buscar empresas por rol");
            return BadRequest(Result<List<ProcesoArchivoDTO>>.Failure(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener archivos de la empresa {EmpresaId}", id);
            return StatusCode(
                500,
                Result<List<ProcesoArchivoDTO>>.Failure("Error interno del servidor")
            );
        }
    }
}
