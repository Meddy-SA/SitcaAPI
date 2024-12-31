using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Extensions;
using Utilities.Common;

namespace Sitca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniaAuditoraController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;


        public CompaniaAuditoraController(
            IUnitOfWork unitOfWork,
            IConfiguration config,
            UserManager<ApplicationUser> userManager
        )
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _userManager = userManager;
        }

        [HttpGet("Company")]
        [Authorize]
        public async Task<ActionResult<Result<CompAuditoras>>> GetAuditCompany(int id = 0)
        {
            try
            {
                var res = await _unitOfWork.CompañiasAuditoras.Get(id);

                if (res == null) return NotFound();

                var response = Result<CompAuditoras>.Success(res);

                return this.HandleResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Result<CompAuditoras>.Failure($"An error occurred while retrieving the audit company: {ex.Message}"));
            }
        }

        [HttpGet]
        [Authorize]
        [Route("GetPersonal")]
        public async Task<IActionResult> GetPersonal(int? idPais, int idEmpresa)
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (!User.IsInRole(Constants.Roles.Admin))
            {
                idPais = appUser.PaisId ?? 0;
            }

            var res = await _unitOfWork.Users.GetPersonal(idPais ?? 0, idEmpresa);

            return Ok(JsonConvert.SerializeObject(res, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [HttpGet("Companies")]
        [ProducesResponseType(typeof(Result<List<CompAuditoras>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize]
        public async Task<ActionResult<Result<List<CompAuditoras>>>> GetAuditCompaniesList(int? idPais, bool special = true)
        {
            try
            {
                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null) return BadRequest("User not found");

                if (!User.IsInRole(Constants.Roles.Admin))
                {
                    idPais = appUser.PaisId ?? 0;
                }

                var res = await _unitOfWork.CompañiasAuditoras.GetAll(x => x.PaisId == idPais || idPais == 0, null, "Pais");

                if (!special)
                {
                    //quitar las compañias auditoras especiales -institucional, -independiente
                    res = res.Where(x => !x.Special);
                }

                var response = Result<List<CompAuditoras>>.Success(res.ToList());
                return this.HandleResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Result<List<CompAuditoras>>.Failure($"An error occurred while retrieving the companies list: {ex.Message}"));
            }
        }

        [HttpPost("Save")]
        [Authorize]
        [ProducesResponseType(typeof(Result<CompAuditoras>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Result<CompAuditoras>>> CreateOrUpdateCompany(CompAuditoras data)
        {
            try
            {
                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null) return BadRequest(Result<CompAuditoras>.Failure("User not found"));

                if (!User.IsInRole(Constants.Roles.Admin))
                {
                    data.PaisId = appUser.PaisId ?? 0;
                }

                var res = await _unitOfWork.CompañiasAuditoras.SaveAsync(data);

                return this.HandleResponse(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Result<CompAuditoras>.Failure($"An error occurred while saving the company: {ex.Message}"));
            }
        }
    }
}
