namespace Sitca.Models.DTOs;

public class CompanyFilterDTO
{
  public string Name { get; set; } = null!;
  public int CountryId { get; set; }
  public int TypologyId { get; set; }
  public int DistinctiveId { get; set; }
  public int? StatusId { get; set; }
}
