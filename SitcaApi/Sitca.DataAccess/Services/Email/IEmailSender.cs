using System.Threading.Tasks;
namespace Core.Services.Email
{
  public interface IEmailSender
  {
    Task SendEmailBrevoAsync(string toAddress, string subject, string message);

    Task SendEmailAsync(string fromAddress, string toAddress, string subject, string message);

    Task SendEmailWithTemplateAsync(string fromAddress, string toAddress, string subject, string message);
  }
}
