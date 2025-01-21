using Sitca.Models.ViewModels;

namespace Sitca.Models.DTOs;

public class RegisterDTO
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Empresa { get; set; } = null!;
    public string Language { get; set; } = "es";
    public List<CommonVm> Tipologias { get; set; } = null!;
    public string Representante { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public int? CompanyId { get; set; }
    public int CountryId { get; set; }
    public bool AcceptTerms { get; set; } = true;
}
