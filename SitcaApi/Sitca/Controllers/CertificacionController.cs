using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.Constants;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Extensions;
using Sitca.DataAccess.Services.Notification;
using Sitca.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;
using Policies = Utilities.Common.AuthorizationPolicies.Certificaciones;

namespace Sitca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificacionController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<CertificacionController> _logger;

        public CertificacionController(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            ILogger<CertificacionController> logger
        )
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [Authorize(Roles = Policies.ConvertirARecetificacion)]
        [HttpPost("ConvertirARecertificacion")]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<bool>>> ConvertirARecertificacionAsync(
            [FromBody] EmpresaVm data
        )
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null)
                    return Unauthorized();

                var result = await _unitOfWork.ProcesoCertificacion.ConvertirARecertificacionAsync(
                    appUser,
                    data
                );

                return this.HandleResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al procesar conversión a recertificación para empresa {EmpresaId}",
                    data.Id
                );
                return StatusCode(500, Result<bool>.Failure("Error interno del servidor"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("ReAbrirCuestionario/{id}")]
        public async Task<ActionResult<Result<bool>>> ReAbrirCuestionario(int id)
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var res = await _unitOfWork.ProcesoCertificacion.ReAbrirCuestionario(appUser, id);

            return this.HandleResponse(res);
        }

        [Authorize(Roles = "Asesor, Auditor, Admin")]
        [HttpPost("SaveObservaciones")]
        public async Task<ActionResult<Result<string>>> SaveObservaciones(ObservacionesDTO data)
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var res = await _unitOfWork.ProcesoCertificacion.SaveObservaciones(appUser, data);

            return this.HandleResponse(res);
        }

        [Authorize]
        [HttpGet("GetObservaciones")]
        public async Task<ActionResult<ObservacionesDTO>> GetObservaciones(int idRespuesta)
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var res = await _unitOfWork.ProcesoCertificacion.GetObservaciones(idRespuesta);

            return this.HandleResponse(res);
        }

        [Authorize(Roles = "Admin,TecnicoPais")]
        [HttpPost]
        [Route("SolicitarAsesoria")]
        public async Task<IActionResult> SolicitarAsesoria(EmpresaUpdateVm data)
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var userFromDb = await _userManager.FindByEmailAsync(appUser.Email);

            return Ok();
        }

        [Authorize]
        [HttpPost("SolicitaAuditoria")]
        public async Task<ActionResult<Result<bool>>> SolicitaAuditoria(EmpresaUpdateVm data)
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var res = await _unitOfWork.Empresa.SolicitaAuditoriaAsync(appUser.EmpresaId ?? 0);

            return this.HandleResponse(res);
        }

        [Authorize]
        [HttpPost]
        [Route("SaveCalificacion")]
        public async Task<ActionResult<bool>> SaveCalificacion(SaveCalificacionVm data)
        {
            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var res = await _unitOfWork.ProcesoCertificacion.SaveCalificacion(data, appUser, role);

            try
            {
                await _notificationService.SendNotification(data.idProceso, null, appUser.Lenguage);
            }
            catch (Exception) { }
            return Ok(res.Value);
        }

        [Authorize(Roles = Policies.UpdateAuditor)]
        [HttpPost("CambiarAuditor")]
        public async Task<ActionResult<Result<bool>>> CambiarAuditor(CambioAuditor data)
        {
            try
            {
                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null)
                    return Unauthorized(Result<bool>.Failure("Usuario no autorizado"));

                if (!data.IsValid())
                {
                    return BadRequest(Result<bool>.Failure("Datos de cambios no validas"));
                }

                var result = await _unitOfWork.ProcesoCertificacion.CambiarAuditorAsync(data);

                if (!result.IsSuccess)
                    return this.HandleResponse(result);

                try
                {
                    if (data.auditor)
                    {
                        await _notificationService.SendNotification(
                            data.idProceso,
                            (int)NotificationTypes.CambioAuditor,
                            appUser.Lenguage
                        );
                    }
                    else
                    {
                        await _notificationService.SendNotification(
                            data.idProceso,
                            (int)NotificationTypes.CambioAsesor,
                            appUser.Lenguage
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error al enviar notificación de cambio de {Rol} para el proceso {ProcesoId}",
                        data.auditor ? "auditor" : "asesor",
                        data.idProceso
                    );
                    // No retornamos el error ya que el cambio fue exitoso
                }
                return this.HandleResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error no controlado al cambiar {Rol} para el proceso {ProcesoId}",
                    data.auditor ? "auditor" : "asesor",
                    data.idProceso
                );
                return StatusCode(
                    500,
                    Result<bool>.Failure("Error interno del servidor al procesar la solicitud")
                );
            }
        }

        [Authorize]
        [HttpPost("ChangeStatus")]
        public async Task<ActionResult> ChangeStatus(CertificacionStatusVm data)
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();
            //recibir empresa, asesor y obtener el usuario que la genera

            // Busca el estado y devuelve el id
            int toStatus = StatusConstants.GetStatusId(data.Status, "es");
            var result = await _unitOfWork.ProcesoCertificacion.ChangeStatus(data, toStatus);

            return Ok();
        }

        [Authorize(Roles = Policies.GenerarCuestionario)]
        [HttpPost("GenerarCuestionario")]
        public async Task<ActionResult<Result<CuestionarioDetailsMinVm>>> GenerarCuestionario(
            CuestionarioCreateVm data
        )
        {
            // Validación del modelo
            if (!ModelState.IsValid)
                return BadRequest(
                    Result<CuestionarioDetailsMinVm>.Failure(ModelState.GetErrorMessages())
                );

            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
            if (appUser == null)
                return Unauthorized(
                    Result<CuestionarioDetailsMinVm>.Failure("Usuario no autorizado")
                );

            try
            {
                var result = await _unitOfWork.ProcesoCertificacion.GenerarCuestionarioAsync(
                    data,
                    appUser.Id,
                    role
                );
                if (result == null)
                    return BadRequest("No se pudo generar el cuestionario");

                return this.HandleResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al generar cuestionario. Usuario: {UserId}, Empresa: {EmpresaId}",
                    appUser.Id,
                    data.EmpresaId
                );
                return StatusCode(
                    500,
                    Result<CuestionarioDetailsMinVm>.Failure("Error interno del servidor")
                );
            }
        }

        [Authorize]
        [HttpGet("GetCuestionario")]
        public async Task<ActionResult<CuestionarioDetailsVm>> GetCuestionario(int idEmpresa)
        {
            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _unitOfWork.ProcesoCertificacion.GetCuestionario(
                idEmpresa,
                appUser,
                role
            );

            return this.HandleResponse(result, true);
        }

        [Authorize]
        [HttpGet("GetCuestionarios")]
        public async Task<ActionResult<List<CuestionarioDetailsMinVm>>> GetCuestionarios(
            int idEmpresa,
            string lang
        )
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _unitOfWork.ProcesoCertificacion.GetCuestionariosList(
                idEmpresa,
                appUser
            );

            return this.HandleResponse(result, true);
        }

        [Authorize(Roles = "Asesor, Auditor, Admin")]
        [Route("SavePregunta")]
        [HttpPost]
        public async Task<IActionResult> SavePregunta(CuestionarioItemVm obj)
        {
            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);

            var result = await _unitOfWork.ProcesoCertificacion.SavePregunta(obj, appUser, role);

            return Ok(result);
        }

        [Authorize(Roles = "Asesor, Auditor, TecnicoPais")]
        [HttpPost("FinCuestionario")]
        public async Task<ActionResult<Result<int>>> FinCuestionario(CuestionarioDetailsVm data)
        {
            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            try
            {
                var completo = await _unitOfWork.ProcesoCertificacion.IsCuestionarioCompleto(data);
                if (!completo)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si el cuestionario es completo");
            }

            var result = await _unitOfWork.ProcesoCertificacion.FinCuestionario(
                data.Id,
                appUser,
                role
            );
            try
            {
                if (result.IsSuccess)
                {
                    await _notificationService.SendNotification(
                        result.Value,
                        null,
                        appUser.Lenguage
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificacion");
            }

            return this.HandleResponse(result);
        }

        [Authorize(Roles = "Asesor, Auditor, TecnicoPais")]
        [HttpGet("CanFinalizeCuestionario")]
        public async Task<ActionResult<Result<bool>>> CanFinalizarCuestionario(int id)
        {
            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _unitOfWork.ProcesoCertificacion.CanFinalizeCuestionario(id, role);
            return this.HandleResponse(result);
        }
    }
}
