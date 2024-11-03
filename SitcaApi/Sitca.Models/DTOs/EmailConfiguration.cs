namespace Sitca.Models.DTOs;

public class EmailConfiguration
{
  public string ApiEndpoint { get; set; } = null!;
  public string ApiKey { get; set; } = null!;
  public string SenderName { get; set; } = null!;
  public string SenderEmail { get; set; } = null!;
  public string Host { get; set; } = null!;
  public int Port { get; set; }
  public string Username { get; set; } = null!;
  public string Password { get; set; } = null!;
  public string DefaultFromAddress { get; set; } = "notificaciones@calidadcentroamerica.com";
}
