using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Core.Services.Email;
using Microsoft.Extensions.Configuration;


namespace Sitca.DataAccess.Services.Email
{
    public class EmailSender: IEmailSender
    {
        private readonly IConfiguration _config;
        

        public EmailSender(IConfiguration config)
        {
            _config = config;            
        }

        public async Task SendEmailAsync(string fromAddress, string toAddress, string subject, string message)
        {
            if (fromAddress == "apikey")
            {
                fromAddress = "notificaciones@calidadcentroamerica.com";
            }

            var mailMessage = new MailMessage(fromAddress, toAddress, subject, message);
            mailMessage.IsBodyHtml = true;
            using (var client = new SmtpClient(_config["EmailSender:Host"], int.Parse(_config["EmailSender:Port"]))
            {
                Credentials = new NetworkCredential(_config["EmailSender:Username"], _config["EmailSender:Password"])
            })
            {
                try
                {
                    await client.SendMailAsync(mailMessage);
                }
                catch (System.Exception e)
                {

                    var a = e;
                }
                
            }
        }

        public async Task SendEmailWithTemplateAsync(string fromAddress, string toAddress, string subject, string message)
        {
            if (fromAddress == "apikey")
            {
                fromAddress = "notificaciones@calidadcentroamerica.com";
            }

            var mailMessage = new MailMessage(fromAddress, toAddress, subject, message);
            mailMessage.IsBodyHtml = true;
            using (var client = new SmtpClient(_config["EmailSender:Host"], int.Parse(_config["EmailSender:Port"]))
            {
                Credentials = new NetworkCredential(_config["EmailSender:Username"], _config["EmailSender:Password"])
            })
            {
                try
                {
                    await client.SendMailAsync(mailMessage);
                }
                catch (System.Exception e)
                {

                    var a = e;
                }

            }
        }        

    }
}
