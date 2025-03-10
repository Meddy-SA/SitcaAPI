using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Services.Files;
using Sitca.DataAccess.Services.Pdf;
using Sitca.DataAccess.Services.ViewToString;
using Sitca.Extensions;
using Sitca.Models;
using Sitca.Models.ViewModels;

namespace Sitca.Controllers;

/// <summary>
/// Controlador para generación de reportes en formato PDF
/// </summary>
[Authorize]
[Route("api/reports")]
[ApiController]
[Produces("application/pdf")]
public class ReportPdfController : ControllerBase
{
    private readonly ILogger<ReportPdfController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReportService _reportService;
    private readonly IConfiguration _config;
    private IWebHostEnvironment _hostEnvironment;
    private readonly IIconService _iconService;
    private readonly IViewRenderService _viewRenderService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReportPdfController(
        ILogger<ReportPdfController> logger,
        IReportService reportService,
        IConfiguration config,
        IViewRenderService viewRenderService,
        IUnitOfWork unitOfWork,
        IIconService iconService,
        IWebHostEnvironment environment,
        UserManager<ApplicationUser> userManager
    )
    {
        _logger = logger;
        _reportService = reportService;
        _config = config;
        _viewRenderService = viewRenderService;
        _unitOfWork = unitOfWork;
        _iconService = iconService;
        _hostEnvironment = environment;
        _userManager = userManager;
    }

