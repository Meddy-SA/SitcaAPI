using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Services.Notification;
using Sitca.DataAccess.Services.ProcesosDeletion;
using Sitca.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;
using Utilities.Common;
using Policies = Utilities.Common.AuthorizationPolicies.Proceso;

namespace Sitca.Controllers;

[Route("api/procesos")]
[ApiController]
public class ProcesoCertificacionController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IProcesosDeletionService _procesosDeletionService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ProcesoCertificacionController> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ProcesoCertificacionController(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IProcesosDeletionService procesosDeletionService,
        UserManager<ApplicationUser> userManager,
        ILogger<ProcesoCertificacionController> logger,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _procesosDeletionService = procesosDeletionService;
        _userManager = userManager;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// Obtiene un proceso de certificación por su ID
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<ProcesoCertificacionDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<ProcesoCertificacionDTO>>> GetById(int id)
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var res = await _unitOfWork.Proceso.GetProcesoForIdAsync(id, appUser.Id);
            return this.HandleResponse(res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el proceso de certificación: {ProcesoId}", id);
            return StatusCode(
                500,
                Result<ProcesoCertificacionDTO>.Failure("Error interno del servidor")
            );
        }
    }

    /// <summary>
    /// Actualizar el numero de expediente segun el id del proceso de certificación
    /// </summary>
    [Authorize(Roles = Policies.UpdateCaseNumber)]
    [HttpPut("expediente/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<ExpedienteDTO>>> UpdateExpediente(
        int id,
        [FromBody] ExpedienteDTO expediente
    )
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized(Result<bool>.Failure("Usuario no autorizado"));

            if (expediente == null)
                return BadRequest(Result<bool>.Failure("Datos no válidos"));

            expediente.Id = id;

            var result = await _unitOfWork.Proceso.UpdateCaseNumberAsync(expediente, appUser.Id);

            return this.HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error no controlado al actualizar número de expediente para certificación {CertificacionId}",
                id
            );
            return StatusCode(
                500,
                Result<bool>.Failure("Error interno del servidor al procesar la solicitud")
            );
        }
    }

    /// <summary>
    /// Guarda la calificación de un proceso de certificación
    /// </summary>
    [Authorize(Roles = Policies.SaveCalification)]
    [HttpPost("calificacion")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<bool>>> SaveCalificacion(
        [FromBody] SaveCalificacionVm data
    )
    {
        try
        {
            if (data == null || data.idProceso <= 0)
                return BadRequest(Result<bool>.Failure("Datos de calificación no válidos"));

            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
            if (appUser == null)
                return Unauthorized(Result<bool>.Failure("Usuario no autorizado"));

            var result = await _unitOfWork.ProcesoCertificacion.SaveCalificacion(
                data,
                appUser,
                role
            );

            if (!result.IsSuccess)
                return this.HandleResponse(result);

            // Enviar notificación solo si la operación fue exitosa
            try
            {
                await _notificationService.SendNotification(data.idProceso, null, appUser.Lenguage);
            }
            catch (Exception ex)
            {
                // Logueamos el error pero no interrumpimos el flujo ya que la operación principal fue exitosa
                _logger.LogWarning(
                    ex,
                    "Error al enviar la notificación para el proceso {ProcesoId}",
                    data.idProceso
                );
            }

            return this.HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error no controlado al guardar calificación para el proceso {ProcesoId}",
                data.idProceso
            );
            return StatusCode(
                500,
                Result<bool>.Failure(
                    "Error interno del servidor al procesar la solicitud de calificación"
                )
            );
        }
    }

    /// <summary>
    /// Crea un nuevo proceso de recertificación para una empresa
    /// </summary>
    [Authorize(Roles = Policies.CreateRecertification)]
    [HttpPost("recertificacion/{empresaId}")]
    [ProducesResponseType(typeof(Result<ProcesoCertificacionDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<ProcesoCertificacionDTO>>> CrearRecertificacion(
        int empresaId
    )
    {
        try
        {
            if (empresaId <= 0)
                return BadRequest(
                    Result<ProcesoCertificacionDTO>.Failure("El ID de empresa no es válido")
                );

            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized(
                    Result<ProcesoCertificacionDTO>.Failure("Usuario no autorizado")
                );

            var result = await _unitOfWork.Proceso.CrearRecertificacionAsync(empresaId, appUser);

            return this.HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error no controlado al crear recertificación para la empresa {EmpresaId}",
                empresaId
            );
            return StatusCode(
                500,
                Result<ProcesoCertificacionDTO>.Failure(
                    "Error interno del servidor al procesar la solicitud"
                )
            );
        }
    }

    /// <summary>
    /// Cambia el proceso para comenzar el proceso de asesoria
    /// </summary>
    [Authorize(Roles = Policies.StartedConsulting)]
    [HttpPut("started/{procesoId}")]
    [ProducesResponseType(typeof(Result<ProcessStartedVm>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<ProcessStartedVm>>> StartedRecertificacion(
        int procesoId,
        ProcessStartedVm process
    )
    {
        try
        {
            if (procesoId <= 0)
                return BadRequest(
                    Result<ProcesoCertificacionDTO>.Failure("El ID del proceso no es válido")
                );

            process.Id = procesoId;
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized(
                    Result<ProcesoCertificacionDTO>.Failure("Usuario no autorizado")
                );

            var result = await _unitOfWork.Proceso.ComenzarProcesoAsesoriaAsync(process, appUser);

            // Enviar notificación en un hilo separado sin esperar a que termine
            _notificationService.SendNotificationProcessAsync(
                procesoId,
                appUser.Lenguage,
                _logger,
                _serviceScopeFactory,
                result.Value.NewStatus,
                "Proceso de asesoría iniciado"
            );

            return this.HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al comenzar el proceso de certificación para el id {ProcesoId}",
                procesoId
            );
            return StatusCode(
                500,
                Result<ProcessStartedVm>.Failure(
                    "Error interno del servidor al procesar la solicitud"
                )
            );
        }
    }

    [Authorize]
    [HttpPost("solicitar-auditoria/{procesoId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<bool>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<bool>>> SolicitaAuditoria(int procesoId)
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            // Verificar que el usuario tenga permiso para este proceso
            // Si el usuario es de tipo Empresa, solo puede solicitar auditoría para su propia empresa
            if (User.IsInRole(Constants.Roles.Empresa) && appUser.EmpresaId.HasValue)
            {
                // Verificar que el proceso pertenezca a la empresa del usuario
                var procesoEmpresa = await _unitOfWork.Proceso.ValidarProcesoEmpresaAsync(
                    procesoId,
                    appUser.EmpresaId.Value
                );
                if (!procesoEmpresa.IsSuccess)
                    return Forbid();
            }

            var res = await _unitOfWork.Proceso.SolicitarAuditoriaAsync(procesoId, appUser.Id);
            return this.HandleResponse(res);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al solicitar auditoría para proceso {ProcesoId}",
                procesoId
            );
            return StatusCode(500, Result<bool>.Failure("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Asigna un auditor a un proceso de certificación y cambia su estado
    /// </summary>
    [Authorize(Roles = Policies.AssignAuditor)]
    [HttpPut("assign-auditor/{procesoId}")]
    [ProducesResponseType(typeof(Result<AsignaAuditoriaVm>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<AsignaAuditoriaVm>>> AsignarAuditor(
        int procesoId,
        AsignaAuditoriaVm request
    )
    {
        try
        {
            // Validaciones básicas
            if (request.ProcesoId <= 0)
                return BadRequest(
                    Result<AsignaAuditoriaVm>.Failure("El ID del proceso no es válido")
                );

            if (string.IsNullOrEmpty(request.AuditorId))
                return BadRequest(
                    Result<AsignaAuditoriaVm>.Failure("El ID del auditor es requerido")
                );

            if (string.IsNullOrEmpty(request.Fecha))
                return BadRequest(
                    Result<AsignaAuditoriaVm>.Failure("La fecha de auditoría es requerida")
                );

            // Obtener el usuario actual
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized(Result<AsignaAuditoriaVm>.Failure("Usuario no autorizado"));

            // Llamar al servicio para asignar auditor
            var result = await _unitOfWork.Proceso.AsignarAuditorAsync(request, appUser.Id);

            // Enviar notificación en segundo plano sin esperar respuesta
            _notificationService.SendNotificationProcessAsync(
                procesoId,
                appUser.Lenguage,
                _logger,
                _serviceScopeFactory,
                result.Value.NewStatus,
                "Auditor asignado"
            );

            return this.HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al asignar auditor para el proceso {ProcesoId}",
                request.ProcesoId
            );

            return StatusCode(
                500,
                Result<AsignaAuditoriaVm>.Failure(
                    "Error interno del servidor al procesar la solicitud de asignación de auditor"
                )
            );
        }
    }

    /// <summary>
    /// Obtiene información sobre las dependencias de un proceso antes de eliminarlo
    /// </summary>
    [Authorize(Roles = Policies.AssignAuditor)] // Usamos una política existente para permisos de eliminación
    [HttpGet("{id}/deletion-info")]
    [ProducesResponseType(typeof(Result<ProcesosDeletionInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<ProcesosDeletionInfo>>> GetDeletionInfo(int id)
    {
        try
        {
            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
            if (appUser == null)
                return Unauthorized(Result<ProcesosDeletionInfo>.Failure("Usuario no autorizado"));

            var result = await _procesosDeletionService.CanDeleteProcesoAsync(
                id,
                appUser.PaisId ?? 0,
                role
            );
            return this.HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener información de eliminación para proceso {ProcesoId}",
                id
            );
            return StatusCode(
                500,
                Result<ProcesosDeletionInfo>.Failure("Error interno del servidor")
            );
        }
    }

    /// <summary>
    /// Elimina un proceso de certificación y todas sus dependencias relacionadas
    /// </summary>
    [Authorize(Roles = Policies.AssignAuditor)] // Usamos una política existente para permisos de eliminación
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Result<ProcesosDeletionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<ProcesosDeletionResult>>> Delete(int id)
    {
        try
        {
            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
            if (appUser == null)
                return Unauthorized(
                    Result<ProcesosDeletionResult>.Failure("Usuario no autorizado")
                );

            var result = await _procesosDeletionService.DeleteProcesoWithRelatedEntitiesAsync(
                id,
                appUser.PaisId ?? 0,
                role
            );
            return this.HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar proceso {ProcesoId}", id);
            return StatusCode(
                500,
                Result<ProcesosDeletionResult>.Failure("Error interno del servidor")
            );
        }
    }
}
