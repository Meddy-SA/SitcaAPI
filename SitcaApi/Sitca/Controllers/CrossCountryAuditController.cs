using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.Enums;
using Utilities.Common;

namespace Sitca.Controllers;

[Route("api/cross-country-audit")]
[ApiController]
[Authorize]
public class CrossCountryAuditController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CrossCountryAuditController> _logger;

    public CrossCountryAuditController(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ILogger<CrossCountryAuditController> logger
    )
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las solicitudes para un país, opcionalmente filtradas por estado
    /// </summary>
    [HttpGet("requests")]
    [Authorize]
    [ProducesResponseType(
        StatusCodes.Status200OK,
        Type = typeof(Result<List<CrossCountryAuditRequestDTO>>)
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<List<CrossCountryAuditRequestDTO>>>> GetRequests(
        [FromQuery] int? countryId,
        [FromQuery] string status
    )
    {
        try
        {
            // Obtener usuario autenticado
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized(
                    Result<List<CrossCountryAuditRequestDTO>>.Failure("Usuario no autenticado")
                );

            // Determinar el país para el que se van a obtener las solicitudes
            int targetCountryId;

            // Si no se especifica un país, usar el país del usuario
            if (!countryId.HasValue)
            {
                if (!appUser.PaisId.HasValue)
                    return BadRequest(
                        Result<List<CrossCountryAuditRequestDTO>>.Failure(
                            "Debe especificar un país"
                        )
                    );

                targetCountryId = appUser.PaisId.Value;
            }
            else
            {
                // Si se especifica un país, verificar permisos
                if (
                    !User.IsInRole(Constants.Roles.Admin)
                    && (!appUser.PaisId.HasValue || appUser.PaisId.Value != countryId.Value)
                )
                {
                    return Forbid();
                }

                targetCountryId = countryId.Value;
            }

            // Parsear el estado si se proporciona
            CrossCountryAuditRequestStatus? statusEnum = null;
            if (
                !string.IsNullOrEmpty(status)
                && Enum.TryParse<CrossCountryAuditRequestStatus>(status, true, out var parsedStatus)
            )
            {
                statusEnum = parsedStatus;
            }

            // Obtener las solicitudes
            var result = await _unitOfWork.CrossCountryAuditRequest.GetForCountryAsync(
                targetCountryId,
                statusEnum,
                appUser.Id
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener solicitudes de auditoría cruzada");
            return StatusCode(
                500,
                Result<List<CrossCountryAuditRequestDTO>>.Failure(
                    "Error interno del servidor: " + ex.Message
                )
            );
        }
    }

    /// <summary>
    /// Crea una nueva solicitud de auditoría cruzada
    /// </summary>
    [HttpPost("requests")]
    [Authorize(Roles = Constants.Roles.Admin + "," + Constants.Roles.TecnicoPais)]
    [ProducesResponseType(
        StatusCodes.Status200OK,
        Type = typeof(Result<CrossCountryAuditRequestDTO>)
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<CrossCountryAuditRequestDTO>>> CreateRequest(
        [FromBody] CreateCrossCountryAuditRequestDTO dto
    )
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(
                    Result<CrossCountryAuditRequestDTO>.Failure(ModelState.GetErrorMessagesLines())
                );

            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized(
                    Result<CrossCountryAuditRequestDTO>.Failure("Usuario no autenticado")
                );

            var result = await _unitOfWork.CrossCountryAuditRequest.CreateAsync(dto, appUser);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear solicitud de auditoría cruzada");
            return StatusCode(
                500,
                Result<CrossCountryAuditRequestDTO>.Failure(
                    "Error interno del servidor: " + ex.Message
                )
            );
        }
    }

    /// <summary>
    /// Obtiene una solicitud específica por su ID
    /// </summary>
    [HttpGet("requests/{id}")]
    [Authorize(Roles = Constants.Roles.Admin + "," + Constants.Roles.TecnicoPais)]
    [ProducesResponseType(
        StatusCodes.Status200OK,
        Type = typeof(Result<CrossCountryAuditRequestDTO>)
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<CrossCountryAuditRequestDTO>>> GetRequestById(int id)
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized(
                    Result<CrossCountryAuditRequestDTO>.Failure("Usuario no autenticado")
                );

            var result = await _unitOfWork.CrossCountryAuditRequest.GetByIdAsync(id, appUser);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener solicitud de auditoría cruzada {RequestId}", id);
            return StatusCode(
                500,
                Result<CrossCountryAuditRequestDTO>.Failure(
                    "Error interno del servidor: " + ex.Message
                )
            );
        }
    }

    /// <summary>
    /// Aprueba una solicitud de auditoría cruzada
    /// </summary>
    [HttpPut("requests/{id}/approve")]
    [Authorize(Roles = Constants.Roles.Admin + "," + Constants.Roles.TecnicoPais)]
    [ProducesResponseType(
        StatusCodes.Status200OK,
        Type = typeof(Result<CrossCountryAuditRequestDTO>)
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<CrossCountryAuditRequestDTO>>> ApproveRequest(
        int id,
        [FromBody] ApproveCrossCountryAuditRequestDTO dto
    )
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(
                    Result<CrossCountryAuditRequestDTO>.Failure(ModelState.GetErrorMessagesLines())
                );

            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized(
                    Result<CrossCountryAuditRequestDTO>.Failure("Usuario no autenticado")
                );

            var result = await _unitOfWork.CrossCountryAuditRequest.ApproveAsync(id, dto, appUser);
            if (!result.IsSuccess)
            {
                if (result.Error.Contains("No se encontró"))
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aprobar solicitud de auditoría cruzada {RequestId}", id);
            return StatusCode(
                500,
                Result<CrossCountryAuditRequestDTO>.Failure(
                    "Error interno del servidor: " + ex.Message
                )
            );
        }
    }

    /// <summary>
    /// Rechaza una solicitud de auditoría cruzada
    /// </summary>
    [HttpPut("requests/{id}/reject")]
    [Authorize(Roles = Constants.Roles.Admin + "," + Constants.Roles.TecnicoPais)]
    [ProducesResponseType(
        StatusCodes.Status200OK,
        Type = typeof(Result<CrossCountryAuditRequestDTO>)
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<CrossCountryAuditRequestDTO>>> RejectRequest(
        int id,
        [FromBody] RejectCrossCountryAuditRequestDTO dto
    )
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(
                    Result<CrossCountryAuditRequestDTO>.Failure(ModelState.GetErrorMessagesLines())
                );

            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized(
                    Result<CrossCountryAuditRequestDTO>.Failure("Usuario no autenticado")
                );

            var result = await _unitOfWork.CrossCountryAuditRequest.RejectAsync(id, dto, appUser);
            if (!result.IsSuccess)
            {
                if (result.Error.Contains("No se encontró"))
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al rechazar solicitud de auditoría cruzada {RequestId}",
                id
            );
            return StatusCode(
                500,
                Result<CrossCountryAuditRequestDTO>.Failure(
                    "Error interno del servidor: " + ex.Message
                )
            );
        }
    }

    /// <summary>
    /// Revoca una solicitud de auditoría cruzada previamente aprobada
    /// </summary>
    [HttpPut("requests/{id}/revoke")]
    [Authorize(Roles = Constants.Roles.Admin + "," + Constants.Roles.TecnicoPais)]
    [ProducesResponseType(
        StatusCodes.Status200OK,
        Type = typeof(Result<CrossCountryAuditRequestDTO>)
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<CrossCountryAuditRequestDTO>>> RevokeRequest(int id)
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized(
                    Result<CrossCountryAuditRequestDTO>.Failure("Usuario no autenticado")
                );

            var result = await _unitOfWork.CrossCountryAuditRequest.RevokeAsync(id, appUser);
            if (!result.IsSuccess)
            {
                if (result.Error.Contains("No se encontró"))
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al revocar solicitud de auditoría cruzada {RequestId}", id);
            return StatusCode(
                500,
                Result<CrossCountryAuditRequestDTO>.Failure(
                    "Error interno del servidor: " + ex.Message
                )
            );
        }
    }
}
