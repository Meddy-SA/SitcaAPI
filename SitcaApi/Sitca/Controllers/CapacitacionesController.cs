using Core.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sitca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CapacitacionesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        
        public CapacitacionesController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> List()
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                var userFromDb = await _userManager.FindByEmailAsync(user);
                ApplicationUser appUser = (ApplicationUser)userFromDb;          

                var result = _unitOfWork.Capacitaciones.GetAll();

                return Ok(JsonConvert.SerializeObject(result, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [Authorize(Roles = "Admin,TecnicoPais")]
        [HttpPost]
        [Route("DeleteFile")]
        public async Task<IActionResult> DeleteFile(Capacitaciones data)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                var userFromDb = await _userManager.FindByEmailAsync(user);
                ApplicationUser appUser = (ApplicationUser)userFromDb;        

                var result = await _unitOfWork.Capacitaciones.DeleteFile(data.Id);
        
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        

    }
}
