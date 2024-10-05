using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Services.Pdf;
using Sitca.DataAccess.Services.ViewToString;
using Sitca.Models;
using Sitca.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sitca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportPdfController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReportService _reportService;
        private readonly IConfiguration _config;
        private IWebHostEnvironment _hostEnvironment;
        //view to string
        private readonly IViewRenderService _viewRenderService;
        private readonly UserManager<IdentityUser> _userManager;

        public ReportPdfController(IReportService reportService, IConfiguration config, IViewRenderService viewRenderService, IUnitOfWork unitOfWork, IWebHostEnvironment environment, UserManager<IdentityUser> userManager)
        {
            _reportService = reportService;
            _config = config;
            _viewRenderService = viewRenderService;
            _unitOfWork = unitOfWork;
            _hostEnvironment = environment;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        [Route("GenerarRecomendacionCTC")]
        public async Task<IActionResult> GenerarRecomendacionCTC(int empresaId)
        {

            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var IdentityUser = await _userManager.FindByEmailAsync(user);           
            ApplicationUser appUser = (ApplicationUser)IdentityUser;

            var res = await _unitOfWork.Empresa.Data(empresaId, appUser.Id);
            res.Language = appUser.Lenguage;
            res.RutaPdf = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;
            try
            {
                var view = await _viewRenderService.RenderToStringAsync("RecomendacionACTC", res);
                var pdfFile = _reportService.GeneratePdfReport(view);
                return File(pdfFile,
                "application/octet-stream", "SimplePdf.pdf");
            }
            catch (Exception e)
            {
                return BadRequest();             
            }
            
        }

        [HttpGet]
        [Authorize]
        [Route("ReporteDeHallazgos")]
        public async Task<IActionResult> ReporteDeHallazgos(int cuestionarioId)
        {

            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var IdentityUser = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)IdentityUser;
            var role = User.Claims.ToList()[2].Value;

            var res = await _unitOfWork.ProcesoCertificacion.ReporteHallazgos(cuestionarioId, appUser, role);
            
            res.Language = appUser.Lenguage;
            res.RutaPdf = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;
            try
            {
                var view = await _viewRenderService.RenderToStringAsync("ReporteHallazgos", res);
                var pdfFile = _reportService.GeneratePdfReport(view);
                return File(pdfFile,
                "application/octet-stream", "ReporteHallazgos.pdf");
            }
            catch (Exception e)
            {
                return BadRequest();
            }

        }

        [HttpGet]
        [Authorize]
        [Route("GenerarDeclaracionJurada")]
        public async Task<IActionResult> GenerarDeclaracionJurada()
        {

            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var IdentityUser = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)IdentityUser;

            var res = await _unitOfWork.Users.GetUserById(appUser.Id);
            res.Lang = appUser.Lenguage;
            var MesHoy = DateTime.Now.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"));
            res.RutaPdf = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;
            res.Codigo = MesHoy;
            try
            {
                var view = await _viewRenderService.RenderToStringAsync("DeclaracionJurada", res);
                var pdfFile = _reportService.GeneratePdfReport(view);
                return File(pdfFile,
                "application/octet-stream", "SimplePdf.pdf");
            }
            catch (Exception e)
            {
                return BadRequest();
            }

        }

        [HttpGet]
        [Authorize]
        [Route("GenerarCompromiso")]
        public async Task<IActionResult> GenerarCompromiso()
        {

            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var IdentityUser = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)IdentityUser;

            var res = await _unitOfWork.Users.GetUserById(appUser.Id);
            res.Lang = appUser.Lenguage;
            var MesHoy = DateTime.Now.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"));

            res.RutaPdf = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;

            res.Codigo = MesHoy;
            try
            {
                var view = await _viewRenderService.RenderToStringAsync("CompromisoConfidencialidad", res);
                var pdfFile = _reportService.GeneratePdfReport(view);
                return File(pdfFile,
                "application/octet-stream", "SimplePdf.pdf");
            }
            catch (Exception e)
            {
                return BadRequest();
            }

        }

        [HttpGet]
        [Authorize]
        [Route("GenerarCompromisoAuditor")]
        public async Task<IActionResult> GenerarCompromisoAuditor()
        {

            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var IdentityUser = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)IdentityUser;

            var res = await _unitOfWork.Users.GetUserById(appUser.Id);
            res.Lang = appUser.Lenguage;
            var MesHoy = DateTime.Now.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"));

            res.RutaPdf = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;

            res.Codigo = MesHoy;
            try
            {
                var view = await _viewRenderService.RenderToStringAsync("CompromisoConfidencialidadAuditor", res);
                var pdfFile = _reportService.GeneratePdfReport(view);
                return File(pdfFile,
                "application/octet-stream", "SimplePdf.pdf");
            }
            catch (Exception e)
            {
                return BadRequest();
            }

        }

        [HttpGet]
        [Authorize]
        [Route("GenerarDictamen")]
        public async Task<IActionResult> GenerarDictamen(int empresaId)
        {

            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var IdentityUser = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)IdentityUser;

            var res = await _unitOfWork.Empresa.Data(empresaId, appUser.Id);
            res.CertificacionActual.FechaFin = DateTime.Parse(res.CertificacionActual.FechaFin).ToString("dd/MM/yyyy"); 
            res.Language = appUser.Lenguage;
            var MesHoy = DateTime.Now.ToString("MMMM", CultureInfo.CreateSpecificCulture(res.Language));
            res.MesHoy = MesHoy;
            res.RutaPdf = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;
            try
            {
                var view = await _viewRenderService.RenderToStringAsync("DictamenTecnico", res);
                var pdfFile = _reportService.GeneratePdfReport(view);
                return File(pdfFile,
                "application/octet-stream", "SimplePdf.pdf");
            }
            catch (Exception e)
            {
                return BadRequest();
            }

        }

        [HttpGet]
        [Authorize]
        [Route("InformeNoCumplimientos")]
        public async Task<IActionResult> InformeNoCumplimientos(int idCuestionario)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var IdentityUser = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)IdentityUser;
            var role = User.Claims.ToList()[2].Value;

            var resultados = await _unitOfWork.ProcesoCertificacion.GetNoCumplimientos(idCuestionario, appUser, role);
            resultados.path = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;

            resultados.Lenguage = appUser.Lenguage;

            try
            {
                var view = await _viewRenderService.RenderToStringAsync("InformeNoCumplimientos", resultados);
                var pdfFile = _reportService.GeneratePdfReport(view);
                return File(pdfFile,
                "application/octet-stream", "InformeNoCumplimientos.pdf");
            }
            catch (Exception e)
            {
                return BadRequest();
            }

        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {

            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var IdentityUser = await _userManager.FindByEmailAsync(user);


            ApplicationUser appUser = (ApplicationUser)IdentityUser;

            

            var res = await _unitOfWork.Empresa.Data(appUser.EmpresaId ?? 0, appUser.Id);
            res.RutaPdf = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;
            try
            {
                var view = await _viewRenderService.RenderToStringAsync("ProtocoloAdhesion", res);
                var pdfFile = _reportService.GeneratePdfReport(view);
                return File(pdfFile,
                "application/octet-stream", "SimplePdf.pdf");
            }
            catch (Exception e)
            {
                return BadRequest();
            }

        }

        [HttpGet]
        [Route("SolicitudCertificacion")]
        //[Authorize]
        public async Task<IActionResult> SolicitudCertificacion()
        {

            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var IdentityUser = await _userManager.FindByEmailAsync(user);

            ApplicationUser appUser = (ApplicationUser)IdentityUser;

            var res = await _unitOfWork.Empresa.Data(appUser.EmpresaId ?? 0,IdentityUser.Id);
            res.RutaPdf = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;

            res.CertificacionActual.TipologiaName = res.Tipologias.First(s => s.isSelected).name;

            try
            {
                var view = await _viewRenderService.RenderToStringAsync("SolicitudCertificacion", res);
                var pdfFile = _reportService.GeneratePdfReport(view);
                return File(pdfFile,
                "application/octet-stream", "SolicitudCertificacion.pdf");
            }
            catch (Exception e)
            {
                return BadRequest();
            }

        }

        [HttpGet]
        [Route("SolicitudReCertificacion")]
        //[Authorize]
        public async Task<IActionResult> SolicitudReCertificacion()
        {

            var user = User.Claims.First().Value;

            var IdentityUser = await _userManager.FindByEmailAsync(user);

            ApplicationUser appUser = (ApplicationUser)IdentityUser;

            var res = await _unitOfWork.Empresa.Data(appUser.EmpresaId ?? 0, IdentityUser.Id);
            res.RutaPdf = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;

            res.CertificacionActual.TipologiaName = res.Tipologias.First(s => s.isSelected).name;

            try
            {
                var view = await _viewRenderService.RenderToStringAsync("SolicitudReCertificacion", res);
                var pdfFile = _reportService.GeneratePdfReport(view);
                return File(pdfFile,
                "application/octet-stream", "SolicitudReCertificacion.pdf");
            }
            catch (Exception e)
            {
                return BadRequest();
            }

        }


        [HttpGet]
        [Route("ReporteCertificacion")]
        [Authorize]
        public async Task<IActionResult> ReporteCertificacion(int idCuestionario)
        {

            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var IdentityUser = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)IdentityUser;
            var role = User.Claims.ToList()[2].Value;

            var resultados = await _unitOfWork.ProcesoCertificacion.GetCuestionario(idCuestionario, appUser, role);
            
            var archivoFilter = new ArchivoFilterVm
            {
                idCuestionario = idCuestionario,
                type = "cuestionario"
            };
                       
            resultados.path = HttpContext.Request.Scheme +"://" + HttpContext.Request.Host.Value;


            //cambiar
            //resultados.Pais = 2;

            var obs = new List<ObservacionesDTO>();
            //agrego comentarios al reporte
            
            //SOLO GUATEMALA
            //if (resultados.Pais == 2)
            //{
            //    var respuestasIds = resultados.Modulos.SelectMany(s => s.Items.Where(s => s.IdRespuesta > 0).Select(s => s.IdRespuesta.GetValueOrDefault()));
            //    obs = await _unitOfWork.ProcesoCertificacion.GetListObservaciones(respuestasIds);
            //}

            //TODOS LOS PAISES
            var respuestasIds = resultados.Modulos.SelectMany(s => s.Items.Where(s => s.IdRespuesta > 0).Select(s => s.IdRespuesta.GetValueOrDefault()));
            obs = await _unitOfWork.ProcesoCertificacion.GetListObservaciones(respuestasIds);

            //agrego archivos al reporte
            var archivos = await _unitOfWork.Archivo.GetList(archivoFilter, appUser, role);
            
            //base 64 de imagenes de multimedia default
            var filePdfIcon = "data:image/png;base64,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";
            var fileWordIcon = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAHUAAABkCAYAAAC4jn+CAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAAAZdEVYdFNvZnR3YXJlAEFkb2JlIEltYWdlUmVhZHlxyWU8AAADImlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS4zLWMwMTEgNjYuMTQ1NjYxLCAyMDEyLzAyLzA2LTE0OjU2OjI3ICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIiB4bWxuczpzdFJlZj0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlUmVmIyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ1M2IChXaW5kb3dzKSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDozQTlEQzFBMEMwOEMxMUVDQjhEOUYxMDkyODkzRkU2MCIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDozQTlEQzFBMUMwOEMxMUVDQjhEOUYxMDkyODkzRkU2MCI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjNBOURDMTlFQzA4QzExRUNCOEQ5RjEwOTI4OTNGRTYwIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjNBOURDMTlGQzA4QzExRUNCOEQ5RjEwOTI4OTNGRTYwIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwv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";
            var fileExcelIcon = "data:image/jpeg;base64,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";
            var filePPTIcon = "data:image/png;base64,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";

            var imageIcon = "data:image/png;base64,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";
            foreach (var item in resultados.Modulos)
            {
                foreach (var pregunta in item.Items.Where(s =>s.Type == "pregunta"))
                {
                    pregunta.Archivos = archivos.Where(s => s.CuestionarioItemId == pregunta.IdRespuesta).ToList();

                    foreach (var arc in pregunta.Archivos)
                    {
                        if (arc.Tipo == ".jpg" || arc.Tipo == ".png" || arc.Tipo == ".jpeg" || arc.Tipo == ".jfif" || arc.Tipo == ".jfif")
                        {
                            //generar base64
                            var finalRoute = _hostEnvironment.WebRootPath.Replace("\\wwwroot", "") + "\\Resources\\Files\\" + arc.Ruta;

                            finalRoute = Path.Combine(Directory.GetCurrentDirectory(), @"Resources//Files//", arc.Ruta);

                            //try
                            //{
                            //    Byte[] bytes = System.IO.File.ReadAllBytes(finalRoute);
                            //    arc.Base64Str = $"data:image/{arc.Tipo};base64," + Convert.ToBase64String(bytes);
                            //}
                            //catch (Exception)
                            //{
                            //    arc.Base64Str = "data:image/png;base64,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";
                            //}
                            arc.Base64Str = imageIcon;
                        }
                        else
                        {
                            switch (arc.Tipo)
                            {
                                case ".pdf" :
                                    arc.Base64Str = filePdfIcon;
                                    break;
                                case ".ppt":
                                    arc.Base64Str = filePPTIcon;
                                    break;
                                case ".xlsx":
                                    arc.Base64Str = fileExcelIcon;
                                    break;
                                case ".docx":
                                case ".doc":
                                    arc.Base64Str = fileWordIcon;
                                    break;
                      
                            }
                        }                        
                    }

                    pregunta.Observacion = obs.Any(s => s.IdRespuesta == pregunta.IdRespuesta) ? obs.First(s => s.IdRespuesta == pregunta.IdRespuesta).Observaciones : String.Empty;

                }
            }



            try
            {
                resultados.Lang = appUser.Lenguage;


                //var vista = resultados.Pais == 2 ? "ReporteCertificacionV2" : "ReporteCertificacion";
                var vista = resultados.Prueba?"ReporteAsesoria": "ReporteCertificacionV2";
                var view = await _viewRenderService.RenderToStringAsync(vista, resultados);
                var pdfFile = _reportService.GeneratePdfReport(view);
                return File(pdfFile,
                "application/octet-stream", "SolicitudCertificacion.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message + " " + ex.InnerException);
            }

        }


    }
}
