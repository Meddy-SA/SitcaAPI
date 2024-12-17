using Core.Services.Email;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data;
using Sitca.DataAccess.Data.Repository.Constants;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Services.ViewToString;
using Sitca.Models;
using Sitca.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Services.Notification;

public class NotificationService : INotificationService
{
  private readonly ApplicationDbContext _db;
  private readonly IDapper _dapper;
  private readonly IEmailSender _emailSender;
  private readonly IViewRenderService _viewRenderService;
  private readonly IConfiguration _config;
  private readonly ILogger<NotificationService> _logger;

  public NotificationService(
      ApplicationDbContext db,
      IDapper dapper,
      IEmailSender emailSender,
      IViewRenderService viewRenderService,
      IConfiguration config,
      ILogger<NotificationService> logger)
  {
    _db = db ?? throw new ArgumentNullException(nameof(db));
    _dapper = dapper ?? throw new ArgumentNullException(nameof(dapper));
    _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
    _viewRenderService = viewRenderService ?? throw new ArgumentNullException(nameof(viewRenderService));
    _config = config ?? throw new ArgumentNullException(nameof(config));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public async Task<bool> HasBeenNotifiedAsync(string userId, int certificacionId)
  {
    return await _db.NotificacionesEnviadas
        .AnyAsync(n => n.UserId == userId &&
                      n.CertificacionId == certificacionId &&
                      n.FechaNotificacion >= DateTime.UtcNow.AddMonths(-1));
  }

  public async Task SendExpirationNotificationAsync(ApplicationUser user, CertificacionDetailsVm certification)
  {
    try
    {
      var notificacionTemplate = await GetNotificationTemplate(NotificationTypes.Expiracion);
      if (notificacionTemplate == null)
      {
        _logger.LogWarning("No se encontró plantilla de notificación para vencimiento");
        return;
      }

      var recipientGroups = await GetNotificationRecipients(user, certification);

      foreach (var recipient in recipientGroups.InternalRecipients)
      {
        await SendInternalNotificationAsync(recipient, certification, notificacionTemplate);
      }

      foreach (var recipient in recipientGroups.CompanyRecipients)
      {
        await SendCompanyNotificationAsync(recipient, certification, notificacionTemplate);
      }

      // Registrar la notificación enviada
      await RegisterNotificationSentAsync(user.Id, certification.Id);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error al enviar notificación de vencimiento para certificación {CertificacionId}",
          certification.Id);
      throw;
    }
  }

  private async Task<Notificacion> GetNotificationTemplate(NotificationTypes type)
  {
    return await _db.Notificacion
        .AsNoTracking()
        .Include(x => x.NotificationGroups)
        .FirstOrDefaultAsync(s => s.Status == (int)type);
  }

  private async Task SendInternalNotificationAsync(
      UsersListVm recipient,
      CertificacionDetailsVm certification,
      Notificacion template)
  {
    var notificationModel = new NotificacionSigleVm
    {
      Data = template,
      User = recipient
    };

    var emailContent = await _viewRenderService.RenderToStringAsync(
        "EmailStatusTemplate",
        notificationModel);

    emailContent = emailContent.Replace("{0}",
        $"{certification.Empresa.Nombre} ({certification.Empresa.Pais})");

    await _emailSender.SendEmailBrevoAsync(
        recipient.Email,
        template.TituloInterno,
        emailContent);

    _logger.LogInformation(
        "Notificación interna enviada a {Email} para certificación {CertificacionId}",
        recipient.Email,
        certification.Id);
  }

  private async Task SendCompanyNotificationAsync(
      UsersListVm recipient,
      CertificacionDetailsVm certification,
      Notificacion template)
  {
    var notificationModel = new NotificacionSigleVm
    {
      Data = template,
      User = recipient
    };

    var emailContent = await _viewRenderService.RenderToStringAsync(
        "EmailStatusTemplate",
        notificationModel);

    await _emailSender.SendEmailBrevoAsync(
        recipient.Email,
        template.TituloParaEmpresa,
        emailContent);

    _logger.LogInformation(
        "Notificación empresarial enviada a {Email} para certificación {CertificacionId}",
        recipient.Email,
        certification.Id);
  }

