namespace Sitca.Models.DTOs;

public class BrevoEmailRequest
{
  public EmailContact Sender { get; set; } = null!;
  public List<EmailContact> To { get; set; } = null!;
  public string HtmlContent { get; set; } = null!;
  public string Subject { get; set; } = null!;
  public EmailContact ReplyTo { get; set; } = null!;
  public List<string> Tags { get; set; } = null!;
}
