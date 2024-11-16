using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Core.Services.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Services.Email
{
  public class EmailSender : IEmailSender
  {
    private readonly ILogger<EmailSender> _logger;
    private readonly HttpClient _httpClient;
    private readonly EmailConfiguration _emailConfig;
    private readonly AsyncRetryPolicy<bool> _retryPolicy;


    public EmailSender(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<EmailSender> logger)
    {
      _logger = logger;
      _emailConfig = configuration.GetSection("EmailSender").Get<EmailConfiguration>()
        ?? throw new ArgumentNullException(nameof(configuration), "La configuración de email es requerida");
      _httpClient = httpClientFactory.CreateClient("BrevoClient");
      ConfigureHttpClient();

      // Configurar política de reintentos
      _retryPolicy = Policy<bool>
          .Handle<SmtpException>()
          .Or<HttpRequestException>()
          .WaitAndRetryAsync(
              3,
              retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
              (outcome, timeSpan, attemptNumber, context) =>
              {
                _logger.LogWarning("Intento {AttemptNumber} fallido al enviar email. Reintentando en {RetrySeconds} segundos. Error: {Error}",
                          attemptNumber,
                          timeSpan.TotalSeconds,
                          outcome.Exception?.GetType().Name + ": " + outcome.Exception?.Message);
              });
    }

    public async Task SendEmailBrevoAsync(string toAddress, string subject, string message)
    {
      ValidateEmailParameters(toAddress, subject, message);

      try
      {
        var emailRequest = CreateBrevoEmailRequest(toAddress, subject, message);
        var success = await _retryPolicy.ExecuteAsync(async () =>
                {
                  var response = await SendBrevoRequest(emailRequest);
                  return response;
                });

        if (!success)
        {
          throw new EmailSendException("No se pudo enviar el email después de varios intentos");
        }
      }
      catch (Exception ex) when (ex is not EmailSendException)
      {
        _logger.LogError(ex, "Error al enviar email via Brevo a {ToAddress}", toAddress);
        throw new EmailSendException("An error occurred while sending email via Brevo", ex);
      }
    }

    public async Task SendEmailAsync(string fromAddress, string toAddress, string subject, string message)
    {
      await SendEmailInternalAsync(fromAddress, toAddress, subject, message, false);
    }

    public async Task SendEmailWithTemplateAsync(string fromAddress, string toAddress, string subject, string message)
    {
      await SendEmailInternalAsync(fromAddress, toAddress, subject, message, true);
    }

    private void ConfigureHttpClient()
    {
      _httpClient.DefaultRequestHeaders.Accept.Clear();
      _httpClient.DefaultRequestHeaders.Accept.Add(
          new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
      _httpClient.DefaultRequestHeaders.Add("api-key", _emailConfig.ApiKey);
    }

    private async Task SendEmailInternalAsync(
              string fromAddress,
              string toAddress,
              string subject,
              string message,
              bool isTemplate)
    {
      ValidateEmailParameters(toAddress, subject, message);

      fromAddress = string.IsNullOrEmpty(fromAddress) || fromAddress == "apikey"
          ? _emailConfig.DefaultFromAddress
          : fromAddress;

      try
      {
        await _retryPolicy.ExecuteAsync(async () =>
        {
          using var mailMessage = CreateMailMessage(fromAddress, toAddress, subject, message);
          using var client = CreateSmtpClient();

          await client.SendMailAsync(mailMessage);
          _logger.LogInformation(
                        "Email {Template} enviado exitosamente a {ToAddress}",
                        isTemplate ? "con plantilla" : "simple",
                        toAddress);
          return true;
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(
            ex,
            "Error al enviar email {Template} a {ToAddress}",
            isTemplate ? "con plantilla" : "simple",
            toAddress);
        throw new EmailSendException(
            $"Error al enviar email {(isTemplate ? "con plantilla" : "simple")}", ex);
      }
    }

    private void ValidateEmailParameters(string toAddress, string subject, string message)
    {
      if (string.IsNullOrWhiteSpace(toAddress))
        throw new ArgumentException("La dirección de destino es requerida", nameof(toAddress));

      if (string.IsNullOrWhiteSpace(subject))
        throw new ArgumentException("El asunto es requerido", nameof(subject));

      if (string.IsNullOrWhiteSpace(message))
        throw new ArgumentException("El mensaje es requerido", nameof(message));

      if (!IsValidEmail(toAddress))
        throw new ArgumentException("La dirección de email no es válida", nameof(toAddress));
    }

    private bool IsValidEmail(string email)
    {
      try
      {
        var addr = new System.Net.Mail.MailAddress(email);
        return addr.Address == email;
      }
      catch
      {
        return false;
      }
    }

    private BrevoEmailRequest CreateBrevoEmailRequest(string toAddress, string subject, string message)
    {
      return new BrevoEmailRequest
      {
        Sender = new EmailContact
        {
          Name = _emailConfig.SenderName,
          Email = _emailConfig.SenderEmail
        },
        To = new List<EmailContact>
        {
            new() { Email = toAddress }
        },
        HtmlContent = message,
        Subject = subject,
        Headers = new Dictionary<string, string> {
          { "api-key", _emailConfig.ApiKey }
        },
        Tags = new List<string> { "sitca", "notification" }
      };
    }

    private async Task<bool> SendBrevoRequest(BrevoEmailRequest emailRequest)
    {
      var jsonContent = JsonSerializer.Serialize(emailRequest, new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      });

      var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync(_emailConfig.ApiEndpoint, content);

      if (!response.IsSuccessStatusCode)
      {
        var errorContent = await response.Content.ReadAsStringAsync();
        _logger.LogError(
            "Error en la respuesta de Brevo. Status: {StatusCode}, Error: {Error}",
            response.StatusCode,
            errorContent);
        return false;
      }

      return true;
    }

    private MailMessage CreateMailMessage(string fromAddress, string toAddress, string subject, string message)
    {
      var mailMessage = new MailMessage(fromAddress, toAddress, subject, message)
      {
        IsBodyHtml = true
      };
      return mailMessage;
    }

    private SmtpClient CreateSmtpClient()
    {
      return new SmtpClient(_emailConfig.Host, _emailConfig.Port)
      {
        Credentials = new NetworkCredential(_emailConfig.ApiKey, ""),
        EnableSsl = true,
        DeliveryMethod = SmtpDeliveryMethod.Network
      };
    }

    public class EmailSendException : Exception
    {
      public EmailSendException(string message) : base(message) { }
      public EmailSendException(string message, Exception innerException)
          : base(message, innerException) { }
    }
  }
}
