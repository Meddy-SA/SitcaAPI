using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitca.DataAccess.Data.Repository.Constants;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Services.Notification;
using Sitca.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;
using Utilities.Common;

namespace Sitca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpresaController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EmpresaController> _logger;

        public EmpresaController(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IConfiguration config,
            UserManager<ApplicationUser> userManager,
            ILogger<EmpresaController> logger
        )
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _config = config;
            _userManager = userManager;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("Servicio")]
        public async Task<IActionResult> GetEmpresasCertificadas(ListadoExternoFiltro data)
        {
            var res = await _unitOfWork.Empresa.GetCertificadasParaExterior(data);

            return Ok(res);
        }

        [Authorize(Roles = AuthorizationPolicies.Empresa.AdminTecnico)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;
            var role = User.Claims.ToList()[2].Value;

            var res = await _unitOfWork.Empresa.Delete(
                id,
                appUser.PaisId.GetValueOrDefault(),
                role
            );

            if (res.Success)
            {
                var userToDelete = await _unitOfWork.Users.GetAll(s => s.EmpresaId == id);
                try
                {
                    var result = await _userManager.DeleteAsync(userToDelete.FirstOrDefault());
                }
                catch (Exception) { }
            }

            return Ok(
                JsonConvert.SerializeObject(
                    res,
                    Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    }
                )
            );
        }

        [Authorize(Roles = AuthorizationPolicies.Empresa.AdminTecnico)]
        [HttpGet("{idPais}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<EmpresaVm>>))]
        public async Task<ActionResult<List<EmpresaVm>>> Get(int? idPais)
        {
            try
            {
                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null)
                    return Unauthorized();

                var filter = new CompanyFilterDTO();
                if (!User.IsInRole("Admin"))
                {
                    filter.CountryId = appUser.PaisId ?? 0;
                }

                var companies = await _unitOfWork.Empresa.GetCompanyListAsync(
                    filter,
                    appUser.Lenguage
                );
                return this.HandleResponse(companies, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company list");
                return this.HandleResponse<List<EmpresaVm>>(
                    null,
                    false,
                    StatusCodes.Status500InternalServerError
                );
            }
        }

        [Authorize(Roles = AuthorizationPolicies.Empresa.AdmTecCons)]
        [HttpPost("list")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<EmpresaVm>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<EmpresaVm>>> List(CompanyFilterDTO filter)
        {
            try
            {
                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null)
                    return Unauthorized();
                if (!User.IsInRole("Admin"))
                {
                    filter.CountryId = appUser.PaisId ?? 0;
                }

                var companies = await _unitOfWork.Empresa.GetCompanyListAsync(
                    filter,
                    appUser.Lenguage
                );
                return this.HandleResponse(companies, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company list with filter {@Filter}", filter);
                return this.HandleResponse<List<EmpresaVm>>(
                    null,
                    false,
                    StatusCodes.Status500InternalServerError
                );
            }
        }

        [HttpPost("ListForRole")]
        [Authorize]
        [ProducesResponseType(typeof(List<EmpresaVm>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<EmpresaVm>>> ListForRole(CompanyFilterDTO filter)
        {
            try
            {
                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null)
                    return Unauthorized();

                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(role))
                    return BadRequest("Role not found in claims");

                var empresas = await _unitOfWork.Empresa.ListForRoleAsync(appUser, role, filter);
                return this.HandleResponse(empresas, true);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when getting company list");
                return this.HandleResponse<List<EmpresaVm>>(
                    null,
                    false,
                    StatusCodes.Status400BadRequest
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company list");
                return this.HandleResponse<List<EmpresaVm>>(
                    null,
                    false,
                    StatusCodes.Status500InternalServerError
                );
            }
        }

        [Authorize(Roles = AuthorizationPolicies.Empresa.AdminTecnico)]
        [HttpPost]
        [Route("ListReporte")]
        public async Task<IActionResult> ListReporte(FiltroEmpresaReporteVm data)
        {
            try
            {
                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null)
                    return Unauthorized();

                if (!User.IsInRole(Constants.Roles.Admin))
                {
                    data.country = appUser.PaisId ?? 0;
                }

                var res = _unitOfWork.Empresa.GetListReporte(data);

                return Ok(
                    JsonConvert.SerializeObject(
                        res,
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
                _logger.LogError(ex, "Error getting company list with filter {@Filter}", data);
                throw;
            }
        }

        [Authorize(Roles = AuthorizationPolicies.Empresa.AdminTecnico)]
        [HttpPost]
        [Route("GetListRenovacionReporte")]
        public async Task<IActionResult> GetListRenovacionReporte(FiltroEmpresaReporteVm data)
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            if (!User.IsInRole("Admin"))
            {
                data.country = appUser.PaisId ?? 0;
            }

            var res = _unitOfWork.Empresa.GetListRenovacionReporte(data);

            return Ok(
                JsonConvert.SerializeObject(
                    res,
                    Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    }
                )
            );
        }

        [Authorize(Roles = AuthorizationPolicies.Empresa.AdminTecnico)]
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

            return Ok(
                JsonConvert.SerializeObject(
                    res,
                    Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    }
                )
            );
        }

        [HttpPost("ListReportePersonal")]
        [Authorize(Roles = AuthorizationPolicies.Empresa.AdminTecnico)]
        [ProducesResponseType(typeof(Result<List<EmpresaPersonalVm>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<List<EmpresaPersonalVm>>>> ListReportePersonal(
            FiltroEmpresaReporteVm data
        )
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized(
                    Result<List<EmpresaPersonalVm>>.Failure("Usuario no autenticado")
                );

            if (!User.IsInRole(Constants.Roles.Admin))
            {
                data.country = appUser.PaisId ?? 0;
            }
            var res = await _unitOfWork.Empresa.GetListReportePersonalAsync(data);
            return this.HandleResponse(res);
        }

        [Route("EvaluadasEnCtc")]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EvaluadasEnCtc()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            var res = await _unitOfWork.Empresa.EvaluadasEnCtc(
                appUser.PaisId ?? 0,
                appUser.Lenguage
            );
            return Ok(
                JsonConvert.SerializeObject(
                    res,
                    Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    }
                )
            );
        }

        [Route("EnCertificacion")]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EnCertificacion()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            var res = await _unitOfWork.Empresa.EnCertificacion(
                appUser.PaisId ?? 0,
                appUser.Lenguage
            );
            return Ok(
                JsonConvert.SerializeObject(
                    res,
                    Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    }
                )
            );
        }

        [Route("EstadisticasCtc")]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EstadisticasCtc()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            var res = await _unitOfWork.Empresa.EstadisticasCtc(
                appUser.PaisId ?? 0,
                appUser.Lenguage
            );
            return Ok(
                JsonConvert.SerializeObject(
                    res,
                    Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    }
                )
            );
        }

        [Authorize(Roles = AuthorizationPolicies.Empresa.View)]
        [HttpGet("MyCompany/Status")]
        public async Task<ActionResult<int>> MyCompanyStatus()
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);

            if (appUser == null)
                return Unauthorized();

            if (!appUser.EmpresaId.HasValue)
                return Ok(0);

            var res = await _unitOfWork.Empresa.GetCompanyStatusAsync(appUser.EmpresaId.Value);

            return Ok(res);
        }

        [Authorize(Roles = AuthorizationPolicies.Empresa.View)]
        [HttpGet("MiEmpresa")]
        public async Task<IActionResult> MiEmpresa()
        {
            var appUser = await this.GetCurrentUserAsync(_userManager);

            int EmpresaId = appUser.EmpresaId ?? 0;
            var res = await _unitOfWork.Empresa.Data(EmpresaId, appUser);

            return Ok(
                JsonConvert.SerializeObject(
                    res,
                    Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    }
                )
            );
        }

        [Authorize(Roles = AuthorizationPolicies.Empresa.Details)]
        [HttpGet("Details/{id}")]
        public async Task<ActionResult<EmpresaUpdateVm>> Details(int Id)
        {
            const int toStatus = 7;
            var role = User.Claims.ToList()[2].Value;

            var appUser = await this.GetCurrentUserAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var res = await _unitOfWork.Empresa.Data(Id, appUser);

            if (User.IsInRole("CTC") && res.CertificacionActual != null && res.Estado < toStatus)
            {
                //cambiar estado en certificacion
                var status = new CertificacionStatusVm
                {
                    CertificacionId = res.CertificacionActual.Id,
                    Status = StatusConstants.GetLocalizedStatus(toStatus, "es"),
                };
                await _unitOfWork.ProcesoCertificacion.ChangeStatus(status, toStatus);

                res.Estado = toStatus;
                res.CertificacionActual.Status = StatusConstants.GetLocalizedStatus(
                    toStatus,
                    appUser.Lenguage
                );
            }

            return this.HandleResponse(res, true);
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

        [Authorize]
        [HttpPost("ActualizarDatos")]
        public async Task<ActionResult<bool>> ActualizarDatos([FromBody] EmpresaUpdateVm datos)
        {
            var (appUser, role) = await this.GetCurrentUserWithRoleAsync(_userManager);
            if (appUser == null)
                return Unauthorized();

            var res = await _unitOfWork.Empresa.ActualizarDatos(datos, appUser, role);
            try
            {
                if (role == Constants.Roles.Empresa)
                {
                    await _notificationService.SendNotificacionSpecial(
                        datos.Id,
                        NotificationTypes.NuevaEmpresa,
                        appUser.Lenguage
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al enviar notificación Actualizando datos de la empresa"
                );
            }

            return this.HandleResponse(res, true);
        }
    }
}
