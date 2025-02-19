using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Sitca.Models;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Services;

public class DbUserInitializer(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ILogger<DbUserInitializer> logger
)
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly ILogger<DbUserInitializer> _logger = logger;

    private const string DefaultPassword = "Password123!";
    private const int AsesorCompanyId = 47;

    public async Task<Result<bool>> InitializeUsersAsync()
    {
        try
        {
            var users = new List<(ApplicationUser User, string Role)>
            {
                (CreateUserModel("empresa", "Empresa", "Test"), "Empresa"),
                (CreateUserModel("ctc", "CTC", "Test"), "CTC"),
                (CreateUserModel("tecnicopais", "Técnico País", "Test"), "TecnicoPais"),
                (CreateUserModel("admin", "Admin", "Test"), "Admin"),
                (CreateUserModel("asesor", "Asesor", "Test", AsesorCompanyId), "Asesor"),
                (CreateUserModel("auditor", "Auditor", "Test"), "Auditor"),
                (CreateUserModel("consultor", "Consultor", "Test"), "Consultor"),
                (CreateUserModel("empresa_auditora", "Empresa", "Auditora", 42), "EmpresaAuditora")
            };

            foreach (var (user, role) in users)
            {
                await EnsureRolesAsync(role);
                await CreateUserIfNotExistsAsync(user, role);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar usuarios de prueba");
            return Result<bool>.Failure("Error al inicializar usuarios de prueba");
        }
    }

    private ApplicationUser CreateUserModel(
        string identifier,
        string firstName,
        string lastName,
        int? compAuditoraId = null
    )
    {
        return new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            UserName = $"test.{identifier}@sitca.test",
            Email = $"test.{identifier}@sitca.test",
            EmailConfirmed = true,
            PaisId = 5, // Nicaragua
            Codigo = $"TEST-{identifier.ToUpper()}",
            Ciudad = "Managua",
            Departamento = "Managua",
            Direccion = "Managua, Nicaragua",
            Active = true,
            Notificaciones = true,
            Lenguage = "es",
            PhoneNumber = "+505555555",
            DocumentoIdentidad = "TEST-ID",
            Profesion = "Tester",
            Nacionalidad = "Nicaragua",
            NumeroCarnet = $"CARD-{identifier.ToUpper()}",
            FechaIngreso = DateTime.Now.ToString("dd/MM/yyyy"),
            HojaDeVida = "N/A",
            DocumentoAcreditacion = "N/A",
            CompAuditoraId = compAuditoraId,
        };
    }

    public async Task<Result<bool>> EnsureRolesAsync(string rol)
    {
        try
        {
            if (string.IsNullOrEmpty(rol))
            {
                return Result<bool>.Failure("El nombre del rol no puede estar vacío");
            }

            var nameRol = rol.Trim();
            var alreadyExists = await _roleManager.RoleExistsAsync(nameRol);
            if (!alreadyExists)
            {
                var role = new ApplicationRole(nameRol);
                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    _logger.LogError(
                        "Error al crear rol {Role}: {Errors}",
                        nameRol,
                        string.Join(", ", result.Errors.Select(e => e.Description))
                    );
                    return Result<bool>.Failure($"Error al crear el rol {nameRol}");
                }
                _logger.LogInformation("Rol creado: {Role}", nameRol);
            }
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asegurar roles");
            return Result<bool>.Failure($"Error interno al crear rol: {ex.Message}");
        }
    }

    private async Task<bool> CreateUserIfNotExistsAsync(ApplicationUser user, string role)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser != null)
            {
                _logger.LogInformation("Usuario {Email} ya existe", user.Email);
                // Actualizar CompAuditoraId si es necesario
                if (role == "Asesor" && existingUser.CompAuditoraId != AsesorCompanyId)
                {
                    var appUser = (ApplicationUser)existingUser;
                    appUser.CompAuditoraId = AsesorCompanyId;
                    await _userManager.UpdateAsync(appUser);
                    _logger.LogInformation(
                        "Actualizado CompAuditoraId para usuario {Email}",
                        user.Email
                    );
                }
                if (role == "Empresa" && existingUser.EmpresaId == null)
                {
                    var appUser = (ApplicationUser)existingUser;
                    appUser.CompAuditoraId = 140;
                    await _userManager.UpdateAsync(appUser);
                }
                return true;
            }

            var result = await _userManager.CreateAsync(user, DefaultPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                _logger.LogInformation(
                    "Usuario {Email} creado exitosamente con rol {Role}",
                    user.Email,
                    role
                );
                return true;
            }

            _logger.LogWarning(
                "Error al crear usuario {Email}: {Errors}",
                user.Email,
                string.Join(", ", result.Errors.Select(e => e.Description))
            );
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario {Email}", user.Email);
            return false;
        }
    }
}
