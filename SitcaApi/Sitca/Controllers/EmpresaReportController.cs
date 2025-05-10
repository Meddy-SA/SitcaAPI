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

namespace Sitca.Controllers;

[Authorize]
[Route("api/reports/empresas")]
[ApiController]
public class EmpresaReportController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<EmpresaReportController> _logger;

    public EmpresaReportController(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ILogger<EmpresaReportController> logger
    )
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene un reporte de empresas agrupadas por sus procesos de certificación según los filtros aplicados
    /// </summary>
    /// <param name="filter">Filtros a aplicar</param>
    /// <returns>Reporte de empresas</returns>
    [HttpPost("report")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<EmpresaReportResponseDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<EmpresaReportResponseDTO>>> GetEmpresaReport(
        [FromBody] EmpresaReportFilterDTO filter
    )
    {
        try
        {
            // Validar autenticación
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            // Asegurarse que el filtro exista
            filter ??= new EmpresaReportFilterDTO { Language = appUser.Lenguage };

            // Establecer idioma por defecto si no viene
            if (string.IsNullOrEmpty(filter.Language))
            {
                filter.Language = appUser.Lenguage;
            }

            // Aplicar restricciones según rol
            if (!User.IsInRole(Utilities.Common.Constants.Roles.Admin))
            {
                // Si no es admin, restringir a su país
                filter.CountryIds = new List<int> { appUser.PaisId ?? 0 };
            }

            // Manejar valor nulo para distintivos - asegurar que sea una lista vacía en lugar de null
            if (filter.DistintivoIds == null)
            {
                filter.DistintivoIds = new List<int>();
            }

            // Generar reporte
            var result = await _unitOfWork.EmpresaReport.GetEmpresaReportAsync(filter);
            return this.HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al generar reporte de empresas con filtro {@Filter}",
                filter
            );
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                Result<EmpresaReportResponseDTO>.Failure("Error interno del servidor")
            );
        }
    }

    /// <summary>
    /// Obtiene los metadatos necesarios para el filtro del reporte de empresas
    /// </summary>
    /// <returns>Metadatos para el filtro</returns>
    [HttpGet("metadata")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<MetadatosDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<MetadatosDTO>>> GetReportMetadata()
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var result = await _unitOfWork.EmpresaReport.GetReportMetadataAsync(appUser.Lenguage);
            return this.HandleResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener metadatos para el reporte de empresas");
            return StatusCode(500, Result<MetadatosDTO>.Failure("Error interno del servidor"));
        }
    }
}
