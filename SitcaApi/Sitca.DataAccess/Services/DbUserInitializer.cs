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
    RoleManager<IdentityRole> roleManager,
    ILogger<DbUserInitializer> logger
)
{
  private readonly UserManager<ApplicationUser> _userManager = userManager;
  private readonly RoleManager<IdentityRole> _roleManager = roleManager;
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
          (CreateUserModel("auditor", "Auditor", "Test"), "Auditor")
        };

      foreach (var (user, role) in users)
      {
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

  private ApplicationUser CreateUserModel(string identifier, string firstName, string lastName, int? compAuditoraId = null)
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
      CompAuditoraId = compAuditoraId
    };
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
          _logger.LogInformation("Actualizado CompAuditoraId para usuario {Email}", user.Email);
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
        _logger.LogInformation("Usuario {Email} creado exitosamente con rol {Role}", user.Email, role);
        return true;
      }

      _logger.LogWarning("Error al crear usuario {Email}: {Errors}",
          user.Email,
          string.Join(", ", result.Errors.Select(e => e.Description)));
      return false;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error al crear usuario {Email}", user.Email);
      return false;
    }
  }
}

