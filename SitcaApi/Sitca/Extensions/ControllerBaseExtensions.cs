using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sitca.Models;

namespace Sitca.Extensions;

public static class ControllerBaseExtensions
{
  /// <summary>
  /// Gets the current authenticated ApplicationUser with basic user information
  /// </summary>
  /// <param name="controller">The controller instance</param>
  /// <param name="userManager">The UserManager instance</param>
  /// <returns>The current ApplicationUser or null if not authenticated</returns>
  public static async Task<ApplicationUser> GetCurrentUserAsync(
      this ControllerBase controller,
      UserManager<ApplicationUser> userManager)
  {
    var email = controller.User.FindFirstValue(ClaimTypes.Email);
    if (string.IsNullOrEmpty(email)) return null;

    var user = await userManager.FindByEmailAsync(email);
    return user as ApplicationUser;
  }

  /// <summary>
  /// Gets the current authenticated IdentityUser with basic user information
  /// </summary>
  /// <param name="controller">The controller instance</param>
  /// <param name="userManager">The UserManager instance</param>
  /// <returns>The current IdentityUser or null if not authenticated</returns>
  public static async Task<IdentityUser> GetIdentityUserAsync(
      this ControllerBase controller,
      UserManager<ApplicationUser> userManager)
  {
    var email = controller.User.FindFirstValue(ClaimTypes.Email);
    if (string.IsNullOrEmpty(email)) return null;

    return await userManager.FindByEmailAsync(email);
  }

  /// <summary>
  /// Gets the current user's role from claims
  /// </summary>
  /// <param name="controller">The controller instance</param>
  /// <returns>The current user's role or null if not found</returns>
  public static string GetCurrentUserRole(this ControllerBase controller)
  {
    return controller.User.FindFirstValue(ClaimTypes.Role);
  }

  /// <summary>
  /// Gets both the current user and their role in a single object
  /// </summary>
  /// <param name="controller">The controller instance</param>
  /// <param name="userManager">The UserManager instance</param>
  /// <returns>Tuple containing the user and their role</returns>
  public static async Task<(ApplicationUser User, string Role)> GetCurrentUserWithRoleAsync(
      this ControllerBase controller,
      UserManager<ApplicationUser> userManager)
  {
    var user = await controller.GetCurrentUserAsync(userManager);
    var role = controller.GetCurrentUserRole();
    return (user, role);
  }

  public static ActionResult<T> HandleResponse<T>(this ControllerBase controller, T response, int statusCode = StatusCodes.Status200OK)
  {
    switch (statusCode)
    {
      case StatusCodes.Status200OK:
        return controller.Ok(response);
      case StatusCodes.Status201Created:
        return controller.Created("", response);
      case StatusCodes.Status204NoContent:
        return controller.NoContent();
      case StatusCodes.Status400BadRequest:
        return controller.BadRequest();
      case StatusCodes.Status404NotFound:
        return controller.NotFound();
      case StatusCodes.Status401Unauthorized:
        return controller.Unauthorized();
      case StatusCodes.Status403Forbidden:
        return controller.Forbid();
      default:
        return controller.BadRequest();

    }
  }
}
