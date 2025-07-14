namespace Sitca.Models.DTOs;

public class GetUsersRequest
{
  public string PaisId { get; set; } = null!;
  public string? Query { get; set; }
  public string RoleName { get; set; } = "All";
  public string ActiveFilter { get; set; } = "Todos";
}
