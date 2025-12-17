using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;
using static Utilities.Common.Constants;

namespace Sitca.Controllers;

[ApiController]
[Route("api/v1/profesionales")]
[Authorize]
public class ProfesionalesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProfesionalesController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfesionalesController(
        IUnitOfWork unitOfWork,
        ILogger<ProfesionalesController> logger,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    /// <summary>
    /// Obtiene una lista de asesores y auditores habilitados agrupados por país
    /// </summary>
    /// <param name="paisId">ID del país (opcional)</param>
    /// <param name="tipoProfesional">Tipo de profesional: auditor, asesor, auditor/asesor (opcional)</param>
    /// <param name="soloActivos">Filtrar solo usuarios activos (por defecto: true)</param>
    /// <param name="soloConCarnet">Filtrar solo usuarios con carnet (por defecto: true)</param>
    /// <param name="language">Idioma de respuesta (es/en, por defecto: es)</param>
    /// <returns>Lista de profesionales habilitados agrupados por país</returns>
    [HttpGet("habilitados")]
    public async Task<
        ActionResult<Result<ProfesionalesHabilitadosResponseDTO>>
    > GetProfesionalesHabilitados(
        [FromQuery] int? paisId = null,
        [FromQuery] string tipoProfesional = null,
        [FromQuery] bool? soloActivos = true,
        [FromQuery] bool? soloConCarnet = true,
        [FromQuery] string language = "es"
    )
    {
        try
        {
            // Obtener usuario actual y rol
            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);

            // Si no es Admin, forzar filtro por país del usuario
            int? effectivePaisId = paisId;
            if (role != Roles.Admin && appUser?.PaisId != null)
            {
                effectivePaisId = appUser.PaisId;
            }

            var filter = new ProfesionalesHabilitadosFilterDTO
            {
                PaisId = effectivePaisId,
                TipoProfesional = tipoProfesional?.ToLower(),
                SoloActivos = soloActivos,
                SoloConCarnet = soloConCarnet,
                Language = language,
            };

            // Validar tipo de profesional si se proporciona
            if (!string.IsNullOrEmpty(filter.TipoProfesional))
            {
                var tiposValidos = new[] { "auditor", "asesor", "auditor/asesor" };
                if (!tiposValidos.Contains(filter.TipoProfesional))
                {
                    return BadRequest(
                        new
                        {
                            error = "Tipo de profesional inválido",
                            message = $"Los tipos válidos son: {string.Join(", ", tiposValidos)}",
                            tipos_validos = tiposValidos,
                        }
                    );
                }
            }

            var result = await _unitOfWork.Profesionales.GetProfesionalesHabilitadosAsync(filter);

            if (!result.IsSuccess)
            {
                _logger.LogError(
                    "Error al obtener profesionales habilitados: {Error}",
                    result.Error
                );
                return StatusCode(
                    500,
                    new { error = "Error interno del servidor", message = result.Error }
                );
            }

            return this.HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener profesionales habilitados");
            return StatusCode(
                500,
                new
                {
                    error = "Error interno del servidor",
                    message = "Error inesperado al procesar la solicitud",
                }
            );
        }
    }
}