  private async Task<NotificationRecipientGroups> GetNotificationRecipients(
      ApplicationUser user,
      CertificacionDetailsVm certification
  )
  {
    var recipients = new NotificationRecipientGroups();

    // Obtener usuarios internos (Admin, TecnicoPais)
    var internalRoles = await _db.Roles
        .Where(r => r.Name == "Admin" || r.Name == "TecnicoPais")
        .ToListAsync();

    var empresa = await _db.Empresa
      .AsNoTracking()
      .Include(x => x.Pais)
      .Where(x => x.Id == certification.Empresa.Id)
      .FirstOrDefaultAsync();

    foreach (var role in internalRoles)
    {
      var usersInRole = await GetUsersInRole(role.Id, empresa.Pais.Id);
      recipients.InternalRecipients.AddRange(usersInRole);
    }

    // Obtener usuarios de la empresa
    var companyUsers = await GetCompanyUsers(certification.Empresa.Id);
    recipients.CompanyRecipients.AddRange(companyUsers);

    return recipients;
  }

  private async Task<List<UsersListVm>> GetCompanyUsers(int empresaId)
  {
    var users = new List<UsersListVm>();
    var encargado = await _db.ApplicationUser
        .FirstOrDefaultAsync(x => x.EmpresaId == empresaId);

    if (encargado != null)
    {
      users.Add(new UsersListVm
      {
        Rol = "Empresa",
        FirstName = encargado.FirstName,
        LastName = encargado.LastName,
        Email = encargado.Email,
        Lang = encargado.Lenguage
      });
    }

    return users;
  }

  private async Task<List<UsersListVm>> GetUsersInRole(string roleId, int paisId)
  {
    var parameters = new DynamicParameters();
    parameters.Add("Pais", paisId);
    parameters.Add("Role", roleId);

    var users = await Task.FromResult(_dapper.GetAll<UsersListVm>(
        "[dbo].[GetUsersByRole]",
        parameters,
        commandType: CommandType.StoredProcedure));

    return users.Where(u => u.Notificaciones && u.Active).ToList();
  }

  private async Task RegisterNotificationSentAsync(string userId, int certificacionId)
  {
    var notificacion = new NotificacionesEnviadas
    {
      UserId = userId,
      CertificacionId = certificacionId,
      FechaNotificacion = DateTime.UtcNow
    };

    _db.NotificacionesEnviadas.Add(notificacion);
    await _db.SaveChangesAsync();
  }

  public async Task<NotificacionVm> SendNotification(int idCertificacion, int? status, string lang = "es")
  {
    try
    {
      var (certificacion, empresa, paisData) =
        await GetCertificationDetailsAsync(idCertificacion);
      if (certificacion == null) return null;

      var notifData = await GetNotificationDataAsync(status ?? empresa.Estado);
      if (notifData == null) return null;

      UpdateNotificationTextForRecertification(notifData, certificacion.Recertificacion, lang);

      if (lang != "es")
      {
        notifData = SetTextLanguages(notifData, lang);
      }

      var usersToNotify = await GetUsersToNotifyAsync(notifData, certificacion, empresa);

      await SendNotificationsToAllUsersAsync(usersToNotify, notifData, certificacion, empresa, paisData);

      return new NotificacionVm
      {
        Data = notifData,
        Users = usersToNotify
      };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error sending notification for certification {CertificationId}", idCertificacion);
      throw;
    }
  }

