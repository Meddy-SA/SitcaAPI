using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
//using API.ViewModels;
//using Core.Services.Email;
using Sitca.DataAccess.Services.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;

using Sitca.Models.ViewModels;
using Sitca.Models;
using Core.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;
using Sitca.DataAccess.Services.ViewToString;
using Sitca.DataAccess.Data.Repository;
using Microsoft.AspNetCore.Cors;
using System.Text;

namespace Sitca.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class AuthController : ControllerBase
	{

		private readonly UserManager<IdentityUser> _userManager;
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly IJWTTokenGenerator _jwtToken;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDapper _dapper;
        //view to string
        private readonly IViewRenderService _viewRenderService;

        public AuthController(
            IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
			 SignInManager<IdentityUser> signInManager,
			  IJWTTokenGenerator jwtToken,
			  RoleManager<IdentityRole> roleManager,
			  IConfiguration config,

              IViewRenderService viewRenderService,

              IEmailSender emailSender,
              IDapper dapper)
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
        }


        //[HttpGet("CreateRole")]
        //public async Task<IActionResult> CreateRole(string roleName)
        //{
        //    if (!(await _roleManager.RoleExistsAsync(roleName)))
        //    {
        //        await _roleManager.CreateAsync(new IdentityRole(roleName));
        //    }

        //    return Ok();
        //}

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

            //var result = await _signInManager.CheckPasswordSignInAsync(userFromDb, data.currentPassword, false);

            var res = await _userManager.ChangePasswordAsync(userFromDb, data.currentPassword, data.newPassword);

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
        [Authorize (Roles = "Admin,TecnicoPais")]
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
        
            if(data.code != "uIpyvT7EJdxeppQ5AbaSto8FPxAoFvHiet8gjvKJWCLQH")
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

        [HttpGet]
        [Route("ResetPassKey")]
        public async Task<IActionResult> ResetPassKey(string email)
        {

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest();
            }
                       
            var userFromDb = await _userManager.FindByEmailAsync(email);
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            var code = await _userManager.GeneratePasswordResetTokenAsync(userFromDb);
            
            //SEND EMAIL - resetear password
            var host = _config["Angular:testUrl"];
            var getUrl = HttpContext.Request.Host.Value;
            if (!getUrl.Contains("localhost"))
            {
                //prod
                host = _config["Angular:productionUrl"];
            }

            //var urlString = host + "/auth/confirm/" + userFromDb.Id + "/" + "3SQ3jh2F3YtJZDq4";
            //var urlString = host + "/auth/resetPass/" + userFromDb.Id + "/" + HttpUtility.UrlEncode(code);
            var urlString = host + "/auth/resetPass/" + userFromDb.Id + "/" + "uIpyvT7EJdxeppQ5AbaSto8FPxAoFvHiet8gjvKJWCLQH";

            var userAux = new RegisterVm
            {
                email = email,
                empresa = email
            };

            var loginData = new LoginMailVm
            {
                Url = urlString,
                UserData = userAux,                
            };


            var viewName = appUser.Lenguage == "es" ? "ResetPassEMail" : "ResetPassEMailEnglish";

            var view = await _viewRenderService.RenderToStringAsync(viewName, loginData);

            var senderEmail = _config["EmailSender:UserName"];

            //var senderEmail = "notificaciones@siccs.info";
            //var senderEmail = "prueba@agricolasanchez.com.ar";
            //var senderEmail = "notificaciones@calidadcentroamerica.com";
            
            var title = appUser.Lenguage == "es" ? "Solicitud de recuperación de cuenta" : "Account recovery request";

            try
            {
                await _emailSender.SendEmailAsync(senderEmail, userFromDb.Email, title, view);
            }
            catch (Exception)
            {
                
            }
            

            return Ok();           
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
            if (User.IsInRole("Admin") || User.IsInRole("TecnicoPais") && !string.IsNullOrEmpty(userId))
            {
                //puede editar otro usuario, utilizar parametro userId
                if(userId != null)
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
        public async Task<IActionResult> GetRoles(string language)
        {            
            var roles = await _roleManager.Roles.Select(s =>s.Name).ToListAsync();

            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var userFromDb = await _userManager.FindByEmailAsync(user);
            
            ApplicationUser appUser = (ApplicationUser)userFromDb;

            roles.Remove(roles.First(s => s == "Empresa"));

            if (User.IsInRole("TecnicoPais"))
            {
                roles.Remove(roles.First(s => s == "Admin"));
            }

            var lang = appUser.Lenguage;
            if (lang == "en")
            {
                roles = Utilities.Utilities.TranslateRoles(roles, lang);
            }
            
            return Ok(JsonConvert.SerializeObject(roles));
        }

        [Authorize (Roles ="Empresa")]
        [Route("ListTecnicos")]
        [HttpGet]
        //[Authorize]
        public async Task<IActionResult> ListTecnicos()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            
            var userFromDb = await _userManager.FindByEmailAsync(user);          
            ApplicationUser appUser = (ApplicationUser)userFromDb;
                        
            string q = null;
            
            var result = await _unitOfWork.Users.GetUsersAsync(q, appUser.PaisId??0, "TecnicoPais");
            
            return Ok(JsonConvert.SerializeObject(result, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [Authorize]
        [Route("GetUsers")]
        [HttpGet]
        //[Authorize]
        public async Task<IActionResult> GetUsers( string pais, string q, string roleName ="All")
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var userFromDb = await _userManager.FindByEmailAsync(user);
            //var roles = await _userManager.GetRolesAsync(userFromDb);
            ApplicationUser appUser = (ApplicationUser)userFromDb;
            var paisx = Int32.Parse(pais);
            if (!User.IsInRole("Admin"))
            {
                paisx = appUser.PaisId??0;
            }

            if (q == "undefined" || string.IsNullOrEmpty(q))
            {
                q = null;
            }

            var result = await _unitOfWork.Users.GetUsersAsync(q, paisx, roleName);

            if (appUser.Lenguage != "es")
            {
                foreach (var item in result)
                {
                    switch (item.Rol.ToLower())
                    {
                        case "asesor":
                            item.Rol = "Consultant";
                            break;
                        case "ctc":
                            item.Rol = "CCT";
                            break;
                        case "tecnicopais":
                            item.Rol = "Country Technician";
                            break;
                        case "asesor/auditor":
                            item.Rol = "Consultant/Auditor";
                            break;
                    }
                }
            }
            else
            {
                foreach (var item in result)
                {
                    switch (item.Rol.ToLower())
                    {                       
                        case "tecnicopais":
                            item.Rol = "Técnico País";
                            break;                       
                    }
                }
            }
            

            return Ok(JsonConvert.SerializeObject(result, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }
      
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var mail = model.email.ToUpper();
        
            var userFromDb = await _userManager.FindByEmailAsync(mail);

            if (userFromDb == null)
            {
                return BadRequest();
            }

            ApplicationUser appUser = (ApplicationUser)userFromDb;

            if (!appUser.Active)
            {
                return Unauthorized();
            }            

            var result = await _signInManager.CheckPasswordSignInAsync(userFromDb, model.password, false);

            if (!result.Succeeded)
            {
                return BadRequest();
            }

            var roles = await _userManager.GetRolesAsync(userFromDb);

            IList<Claim> claims = await _userManager.GetClaimsAsync(userFromDb);

            //var token =             
            var roleText = Utilities.Utilities.RoleToText(roles, appUser.Lenguage);

            return Ok(new
            {
                result = result,
                roleText = roleText,
                country = Utilities.Utilities.GetCountry(appUser.PaisId??0),
                username = userFromDb.UserName,
                email = userFromDb.Email,
                language = appUser.Lenguage,
                token = _jwtToken.GenerateToken(userFromDb, roles, claims)
            });
        }

        [Authorize]
        [HttpPost("renewToken")]
        public async Task<IActionResult> RenewToken()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

            var userFromDb = await _userManager.FindByEmailAsync(user);

            if (userFromDb == null)
            {
                return BadRequest();
            }

            ApplicationUser appUser = (ApplicationUser)userFromDb;

            if (!appUser.Active)
            {
                return Unauthorized();
            }                

            var roles = await _userManager.GetRolesAsync(userFromDb);

            IList<Claim> claims = await _userManager.GetClaimsAsync(userFromDb);

            //var token =             
            var roleText = Utilities.Utilities.RoleToText(roles, appUser.Lenguage);

            var result = new { Succeeded = true };


            return Ok(new
            {
                result = result,
                roleText = roleText,
                country = Utilities.Utilities.GetCountry(appUser.PaisId ?? 0),
                username = userFromDb.UserName,
                email = userFromDb.Email,
                language = appUser.Lenguage,
                token = _jwtToken.GenerateToken(userFromDb, roles, claims)
            });
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

            if (roles.Any(s =>s == role))
            {
                return Ok(new
                {                    
                    username = userFromDb.UserName,
                    email = userFromDb.Email,
                    roleText = roleText,
                    country = Utilities.Utilities.GetCountry(appUser.PaisId ?? 0),
                    token = _jwtToken.GenerateToken(userFromDb, listRoles, claims)
                });
            }

            return BadRequest();
            
        }

        [Authorize]
        [HttpGet("SetLanguage")]
        public async Task<IActionResult> SetLanguage(string lang)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var result = await _unitOfWork.Users.SetLanguageAsync(lang, user);

            return Ok(JsonConvert.SerializeObject(result));
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterVm model)
        {
            var checkEmail = await _userManager.FindByEmailAsync(model.email);
            if (checkEmail != null)
                return BadRequest("Check email");

            var Role = "Empresa";
            //CREAR EMPRESA
            var empresaId = _unitOfWork.Empresa.SaveEmpresa(model);

            if (model.language != "en" && model.language != "es")
            {
                model.language = model.country == 1 ? "en" : "es";
            }

            var userToCreate = new ApplicationUser
            {
                Email = model.email,
                UserName = model.email,
                EmpresaId = empresaId,
                PaisId = model.country,
                FirstName = model.representante,
                Notificaciones = true,
                Active = true,
                Lenguage = model.language
            };

            //Create User
            var result = await _userManager.CreateAsync(userToCreate, model.password);
            if (result.Succeeded)
            {
                var userFromDb = await _userManager.FindByNameAsync(userToCreate.UserName);

                //Add role to user
                await _userManager.AddToRoleAsync(userFromDb, Role);

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(userFromDb);

                //SEND EMAIL - clave confirmacion cuenta
                var host = _config["Angular:testUrl"];
                var getUrl = HttpContext.Request.Host.Value;
                if (!getUrl.Contains("localhost"))
                {
                    //prod
                    host = _config["Angular:productionUrl"];
                }

                //var urlString = host + "/auth/confirm/" + userFromDb.Id + "/" + HttpUtility.UrlEncode(token);
                var urlString = host + "/auth/confirm/" + userFromDb.Id + "/" + "3SQ3jh2F3YtJZDq4";

                var loginData = new LoginMailVm
                {
                    Url = urlString,
                    UserData = model,                    
                };

                var view = await _viewRenderService.RenderToStringAsync("WelcomeMail", loginData);

                var senderEmail = _config["EmailSender:UserName"];
                //var senderEmail = "notificaciones@siccs.info";
                //var senderEmail = "prueba@agricolasanchez.com.ar";

                var subject = userToCreate.Lenguage == "es" ? "Confirma tu direccion de correo" : "Confirm your account";

                _emailSender.SendEmailAsync(senderEmail, userFromDb.Email, subject, view);

                return Ok(result);
            }
            return BadRequest();
        }

        [Route("GetUser")]
        [Authorize]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var currentFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)currentFromDb;
            //check role
            var adm = User.IsInRole("Admin");


            #region getting user roles

            var searchedUser = await _userManager.FindByIdAsync(id);
            var roles = await _userManager.GetRolesAsync(searchedUser);                        

            #endregion

            var result = await _unitOfWork.Users.GetUserById(id);
            result.CanDeactivate = (currentFromDb.Id != id && (User.IsInRole("Admin") || User.IsInRole("TecnicoPais")));

            if (roles.Count > 1)
            {
                result.Rol = string.Join('/', roles);


                if (appUser.Lenguage == "en")
                {
                    if (result.Rol.Contains("Asesor"))
                    {
                        result.Rol = result.Rol.Replace("Asesor", "Consultant");
                    }

                    if (result.Rol.Contains("TecnicoPais"))
                    {
                        result.Rol = result.Rol.Replace("TecnicoPais", "Consultant");
                    }

                }
                
            }

            return Ok(JsonConvert.SerializeObject(result, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [Route("GetMyId")]
        [Authorize]
        public async Task<IActionResult> GetMyId()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var currentFromDb = await _userManager.FindByEmailAsync(user);

            return Ok(JsonConvert.SerializeObject(currentFromDb, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [Route("GetMyFiles")]
        [Authorize]
        public async Task<IActionResult> GetMyFiles()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var currentFromDb = await _userManager.FindByEmailAsync(user);

            var files = _unitOfWork.Archivo.GetAll(x =>x.UsuarioId == currentFromDb.Id && x.Activo);


            return Ok(JsonConvert.SerializeObject(files, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [Route("UserFiles")]
        [Authorize]
        public async Task<IActionResult> UserFiles(string userId)
        {


            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var currentFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)currentFromDb;

            if (!User.IsInRole("Admin") && !User.IsInRole("TecnicoPais") && appUser.Id != userId )
            {
                return BadRequest();
            }

            var files = _unitOfWork.Archivo.GetAll(x => x.UsuarioId == userId && x.Activo);


            return Ok(JsonConvert.SerializeObject(files, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
        }

        [HttpPost("SaveUser")]

        [Authorize(Roles = "Admin,TecnicoPais")]
        public async Task<IActionResult> SaveUser(RegisterStaffVm model)
        {

            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var currentFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)currentFromDb;            

            if (User.IsInRole("TecnicoPais"))
            {
                model.Country = appUser.PaisId??0;
            }
            var codigo = (model.Role == "Asesor" || model.Role == "Auditor" || model.Role == "Asesor/Auditor") ? model.Codigo : null;

            var userToCreate = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,                
                PaisId = model.Country,
                FirstName = model.Name,
                LastName = model.LastName,
                PhoneNumber = model.Phone,
                Codigo = codigo,

                Departamento = model.Departamento,
                Ciudad = model.Ciudad,
                Direccion = model.Direccion,
                FechaIngreso = model.FechaIngreso,
                NumeroCarnet = model.NumeroCarnet,
                Lenguage = model.Country == 1?"en":"es",

                Nacionalidad = model.Nacionalidad,
                Profesion = model.Profesion,
                Active = true,
                Notificaciones = true,
                DocumentoIdentidad = model.DocumentoIdentidad,

                VencimientoCarnet = model.VencimientoCarnet >  DateTime.MinValue? model.VencimientoCarnet: (DateTime?)null,
            };


            if (model.CompAuditoraId > 0)
            {
                userToCreate.CompAuditoraId = model.CompAuditoraId;
            }


            if (model.Role == "Country Technician")
            {
                model.Role = "TecnicoPais";
            }

            if (model.Role == "CCT")
            {
                model.Role = "CTC";
            }

            if (model.Role.Contains("Consultant"))
            {
                model.Role = model.Role.Replace("Consultant", "Asesor");
            }

            //Create User
            var result = await _userManager.CreateAsync(userToCreate, model.Password);
            if (result.Succeeded)
            {
                var userFromDb = await _userManager.FindByNameAsync(userToCreate.UserName);

                //Add role to user
                if (model.Role.Contains('/')){
                    var roleList = model.Role.Split('/');
                    foreach (var item in roleList)
                    {
                        await _userManager.AddToRoleAsync(userFromDb, item);
                    }
                }
                else
                {
                    await _userManager.AddToRoleAsync(userFromDb, model.Role);
                }
                

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(userFromDb);

                //SEND EMAIL - clave confirmacion cuenta
                var host = _config["Angular:testUrl"];
                var getUrl = HttpContext.Request.Host.Value;
                if (!getUrl.Contains("localhost"))
                {
                    //prod
                    host = _config["Angular:productionUrl"];
                }
                var urlString = host + "/auth/confirm/" + userFromDb.Id + "/" + "3SQ3jh2F3YtJZDq4";
                //var urlString = host + "/auth/confirm/" + userFromDb.Id + "/" + HttpUtility.UrlEncode(token);
                //var senderEmail = "notificaciones@siccs.info";
                //var senderEmail = "prueba@agricolasanchez.com.ar";
                var senderEmail = _config["EmailSender:UserName"];

                var userAux = new RegisterVm
                {
                    email = model.Email,
                    empresa = model.Name,
                    language = userToCreate.Lenguage
                };

                var loginData = new LoginMailVm
                {
                    Url = urlString,
                    UserData = userAux
                };

                var view = await _viewRenderService.RenderToStringAsync("WelcomeMail", loginData);
                var subject = userToCreate.Lenguage == "es" ? "Confirma tu direccion de correo" : "Confirm your account";
                _emailSender.SendEmailAsync(senderEmail, userFromDb.Email, subject, view);

                return Ok(result);
            }
            return BadRequest();
        }


        [HttpPost]
        [Authorize]
        [Route("Updateuser")]
        public async Task<IActionResult> Updateuser(RegisterStaffVm model)
        {

            var user = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var currentFromDb = await _userManager.FindByEmailAsync(user);
            ApplicationUser appUser = (ApplicationUser)currentFromDb;            

            if ( !User.IsInRole("Admin"))
            {
                //si no es admin...
                if (!User.IsInRole("TecnicoPais") && currentFromDb.Id != model.Id)
                {
                    //si no es tecnico pais y esta intentanto modificar otro usuario
                    return BadRequest();
                }
                model.Country = appUser.PaisId ?? 0;

            }

            var identityUser = await _userManager.FindByIdAsync(model.Id);
            ApplicationUser userToEdit = (ApplicationUser)identityUser;

            userToEdit.FirstName = model.Name;
            userToEdit.LastName = model.LastName;
            userToEdit.PhoneNumber = model.PhoneNumber;
            userToEdit.Departamento = model.Departamento;
            userToEdit.Ciudad = model.Ciudad;
            userToEdit.Direccion = model.Direccion;
            userToEdit.FechaIngreso = model.FechaIngreso;
            userToEdit.NumeroCarnet = model.NumeroCarnet;
            userToEdit.PaisId = model.Country;
            userToEdit.Nacionalidad = model.Nacionalidad;
            userToEdit.Profesion = model.Profesion;
            userToEdit.DocumentoIdentidad = model.DocumentoIdentidad;
            userToEdit.Active = model.Active;
            userToEdit.Notificaciones = model.Notificaciones;

            if (model.VencimientoCarnet > DateTime.MinValue)
            {
                userToEdit.VencimientoCarnet = model.VencimientoCarnet;
            }
            else
            {
                userToEdit.VencimientoCarnet = (DateTime?)null;
            }

            

            userToEdit.Codigo = (model.Role == "Asesor" || model.Role == "Auditor" || model.Role == "Asesor/Auditor") ? model.Codigo : null;


            if (model.CompAuditoraId == 0)
            {
                userToEdit.CompAuditoraId = null;
            }
            else
            {
                userToEdit.CompAuditoraId = model.CompAuditoraId;
            }

            //Edit User            
            var result2 = await _userManager.UpdateAsync(userToEdit);


            if (!User.IsInRole("Admin"))
            {
                return Ok();
            }

            //Edit Role?
            var roles = await _userManager.GetRolesAsync(identityUser);
            
            if (model.Role == "Country Technician")
            {
                model.Role = "TecnicoPais";
            }

            if (model.Role == "CCT")
            {
                model.Role = "CTC";
            }

            if (model.Role.Contains("Consultant"))
            {
                model.Role = model.Role.Replace("Consultant", "Asesor");
            }

            //remove roles
            foreach (var item in roles)
            {
                await _userManager.RemoveFromRoleAsync(identityUser, item);
            }
            

            //Add role to user
            if (model.Role.Contains('/'))
            {
                var roleList = model.Role.Split('/');
                foreach (var item in roleList)
                {
                    await _userManager.AddToRoleAsync(identityUser, item);
                }
            }
            else
            {
                await _userManager.AddToRoleAsync(identityUser, model.Role);
            }

            return Ok();
            
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

        public class LoginModel
		{
			public string email { get; set; }

			public string password { get; set; }

            public string lang { get; set; }
        }
	}
}
