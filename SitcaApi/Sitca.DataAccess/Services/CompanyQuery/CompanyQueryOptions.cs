using Sitca.Models;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Services.CompanyQuery;

public class CompanyQueryOptions
{
    public CompanyQueryType QueryType { get; set; }
    public ApplicationUser User { get; set; }
    public string Role { get; set; }
    public CompanyFilterDTO Filter { get; set; }
    public string Language { get; set; } = "es";
    public bool IncludeHomologacion { get; set; } = false;
    public int? DistintivoId { get; set; }
}
