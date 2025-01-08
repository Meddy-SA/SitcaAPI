using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Models;
using Utilities.Common;

namespace Sitca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CapacitacionesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public CapacitacionesController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager
        )
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [HttpGet("list")]
        [Authorize]
        public async Task<IActionResult> List()
        {
            try
            {
                var result = await _unitOfWork.Capacitaciones.GetAll();

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
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [HttpPost("DeleteFile")]
        [Authorize(Roles = AuthorizationPolicies.Capacitaciones.DeleteFiles)]
        public async Task<IActionResult> DeleteFile(Capacitaciones data)
        {
            try
            {
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
