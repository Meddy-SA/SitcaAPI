using System;
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

namespace Sitca.Controllers;

[Route("api/procesos")]
[ApiController]
public class ProcesoCertificacionController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ProcesoCertificacionController> _logger;

    public ProcesoCertificacionController(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager,
        ILogger<ProcesoCertificacionController> logger
    )
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _userManager = userManager;
        _logger = logger;
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

            var res = await _unitOfWork.Proceso.GetProcesoForIdAsync(id);
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
}
