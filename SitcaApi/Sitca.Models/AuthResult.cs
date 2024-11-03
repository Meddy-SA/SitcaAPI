using Sitca.Models.DTOs;

namespace Sitca.Models;
public class AuthResult
{
  public bool Succeeded { get; private set; }
  public string? Token { get; private set; }
  public IList<string> Errors { get; private set; }
  public UserInfoDTO? User { get; private set; }
  public IList<string>? Roles { get; private set; }
  public string? RoleText { get; set; }
  public string? Country { get; set; }

  private AuthResult()
  {
    Succeeded = false;
    Errors = new List<string>();
    Token = null;
    User = null;
    Roles = null;
  }

  public static AuthResult Success(string token, ApplicationUser user, IList<string> roles, string roleText, string country)
  {
    return new AuthResult
    {
      Succeeded = true,
      Token = token,
      User = new UserInfoDTO
      {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        FirstName = user.FirstName ?? string.Empty,
        LastName = user.LastName ?? string.Empty,
        Language = user.Lenguage ?? "es"
      },
      Roles = roles,
      RoleText = roleText,
      Country = country
    };
  }

  public static AuthResult Failed(string error)
  {
    var result = new AuthResult
    {
      Succeeded = false
    };
    result.Errors.Add(error);
    return result;
  }

  public static AuthResult Failed(IEnumerable<string> errors)
  {
    var result = new AuthResult
    {
      Succeeded = false
    };
    foreach (var error in errors)
    {
      result.Errors.Add(error);
    }
    return result;
  }
}

