using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Middlewares;
using Sitca.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;

namespace Sitca.Controllers;

[Route("api/authentication")]
[ApiController]
[Authorize]
public class AuthenticationController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ProcesoArchivosController> _logger;

    public AuthenticationController(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ILogger<ProcesoArchivosController> logger
    )
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene la información del país asociado al usuario autenticado
    /// </summary>
    /// <returns>Información del país del usuario actual</returns>
    /// <response code="200">País obtenido exitosamente</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <response code="404">País no encontrado</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("user-country")]
    [ProducesResponseType(typeof(Result<Pais>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<Pais>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<Pais>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<Pais>>> GetUserCountry()
    {
        try
        {
            // Obtener usuario autenticado actual
            var currentUser = await this.GetCurrentUserAsync(_userManager);
            if (currentUser == null)
            {
                _logger.LogWarning("Intento de acceso sin usuario autenticado para obtener país");
                return Unauthorized();
            }

            // Validar que el usuario tenga un país asignado
            if (!currentUser.PaisId.HasValue || currentUser.PaisId.Value <= 0)
            {
                _logger.LogWarning(
                    "Usuario {UserId} no tiene un país válido asignado. PaisId: {PaisId}",
                    currentUser.Id,
                    currentUser.PaisId
                );
                return this.HandleResponse(
                    Result<Pais>.Failure("El usuario no tiene un país asignado")
                );
            }

            // Obtener información del país
            var countryResult = await _unitOfWork.Authentication.GetCountry(
                currentUser.PaisId.Value
            );

            if (!countryResult.IsSuccess)
            {
                _logger.LogWarning(
                    "No se pudo obtener el país {PaisId} para el usuario {UserId}: {Error}",
                    currentUser.PaisId.Value,
                    currentUser.Id,
                    countryResult.Error
                );
            }
            else
            {
                _logger.LogInformation(
                    "País {PaisId} obtenido exitosamente para el usuario {UserId}",
                    currentUser.PaisId.Value,
                    currentUser.Id
                );
            }

            return this.HandleResponse(countryResult);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "País no encontrado para el usuario");
            return this.HandleResponse(Result<Pais>.Failure("País no encontrado"));
        }
        catch (DatabaseException ex)
        {
            _logger.LogError(ex, "Error de base de datos al obtener el país del usuario");
            return this.HandleResponse(Result<Pais>.Failure("Error al acceder a la base de datos"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener el país del usuario autenticado");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                Result<Pais>.Failure("Error interno del servidor")
            );
        }
    }
}
