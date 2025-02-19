using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;
using Roles = Utilities.Common.Constants.Roles;

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

                if (res == null)
                    return NotFound();

                var response = Result<CompAuditoras>.Success(res);

                return this.HandleResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    Result<CompAuditoras>.Failure(
                        $"An error occurred while retrieving the audit company: {ex.Message}"
                    )
                );
            }
        }

        [HttpGet("available/{id}")]
        [Authorize]
        public async Task<ActionResult<Result<List<UsersListVm>>>> GetUsersByCompany(int id)
        {
            var res = await _unitOfWork.Users.GetPersonalAsync(0, id);
            return this.HandleResponse(res);
        }

        [HttpGet("get-companies")]
        [Authorize]
        public async Task<ActionResult<Result<List<CompAuditoraListVm>>>> GetListCompanyByCountry(
            int idPais
        )
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (!User.IsInRole(Roles.Admin))
            {
                idPais = appUser.PaisId ?? 0;
            }

            var res = await _unitOfWork.Users.GetCompaniesAsync(idPais);

            return this.HandleResponse(res);
        }

        [HttpGet("Companies")]
        [ProducesResponseType(typeof(Result<List<CompAuditoras>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize]
        public async Task<ActionResult<Result<List<CompAuditoras>>>> GetAuditCompaniesList(
            int? idPais,
            bool special = true
        )
        {
            try
            {
                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null)
                    return BadRequest("User not found");

                if (!User.IsInRole(Roles.Admin))
                {
                    idPais = appUser.PaisId ?? 0;
                }

                var res = await _unitOfWork.CompañiasAuditoras.GetAll(
                    x => x.PaisId == idPais || idPais == 0,
                    null,
                    "Pais"
                );

                if (!special)
                {
                    //quitar las compañias auditoras especiales -institucional, -independiente
                    res = res.Where(x => !x.Special);
                }

                var response = Result<List<CompAuditoras>>.Success([.. res]);
                return this.HandleResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    Result<List<CompAuditoras>>.Failure(
                        $"An error occurred while retrieving the companies list: {ex.Message}"
                    )
                );
            }
        }

        [HttpPost("Save")]
        [Authorize]
        [ProducesResponseType(typeof(Result<CompAuditoras>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Result<CompAuditoras>>> CreateOrUpdateCompany(
            CompAuditoras data
        )
        {
            try
            {
                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null)
                    return BadRequest(Result<CompAuditoras>.Failure("User not found"));

                if (!User.IsInRole(Roles.Admin))
                {
                    data.PaisId = appUser.PaisId ?? 0;
                }

                var res = await _unitOfWork.CompañiasAuditoras.SaveAsync(data);

                return this.HandleResponse(res);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    Result<CompAuditoras>.Failure(
                        $"An error occurred while saving the company: {ex.Message}"
                    )
                );
            }
        }

        [HttpGet("usuarios/pais/{id}")]
        [Authorize]
        public async Task<ActionResult<Result<List<UsersListVm>>>> GetUsersByCountry(int id)
        {
            var res = await _unitOfWork.Users.GetPersonalAsync(id, 0);
            return this.HandleResponse(res);
        }

        [HttpPost("{id}/usuarios")]
        [Authorize]
        public async Task<ActionResult<Result<CompAuditoras>>> AddUserToCompany(
            int id,
            [FromBody] string[] users
        )
        {
            try
            {
                var res = await _unitOfWork.Users.AssignUserToCompanyAsync(id, users);
                return this.HandleResponse(res);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    Result<bool>.Failure(
                        $"An error occurred while adding users to company: {ex.Message}"
                    )
                );
            }
        }

        [HttpDelete("{id}/usuarios/{userId}")]
        [Authorize]
        public async Task<ActionResult<Result<bool>>> AddUserToCompany(int id, string userId)
        {
            try
            {
                var res = await _unitOfWork.Users.UnassignUsersFromCompanyAsync(id, [userId]);
                return this.HandleResponse(res);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    Result<bool>.Failure(
                        $"An error occurred while adding users to company: {ex.Message}"
                    )
                );
            }
        }
    }
}
