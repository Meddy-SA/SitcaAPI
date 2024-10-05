using Core.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Models;
using Sitca.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sitca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificacionController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public CertificacionController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [Authorize(Roles = "Admin,TecnicoPais")]
        [HttpPost]
        [Route("ConvertirARecertificacion")]
        public async Task<IActionResult> ConvertirARecertificacion(EmpresaVm data)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            var res = await _unitOfWork.ProcesoCertificacion.ConvertirARecertificacion(appUser, data);

            return Ok(res);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("ReAbrirCuestionario")]
        public async Task<IActionResult> ReAbrirCuestionario(int CuestionarioId)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            var res = await _unitOfWork.ProcesoCertificacion.ReAbrirCuestionario(appUser, CuestionarioId);

            return Ok(res);
        }

        [Authorize(Roles = "Asesor,Auditor")]
        [HttpPost]
        [Route("SaveObservaciones")]
        public async Task<IActionResult> SaveObservaciones(ObservacionesDTO data)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            var res = await _unitOfWork.ProcesoCertificacion.SaveObservaciones(appUser,data);

            return Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("GetObservaciones")]
        public async Task<IActionResult> GetObservaciones(int idRespuesta)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;
            var role = User.Claims.ToList()[2].Value;
            var res = await _unitOfWork.ProcesoCertificacion.GetObservaciones( idRespuesta, appUser, role);

            return Ok(res);
        }

        [Authorize(Roles = "Admin,TecnicoPais")]
        [HttpPost]
        [Route("SolicitarAsesoria")]
        public async Task<IActionResult> SolicitarAsesoria(EmpresaUpdateVm data)
        {            
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);

            return Ok();
        }


        [Authorize]
        [HttpPost]
        [Route("SolicitaAuditoria")]
        public async Task<IActionResult> SolicitaAuditoria(EmpresaUpdateVm data)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            var res = await _unitOfWork.Empresa.SolicitaAuditoria(appUser.EmpresaId ?? 0);

            return Ok();
        }

        [Authorize]
        [HttpPost]
        [Route("SaveCalificacion")]
        public async Task<IActionResult> SaveCalificacion(SaveCalificacionVm data)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var userFromDb = await _userManager.FindByEmailAsync(user);

            var appUser = (ApplicationUser)userFromDb;
            var role = User.Claims.ToList()[2].Value;

            var res = await _unitOfWork.ProcesoCertificacion.SaveCalificacion(data, appUser, role);

            try
            {
                await _unitOfWork.Notificacion.SendNotification(data.idProceso, null, appUser.Lenguage);
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
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var userFromDb = await _userManager.FindByEmailAsync(user);

            var appUser = (ApplicationUser)userFromDb;
       
            var result = await _unitOfWork.ProcesoCertificacion.CambiarAuditor(data);

            try
            {
                if (data.auditor)
                {
                    await _unitOfWork.Notificacion.SendNotification(data.idProceso, -5, appUser.Lenguage);
                }
                else
                {
                    await _unitOfWork.Notificacion.SendNotification(data.idProceso, -4, appUser.Lenguage);
                }
            }
            catch (Exception)
            {

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
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            var appUser = (ApplicationUser)userFromDb;

            //recibir empresa, asesor y obtener el usuario que la genera
            var result = await _unitOfWork.ProcesoCertificacion.ComenzarProceso(data, userFromDb.Id);

            try
            {                
                await _unitOfWork.Notificacion.SendNotification(result,null, appUser.Lenguage);
            }
            catch (Exception)
            {

                
            }
            
            return Ok();

            //if (result > 0)
            //{                
            //    //notificar asesor
            //    var asesor = await _userManager.FindByIdAsync(data.Asesor);
            //    ApplicationUser appUser = (ApplicationUser)asesor;
            //    var senderEmail = "fedehit_2@hotmail.com";
            //    var texto = "Hola " + appUser.FirstName + " " + appUser.LastName + "<br/>" + "Se le ha asignado una nueva empresa, ingrese a la plataforma para conocer mas información";
            //    await _emailSender.SendEmailAsync(senderEmail, appUser.Email,"Se le ha asignado una nueva Empresa para asesorar", texto);
            //}
            
            //return Ok();
        }

        [Route("Test")]
        [HttpGet]
        public async Task<IActionResult> Test(int id)
        {
            var result = await _unitOfWork.Notificacion.SendNotification(id,null,"es");
            return Ok();
        }


        [Authorize]
        [HttpPost]
        [Route("ChangeStatus")]
        public async Task<IActionResult> ChangeStatus(CertificacionStatusVm data)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = (ApplicationUser)await _userManager.FindByEmailAsync(user);
            var role = User.Claims.ToList()[2].Value;
            //recibir empresa, asesor y obtener el usuario que la genera
            var result = await _unitOfWork.ProcesoCertificacion.ChangeStatus(data, userFromDb, role);

            return Ok();
        }

        [Authorize(Roles = "Admin,TecnicoPais,Asesor,Auditor")]
        [HttpPost]
        [Route("GenerarCuestionario")]
        public async Task<IActionResult> GenerarCuestionario(CuestionarioCreateVm data)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            var role = User.Claims.ToList()[2].Value;

            var result = await _unitOfWork.ProcesoCertificacion.GenerarCuestionario(data, userFromDb.Id, role);

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
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var userFromDb = await _userManager.FindByEmailAsync(user);
            var appUser = (ApplicationUser)userFromDb;
            var role = User.Claims.ToList()[2].Value;

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
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var userFromDb = await _userManager.FindByEmailAsync(user);
            var appUser = (ApplicationUser)userFromDb;
            var role = User.Claims.ToList()[2].Value;

            var result = await _unitOfWork.ProcesoCertificacion.GetCuestionariosList(idEmpresa, appUser, role);

            return Ok(JsonConvert.SerializeObject(result, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [Authorize (Roles = "Asesor,Auditor")]
        [Route("SavePregunta")]
        [HttpPost]
        public async Task<IActionResult> SavePregunta(CuestionarioItemVm obj)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var userFromDb = await _userManager.FindByEmailAsync(user);

            var appUser = (ApplicationUser)userFromDb;
            var role = User.Claims.ToList()[2].Value;

            var result = await _unitOfWork.ProcesoCertificacion.SavePregunta( obj, appUser, role);

            return Ok(result);

        }

        [Authorize(Roles = "Asesor,Auditor")]
        [Route("FinCuestionario")]
        [HttpPost]
        public async Task<IActionResult> FinCuestionario(CuestionarioDetailsVm data)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var userFromDb = await _userManager.FindByEmailAsync(user);

            var appUser = (ApplicationUser)userFromDb;
            var role = User.Claims.ToList()[2].Value;

            try
            {
                var completo = await _unitOfWork.ProcesoCertificacion.IsCuestionarioCompleto(data);
                if (!completo)
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                
            }
            

            var result = await _unitOfWork.ProcesoCertificacion.FinCuestionario(data.Id, appUser, role);
            try
            {
                await _unitOfWork.Notificacion.SendNotification(result, null, appUser.Lenguage);
            }
            catch (Exception)
            { }

            return Ok();
        }

        [Authorize(Roles = "TecnicoPais")]
        [Route("AsignaAuditor")]
        [HttpPost]
        public async Task<IActionResult> AsignaAuditor(AsignaAuditoriaVm data)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var userFromDb = await _userManager.FindByEmailAsync(user);
            var appUser = (ApplicationUser)userFromDb;
            var role = User.Claims.ToList()[2].Value;
            //var roles = await _userManager.GetRolesAsync(userFromDb);

            var result = await _unitOfWork.ProcesoCertificacion.AsignaAuditor(data, appUser, role);

            try
            {
                await _unitOfWork.Notificacion.SendNotification(result, null, appUser.Lenguage);
            }
            catch (Exception)
            {


            }

            return Ok(JsonConvert.SerializeObject(result, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }


    }
}