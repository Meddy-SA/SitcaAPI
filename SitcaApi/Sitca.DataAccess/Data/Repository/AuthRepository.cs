using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Services.Token;
using Sitca.DataAccess.Services.ViewToString;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;
using Utilities.Common;

namespace Sitca.DataAccess.Data.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJWTTokenGenerator _jwtToken;
    private readonly IEmailSender _emailService;
    private readonly IConfiguration _config;
    private readonly IViewRenderService _viewRenderService;
    private readonly ILogger<AuthRepository> _logger;
    private readonly string _url;

    public AuthRepository(
        UserManager<ApplicationUser> userManager,
        IJWTTokenGenerator jwtToken,
        IEmailSender emailService,
        IConfiguration config,
        IViewRenderService viewRenderService,
        ILogger<AuthRepository> logger
    )
    {
        _userManager = userManager;
        _jwtToken = jwtToken;
        _emailService = emailService;
        _config = config;
        _viewRenderService = viewRenderService;
        _logger = logger;
        _url =
            _config["ExternalServices:WebUrl"]
            ?? throw new InvalidOperationException("WebUrl configuration is missing");
    }

    public async Task<AuthResult> LoginAsync(LoginDTO loginDto)
    {
        try
        {
            _logger.LogInformation("Attempting login for user: {Email}", loginDto.Email);

            var (user, error) = await GetAndValidateUserAsync(loginDto.Email, loginDto.Language);
            if (error != null)
            {
                return AuthResult.Failed(error);
            }

            var validPassword = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!validPassword)
            {
                _logger.LogWarning("Invalid password attempt for user: {Email}", loginDto.Email);
                return AuthResult.Failed(
                    GetLocalizedMessage("InvalidCredentials", loginDto.Language)
                );
            }

            return await GenerateAuthResultAsync(user, loginDto.Language);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Email}", loginDto.Email);
            return AuthResult.Failed(GetLocalizedMessage("GeneralError", loginDto.Language));
        }
    }

    public async Task<AuthResult> RegisterAsync(RegisterDTO register)
    {
        try
        {
            _logger.LogInformation("Attempting registration for user: {Email}", register.Email);

            if (await UserExistsAsync(register.Email))
            {
                return AuthResult.Failed(GetLocalizedMessage("EmailExists", register.Language));
            }

            var newUser = CreateApplicationUser(register);
            var result = await CreateUserAsync(newUser, register);

            if (!result.Succeeded)
            {
                return AuthResult.Failed(
                    GetLocalizedMessage("RegistrationError", register.Language)
                        + string.Join(", ", result.Errors.Select(e => e.Description))
                );
            }

            await AssignDefaultRoleAsync(newUser);
            await SendWelcomeEmailAsync(newUser, register.Language);

            return await GenerateAuthResultAsync(newUser, register.Language);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {Email}", register.Email);
            return AuthResult.Failed(GetLocalizedMessage("GeneralError", register.Language));
        }
    }

    public async Task<AuthResult> RenewTokenAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Attempting token renewal for user ID: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.Active)
            {
                return AuthResult.Failed(
                    GetLocalizedMessage("UserNotFound", user?.Lenguage ?? "en")
                );
            }

            return await GenerateAuthResultAsync(user, user.Lenguage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token renewal for user ID: {UserId}", userId);
            return AuthResult.Failed(GetLocalizedMessage("GeneralError", "en"));
        }
    }

    #region Private Helper Methods

    private async Task<(ApplicationUser User, string Error)> GetAndValidateUserAsync(
        string email,
        string language
    )
    {
        try
        {
            // Verificar si _userManager está inicializado
            if (_userManager == null)
            {
                _logger.LogError("UserManager es null");
                return (null, "Error de configuración");
            }

            // Verificar el email normalizado
            var normalizedEmail = _userManager.NormalizeEmail(email);
            _logger.LogInformation("Email normalizado: {NormalizedEmail}", normalizedEmail);

            // Intentar buscar directo en el DbContext
            using var scope = _logger.BeginScope("Búsqueda de usuario");

            var userFromDb = await _userManager.Users.FirstOrDefaultAsync(u =>
                u.NormalizedEmail == normalizedEmail
            );

            if (userFromDb == null)
            {
                _logger.LogWarning("Usuario no encontrado en la base de datos");
                return (null, GetLocalizedMessage("InvalidCredentials", language));
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning(
                    "FindByEmailAsync retornó null para un usuario que existe en DB"
                );
                return (null, GetLocalizedMessage("InvalidCredentials", language));
            }
            if (!user.Active)
            {
                _logger.LogWarning("Usuario encontrado pero no está activo");
                return (null, GetLocalizedMessage("InvalidCredentials", language));
            }

            return (user, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error buscando usuario");
            throw;
        }
    }

    private async Task<AuthResult> GenerateAuthResultAsync(ApplicationUser user, string language)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        var languageClaim = new Claim("Language", user.Lenguage ?? "es");
        await _userManager.AddClaimAsync(user, languageClaim);
        var token = _jwtToken.GenerateToken(user, roles, claims);
        var roleText = LocalizationUtilities.RoleToText(roles, language);
        var country = LocalizationUtilities.GetCountry(user.PaisId ?? 0);

        return AuthResult.Success(token, user, roles, roleText, country);
    }

    private ApplicationUser CreateApplicationUser(RegisterDTO register) =>
        new()
        {
            Email = register.Email,
            UserName = register.Email,
            FirstName = register.FirstName,
            LastName = register.LastName,
            PhoneNumber = register.PhoneNumber,
            PaisId = register.CountryId,
            Lenguage = register.Language,
            Active = true,
            Notificaciones = true,
        };

    private async Task<bool> UserExistsAsync(string email) =>
        await _userManager.FindByEmailAsync(email) != null;

    private async Task<IdentityResult> CreateUserAsync(
        ApplicationUser user,
        RegisterDTO register
    ) => await _userManager.CreateAsync(user, register.Password);

    private async Task AssignDefaultRoleAsync(ApplicationUser user) =>
        await _userManager.AddToRoleAsync(user, Utilities.Common.Constants.Roles.Empresa);

    private async Task SendWelcomeEmailAsync(ApplicationUser user, string language)
    {
        try
        {
            var confirmUrl = $"{_url}/auth/confirm/{user.Id}/3SQ3jh2F3YtJZDq4";
            var mailData = new LoginMailVm
            {
                Url = confirmUrl,
                UserData = new RegisterVm
                {
                    email = user.Email,
                    empresa = user.FirstName,
                    language = language,
                },
            };

            var emailContent = await _viewRenderService.RenderToStringAsync(
                "WelcomeMail",
                mailData
            );
            var subject = GetLocalizedMessage("WelcomeEmailSubject", language);

            await _emailService.SendEmailBrevoAsync(user.Email!, subject, emailContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to user: {Email}", user.Email);
            // No lanzamos la excepción para no interrumpir el registro
        }
    }

    private string GetLocalizedMessage(string messageKey, string language) =>
        messageKey switch
        {
            "InvalidCredentials" => language == "en"
                ? "Invalid credentials or inactive user"
                : "Credenciales inválidas o usuario inactivo",
            "EmailExists" => language == "en"
                ? "Email already registered"
                : "El correo electrónico ya está registrado",
            "RegistrationError" => language == "en"
                ? "Failed to create user: "
                : "Error al crear usuario: ",
            "UserNotFound" => language == "en"
                ? "User not found or inactive"
                : "Usuario no encontrado o inactivo",
            "GeneralError" => language == "en"
                ? "An unexpected error occurred"
                : "Ocurrió un error inesperado",
            "WelcomeEmailSubject" => language == "en"
                ? "Confirm your account"
                : "Confirma tu dirección de correo",
            _ => language == "en" ? "An error occurred" : "Ocurrió un error",
        };

    #endregion
}
