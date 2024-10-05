using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    public class EmpresaController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly UserManager<IdentityUser> _userManager;

        public EmpresaController(
            IUnitOfWork unitOfWork,
            IConfiguration config,
            UserManager<IdentityUser> userManager
        )
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost("Servicio")]
        public async Task<IActionResult> GetEmpresasCertificadas(ListadoExternoFiltro data)
        {
            var res = await _unitOfWork.Empresa.GetCertificadasParaExterior(data);
           
            return Ok(res);
        }

        [Authorize(Roles = "Admin,TecnicoPais")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var userFromDb = await _userManager.FindByEmailAsync(user);            
            ApplicationUser appUser = (ApplicationUser)userFromDb;
            var role = User.Claims.ToList()[2].Value;

            var res = await _unitOfWork.Empresa.Delete(id, appUser.PaisId.GetValueOrDefault(), role);

            if (res.Success)
            {
                var userToDelete = _unitOfWork.Users.GetAll(s => s.EmpresaId == id).First();
                try
                {
                    var result = await _userManager.DeleteAsync(userToDelete);
                }
                catch (Exception)
                {                   
                 
                }
                
            }

            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [HttpGet]
        [Authorize (Roles = "Admin,TecnicoPais")]
        [HttpGet("{idPais}")]
        public async Task<IActionResult> Get(int? idPais)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            //var roles = await _userManager.GetRolesAsync(userFromDb);
            ApplicationUser appUser = (ApplicationUser)userFromDb;           

            if (!User.IsInRole("Admin"))
            {
                idPais = appUser.PaisId ?? 0;
            }

            var res = _unitOfWork.Empresa.GetList(null,idPais??0, 0, null, appUser.Lenguage);

            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        public class FiltroVm
        {
            public string Nombre { get; set; }
            public int? country { get; set; }
            public int? tipologia { get; set; }
            public int? estado { get; set; }
        }

        
        [Authorize(Roles = "Admin,TecnicoPais")]
        [HttpPost("{List}")]

        public async Task<IActionResult> List(FiltroVm data)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);         
            ApplicationUser appUser = (ApplicationUser)userFromDb;            
    
            if (!User.IsInRole("Admin"))
            {
                data.country = appUser.PaisId ?? 0;
            }

            var res = _unitOfWork.Empresa.GetList(data.estado,data.country??0, data.tipologia??0,data.Nombre, appUser.Lenguage );

            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [Authorize(Roles = "Admin,TecnicoPais")]
        [HttpPost]
        [Route("ListReporte")]
        public async Task<IActionResult> ListReporte(FiltroEmpresaReporteVm data)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            
            ApplicationUser appUser = (ApplicationUser)userFromDb;
            //var role = User.Claims.ToList()[2].Value;

            if (!User.IsInRole("Admin"))
            {
                data.country = appUser.PaisId ?? 0;
            }

            var res = _unitOfWork.Empresa.GetListReporte(data);

            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [Authorize(Roles = "Admin,TecnicoPais")]
        [HttpPost]
        [Route("GetListRenovacionReporte")]
        public async Task<IActionResult> GetListRenovacionReporte(FiltroEmpresaReporteVm data)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);

            ApplicationUser appUser = (ApplicationUser)userFromDb;            

            if (!User.IsInRole("Admin"))
            {
                data.country = appUser.PaisId ?? 0;
            }

            var res = _unitOfWork.Empresa.GetListRenovacionReporte(data);

            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [Authorize(Roles = "Admin,TecnicoPais")]
        [HttpPost]
        [Route("ListXVencer")]
        public async Task<IActionResult> ListXVencer(FiltroEmpresaReporteVm data)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);

            ApplicationUser appUser = (ApplicationUser)userFromDb;            

            if (!User.IsInRole("Admin"))
            {
                data.country = appUser.PaisId ?? 0;
            }

            var res = _unitOfWork.Empresa.GetListXVencerReporte(data);

            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [Authorize(Roles = "Admin,TecnicoPais")]
        [HttpPost]
        [Route("ListReportePersonal")]
        public async Task<IActionResult> ListReportePersonal(FiltroEmpresaReporteVm data)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);

            ApplicationUser appUser = (ApplicationUser)userFromDb;            

            if (!User.IsInRole("Admin"))
            {
                data.country = appUser.PaisId ?? 0;
            }

            var res = _unitOfWork.Empresa.GetListReportePersonal(data);

            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }


        [HttpGet]
        [Authorize]
        [Route("ListForRole")]
        public async Task<IActionResult> ListForRole()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            
            ApplicationUser appUser = (ApplicationUser)userFromDb;
            var role = User.Claims.ToList()[2].Value;

            var res = await _unitOfWork.Empresa.ListForRole(appUser, role);

            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }


        [Route("EvaluadasEnCtc")]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EvaluadasEnCtc()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            var res = await _unitOfWork.Empresa.EvaluadasEnCtc(appUser.PaisId ?? 0, appUser.Lenguage);
            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [Route("EnCertificacion")]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EnCertificacion()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            var res = await _unitOfWork.Empresa.EnCertificacion(appUser.PaisId??0, appUser.Lenguage);
            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [Route("EstadisticasCtc")]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EstadisticasCtc()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            var res = await _unitOfWork.Empresa.EstadisticasCtc(appUser.PaisId ?? 0, appUser.Lenguage);
            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [Route("MiEmpresa")]
        [Authorize(Roles = "Empresa")]
        [HttpGet]
        public async Task<IActionResult> MiEmpresa()
        {

            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;
            
            var res = await _unitOfWork.Empresa.Data(appUser.EmpresaId??0,userFromDb.Id);

            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [Route("Details")]
        [Authorize(Roles = "TecnicoPais, Admin, Asesor, Auditor,CTC")]
        [HttpGet]
        public async Task<IActionResult> Details(int Id)
        {
            var role = User.Claims.ToList()[2].Value;
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            var res = await _unitOfWork.Empresa.Data(Id, userFromDb.Id);

            if (User.IsInRole("CTC") && res.CertificacionActual != null && res.Estado < 7)
            {
                //cambiar estado en certificacion
                var status = new CertificacionStatusVm
                {
                    CertificacionId = res.CertificacionActual.Id,
                    Status = "7 - En revisión de CTC"
                };
                await _unitOfWork.ProcesoCertificacion.ChangeStatus(status, appUser, role);

                res.Estado = 7;
                res.CertificacionActual.Status = Utilities.Utilities.CambiarIdiomaEstado("7 - En revisión de CTC", appUser.Lenguage);
            }

            //var test = await _unitOfWork.ProcesoCertificacion.SaveResultadoSugerido(35, appUser, role);

            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [Route("Estadisticas")]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Estadisticas()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            var res = _unitOfWork.Empresa.Estadisticas(appUser.Lenguage);
            return Ok(JsonConvert.SerializeObject(res));

        }

        [Route("ActualizarDatos")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ActualizarDatos([FromBody] EmpresaUpdateVm datos)
        {            
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            var res = _unitOfWork.Empresa.ActualizarDatos(datos, user, role);

            try
            {
                if (role == "Empresa")
                {
                    await _unitOfWork.Notificacion.SendNotificacionSpecial(datos.Id, -1,appUser.Lenguage);
                }                
            }
            catch (Exception)
            {

            }

            return Ok(JsonConvert.SerializeObject(res));
        }

    }
}
