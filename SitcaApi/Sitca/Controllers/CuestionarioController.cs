using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Services.Notification;
using Sitca.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;

namespace Sitca.Controllers;

[Route("api/cuestionarios")]
[ApiController]
public class CuestionarioController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CuestionarioController> _logger;

    public CuestionarioController(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager,
        ILogger<CuestionarioController> logger
    )
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene un cuestionario específico por su ID
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CuestionarioDetailsVm), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CuestionarioDetailsVm>> GetById(int id)
    {
        try
        {
            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _unitOfWork.ProcesoCertificacion.GetCuestionario(id, appUser, role);

            if (result == null)
                return NotFound();

            return this.HandleResponse(result, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuestionario: {CuestionarioId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene todos los cuestionarios para una empresa
    /// </summary>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(List<CuestionarioDetailsMinVm>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<CuestionarioDetailsMinVm>>> GetAll(
        [FromQuery] int empresaId,
        [FromQuery] string lang = "es"
    )
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _unitOfWork.ProcesoCertificacion.GetCuestionariosList(
                empresaId,
                appUser
            );
            return this.HandleResponse(result, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener lista de cuestionarios para empresa: {EmpresaId}",
                empresaId
            );
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Guarda respuesta a una pregunta del cuestionario
    /// </summary>
    [Authorize(Roles = "Asesor, Auditor, Admin")]
    [HttpPut("preguntas")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> SavePregunta([FromBody] CuestionarioItemVm obj)
    {
        try
        {
            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            if (obj == null)
                return BadRequest("Los datos de la pregunta son requeridos");

            var result = await _unitOfWork.ProcesoCertificacion.SavePregunta(obj, appUser, role);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al guardar pregunta para cuestionario: {CuestionarioId}",
                obj?.CuestionarioId
            );
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Finaliza un cuestionario
    /// </summary>
    [Authorize(Roles = "Asesor, Auditor, TecnicoPais")]
    [HttpPost("{id}/finalizar")]
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<int>>> Finalizar(
        int id,
        [FromBody] CuestionarioDetailsVm data
    )
    {
        try
        {
            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            // Validar que el ID coincida con los datos
            if (data.Id != id)
            {
                return BadRequest(
                    Result<int>.Failure("El ID en la ruta no coincide con el ID en los datos")
                );
            }

            // Verificar si el cuestionario está completo
            try
            {
                var completo = await _unitOfWork.ProcesoCertificacion.IsCuestionarioCompleto(data);
                if (!completo)
                {
                    return BadRequest(
                        Result<int>.Failure(
                            "El cuestionario debe estar completo para poder finalizarlo"
                        )
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al verificar si el cuestionario está completo: {CuestionarioId}",
                    id
                );
                return BadRequest(
                    Result<int>.Failure("Error al verificar si el cuestionario está completo")
                );
            }

            var result = await _unitOfWork.ProcesoCertificacion.FinCuestionario(id, appUser, role);

            if (result.IsSuccess)
            {
                try
                {
                    await _notificationService.SendNotification(
                        result.Value,
                        null,
                        appUser.Lenguage
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error al enviar notificación para cuestionario finalizado: {CuestionarioId}",
                        id
                    );
                }
            }

            return this.HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al finalizar cuestionario: {CuestionarioId}", id);
            return StatusCode(500, Result<int>.Failure("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Obtiene el historial de un cuestionario específico
    /// </summary>
    [Authorize]
    [HttpGet("{id}/historial")]
    [ProducesResponseType(typeof(Result<List<HistorialVm>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<List<HistorialVm>>>> GetHistory(int id)
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _unitOfWork.ProcesoCertificacion.GetHistory(id);

            if (result == null)
                return NotFound();

            return this.HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener historial de cuestionario: {CuestionarioId}",
                id
            );
            return StatusCode(500, "Error interno del servidor");
        }
    }
}
