using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Services.Token;
using Sitca.DataAccess.Services.ViewToString;
using Sitca.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;
using Utilities.Common;

namespace Sitca.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJWTTokenGenerator _jwtToken;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDapper _dapper;
        private readonly ILogger<AuthController> _logger;
        private readonly IViewRenderService _viewRenderService;

        public AuthController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJWTTokenGenerator jwtToken,
            RoleManager<ApplicationRole> roleManager,
            IConfiguration config,
            IViewRenderService viewRenderService,
            IEmailSender emailSender,
            IDapper dapper,
            ILogger<AuthController> logger
        )
        {
            _unitOfWork = unitOfWork;
            _jwtToken = jwtToken;
            _roleManager = roleManager;
            _config = config;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _userManager = userManager;
            _viewRenderService = viewRenderService;
            _dapper = dapper;
            _logger = logger;
        }

        public class ChangePass
        {
            public string currentPassword { get; set; }
            public string newPassword { get; set; }
            public string confirmPassword { get; set; }
        }

        [Authorize]
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePass data)
        {
            if (data.newPassword != data.confirmPassword)
            {
                return BadRequest();
            }

            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var userFromDb = await _userManager.FindByEmailAsync(user);

            var res = await _userManager.ChangePasswordAsync(
                userFromDb,
                data.currentPassword,
                data.newPassword
            );

            if (res.Succeeded)
            {
                return Ok(JsonConvert.SerializeObject("OK"));
            }

            return Ok(JsonConvert.SerializeObject("Contraseña actual incorrecta"));
        }

        public class ResetModel
        {
            public string code { get; set; }
            public string id { get; set; }
            public string password { get; set; }
            public string passwordConfirm { get; set; }
        }

        [HttpPost]
        [Route("ResetPassAdmin")]
        [Authorize(Roles = "Admin,TecnicoPais")]
        public async Task<ActionResult> ResetPassAdmin(ResetModel data)
        {
            var user = await _userManager.FindByIdAsync(data.id);

            if (user == null)
            {
                // Don't reveal that the user does not exist
                return BadRequest();
            }

            data.code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, data.code, "123456");

            if (result.Succeeded)
            {
                return Ok(result);
            }

            return BadRequest(result.Errors);
        }

        [HttpPost]
        [Route("ResetPass")]
        public async Task<ActionResult> ResetPass(ResetModel data)
        {
            var user = await _userManager.FindByIdAsync(data.id);

            if (user == null)
            {
                // Don't reveal that the user does not exist
                return BadRequest();
            }

            if (data.code != "uIpyvT7EJdxeppQ5AbaSto8FPxAoFvHiet8gjvKJWCLQH")
            {
                return BadRequest();
            }

            data.code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, data.code, data.password);

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        [HttpGet("reset-pass-key/{email}")]
        public async Task<ActionResult<Result<string>>> ResetPassKey(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(Result<string>.Failure("Email no válido"));
                }

                var userFromDb = await _userManager.FindByEmailAsync(email);
                if (userFromDb == null)
                {
                    return NotFound(Result<string>.Failure("Email no válido"));
                }
                ApplicationUser appUser = (ApplicationUser)userFromDb;

                var code = await _userManager.GeneratePasswordResetTokenAsync(userFromDb);
                var resetPassUrl = GeneratePasswordResetLink(appUser.Id, code);

                var emailModel = new LoginMailVm
                {
                    Url = resetPassUrl,
                    UserData = new RegisterVm
                    {
                        email = email,
                        empresa = email,
                        language = appUser.Lenguage,
                    },
                };

                var viewName =
                    appUser.Lenguage == "es" ? "ResetPassEMail" : "ResetPassEMailEnglish";

                var emailContent = await _viewRenderService.RenderToStringAsync(
                    viewName,
                    emailModel
                );
                var subject =
                    appUser.Lenguage == "es"
                        ? "Solicitud de recuperación de cuenta"
                        : "Account recovery request";
                try
                {
                    await _emailSender.SendEmailBrevoAsync(email, subject, emailContent);
                }
                catch (Exception exSendEmailBrevoAsync)
                {
                    _logger.LogError(
                        exSendEmailBrevoAsync,
                        "Error al enviar email para resetear la contraseña"
                    );
                }

                return Ok(
                    Result<string>.Success("Se ha enviado un correo para restablecer la contraseña")
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email para resetear la contraseña");
                return StatusCode(
                    500,
                    Result<string>.Failure("Error al enviar email para resetear la contraseña")
                );
            }
        }

        [Route("CheckEmailDisponible")]
        public async Task<IActionResult> CheckEmailDisponible(string email, string userId)
        {
            var res = await _userManager.FindByEmailAsync(email);

            if (res == null)
            {
                //no existe el email, retornar disponible
                return Ok(JsonConvert.SerializeObject(true));
            }

            //continua el mail existe en el sistema
            if (!User.Claims.Any())
            {
                //se esta registrando, mail ocupado
                return Ok(JsonConvert.SerializeObject(false));
            }

            //esta editando usuario

            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var userFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            var editUser = userFromDb;
            if (
                User.IsInRole("Admin")
                || User.IsInRole("TecnicoPais") && !string.IsNullOrEmpty(userId)
            )
            {
                //puede editar otro usuario, utilizar parametro userId
                if (userId != null)
                {
                    editUser = await _userManager.FindByIdAsync(userId);
                }
            }

            if (editUser.Email == email)
            {
                //es su propio email
                return Ok(JsonConvert.SerializeObject(true));
            }
            // es el email de otro user, ya ocupado
            return Ok(JsonConvert.SerializeObject(false));
        }

        public class RolesVm
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string NormalizedName { get; set; }
            public string ConcurrencyStamp { get; set; }
        }

        [Authorize]
        [HttpGet("GetRoles")]
        public async Task<ActionResult<IEnumerable<string>>> GetRoles()
        {
            try
            {
                var currentUser = await this.GetCurrentUserAsync(_userManager);
                if (currentUser == null)
                    return Unauthorized();

                var roles = await GetFilteredRolesForUserAsync(currentUser);
                var translatedRoles = TranslateAndSortRoles(roles.ToList(), currentUser.Lenguage);

                return this.HandleResponse(translatedRoles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                return StatusCode(500, "Error retrieving roles");
            }
        }

        private async Task<IEnumerable<string>> GetFilteredRolesForUserAsync(ApplicationUser user)
        {
            var excludedRoles = new HashSet<string> { Constants.Roles.Empresa };

            if (User.IsInRole(Constants.Roles.TecnicoPais))
            {
                excludedRoles.Add(Constants.Roles.Admin);
            }

            var roles = await _roleManager
                .Roles.Select(r => r.Name)
                .Where(r => !excludedRoles.Contains(r))
                .ToListAsync();

            return roles;
        }

        private static IEnumerable<string> TranslateAndSortRoles(
            List<string> roles,
            string language
        )
        {
            return LocalizationUtilities.TranslateRoles(roles, language).OrderBy(r => r);
        }

        [Authorize(Roles = "Empresa")]
        [HttpGet("ListTecnicos")]
        public async Task<ActionResult<Result<List<UsersListVm>>>> ListTecnicos()
        {
            try
            {
                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null)
                    return Unauthorized();

                string q = null;

                var result = await _unitOfWork.Users.GetUsersAsync(
                    q,
                    appUser.PaisId ?? 0,
                    Constants.Roles.TecnicoPais
                );

                return this.HandleResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Technicians");
                return StatusCode(500, "Error getting Technicians");
            }
        }

        [Authorize]
        [HttpGet("GetUsers")]
        public async Task<ActionResult<IEnumerable<UsersListVm>>> GetUsers(
            [FromQuery] GetUsersRequest request
        )
        {
            try
            {
                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null)
                    return Unauthorized();

                var paisId = User.IsInRole(Constants.Roles.Admin)
                    ? ParsePaisId(request.PaisId)
                    : appUser.PaisId ?? 0;

                var searchQuery = string.Equals(
                    request.Query,
                    "undefined",
                    StringComparison.OrdinalIgnoreCase
                )
                    ? null
                    : request.Query?.Trim();

                var users = await _unitOfWork.Users.GetUsersAsync(
                    searchQuery,
                    paisId,
                    request.RoleName ?? "All"
                );

                var userList = TranslateUserRoles(users.Value, appUser.Lenguage);
                return this.HandleResponse(userList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with parameters: {@Request}", request);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResult>> Login(LoginDTO model)
        {
            try
            {
                var result = await _unitOfWork.Auth.LoginAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt for user {Email}", model.Email);
                return StatusCode(
                    500,
                    new
                    {
                        error = model.Language == "en"
                            ? "An error occurred during login"
                            : "Ocurrió un error durante el inicio de sesión",
                    }
                );
            }
        }

        [Authorize]
        [HttpPost("renewToken")]
        public async Task<IActionResult> RenewToken()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var result = await _unitOfWork.Auth.RenewTokenAsync(userId);

                if (!result.Succeeded)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error renewing token for user {UserId}",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                );
                return StatusCode(
                    500,
                    new { error = "An error occurred while renewing the token" }
                );
            }
        }

        [HttpGet("GetNewToken")]
        [Authorize]
        public async Task<IActionResult> GetNewToken(string role)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var userFromDb = await _userManager.FindByEmailAsync(user);
            var roles = await _userManager.GetRolesAsync(userFromDb);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            IList<Claim> claims = await _userManager.GetClaimsAsync(userFromDb);

            var listRoles = new List<string> { role };

            var roleText = Utilities.Utilities.RoleToText(listRoles, appUser.Lenguage);

            if (roles.Any(s => s == role))
            {
                return Ok(
                    new
                    {
                        username = userFromDb.UserName,
                        email = userFromDb.Email,
                        roleText = roleText,
                        country = Utilities.Utilities.GetCountry(appUser.PaisId ?? 0),
                        token = _jwtToken.GenerateToken(userFromDb, listRoles, claims),
                    }
                );
            }

            return BadRequest();
        }

        [HttpGet("SetLanguage")]
        [Authorize]
        public async Task<IActionResult> SetLanguage(string lang)
        {
            try
            {
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                if (string.IsNullOrEmpty(email))
                    return BadRequest("Usuario no encontrado");

                var currentUser = await _userManager.FindByEmailAsync(email);
                if (currentUser == null)
                    return NotFound("Usuario no encontrado");

                // Actualizar el lenguaje en la base de datos
                var result = await _unitOfWork.Users.SetLanguageAsync(lang, email);

                // Actualizar el claim si existe
                var existingClaim = await _userManager.GetClaimsAsync(currentUser);
                var languageClaim = existingClaim.FirstOrDefault(c => c.Type == "Language");

                if (languageClaim != null)
                {
                    await _userManager.ReplaceClaimAsync(
                        currentUser,
                        languageClaim,
                        new Claim("Language", lang)
                    );
                }
                else
                {
                    await _userManager.AddClaimAsync(currentUser, new Claim("Language", lang));
                }
                return Ok(JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new { message = "Error al actualizar el idioma", error = ex.Message }
                );
            }
        }

        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="model">Registration data transfer object</param>
        /// <returns>ActionResult with registration result</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<string>),
            StatusCodes.Status500InternalServerError
        )]
        public async Task<ActionResult<ApiResponse<AuthResult>>> Register(RegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new ApiResponse<AuthResult>
                    {
                        Success = false,
                        Message = ModelState.GetErrorMessagesLines(),
                    }
                );
            }
            try
            {
                var result = await _unitOfWork.Auth.RegisterAsync(model);

                return result.Succeeded
                    ? Ok(new ApiResponse<AuthResult> { Data = result, Success = true })
                    : BadRequest(
                        new ApiResponse<AuthResult>
                        {
                            Success = false,
                            Message = string.Join(", ", result.Errors),
                        }
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error during registration for user {Email}. Error: {Error}",
                    model.Email,
                    ex.Message
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>
                    {
                        Success = false,
                        Message =
                            model.Language == "en"
                                ? "An error occurred during registration. Please try again later."
                                : "Ocurrió un error durante el registro. Por favor intente más tarde.",
                    }
                );
            }
        }

        /// <summary>
        /// Obtiene la información detallada de un usuario por su ID
        /// </summary>
        /// <param name="id">ID del usuario a consultar</param>
        /// <returns>Información detallada del usuario</returns>
        /// <response code="200">Retorna la información del usuario</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="400">ID de usuario inválido</response>
        [Route("GetUser")]
        [Authorize]
        [ProducesResponseType(typeof(UsersListVm), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UsersListVm>> GetUser(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("ID de usuario no puede estar vacío");
                }

                // Obtener usuario actual
                var currentUser = await this.GetCurrentUserAsync(_userManager);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                // Obtener usuario buscado
                var searchedUser = await GetSearchedUserAsync(id);
                if (searchedUser == null)
                {
                    return NotFound($"Usuario con ID {id} no encontrado");
                }

                // Obtener información completa del usuario
                var result = await _unitOfWork.Users.GetUserById(id);
                if (result == null)
                {
                    return NotFound($"Información del usuario {id} no encontrada");
                }

                // Verificar permisos y establecer flags
                result.CanDeactivate = CanUserDeactivateOthers(currentUser.Id, id);

                // Traducir y formatear roles
                var roles = await _userManager.GetRolesAsync(searchedUser);
                result.Rol = FormatTranslatedRoles(roles, currentUser.Lenguage);

                return this.HandleResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario {UserId}", id);
                return StatusCode(500, "Error interno del servidor al procesar la solicitud");
            }
        }

        private async Task<ApplicationUser> GetSearchedUserAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        private bool CanUserDeactivateOthers(string currentUserId, string targetUserId)
        {
            return currentUserId != targetUserId
                && (
                    User.IsInRole(Constants.Roles.Admin)
                    || User.IsInRole(Constants.Roles.TecnicoPais)
                );
        }

        private string FormatTranslatedRoles(IList<string> roles, string language)
        {
            var translatedRoles = LocalizationUtilities.TranslateRoles(roles, language);
            return string.Join('/', translatedRoles);
        }

        [Route("GetMyId")]
        [Authorize]
        public async Task<IActionResult> GetMyId()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var currentFromDb = await _userManager.FindByEmailAsync(user);

            return Ok(
                JsonConvert.SerializeObject(
                    currentFromDb,
                    Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    }
                )
            );
        }

        [HttpGet("get-my-files")]
        [Authorize]
        public async Task<IActionResult> GetMyFiles()
        {
            var currentUser = await this.GetCurrentUserAsync(_userManager);
            var files = await _unitOfWork.Archivo.GetAll(x =>
                x.UsuarioId == currentUser.Id && x.Activo
            );
            return Ok(
                JsonConvert.SerializeObject(
                    files,
                    Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    }
                )
            );
        }

        [HttpGet("user-files/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(Result<IEnumerable<Archivo>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Result<IEnumerable<Archivo>>>> UserFiles(string userId)
        {
            try
            {
                // 1. Validación de usuario actual
                var currentUser = await this.GetCurrentUserAsync(_userManager);
                if (currentUser == null)
                {
                    return Unauthorized(Result<string>.Failure("Usuario no autenticado"));
                }

                // 2. Validación de permisos
                if (!HasUserAccessPermission(currentUser, userId))
                {
                    return Forbid();
                }

                var files = await _unitOfWork.Archivo.GetAll(x =>
                    x.UsuarioId == userId && x.Activo
                );

                var res = Result<IEnumerable<Archivo>>.Success(files);

                return this.HandleResponse(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener archivos del usuario {UserId}", userId);
                return StatusCode(
                    500,
                    Result<string>.Failure("Ha ocurrido un error interno al procesar la solicitud.")
                );
            }
        }

        private bool HasUserAccessPermission(ApplicationUser currentUser, string targetUserId)
        {
            var isAdmin = User.IsInRole(Constants.Roles.Admin);
            var isTecnicoPais = User.IsInRole(Constants.Roles.TecnicoPais);
            var isOwnUser = currentUser.Id == targetUserId;

            return isAdmin || isTecnicoPais || isOwnUser;
        }

        [HttpPost("save-user")]
        [Authorize(Roles = AuthorizationPolicies.Auth.CreateUser)]
        [ProducesResponseType(typeof(Result<RegisterStaffVm>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Result<RegisterStaffVm>>> SaveUser(RegisterStaffVm model)
        {
            try
            {
                var currentUser = await this.GetCurrentUserAsync(_userManager);

                if (User.IsInRole(Constants.Roles.TecnicoPais))
                {
                    model.Country = currentUser.PaisId ?? 0;
                }

                var userToCreate = MapToApplicationUser(model);
                //Create User
                var (success, user, errors) = await CreateUserWithRolesAsync(userToCreate, model);
                if (!success)
                {
                    var errorMessage = string.Join(", ", errors);
                    return BadRequest(Result<IEnumerable<string>>.Failure(errorMessage));
                }

                try
                {
                    await SendWelcomeEmailAsync(user);
                }
                catch (Exception exEmail)
                {
                    _logger.LogWarning(exEmail, "Error al enviar correo de bienvenida");
                }

                return this.HandleResponse(Result<RegisterStaffVm>.Success(model));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario {Email}", model.Email);
                return StatusCode(500, Result<string>.Failure("Error interno del servidor"));
            }
        }

        private ApplicationUser MapToApplicationUser(RegisterStaffVm model)
        {
            return new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                PaisId = model.Country,
                FirstName = model.Name,
                LastName = model.LastName,
                PhoneNumber = model.Phone,
                Codigo = model.Role,
                Departamento = model.Departamento,
                Ciudad = model.Ciudad,
                Direccion = model.Direccion,
                FechaIngreso = model.FechaIngreso,
                NumeroCarnet = model.NumeroCarnet,
                Lenguage = model.Country == 1 ? "en" : "es",
                Nacionalidad = model.Nacionalidad,
                Profesion = model.Profesion,
                Active = true,
                Notificaciones = true,
                DocumentoAcreditacion = model.DocumentoAcreditacion ?? "",
                HojaDeVida = model.HojaDeVida ?? "",
                DocumentoIdentidad = model.DocumentoIdentidad,
                VencimientoCarnet =
                    model.VencimientoCarnet > DateTime.MinValue ? model.VencimientoCarnet : null,
                CompAuditoraId = model.CompAuditoraId > 0 ? model.CompAuditoraId : null,
            };
        }

        private async Task<(
            bool success,
            ApplicationUser user,
            IEnumerable<string> errors
        )> CreateUserWithRolesAsync(ApplicationUser user, RegisterStaffVm model)
        {
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return (false, null, result.Errors.Select(e => e.Description));
            }

            var roles = model.Role.Contains('/') ? model.Role.Split('/') : new[] { model.Role };

            foreach (var role in roles)
            {
                var normalizedRole = NormalizeRole(role);

                await _userManager.AddToRoleAsync(user, normalizedRole.Trim());
            }

            return (true, user, Array.Empty<string>());
        }

        private async Task SendWelcomeEmailAsync(ApplicationUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationUrl = GenerateConfirmationLink(user.Id, token);

            var emailModel = new LoginMailVm
            {
                Url = confirmationUrl,
                UserData = new RegisterVm
                {
                    email = user.Email,
                    empresa = user.FirstName,
                    language = user.Lenguage,
                },
            };

            var emailContent = await _viewRenderService.RenderToStringAsync(
                "WelcomeMail",
                emailModel
            );
            var subject =
                user.Lenguage == "es" ? "Confirma tu direccion de correo" : "Confirm your account";

            await _emailSender.SendEmailBrevoAsync(user.Email, subject, emailContent);
        }

        private string GenerateConfirmationLink(string userId, string token)
        {
            var baseUrl = _config["ExternalServices:WebUrl"];
            return $"{baseUrl}/auth/confirm/{userId}/3SQ3jh2F3YtJZDq4";
        }

        private string GeneratePasswordResetLink(string userId, string token)
        {
            var baseUrl = _config["ExternalServices:WebUrl"];
            return $"{baseUrl}/auth/resetPass/{userId}/uIpyvT7EJdxeppQ5AbaSto8FPxAoFvHiet8gjvKJWCLQH";
        }

        /// <summary>
        /// Actualiza la información de un usuario y sus roles
        /// </summary>
        /// <param name="model">Modelo con la información del usuario a actualizar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPut("update-user")]
        [Authorize]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Result<string>>> UpdateUser(RegisterStaffVm model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ValidationProblemDetails(ModelState));
                }

                // Obtener usuario actual
                var currentUser = await this.GetCurrentUserAsync(_userManager);
                if (currentUser == null)
                {
                    return Unauthorized(
                        Result<string>.Failure("Debe estar logeado para realizar esta acción.")
                    );
                }

                // Validar permisos
                if (!HasUpdatePermission(currentUser, model))
                {
                    return Forbid();
                }

                // Obtener usuario a editar
                var userToEdit = await _userManager.FindByIdAsync(model.Id);
                if (userToEdit == null)
                {
                    return NotFound(
                        Result<string>.Failure($"Usuario con ID {model.Id} no encontrado")
                    );
                }

                // Actualizar información del usuario
                await UpdateUserInformation(userToEdit, model, currentUser);

                // Actualizar roles si es administrador
                if (User.IsInRole(Constants.Roles.Admin))
                {
                    await UpdateUserRoles(userToEdit, model.Role);
                }

                return Ok(Result<string>.Success("Usuario actualizado correctamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando usuario {UserId}", model.Id);
                return StatusCode(500, "Error interno del servidor al actualizar el usuario");
            }
        }

        private bool HasUpdatePermission(ApplicationUser currentUser, RegisterStaffVm model)
        {
            if (User.IsInRole(Constants.Roles.Admin))
            {
                return true;
            }

            if (User.IsInRole(Constants.Roles.TecnicoPais))
            {
                model.Country = currentUser.PaisId ?? 0;
                return true;
            }

            return currentUser.Id == model.Id;
        }

        private async Task UpdateUserInformation(
            ApplicationUser user,
            RegisterStaffVm model,
            ApplicationUser currentUser
        )
        {
            try
            {
                // Verificación adicional de nulo
                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user));
                }
                // Mapear propiedades básicas
                MapBasicProperties(user, model);

                // Configurar propiedades especiales
                ConfigureSpecialProperties(user, model);

                // Actualizar usuario
                var result = await _userManager.UpdateAsync(user);

                _logger.LogInformation(
                    "Resultado de UpdateAsync para usuario {UserId}: {Result}",
                    user.Id,
                    result?.Succeeded ?? false
                );

                // Verificar el resultado
                if (result == null)
                {
                    _logger.LogError("UpdateAsync devolvió null para usuario {UserId}", user.Id);
                    throw new InvalidOperationException(
                        $"Error al actualizar el usuario: resultado nulo"
                    );
                }

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError(
                        "Errores en UpdateAsync para usuario {UserId}: {Errors}",
                        user.Id,
                        errors
                    );
                    throw new InvalidOperationException(
                        $"Error al actualizar el usuario: {errors}"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error en UpdateUserInformation para usuario {UserId}",
                    user.Id
                );
                throw; // Relanzar la excepción para manejarla en el controlador
            }
        }

        private void MapBasicProperties(ApplicationUser user, RegisterStaffVm model)
        {
            user.Id = model.Id;
            user.FirstName = model.Name;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Departamento = model.Departamento;
            user.Ciudad = model.Ciudad;
            user.Direccion = model.Direccion;
            user.FechaIngreso = model.FechaIngreso;
            user.NumeroCarnet = model.NumeroCarnet;
            user.PaisId = model.Country;
            user.Nacionalidad = model.Nacionalidad;
            user.Profesion = model.Profesion;
            user.DocumentoIdentidad = model.DocumentoIdentidad;
            user.Active = model.Active;
            user.Notificaciones = model.Notificaciones;
            // Actualizar o mantener si viene null.
            user.Codigo = model.Codigo ?? user.Codigo;
            user.DocumentoAcreditacion = model.DocumentoAcreditacion ?? user.DocumentoAcreditacion;
            user.PhoneNumber = model.PhoneNumber ?? user.PhoneNumber;
            user.HojaDeVida = model.HojaDeVida ?? user.HojaDeVida;
        }

        private void ConfigureSpecialProperties(ApplicationUser user, RegisterStaffVm model)
        {
            // Configurar VencimientoCarnet
            user.VencimientoCarnet =
                model.VencimientoCarnet > DateTime.MinValue ? model.VencimientoCarnet : null;

            // Configurar Código basado en el rol
            if (!string.IsNullOrEmpty(model.Role))
            {
                user.Codigo = model.Codigo ?? user.Codigo;
            }

            // Configurar CompAuditoraId
            user.CompAuditoraId = model.CompAuditoraId == 0 ? null : model.CompAuditoraId;
        }

        private async Task UpdateUserRoles(ApplicationUser user, string newRole)
        {
            // Normalizar el rol
            var normalizedRole = NormalizeRole(newRole);

            // Remover roles actuales
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // Asignar nuevos roles
            if (normalizedRole.Contains('/'))
            {
                var roles = normalizedRole.Split('/').Select(r => r.Trim()).ToList();

                foreach (var role in roles)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
            else
            {
                await _userManager.AddToRoleAsync(user, normalizedRole);
            }
        }

        private string NormalizeRole(string role)
        {
            return role switch
            {
                "Country Technician" => Constants.Roles.TecnicoPais,
                "CCT" => Constants.Roles.CTC,
                "Consultant" => Constants.Roles.Asesor,
                _ => role,
            };
        }

        public class ConfirmEmailViewModel
        {
            public string UserId { get; set; }
            public string Token { get; set; }
        }

        public class UserListItemVm
        {
            public string Id { get; set; }
            public string Email { get; set; }
            public string Roles { get; set; }
        }

        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userId);

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpPost("ConfirmEmailApi")]
        public async Task<IActionResult> ConfirmEmailApi(ConfirmEmailViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            var result = await _userManager.ConfirmEmailAsync(user, model.Token);

            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                //hasta solucionar errores de activacion
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var newResult = await _userManager.ConfirmEmailAsync(user, token);
                if (newResult.Succeeded)
                {
                    return Ok();
                }
            }
            return BadRequest();
        }

        private static int ParsePaisId(string pais)
        {
            return int.TryParse(pais, out int paisId) ? paisId : 0;
        }

        private static IEnumerable<UsersListVm> TranslateUserRoles(
            IEnumerable<UsersListVm> users,
            string language
        )
        {
            if (users == null)
                return Enumerable.Empty<UsersListVm>();

            foreach (var user in users)
            {
                if (string.IsNullOrEmpty(user.Rol))
                    continue;

                // Si es un auditor externo, mantener la información del país externo
                if (user.Pais?.StartsWith("Externo -") == true)
                {
                    // Opcional: traducir "Externo" según el idioma
                    if (language == "en")
                    {
                        user.Pais = user.Pais.Replace("Externo -", "External -");
                    }
                }

                var roles = user
                    .Rol.Split('/', StringSplitOptions.RemoveEmptyEntries)
                    .Select(role =>
                        LocalizationUtilities.RoleToText(new[] { role.Trim() }, language)
                    )
                    .Where(translatedRole => !string.IsNullOrEmpty(translatedRole));

                user.Rol = string.Join("/", roles);
            }

            return users;
        }
    }
}
