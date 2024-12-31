using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitca.DataAccess.Data.Repository.Constants;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Services.Notification;
using Sitca.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities.Common;

namespace Sitca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomologacionController : ControllerBase
    {
        private readonly ILogger<HomologacionController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomologacionController(
            ILogger<HomologacionController> logger,
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IConfiguration config,
            UserManager<ApplicationUser> userManager
        )
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _config = config;
            _userManager = userManager;
        }

        /// <summary>
        /// Alterna el estado de bloqueo de edición de una homologación
        /// </summary>
        /// <param name="id">ID de la homologación</param>
        /// <returns>Estado actualizado del bloqueo</returns>
        [HttpGet("bloquearEdicion/{id}")]
        [Authorize(Roles = Constants.Roles.Admin)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<HomologacionBloqueoDto>>> BloquearEdicion(int id)
        {
            try
            {
                var currentUser = await this.GetCurrentUserAsync(_userManager);
                if (currentUser == null)
                {
                    _logger.LogWarning("Intento de acceso sin usuario autenticado");
                    return Unauthorized("Usuario no autenticado");
                }

                var resultado = await _unitOfWork.Homologacion.ToggleBloqueoEdicionAsync(id);
                if (!resultado.IsSuccess)
                {
                    return NotFound(resultado.Error);
                }

                try
                {
                    // Intentamos enviar la notificación
                    await _notificationService.SendNotificacionSpecial(
                        resultado.Value.EmpresaId,
                        NotificationTypes.Homologacion,
                        currentUser.Lenguage);
                }
                catch (Exception notifEx)
                {
                    // Logueamos el error pero no fallamos la operación principal
                    _logger.LogWarning(notifEx,
                        "Error al enviar notificación para la homologación {Id}", id);
                }

                return this.HandleResponse(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al modificar el estado de bloqueo de la homologación {Id}", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Error al procesar la solicitud de bloqueo");
            }

        }

        [HttpGet]
        [Authorize(Roles = "TecnicoPais, Admin")]
        public async Task<IActionResult> List()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            var pais = appUser.PaisId ?? 0;
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value;

            if (role == "Admin")
            {
                //momentaneamente solo homologacion para costa rica
                pais = 6;
            }

            var result = await _unitOfWork.Homologacion.List(pais);

            return Ok(JsonConvert.SerializeObject(result, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));

        }

        [HttpGet("details/{id}")]
        [Authorize(Roles = "TecnicoPais, Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);

            ApplicationUser appUser = (ApplicationUser)userFromDb;


            var result = await _unitOfWork.Homologacion.Details(appUser, role, id);

            return Ok(JsonConvert.SerializeObject(result, Formatting.None,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));
        }


        [Authorize(Roles = "TecnicoPais")]
        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> Upload()
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                var userFromDb = await _userManager.FindByEmailAsync(user);
                ApplicationUser appUser = (ApplicationUser)userFromDb;

                var formCollection = await Request.ReadFormAsync();

                var files = formCollection.Files;

                var empresa = formCollection["empresa"].ToString();


                var empresa2 = JsonConvert.DeserializeObject<HomologacionDTO>(empresa);

                var folderName = Path.Combine("Resources", "files");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                var empresaId = await _unitOfWork.Homologacion.Create(empresa2, appUser);

                await _notificationService.SendNotificacionSpecial(empresaId,
                  NotificationTypes.ExpedienteHomologacion, appUser.Lenguage);

                //AGREGO LOS ARCHIVOS

                foreach (var item in files)
                {
                    var uploadName = ContentDispositionHeaderValue.Parse(item.ContentDisposition).FileName.Trim('"');
                    var realName = ContentDispositionHeaderValue.Parse(item.ContentDisposition).Name.Trim('"');
                    var extension = Path.GetExtension(realName);

                    var fileName = DateTime.UtcNow.Ticks.ToString() + extension;
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);

                    try
                    {
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            var image = Image.FromStream(item.OpenReadStream());
                            using var imageStream = new MemoryStream();
                            var newSize = 700;
                            decimal width = image.Width;

                            decimal proporcional = width / newSize;
                            decimal heigth = image.Height;


                            int newHeigth = (int)(heigth / proporcional);

                            var bitmap = new Bitmap(image, new Size(newSize, newHeigth));
                            bitmap.Save(imageStream, ImageFormat.Jpeg);


                            var optimizer = new ImageOptimizer();
                            imageStream.Seek(0, SeekOrigin.Begin);
                            //optimizer.Compress(imageStream);
                            var lala = optimizer.LosslessCompress(imageStream);

                            imageStream.WriteTo(stream);

                            await imageStream.CopyToAsync(stream);
                        }
                    }
                    catch (Exception e)
                    {
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await item.CopyToAsync(stream);
                        }
                        _logger.LogError(e, "Error uploading file");
                    }

                    var archivoObj = new Archivo();
                    archivoObj = new Archivo
                    {
                        Activo = true,
                        FechaCarga = DateTime.UtcNow,
                        Ruta = fileName,
                        Tipo = extension,
                        UsuarioCargaId = appUser.Id,
                        Nombre = item.FileName,
                        EmpresaId = empresaId
                    };

                    await _unitOfWork.Archivo.SaveFileData(archivoObj);
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex}");
            }
            return Ok();
        }

        [Authorize(Roles = "TecnicoPais")]
        [HttpPost("update")]
        public async Task<IActionResult> Update(HomologacionDTO empresa)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                var userFromDb = await _userManager.FindByEmailAsync(user);

                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value;

                ApplicationUser appUser = (ApplicationUser)userFromDb;

                await _unitOfWork.Homologacion.Update(appUser, role, empresa);
                return Ok();
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex}");
            }

        }
    }
}
