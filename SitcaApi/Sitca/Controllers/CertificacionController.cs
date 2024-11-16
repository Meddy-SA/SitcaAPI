using Core.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitca.DataAccess.Data.Repository.Constants;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Services.Notification;
using Sitca.Extensions;
using Sitca.Models;
using Sitca.Models.ViewModels;
using System;
using System.Threading.Tasks;

namespace Sitca.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class CertificacionController : ControllerBase
  {
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<CertificacionController> _logger;

    public CertificacionController(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        ILogger<CertificacionController> logger)
    {
      _unitOfWork = unitOfWork;
      _notificationService = notificationService;
      _userManager = userManager;
      _emailSender = emailSender;
      _logger = logger;
    }

    [Authorize(Roles = "Admin,TecnicoPais")]
    [HttpPost]
    [Route("ConvertirARecertificacion")]
    public async Task<IActionResult> ConvertirARecertificacion(EmpresaVm data)
    {
      var appUser = await this.GetCurrentUserAsync(_userManager);
      if (appUser == null) return Unauthorized();

      var res = await _unitOfWork.ProcesoCertificacion.ConvertirARecertificacion(appUser, data);

      return Ok(res);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    [Route("ReAbrirCuestionario")]
    public async Task<IActionResult> ReAbrirCuestionario(int CuestionarioId)
    {
      var appUser = await this.GetCurrentUserAsync(_userManager);
      if (appUser == null) return Unauthorized();

      var res = await _unitOfWork.ProcesoCertificacion.ReAbrirCuestionario(appUser, CuestionarioId);

      return Ok(res);
    }

    [Authorize(Roles = "Asesor,Auditor")]
    [HttpPost]
    [Route("SaveObservaciones")]
    public async Task<IActionResult> SaveObservaciones(ObservacionesDTO data)
    {
      var appUser = await this.GetCurrentUserAsync(_userManager);
      if (appUser == null) return Unauthorized();

      var res = await _unitOfWork.ProcesoCertificacion.SaveObservaciones(appUser, data);

      return Ok();
    }

    [Authorize]
    [HttpGet]
    [Route("GetObservaciones")]
    public async Task<IActionResult> GetObservaciones(int idRespuesta)
    {
      var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
      if (appUser == null) return Unauthorized();

      var res = await _unitOfWork.ProcesoCertificacion.GetObservaciones(idRespuesta, appUser, role);

      return Ok(res);
    }

    [Authorize(Roles = "Admin,TecnicoPais")]
    [HttpPost]
    [Route("SolicitarAsesoria")]
    public async Task<IActionResult> SolicitarAsesoria(EmpresaUpdateVm data)
    {
      var appUser = await this.GetCurrentUserAsync(_userManager);
      if (appUser == null) return Unauthorized();

      var userFromDb = await _userManager.FindByEmailAsync(appUser.Email);

      return Ok();
    }


    [Authorize]
    [HttpPost]
    [Route("SolicitaAuditoria")]
    public async Task<IActionResult> SolicitaAuditoria(EmpresaUpdateVm data)
    {
      var appUser = await this.GetCurrentUserAsync(_userManager);
      if (appUser == null) return Unauthorized();

      var res = await _unitOfWork.Empresa.SolicitaAuditoria(appUser.EmpresaId ?? 0);

      return Ok();
    }

    [Authorize]
    [HttpPost]
    [Route("SaveCalificacion")]
    public async Task<IActionResult> SaveCalificacion(SaveCalificacionVm data)
    {
      var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
      if (appUser == null) return Unauthorized();

      var res = await _unitOfWork.ProcesoCertificacion.SaveCalificacion(data, appUser, role);

      try
      {
        await _notificationService.SendNotification(data.idProceso, null, appUser.Lenguage);
      }
      catch (Exception)
      {

      }

      return Ok();
    }


    [Authorize(Roles = "Admin,TecnicoPais")]
    [HttpPost]
    [Route("UpdateNumeroExp")]
    public async Task<IActionResult> UpdateNumeroExp(CertificacionDetailsVm data)
    {
      var result = await _unitOfWork.ProcesoCertificacion.UpdateNumeroExp(data);

      return Ok(JsonConvert.SerializeObject(result, Formatting.None,
                  new JsonSerializerSettings()
                  {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                  }));
    }


    [Authorize(Roles = "Admin,TecnicoPais")]
    [HttpPost]
    [Route("CambiarAuditor")]
    public async Task<IActionResult> CambiarAuditor(CambioAuditor data)
    {
      var appUser = await this.GetCurrentUserAsync(_userManager);
      if (appUser == null) return Unauthorized();

      var result = await _unitOfWork.ProcesoCertificacion.CambiarAuditor(data);

      try
      {
        if (data.auditor)
        {
          await _notificationService.SendNotification(data.idProceso,
              (int)NotificationTypes.CambioAuditor, appUser.Lenguage);
        }
        else
        {
          await _notificationService.SendNotification(data.idProceso,
              (int)NotificationTypes.CambioAsesor, appUser.Lenguage);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al enviar notificacion");
      }

      return Ok(JsonConvert.SerializeObject(result, Formatting.None,
                  new JsonSerializerSettings()
                  {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                  }));
    }

    [Authorize(Roles = "Admin,TecnicoPais")]
    [HttpPost]
    [Route("Comenzar")]
    public async Task<IActionResult> Comenzar(CertificacionVm data)
    {
      var appUser = await this.GetCurrentUserAsync(_userManager);
      if (appUser == null) return Unauthorized();

      //recibir empresa, asesor y obtener el usuario que la genera
      var result = await _unitOfWork.ProcesoCertificacion.ComenzarProceso(data, appUser.Id);

      try
      {
        await _notificationService.SendNotification(result, null, appUser.Lenguage);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al enviar notificacion");
      }

      return Ok();
    }

    [Route("Test")]
    [HttpGet]
    public async Task<IActionResult> Test(int id)
    {
      var result = await _notificationService.SendNotification(id, null, "es");
      return Ok();
    }


    [Authorize]
    [HttpPost]
    [Route("ChangeStatus")]
    public async Task<IActionResult> ChangeStatus(CertificacionStatusVm data)
    {
      var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
      if (appUser == null) return Unauthorized();
      //recibir empresa, asesor y obtener el usuario que la genera
      var result = await _unitOfWork.ProcesoCertificacion.ChangeStatus(data, appUser, role);

      return Ok();
    }

    [Authorize(Roles = "Admin,TecnicoPais,Asesor,Auditor")]
    [HttpPost]
    [Route("GenerarCuestionario")]
    public async Task<IActionResult> GenerarCuestionario(CuestionarioCreateVm data)
    {
      var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
      if (appUser == null) return Unauthorized();

      var result = await _unitOfWork.ProcesoCertificacion.GenerarCuestionario(data, appUser.Id, role);

      return Ok(JsonConvert.SerializeObject(result, Formatting.None,
                  new JsonSerializerSettings()
                  {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                  }));

    }

    [Authorize]
    [Route("GetCuestionario")]
    public async Task<IActionResult> GetCuestionario(int id)
    {
      var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
      if (appUser == null) return Unauthorized();

      var result = await _unitOfWork.ProcesoCertificacion.GetCuestionario(id, appUser, role);

      return Ok(JsonConvert.SerializeObject(result, Formatting.None,
                  new JsonSerializerSettings()
                  {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                  }));
    }


    [Authorize]
    [Route("GetHistory")]
    public async Task<IActionResult> GetHistory(int idCuestionario)
    {
      var result = await _unitOfWork.ProcesoCertificacion.GetHistory(idCuestionario);

      return Ok(JsonConvert.SerializeObject(result, Formatting.None,
                  new JsonSerializerSettings()
                  {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                  }));
    }

    [Authorize]
    [Route("GetCuestionarios")]
    public async Task<IActionResult> GetCuestionarios(int idEmpresa, string lang)
    {
      var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
      if (appUser == null) return Unauthorized();

      var result = await _unitOfWork.ProcesoCertificacion.GetCuestionariosList(idEmpresa, appUser, role);

      return Ok(JsonConvert.SerializeObject(result, Formatting.None,
                  new JsonSerializerSettings()
                  {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                  }));
    }

    [Authorize(Roles = "Asesor,Auditor")]
    [Route("SavePregunta")]
    [HttpPost]
    public async Task<IActionResult> SavePregunta(CuestionarioItemVm obj)
    {
      var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);

      var result = await _unitOfWork.ProcesoCertificacion.SavePregunta(obj, appUser, role);

      return Ok(result);

    }

    [Authorize(Roles = "Asesor,Auditor")]
    [Route("FinCuestionario")]
    [HttpPost]
    public async Task<IActionResult> FinCuestionario(CuestionarioDetailsVm data)
    {
      var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
      if (appUser == null) return Unauthorized();

      try
      {
        var completo = await _unitOfWork.ProcesoCertificacion.IsCuestionarioCompleto(data);
        if (!completo)
        {
          return BadRequest();
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al verificar si el cuestionario es completo");
      }


      var result = await _unitOfWork.ProcesoCertificacion.FinCuestionario(data.Id, appUser, role);
      try
      {
        await _notificationService.SendNotification(result, null, appUser.Lenguage);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al enviar notificacion");
      }

      return Ok();
    }

    [Authorize(Roles = "TecnicoPais")]
    [Route("AsignaAuditor")]
    [HttpPost]
    public async Task<IActionResult> AsignaAuditor(AsignaAuditoriaVm data)
    {
      var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
      if (appUser == null) return Unauthorized();

      var result = await _unitOfWork.ProcesoCertificacion.AsignaAuditor(data, appUser, role);

      try
      {
        await _notificationService.SendNotification(result, null, appUser.Lenguage);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al enviar notificacion");
      }

      return Ok(JsonConvert.SerializeObject(result, Formatting.None,
                  new JsonSerializerSettings()
                  {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                  }));
    }
  }
}
