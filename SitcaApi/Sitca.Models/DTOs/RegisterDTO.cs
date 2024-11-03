namespace Sitca.Models.DTOs;

public class RegisterDTO
{
  public string Email { get; set; } = null!;
  public string Password { get; set; } = null!;
  public string ConfirmPassword { get; set; } = null!;
  public string FirstName { get; set; } = null!;
  public string LastName { get; set; } = null!;
  public string PhoneNumber { get; set; } = null!;
  public string Language { get; set; } = "es";
  public int CountryId { get; set; }
  public bool AcceptTerms { get; set; }
}
