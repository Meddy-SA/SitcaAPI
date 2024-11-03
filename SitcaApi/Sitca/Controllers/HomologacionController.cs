using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
  public class HomologacionController : ControllerBase
  {
    private readonly ILogger<HomologacionController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;
    private readonly UserManager<ApplicationUser> _userManager;
    public HomologacionController(
        ILogger<HomologacionController> logger,
        IUnitOfWork unitOfWork,
        IConfiguration config,
        UserManager<ApplicationUser> userManager
    )
    {
      _logger = logger;
      _unitOfWork = unitOfWork;
      _config = config;
      _userManager = userManager;
    }

    [HttpGet("bloquearEdicion/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BloquearEdicion(int id)
    {
      var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
      var userFromDb = await _userManager.FindByEmailAsync(user);
      ApplicationUser appUser = (ApplicationUser)userFromDb;

      var result = await _unitOfWork.Homologacion.BloquearEdicion(id);
      var homologacion = _unitOfWork.Homologacion.Get(id);

      await _unitOfWork.Notificacion.SendNotificacionSpecial(homologacion.EmpresaId, -14, appUser.Lenguage);

      return Ok(JsonConvert.SerializeObject(result, Formatting.None,
                  new JsonSerializerSettings()
                  {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                  }));

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

        await _unitOfWork.Notificacion.SendNotificacionSpecial(empresaId, -13, appUser.Lenguage);

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

        //if (file.Length > 0)
        //{

        //    var uploadName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

        //    var extension = Path.GetExtension(uploadName);

        //    var fileName = DateTime.UtcNow.Ticks.ToString() + extension;

        //    var fullPath = Path.Combine(pathToSave, fileName);
        //    var dbPath = Path.Combine(folderName, fileName);

        //    //#region resize

        //    ///*
        //    // * https://blog.elmah.io/upload-and-resize-an-image-natively-with-asp-net-core/
        //    // * */

        //    //try
        //    //{

        //    //    using (var stream = new FileStream(fullPath, FileMode.Create))
        //    //    {
        //    //        var image = Image.FromStream(file.OpenReadStream());
        //    //        using var imageStream = new MemoryStream();
        //    //        var newSize = 700;
        //    //        decimal width = image.Width;

        //    //        decimal proporcional = width / newSize;
        //    //        decimal heigth = image.Height;


        //    //        int newHeigth = (int)(heigth / proporcional);

        //    //        var bitmap = new Bitmap(image, new Size(newSize, newHeigth));
        //    //        bitmap.Save(imageStream, ImageFormat.Jpeg);


        //    //        var optimizer = new ImageOptimizer();
        //    //        imageStream.Seek(0, SeekOrigin.Begin);
        //    //        //optimizer.Compress(imageStream);
        //    //        var lala = optimizer.LosslessCompress(imageStream);

        //    //        imageStream.WriteTo(stream);


        //    //        await imageStream.CopyToAsync(stream);

        //    //    }

        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    using (var stream = new FileStream(fullPath, FileMode.Create))
        //    //    {
        //    //        await file.CopyToAsync(stream);

        //    //    }

        //    //}


        //    //#endregion



        //    //save reference to db
        //    #region SAVETODB

        //    var user = User.Claims.First().Value;
        //    var IdentityUser = await _userManager.FindByEmailAsync(user);
        //    var roles = await _userManager.GetRolesAsync(IdentityUser);
        //    ApplicationUser appUser = (ApplicationUser)IdentityUser;

        //    var archivoObj = new Archivo();
        //    archivoObj = new Archivo
        //    {
        //        Activo = true,
        //        FechaCarga = DateTime.UtcNow,
        //        Ruta = fileName,
        //        Tipo = extension,
        //        UsuarioCargaId = IdentityUser.Id,
        //        Nombre = nombre,
        //    };


        //    if (!empresa)
        //    {
        //        switch (tipo)
        //        {
        //            case "pregunta":
        //                var preguntaId = Int32.Parse(formCollection["idPregunta"]);
        //                var respuestaId = Int32.Parse(formCollection["idRespuesta"]);

        //                archivoObj.CuestionarioItemId = preguntaId;
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        if (roles.Any(s => s.Contains("Empresa")))
        //        {
        //            empresaId = appUser.EmpresaId ?? 0;
        //        }

        //        archivoObj.EmpresaId = empresaId;
        //    }

        //    await _unitOfWork.Archivo.SaveFileData(archivoObj);

        //    #endregion

        //    if (nombre == "Protocolo Adhesión" || nombre == "Accession Protocol")
        //    {
        //        try
        //        {
        //            await _unitOfWork.Notificacion.SendNotificacionSpecial(empresaId, -2, appUser.Lenguage);
        //        }
        //        catch (Exception e)
        //        {
        //        }
        //    }
        //    if (nombre == "Solicitud de Certificación")
        //    {
        //        try
        //        {
        //            await _unitOfWork.Notificacion.SendNotificacionSpecial(empresaId, -3, appUser.Lenguage);
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }

        //    if (nombre == "Recomendación del Auditor al Ctc")
        //    {
        //        try
        //        {
        //            await _unitOfWork.Notificacion.SendNotificacionSpecial(empresaId, -11, appUser.Lenguage);
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }

        //    if (nombre == "Solicitud de ReCertificación")
        //    {
        //        try
        //        {
        //            await _unitOfWork.Notificacion.SendNotificacionSpecial(empresaId, -12, appUser.Lenguage);
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }


        //    return Ok(new { dbPath });
        //}
        //else
        //{
        //    return BadRequest();
        //}
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