  public async Task<NotificacionVm> SendNotificacionSpecial(int idEmpresa, NotificationTypes notifType, string lang = "es")
  {
    try
    {
      var empresa = await _db.Empresa
          .FindAsync(idEmpresa) ?? throw new KeyNotFoundException($"Empresa {idEmpresa} not found");

      var notifData = await GetNotificationDataAsync((int)notifType);
      if (notifData == null) return null;

      if (lang != "es")
      {
        notifData = SetTextLanguages(notifData, lang);
      }

      var usersToNotify = await GetUsersToNotifyForSpecialAsync(notifData, empresa);
      var paisData = await _db.Pais.FindAsync(empresa.PaisId);

      await SendNotificationsToAllUsersAsync(usersToNotify, notifData, null, empresa, paisData);

      return new NotificacionVm
      {
        Data = notifData,
        Users = usersToNotify
      };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error sending special notification for company {CompanyId}", idEmpresa);
      throw;
    }
  }

  private async Task<List<UsersListVm>> GetUsersToNotifyForSpecialAsync(
      Notificacion notifData,
      Empresa empresa)
  {
    var usersToNotify = new List<UsersListVm>();

    // Obtener rol de empresa
    var empresaRole = await _db.Roles.FirstAsync(s => s.Name == "Empresa");

    // Obtener usuarios por rol
    foreach (var item in notifData.NotificationGroups.Where(s => s.RoleId != empresaRole.Id))
    {
      var dbPara = new DynamicParameters();
      dbPara.Add("Pais", empresa.PaisId);
      dbPara.Add("Role", item.RoleId);

      var users = await Task.FromResult(_dapper.GetAll<UsersListVm>(
          "[dbo].[GetUsersByRole]",
          dbPara,
          commandType: CommandType.StoredProcedure));

      if (users.Any())
      {
        usersToNotify.AddRange(users.Where(s => s.Notificaciones && s.Active));
      }
    }

    // Agregar usuarios administradores
    var adminUsers = usersToNotify.Where(s => s.Rol != "Asesor" && s.Rol != "Auditor");
    usersToNotify.AddRange(adminUsers);

    // Agregar cuentas especiales
    try
    {
      var cuentasEspeciales = await _db.NotificationCustomUsers
          .Where(s => s.PaisId == empresa.PaisId || s.Global)
          .ToListAsync();

      foreach (var cuenta in cuentasEspeciales)
      {
        usersToNotify.Add(new UsersListVm
        {
          Email = cuenta.Email,
          FirstName = cuenta.Name,
          Rol = "Admin",
          Lang = "es"
        });
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error al obtener cuentas especiales");
    }

    return usersToNotify;
  }

  private async Task<(ProcesoCertificacion certificacion, Empresa empresa, Pais paisData)>
      GetCertificationDetailsAsync(int idCertificacion)
  {
    var certificacion = await _db.ProcesoCertificacion
      .AsNoTracking()
        .Include(s => s.Empresa)
        .FirstOrDefaultAsync(x => x.Id == idCertificacion);

    if (certificacion == null) return (null, null, null);

    var paisData = await _db.Pais
      .FindAsync(certificacion.Empresa.PaisId);
    return (certificacion, certificacion.Empresa, paisData);
  }

  private async Task<Notificacion> GetNotificationDataAsync(decimal? status)
  {
    return await _db.Notificacion
      .AsNoTracking()
        .Include(x => x.NotificationGroups)
        .FirstOrDefaultAsync(s => s.Status == status);
  }

  private async Task<List<UsersListVm>> GetUsersToNotifyAsync(
      Notificacion notifData,
      ProcesoCertificacion certificacion,
      Empresa empresa)
  {
    var usersToNotify = new List<UsersListVm>();
    var empresaRole = await GetEmpresaRoleAsync();

    await AddNonCompanyUsersAsync(usersToNotify, notifData, certificacion, empresaRole.Id);
    await AddAdminUsersAsync(usersToNotify);
    await AddSpecialAccountUsersAsync(usersToNotify, empresa.PaisId.Value);
    await AddSpecificRoleUsersAsync(usersToNotify, certificacion);

    return usersToNotify.Distinct().ToList();
  }

  private async Task<IdentityRole> GetEmpresaRoleAsync()
  {
    return await _db.Roles.FirstAsync(s => s.Name == "Empresa")
        ?? throw new InvalidOperationException("Empresa role not found");
  }

  private async Task AddNonCompanyUsersAsync(
      List<UsersListVm> usersToNotify,
      Notificacion notifData,
      ProcesoCertificacion certificacion,
      string empresaRoleId)
  {
    var nonCompanyGroups = notifData.NotificationGroups
        .Where(s => s.RoleId != empresaRoleId);

    foreach (var group in nonCompanyGroups)
    {
      var users = await GetUsersByRoleAndCountryAsync(
          group.RoleId,
          certificacion.Empresa.Pais.Id);

      var activeUsers = users
          .Where(s => s.Notificaciones && s.Active)
          .ToList();

      usersToNotify.AddRange(activeUsers);
    }
  }

  private async Task<List<UsersListVm>> GetUsersByRoleAndCountryAsync(
      string roleId,
      int paisId)
  {
    var parameters = new DynamicParameters();
    parameters.Add("Pais", paisId);
    parameters.Add("Role", roleId);

    return await Task.FromResult(
        _dapper.GetAll<UsersListVm>(
            "[dbo].[GetUsersByRole]",
            parameters,
            commandType: CommandType.StoredProcedure));
  }

  private async Task AddAdminUsersAsync(List<UsersListVm> usersToNotify)
  {
    var adminUsers = await Task.Run(() => usersToNotify
        .Where(s => s.Rol != "Asesor" && s.Rol != "Auditor")
        .ToList());

    usersToNotify.AddRange(adminUsers);
  }

  private async Task AddSpecialAccountUsersAsync(
      List<UsersListVm> usersToNotify,
      int paisId)
  {
    try
    {
      var specialAccounts = await _db.NotificationCustomUsers
        .AsNoTracking()
          .Where(s => s.PaisId == paisId || s.Global)
          .ToListAsync();

      var specialUsers = specialAccounts.Select(account => new UsersListVm
      {
        Email = account.Email,
        FirstName = account.Name,
        Rol = "Admin",
        Lang = "es"
      });

      usersToNotify.AddRange(specialUsers);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting special account users for country {PaisId}", paisId);
    }
  }

  private async Task AddSpecificRoleUsersAsync(
      List<UsersListVm> usersToNotify,
      ProcesoCertificacion certificacion)
  {
    await AddRoleUserIfExistsAsync(
        usersToNotify,
        certificacion.AsesorId,
        "Asesor");

    await AddRoleUserIfExistsAsync(
        usersToNotify,
        certificacion.AuditorId,
        "Auditor");
  }

  private async Task AddRoleUserIfExistsAsync(
      List<UsersListVm> usersToNotify,
      string userId,
      string roleName)
  {
    if (string.IsNullOrEmpty(userId)) return;

    var user = await Task.Run(() => usersToNotify.FirstOrDefault(s => s.Id == userId));
    if (user != null)
    {
      _logger.LogInformation(
          "Adding {RoleName} user {UserId} to notification list",
          roleName,
          userId);

      usersToNotify.Add(user);
    }
    else
    {
      _logger.LogWarning(
          "{RoleName} user {UserId} not found in users list",
          roleName,
          userId);
    }
  }

  private async Task SendNotificationsToAllUsersAsync(
      List<UsersListVm> usersToNotify,
      Notificacion notifData,
      ProcesoCertificacion certificacion,
      Empresa empresa,
      Pais paisData)
  {
    var senderEmail = _config["EmailSender:UserName"];
    var empresaUser = await GetCompanyUserAsync(empresa);

    if (empresaUser != null)
    {
      await SendCompanyNotificationAsync(empresaUser, notifData, empresa, senderEmail);
    }

    foreach (var user in usersToNotify.Where(s => s.Rol != "Empresa"))
    {
      await SendInternalNotificationAsync(user, notifData, empresa, paisData, senderEmail);
    }
  }

  private async Task<UsersListVm> GetCompanyUserAsync(Empresa empresa)
  {
    if (empresa == null) return null;

    var encargado = await _db.ApplicationUser
      .AsNoTracking()
        .FirstOrDefaultAsync(x => x.EmpresaId == empresa.Id);

    if (encargado == null) return null;

    return new UsersListVm
    {
      Rol = "Empresa",
      FirstName = empresa.Nombre ?? $"{encargado.FirstName} {encargado.LastName}",
      Email = encargado.Email,
      Lang = encargado.Lenguage
    };
  }

  private async Task SendCompanyNotificationAsync(
      UsersListVm empresaUser,
      Notificacion notifData,
      Empresa empresa,
      string senderEmail)
  {
    var emailContent = await RenderEmailTemplateAsync(new NotificacionSigleVm
    {
      Data = notifData,
      User = empresaUser
    });

    await _emailSender.SendEmailBrevoAsync(
        empresaUser.Email,
        notifData.TituloParaEmpresa,
        emailContent);

    // Send to additional company email if exists
    if (!string.IsNullOrEmpty(empresa.Email))
    {
      try
      {
        await _emailSender.SendEmailBrevoAsync(
            empresa.Email,
            notifData.TituloParaEmpresa,
            emailContent);
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Failed to send notification to additional company email: {Email}", empresa.Email);
      }
    }
  }

  private async Task SendInternalNotificationAsync(
      UsersListVm user,
      Notificacion notifData,
      Empresa empresa,
      Pais paisData,
      string senderEmail)
  {
    var emailContent = await RenderEmailTemplateAsync(new NotificacionSigleVm
    {
      Data = notifData,
      User = user
    });

    emailContent = emailContent.Replace("{0}", $"{empresa.Nombre} ({paisData?.Name ?? "Unknown"})");

    await _emailSender.SendEmailBrevoAsync(
        user.Email,
        notifData.TituloInterno,
        emailContent);
  }

  private async Task<string> RenderEmailTemplateAsync(NotificacionSigleVm model)
  {
    return await _viewRenderService.RenderToStringAsync("EmailStatusTemplate", model);
  }

  // Helper methods...
  private void UpdateNotificationTextForRecertification(Notificacion notifData, bool isRecertification, string lang)
  {
    if (!isRecertification) return;

    var (searchText, replaceText) = lang == "es"
        ? ("certificación", "re certificación")
        : ("certification", "re certification");

    notifData.TituloParaEmpresa = UpdateText(notifData.TituloParaEmpresa, searchText, replaceText);
    notifData.TextoParaEmpresa = UpdateText(notifData.TextoParaEmpresa, searchText, replaceText);
    notifData.TextoInterno = UpdateText(notifData.TextoInterno, searchText, replaceText);
    notifData.TituloInterno = UpdateText(notifData.TituloInterno, searchText, replaceText);
  }

  private string UpdateText(string text, string search, string replace)
  {
    return text?.ToLower().Contains(search) == true
         ? text.Replace(search, replace)
         : text;
  }

  public Notificacion SetTextLanguages(Notificacion notificationData, string language)
  {
    if (language != "en") return notificationData;

    return new Notificacion
    {
      TextoInterno = notificationData.TextoInternoEn ?? notificationData.TextoInterno,
      TextoParaEmpresa = notificationData.TextoParaEmpresaEn ?? notificationData.TextoParaEmpresa,
      TituloInterno = notificationData.TituloInternoEn ?? notificationData.TituloInterno,
      TituloParaEmpresa = notificationData.TituloParaEmpresaEn ?? notificationData.TituloParaEmpresa,
      NotificationGroups = notificationData.NotificationGroups,
      Status = notificationData.Status
    };
  }
}
