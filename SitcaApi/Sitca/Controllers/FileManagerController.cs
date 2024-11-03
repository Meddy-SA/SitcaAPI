using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Models;
using Sitca.Models.ViewModels;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;


namespace Sitca.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class FileManagerController : ControllerBase
  {
    private readonly ILogger<FileManagerController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;


    public FileManagerController(
        ILogger<FileManagerController> logger,
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager)
    {
      _logger = logger;
      _unitOfWork = unitOfWork;
      _userManager = userManager;
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
    [HttpPost, DisableRequestSizeLimit]
    public async Task<IActionResult> Upload()
    {
      try
      {
        var formCollection = await Request.ReadFormAsync();
        var file = formCollection.Files.First();
        var role = User.Claims.ToList()[2].Value;

        var nombre = formCollection["archivo"].ToString();
        var empresa = formCollection["empresa"].ToString() == "true";
        var idEmp = formCollection["empresaId"].ToString();//pasar por parametro porque carga un admin el archivo
        var tipo = formCollection["type"].ToString();

        var folderName = Path.Combine("Resources", "files");
        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

        var empresaId = 0;
        var lenguage = "es";


        if (role == "Empresa")
        {
          var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
          var userFromDb = await _userManager.FindByEmailAsync(user);
          var appUser = (ApplicationUser)userFromDb;
          lenguage = appUser.Lenguage;
          empresaId = appUser.EmpresaId ?? 0;
        }
        else
        {
          if (!string.IsNullOrEmpty(idEmp))
          {
            empresaId = Int32.Parse(idEmp);
          }
        }


        if (file.Length > 0)
        {

          var uploadName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

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

          var user = User.Claims.First().Value;
          var IdentityUser = await _userManager.FindByEmailAsync(user);
          var roles = await _userManager.GetRolesAsync(IdentityUser);
          ApplicationUser appUser = (ApplicationUser)IdentityUser;
          lenguage = appUser.Lenguage;

          var archivoObj = new Archivo();
          archivoObj = new Archivo
          {
            Activo = true,
            FechaCarga = DateTime.UtcNow,
            Ruta = fileName,
            Tipo = extension,
            UsuarioCargaId = IdentityUser.Id,
            Nombre = nombre,
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
            if (roles.Any(s => s.Contains("Empresa")))
            {
              empresaId = appUser.EmpresaId ?? 0;
            }

            archivoObj.EmpresaId = empresaId;
          }

          await _unitOfWork.Archivo.SaveFileData(archivoObj);

          #endregion

          if (nombre == "Protocolo Adhesión" || nombre == "Accession Protocol")
          {
            try
            {
              await _unitOfWork.Notificacion.SendNotificacionSpecial(empresaId, -2, appUser.Lenguage);
            }
            catch (Exception e)
            {
              _logger.LogError(e, "Error al enviar notificacion en Accession Protocol");
            }
          }
          if (nombre == "Solicitud de Certificación" || nombre == "Solicitud de Auditoria" || nombre == "Certification Request")
          {
            try
            {
              await _unitOfWork.Notificacion.SendNotificacionSpecial(empresaId, -3, appUser.Lenguage);
            }
            catch (Exception)
            {
            }
          }

          if (nombre == "Recomendación del Auditor al Ctc" || nombre == "Recommendations to the CTC")
          {
            try
            {
              await _unitOfWork.Notificacion.SendNotificacionSpecial(empresaId, -11, appUser.Lenguage);
            }
            catch (Exception)
            {
            }
          }

          if (nombre == "Solicitud de ReCertificación" || nombre == "Solicitud de Re Certificación" || nombre == "Recertification Application")
          {
            try
            {
              await _unitOfWork.Notificacion.SendNotificacionSpecial(empresaId, -12, appUser.Lenguage);
            }
            catch (Exception)
            {
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
          var uploadName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

          var extension = Path.GetExtension(uploadName);

          var fileName = DateTime.UtcNow.Ticks.ToString() + extension;

          var fullPath = Path.Combine(pathToSave, fileName);
          var dbPath = Path.Combine(folderName, fileName);

          using (var stream = new FileStream(fullPath, FileMode.Create))
          {
            await file.CopyToAsync(stream);
            //file.CopyTo(stream);
          };

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
          var uploadName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

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

      return Ok(JsonConvert.SerializeObject(result, Formatting.None,
                  new JsonSerializerSettings()
                  {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                  }));

    }

  }
}
