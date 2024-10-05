using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Models;
using Sitca.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Sitca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniaAuditoraController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly UserManager<IdentityUser> _userManager;


        public CompaniaAuditoraController(
            IUnitOfWork unitOfWork,
            IConfiguration config,
            UserManager<IdentityUser> userManager
        )
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        [Route("Get")]
        public async Task<IActionResult> Get(int? idEmpresa)
        {
            var res = _unitOfWork.CompañiasAuditoras.Get(idEmpresa??0);            

            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [HttpGet]
        [Authorize]
        [Route("GetPersonal")]
        public async Task<IActionResult> GetPersonal(int? idPais, int idEmpresa)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);            
            ApplicationUser appUser = (ApplicationUser)userFromDb;            

            if (!User.IsInRole("Admin"))
            {
                idPais = appUser.PaisId ?? 0;
            }

            var res = await _unitOfWork.Users.GetPersonal(idPais??0, idEmpresa);

            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [HttpGet]
        [Authorize]
        [Route("List")]
        public async Task<IActionResult> List(int? idPais, bool special = true)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            //var roles = await _userManager.GetRolesAsync(userFromDb);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            if (!User.IsInRole("Admin"))
            {
                idPais = appUser.PaisId ?? 0;
            }

            var res = _unitOfWork.CompañiasAuditoras.GetAll(x => x.PaisId == idPais || idPais == 0, null, "Pais");

            if (!special)
            {
                //quitar las compañias auditoras especiales -institucional, -independiente
                res = res.Where(x => !x.Special);
            }

            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [HttpPost]
        [Authorize]
        [Route("Save")]
        public async Task<IActionResult> Save(CompAuditoras data)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            //var roles = await _userManager.GetRolesAsync(userFromDb);
            ApplicationUser appUser = (ApplicationUser)userFromDb;            

            if (!User.IsInRole("Admin"))
            {
                data.PaisId = appUser.PaisId ?? 0;
            }

            var res = await _unitOfWork.CompañiasAuditoras.Save(data);
        

            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }
    }
}