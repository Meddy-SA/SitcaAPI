namespace Sitca.Models;

public class ApiResponse<T>
{
  public T? Data { get; set; }
  public string Message { get; set; } = null!;
  public bool Success { get; set; }
}