    /// <summary>
    /// Genera el reporte de recomendación CTC para una empresa específica
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>PDF con la recomendación CTC</returns>
    [HttpGet("empresas/{empresaId}/recomendacion-ctc")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRecomendacionCTC(int empresaId)
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var res = await _unitOfWork.Empresa.Data(empresaId, appUser);
            res.Language = appUser.Lenguage;
            res.RutaPdf = GenerateBaseUrl();

            var view = await _viewRenderService.RenderToStringAsync("RecomendacionACTC", res);
            var pdfFile = _reportService.GeneratePdfReport(view);
            return File(pdfFile, "application/pdf", "RecomendacionCTC.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al generar recomendación CTC para empresa: {EmpresaId}",
                empresaId
            );
            return BadRequest("Error al generar el PDF de recomendación CTC");
        }
    }

    /// <summary>
    /// Genera el dictamen técnico para una empresa específica
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>PDF con el dictamen técnico</returns>
    [HttpGet("empresas/{empresaId}/dictamen-tecnico")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDictamen(int empresaId)
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var res = await _unitOfWork.Empresa.Data(empresaId, appUser);
            res.Language = appUser.Lenguage;
            res.RutaPdf = GenerateBaseUrl();

            // Formatear fecha de fin
            if (!string.IsNullOrEmpty(res.CertificacionActual?.FechaFin))
            {
                res.CertificacionActual.FechaFin = DateTime
                    .Parse(res.CertificacionActual.FechaFin)
                    .ToString("dd/MM/yyyy");
            }

            // Obtener el mes actual en el idioma del usuario
            res.MesHoy = DateTime.Now.ToString(
                "MMMM",
                CultureInfo.CreateSpecificCulture(res.Language)
            );

            var view = await _viewRenderService.RenderToStringAsync("DictamenTecnico", res);
            var pdfFile = _reportService.GeneratePdfReport(view);
            return File(pdfFile, "application/pdf", "DictamenTecnico.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al generar dictamen técnico para empresa: {EmpresaId}",
                empresaId
            );
            return BadRequest("Error al generar el PDF de dictamen técnico");
        }
    }

    /// <summary>
    /// Genera el protocolo de adhesión para la empresa del usuario actual
    /// </summary>
    /// <returns>PDF con el protocolo de adhesión</returns>
    [HttpGet("empresas/current/protocolo-adhesion")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProtocoloAdhesion()
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            int empresaId = appUser.EmpresaId ?? 0;
            var res = await _unitOfWork.Empresa.Data(empresaId, appUser);
            res.RutaPdf = GenerateBaseUrl();

            var view = await _viewRenderService.RenderToStringAsync("ProtocoloAdhesion", res);
            var pdfFile = _reportService.GeneratePdfReport(view);
            return File(pdfFile, "application/pdf", "ProtocoloAdhesion.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar protocolo de adhesión");
            return BadRequest("Error al generar el PDF de protocolo de adhesión");
        }
    }

    /// <summary>
    /// Genera la solicitud de certificación para la empresa del usuario actual
    /// </summary>
    /// <returns>PDF con la solicitud de certificación</returns>
    [HttpGet("empresas/current/solicitud-certificacion")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSolicitudCertificacion()
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            int empresaId = appUser.EmpresaId ?? 0;
            var res = await _unitOfWork.Empresa.Data(empresaId, appUser);
            res.RutaPdf = GenerateBaseUrl();

            // Asignar el nombre de la tipología seleccionada
            if (res.Tipologias != null && res.Tipologias.Any(s => s.isSelected))
            {
                res.CertificacionActual.TipologiaName = res
                    .Tipologias.First(s => s.isSelected)
                    .name;
            }

            var view = await _viewRenderService.RenderToStringAsync("SolicitudCertificacion", res);
            var pdfFile = _reportService.GeneratePdfReport(view);
            return File(pdfFile, "application/pdf", "SolicitudCertificacion.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar solicitud de certificación");
            return BadRequest("Error al generar el PDF de solicitud de certificación");
        }
    }

    /// <summary>
    /// Genera la solicitud de recertificación para la empresa del usuario actual
    /// </summary>
    /// <returns>PDF con la solicitud de recertificación</returns>
    [HttpGet("empresas/current/solicitud-recertificacion")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSolicitudReCertificacion()
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            int empresaId = appUser.EmpresaId ?? 0;
            var res = await _unitOfWork.Empresa.Data(empresaId, appUser);
            res.RutaPdf = GenerateBaseUrl();

            // Asignar el nombre de la tipología seleccionada
            if (res.Tipologias != null && res.Tipologias.Any(s => s.isSelected))
            {
                res.CertificacionActual.TipologiaName = res
                    .Tipologias.First(s => s.isSelected)
                    .name;
            }

            var view = await _viewRenderService.RenderToStringAsync(
                "SolicitudReCertificacion",
                res
            );
            var pdfFile = _reportService.GeneratePdfReport(view);
            return File(pdfFile, "application/pdf", "SolicitudReCertificacion.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar solicitud de recertificación");
            return BadRequest("Error al generar el PDF de solicitud de recertificación");
        }
    }

    /// <summary>
    /// Genera el reporte de hallazgos para un cuestionario específico
    /// </summary>
    /// <param name="cuestionarioId">ID del cuestionario</param>
    /// <returns>PDF con el reporte de hallazgos</returns>
    [HttpGet("cuestionarios/{cuestionarioId}/hallazgos")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetReporteHallazgos(int cuestionarioId)
    {
        try
        {
            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var res = await _unitOfWork.ProcesoCertificacion.ReporteHallazgos(
                cuestionarioId,
                appUser,
                role
            );

            res.Language = appUser.Lenguage;
            res.RutaPdf = GenerateBaseUrl();

            var view = await _viewRenderService.RenderToStringAsync("ReporteHallazgos", res);
            var pdfFile = _reportService.GeneratePdfReport(view);
            return File(pdfFile, "application/pdf", "ReporteHallazgos.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al generar reporte de hallazgos para cuestionario: {CuestionarioId}",
                cuestionarioId
            );
            return BadRequest("Error al generar el PDF de reporte de hallazgos");
        }
    }

    /// <summary>
    /// Genera el informe de no cumplimientos para un cuestionario específico
    /// </summary>
    /// <param name="cuestionarioId">ID del cuestionario</param>
    /// <returns>PDF con el informe de no cumplimientos</returns>
    [HttpGet("cuestionarios/{cuestionarioId}/no-cumplimientos")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetInformeNoCumplimientos(int cuestionarioId)
    {
        try
        {
            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var resultados = await _unitOfWork.ProcesoCertificacion.GetNoCumplimientos(
                cuestionarioId,
                appUser,
                role
            );
            resultados.path = GenerateBaseUrl();
            resultados.Lenguage = appUser.Lenguage;

            var view = await _viewRenderService.RenderToStringAsync(
                "InformeNoCumplimientos",
                resultados
            );
            var pdfFile = _reportService.GeneratePdfReport(view);
            return File(pdfFile, "application/pdf", "InformeNoCumplimientos.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al generar informe de no cumplimientos para cuestionario: {CuestionarioId}",
                cuestionarioId
            );
            return BadRequest("Error al generar el PDF de informe de no cumplimientos");
        }
    }

    /// <summary>
    /// Genera el reporte completo de certificación para un cuestionario específico
    /// </summary>
    /// <param name="cuestionarioId">ID del cuestionario</param>
    /// <returns>PDF con el reporte de certificación</returns>
    [HttpGet("cuestionarios/{cuestionarioId}/certificacion")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetReporteCertificacion(int cuestionarioId)
    {
        try
        {
            var (currentUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
            if (currentUser == null)
                return Unauthorized();

            var resultados = await _unitOfWork.ProcesoCertificacion.GetCuestionario(
                cuestionarioId,
                currentUser,
                role
            );

            // Preparar datos para el informe
            resultados.path = GenerateBaseUrl();

            // Obtener observaciones
            var respuestasIds = resultados.Modulos.SelectMany(s =>
                s.Items.Where(s => s.IdRespuesta > 0).Select(s => s.IdRespuesta.GetValueOrDefault())
            );

            var obs = await _unitOfWork.ProcesoCertificacion.GetListObservaciones(respuestasIds);

            // Obtener archivos
            var archivoFilter = new ArchivoFilterVm
            {
                idCuestionario = cuestionarioId,
                type = "cuestionario",
            };
            var archivos = await _unitOfWork.Archivo.GetList(archivoFilter, currentUser, role);

            // Asignar archivos y observaciones a las preguntas
            foreach (var item in resultados.Modulos)
            {
                var preguntas = item.Items.Where(s => s.Type == "pregunta");
                foreach (var pregunta in preguntas)
                {
                    pregunta.Archivos = archivos
                        .Where(s => s.CuestionarioItemId == pregunta.IdRespuesta)
                        .ToList();

                    // Asignar iconos a los archivos
                    foreach (var arc in pregunta.Archivos)
                    {
                        arc.Base64Str = _iconService.GetIconForFileType(arc.Tipo);
                    }

                    pregunta.Observacion = obs.Any(s => s.IdRespuesta == pregunta.IdRespuesta)
                        ? obs.First(s => s.IdRespuesta == pregunta.IdRespuesta).Observaciones
                        : string.Empty;
                }
            }

            // Generar el PDF
            resultados.Lang = currentUser.Lenguage;

            // Seleccionar la vista correcta basada en si es una prueba o no
            var vista = resultados.Prueba ? "ReporteAsesoria" : "ReporteCertificacionV2";
            var view = await _viewRenderService.RenderToStringAsync(vista, resultados);
            var pdfFile = _reportService.GeneratePdfReport(view);
            return File(pdfFile, "application/pdf", "ReporteCertificacion.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al generar reporte de certificación para cuestionario: {CuestionarioId}",
                cuestionarioId
            );
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Genera la declaración jurada para el usuario actual
    /// </summary>
    /// <returns>PDF con la declaración jurada</returns>
    [HttpGet("users/current/declaracion-jurada")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDeclaracionJurada()
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var res = await _unitOfWork.Users.GetUserById(appUser.Id);
            res.Lang = appUser.Lenguage;
            res.RutaPdf = GenerateBaseUrl();
            res.Codigo = DateTime.Now.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"));

            var view = await _viewRenderService.RenderToStringAsync("DeclaracionJurada", res);
            var pdfFile = _reportService.GeneratePdfReport(view);
            return File(pdfFile, "application/pdf", "DeclaracionJurada.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar declaración jurada");
            return BadRequest("Error al generar el PDF de declaración jurada");
        }
    }

    /// <summary>
    /// Genera el compromiso de confidencialidad para el usuario actual
    /// </summary>
    /// <returns>PDF con el compromiso de confidencialidad</returns>
    [HttpGet("users/current/compromiso-confidencialidad")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCompromiso()
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var res = await _unitOfWork.Users.GetUserById(appUser.Id);
            res.Lang = appUser.Lenguage;
            res.RutaPdf = GenerateBaseUrl();
            res.Codigo = DateTime.Now.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"));

            var view = await _viewRenderService.RenderToStringAsync(
                "CompromisoConfidencialidad",
                res
            );
            var pdfFile = _reportService.GeneratePdfReport(view);
            return File(pdfFile, "application/pdf", "CompromisoConfidencialidad.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar compromiso de confidencialidad");
            return BadRequest("Error al generar el PDF de compromiso de confidencialidad");
        }
    }

    /// <summary>
    /// Genera el compromiso de confidencialidad para auditores
    /// </summary>
    /// <returns>PDF con el compromiso de confidencialidad para auditores</returns>
    [HttpGet("users/current/compromiso-confidencialidad-auditor")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCompromisoAuditor()
    {
        try
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var res = await _unitOfWork.Users.GetUserById(appUser.Id);
            res.Lang = appUser.Lenguage;
            res.RutaPdf = GenerateBaseUrl();
            res.Codigo = DateTime.Now.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"));

            var view = await _viewRenderService.RenderToStringAsync(
                "CompromisoConfidencialidadAuditor",
                res
            );
            var pdfFile = _reportService.GeneratePdfReport(view);
            return File(pdfFile, "application/pdf", "CompromisoConfidencialidadAuditor.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar compromiso de confidencialidad para auditores");
            return BadRequest(
                "Error al generar el PDF de compromiso de confidencialidad para auditores"
            );
        }
    }

    private string GenerateBaseUrl()
    {
        return $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}";
    }
}
