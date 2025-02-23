using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Extensions;
using Sitca.DataAccess.Helpers;
using Sitca.DataAccess.Services.Notification;
using Sitca.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.Enums;
using Sitca.Models.ViewModels;
using Utilities.Common;

namespace Sitca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileManagerController : ControllerBase
    {
        private readonly ILogger<FileManagerController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public FileManagerController(
            ILogger<FileManagerController> logger,
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager
        )
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        [HttpGet("GetTypeFilesCompany")]
        public ActionResult<List<EnumValueDto>> Get()
        {
            try
            {
                var result = _unitOfWork.Archivo.GetTypeFilesCompany();
                return this.HandleResponse(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [Authorize(Roles = "Admin,TecnicoPais,Asesor,Auditor,CTC")]
        [HttpGet]
        [Route("DeleteFile")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                var userFromDb = await _userManager.FindByEmailAsync(user);
                ApplicationUser appUser = (ApplicationUser)userFromDb;

                var role = User.Claims.ToList()[2].Value;

                var result = await _unitOfWork.Archivo.DeleteFile(id, appUser, role);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [Authorize]
        [HttpPost]
        [DisableRequestSizeLimit]
        [ProducesResponseType(
            typeof(Result<Models.DTOs.FileUploadResponse>),
            StatusCodes.Status200OK
        )]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<FileUploadResponse>>> UploadFile()
        {
            try
            {
                if (!Request.HasFormContentType || !Request.Form.Files.Any())
                    return BadRequest(
                        Result<FileUploadResponse>.Failure("No se encontró ningún archivo")
                    );

                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null)
                    return Unauthorized();

                var uploadRequest = await _unitOfWork.Archivo.ValidateAndCreateUploadRequest(
                    Request.Form,
                    appUser
                );
                if (!uploadRequest.IsSuccess)
                    return BadRequest(uploadRequest.Error);

                var fileResult = await _unitOfWork.Archivo.ProcessFileUpload(uploadRequest.Value);
                if (!fileResult.IsSuccess)
                    return this.HandleResponse(
                        Result<FileUploadResponse>.Failure(fileResult.Error)
                    );

                await SendNotificationsIfRequired(uploadRequest.Value);

                return this.HandleResponse(Result<FileUploadResponse>.Success(fileResult.Value));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la carga de archivos");
                return StatusCode(
                    500,
                    Result<FileUploadResponse>.Failure("Error interno del servidor")
                );
            }
        }

        [Authorize]
        [HttpPost("old"), DisableRequestSizeLimit]
        public async Task<IActionResult> Upload()
        {
            try
            {
                var (appUser, roleUser) = await this.GetCurrentUserWithRoleAsync(_userManager);

                var formCollection = await Request.ReadFormAsync();
                var file = formCollection.Files.First();
                var role = User.Claims.ToList()[2].Value;

                var nombre = formCollection["archivo"].ToString();
                var empresa = formCollection["empresa"].ToString() == "true";
                var typeFile = formCollection["typeFile"].ToString();
                var idEmp = formCollection["empresaId"].ToString(); //pasar por parametro porque carga un admin el archivo
                var tipo = formCollection["type"].ToString();

                var folderName = Path.Combine("Resources", "files");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                var empresaId = 0;
                var lenguage = "es";

                if (role == Constants.Roles.Empresa)
                {
                    lenguage = appUser.Lenguage;
                    empresaId = appUser.EmpresaId ?? 0;
                }
                else if (!string.IsNullOrEmpty(idEmp))
                {
                    empresaId = Int32.Parse(idEmp);
                }

                if (file.Length > 0)
                {
                    var uploadName = ContentDispositionHeaderValue
                        .Parse(file.ContentDisposition)
                        .FileName.Trim('"');
                    var extension = Path.GetExtension(uploadName);
                    var fileName = DateTime.UtcNow.Ticks.ToString() + extension;
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);

                    #region resize

                    /*
                     * https://blog.elmah.io/upload-and-resize-an-image-natively-with-asp-net-core/
                     * */

                    try
                    {
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            var image = Image.FromStream(file.OpenReadStream());
                            using var imageStream = new MemoryStream();
                            var newSize = image.Width;
                            int newHeigth = image.Height;
                            if (image.Width > 1200 && image.Height > 1000)
                            {
                                newSize = 700;
                                decimal width = image.Width;
                                decimal proporcional = width / newSize;
                                decimal heigth = image.Height;
                                newHeigth = (int)(heigth / proporcional);
                            }

                            var bitmap = new Bitmap(image, new Size(newSize, newHeigth));
                            bitmap.Save(imageStream, ImageFormat.Jpeg);

                            var optimizer = new ImageOptimizer();
                            imageStream.Seek(0, SeekOrigin.Begin);
                            //optimizer.Compress(imageStream);
                            var result = optimizer.LosslessCompress(imageStream);

                            imageStream.WriteTo(stream);
                            await imageStream.CopyToAsync(stream);
                        }
                    }
                    catch (Exception e)
                    {
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        _logger.LogError(e, "Error al redimensionar imagen");
                    }

                    #endregion

                    //save reference to db
                    #region SAVETODB
                    var roles = await _userManager.GetRolesAsync(appUser);
                    lenguage = appUser.Lenguage;

                    var fileTypeCompany = FileCompany.Informativo;
                    if (
                        !string.IsNullOrEmpty(typeFile)
                        && int.TryParse(typeFile, out int parsedValue)
                    )
                    {
                        fileTypeCompany = FileCompanyExtensions.GetFileType(parsedValue);
                    }

                    var archivoObj = new Archivo();
                    archivoObj = new Archivo
                    {
                        Activo = true,
                        FechaCarga = DateTime.UtcNow,
                        Ruta = fileName,
                        Tipo = extension,
                        UsuarioCargaId = appUser.Id,
                        Nombre = nombre,
                        FileTypesCompany = fileTypeCompany,
                    };

                    if (!empresa)
                    {
                        switch (tipo)
                        {
                            case "pregunta":
                                var preguntaId = Int32.Parse(formCollection["idPregunta"]);
                                var respuestaId = Int32.Parse(formCollection["idRespuesta"]);
                                archivoObj.CuestionarioItemId = preguntaId;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        if (roles.Any(s => s.Contains(Constants.Roles.Empresa)))
                        {
                            empresaId = appUser.EmpresaId ?? 0;
                        }
                        archivoObj.EmpresaId = empresaId;
                    }

                    await _unitOfWork.Archivo.SaveFileData(archivoObj);

                    #endregion

                    var notificationType = NotificationHelper.GetNotificationTypeByName(nombre);
                    if (notificationType.HasValue)
                    {
                        try
                        {
                            await _notificationService.SendNotificacionSpecial(
                                empresaId,
                                notificationType.Value,
                                appUser.Lenguage
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Error al enviar notificación de tipo {NotificationType} para empresa {EmpresaId}",
                                notificationType.Value,
                                empresaId
                            );
                        }
                    }
                    return Ok(new { dbPath });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        private async Task SendNotificationsIfRequired(UploadRequest request)
        {
            var notificationType = NotificationHelper.GetNotificationTypeByName(request.FileName);
            if (!notificationType.HasValue)
                return;

            try
            {
                await _notificationService.SendNotificacionSpecial(
                    request.EmpresaId,
                    notificationType.Value,
                    request.User.Lenguage
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al enviar notificación de tipo {NotificationType} para empresa {EmpresaId}",
                    notificationType.Value,
                    request.EmpresaId
                );
                // No relanzamos la excepción para no interrumpir el flujo principal
            }
        }

        [Authorize]
        [HttpPost, DisableRequestSizeLimit]
        [Route("UploadMyFile")]
        public async Task<IActionResult> UploadMyFile()
        {
            try
            {
                var formCollection = await Request.ReadFormAsync();
                var file = formCollection.Files.First();
                var role = User.Claims.ToList()[2].Value;

                var nombre = formCollection["archivo"].ToString();

                var folderName = Path.Combine("Resources", "files");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                var userFromDb = await _userManager.FindByEmailAsync(user);
                var appUser = (ApplicationUser)userFromDb;

                if (file.Length > 0)
                {
                    var uploadName = ContentDispositionHeaderValue
                        .Parse(file.ContentDisposition)
                        .FileName.Trim('"');

                    var extension = Path.GetExtension(uploadName);

                    var fileName = DateTime.UtcNow.Ticks.ToString() + extension;

                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    ;

                    //save reference to db
                    #region SAVETODB

                    var archivoObj = new Archivo();
                    archivoObj = new Archivo
                    {
                        Activo = true,
                        FechaCarga = DateTime.UtcNow,
                        Ruta = fileName,
                        Tipo = extension,
                        UsuarioCargaId = userFromDb.Id,
                        UsuarioId = userFromDb.Id,
                        Nombre = nombre,
                    };

                    await _unitOfWork.Archivo.SaveFileData(archivoObj);

                    #endregion

                    return Ok(new { dbPath });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [Authorize(Roles = "Admin,TecnicoPais")]
        [HttpPost, DisableRequestSizeLimit]
        [Route("SaveItemCapacitacion")]
        public async Task<IActionResult> SaveItemCapacitacion()
        {
            try
            {
                var formCollection = await Request.ReadFormAsync();

                var campos = formCollection["campos"].ToString();
                var camposObj = JsonConvert.DeserializeObject<DatosCapacitacion>(campos);

                var role = User.Claims.ToList()[2].Value;

                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                var userFromDb = await _userManager.FindByEmailAsync(user);
                var appUser = (ApplicationUser)userFromDb;

                //guardar como un link
                var item = new Capacitaciones
                {
                    Descripcion = camposObj.descripcion,
                    Nombre = camposObj.nombre,
                    Tipo = camposObj.tipo,
                    UsuarioCargaId = appUser.Id,
                    FechaCarga = DateTime.UtcNow,
                };

                if (camposObj.tipo == "Link")
                {
                    item.Ruta = camposObj.url;
                    await _unitOfWork.Capacitaciones.SaveCapacitacion(item);
                    return Ok();
                }

                var file = formCollection.Files.First();

                var folderName = Path.Combine("Resources", "files");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0)
                {
                    var uploadName = ContentDispositionHeaderValue
                        .Parse(file.ContentDisposition)
                        .FileName.Trim('"');

                    var extension = Path.GetExtension(uploadName);

                    var fileName = DateTime.UtcNow.Ticks.ToString() + extension;

                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                        //file.CopyTo(stream);
                        item.Ruta = fileName;
                        await _unitOfWork.Capacitaciones.SaveCapacitacion(item);
                        return Ok();
                    }
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [Authorize]
        [HttpPost]
        [Route("GetFiles")]
        public async Task<IActionResult> GetFiles(ArchivoFilterVm data)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var userFromDb = await _userManager.FindByEmailAsync(user);
            var appUser = (ApplicationUser)userFromDb;
            var role = User.Claims.ToList()[2].Value;

            var result = await _unitOfWork.Archivo.GetList(data, appUser, role);

            return Ok(
                JsonConvert.SerializeObject(
                    result,
                    Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    }
                )
            );
        }
    }
}
