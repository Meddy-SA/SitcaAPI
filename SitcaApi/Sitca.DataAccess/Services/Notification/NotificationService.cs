using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
using ConstantRoles = Utilities.Common.Constants.Roles;

namespace Sitca.DataAccess.Services.Notification;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db; // Database context for entity operations
    private readonly IDapper _dapper; // Dapper for optimized database queries
    private readonly IEmailSender _emailSender; // Email sending service
    private readonly IViewRenderService _viewRenderService; // Service for rendering email templates
    private readonly IConfiguration _config; // Configuration access
    private readonly ILogger<NotificationService> _logger; // Logging service
    private const string GET_USERS_BY_ROLE_SP = "[dbo].[GetUsersByRole]";

    public NotificationService(
        ApplicationDbContext db,
        IDapper dapper,
        IEmailSender emailSender,
        IViewRenderService viewRenderService,
        IConfiguration config,
        ILogger<NotificationService> logger
    )
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _dapper = dapper ?? throw new ArgumentNullException(nameof(dapper));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _viewRenderService =
            viewRenderService ?? throw new ArgumentNullException(nameof(viewRenderService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Checks if a user has been notified about a specific certification within the last month
    /// </summary>
    /// <param name="userId">The ID of the user to check</param>
    /// <param name="certificacionId">The ID of the certification</param>
    /// <returns>True if the user has been notified in the last month, false otherwise</returns>
    public async Task<bool> HasBeenNotifiedAsync(string userId, int certificacionId)
    {
        return await _db.NotificacionesEnviadas.AnyAsync(n =>
            n.UserId == userId
            && n.CertificacionId == certificacionId
            && n.FechaNotificacion >= DateTime.UtcNow.AddMonths(-1)
        );
    }

    /// <summary>
    /// Sends expiration notifications for a certification to relevant users
    /// </summary>
    /// <param name="user">The user associated with the certification</param>
    /// <param name="certification">Details of the certification</param>
    /// <param name="empresaId">ID of the company</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task SendExpirationNotificationAsync(
        ApplicationUser user,
        CertificacionDetailsVm certification,
        int empresaId
    )
    {
        try
        {
            var notificacionTemplate = await GetNotificationTemplate(NotificationTypes.Expiracion);
            if (notificacionTemplate == null)
            {
                _logger.LogWarning("No se encontró plantilla de notificación para vencimiento");
                return;
            }

            empresaId = certification.Empresa?.Id ?? empresaId;

            var recipientGroups = await GetNotificationRecipients(user, empresaId);
            var empresa = await GetEmpresaForId(empresaId);

            foreach (var recipient in recipientGroups.InternalRecipients)
            {
                await SendInternalNotificationAsync(recipient, empresa, notificacionTemplate);
            }

            foreach (var recipient in recipientGroups.CompanyRecipients)
            {
                await SendCompanyNotificationAsync(recipient, notificacionTemplate);
            }

            // Registrar la notificación enviada
            await RegisterNotificationSentAsync(user.Id, certification.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al enviar notificación de vencimiento para certificación {CertificacionId}",
                certification.Id
            );
        }
    }

    private async Task<Empresa> GetEmpresaForId(int id)
    {
        return await _db.Empresa.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
    }

    private async Task<Notificacion> GetNotificationTemplate(NotificationTypes type)
    {
        return await _db
            .Notificacion.AsNoTracking()
            .Include(x => x.NotificationGroups)
            .FirstOrDefaultAsync(s => s.Status == (int)type);
    }

    private async Task SendInternalNotificationAsync(
        UsersListVm recipient,
        Empresa empresa,
        Notificacion template
    )
    {
        var notificationModel = new NotificacionSigleVm { Data = template, User = recipient };

        var emailContent = await _viewRenderService.RenderToStringAsync(
            "EmailStatusTemplate",
            notificationModel
        );

        emailContent = emailContent.Replace("{0}", $"{empresa.Nombre} ({empresa.Pais})");

        await _emailSender.SendEmailBrevoAsync(
            recipient.Email,
            template.TituloInterno,
            emailContent
        );

        _logger.LogInformation(
            "Notificación interna enviada a {Email} para Empresa ID {Id}",
            recipient.Email,
            empresa.Id
        );
    }

    /// <summary>
    /// Sends notification to company users
    /// </summary>
    /// <param name="recipient">The recipient user</param>
    /// <param name="template">Notification template</param>
    private async Task SendCompanyNotificationAsync(UsersListVm recipient, Notificacion template)
    {
        var notificationModel = new NotificacionSigleVm { Data = template, User = recipient };

        var emailContent = await _viewRenderService.RenderToStringAsync(
            "EmailStatusTemplate",
            notificationModel
        );

        await _emailSender.SendEmailBrevoAsync(
            recipient.Email,
            template.TituloParaEmpresa,
            emailContent
        );

        _logger.LogInformation(
            "Notificación empresarial enviada a {Email} para certificación",
            recipient.Email
        );
    }

    /// <summary>
    /// Retrieves both internal and company recipients for notifications
    /// </summary>
    /// <param name="user">The user initiating the notification</param>
    /// <param name="empresaId">ID of the company</param>
    /// <returns>NotificationRecipientGroups containing internal and company recipients</returns>
    private async Task<NotificationRecipientGroups> GetNotificationRecipients(
        ApplicationUser user,
        int empresaId
    )
    {
        var recipients = new NotificationRecipientGroups();

        // Obtener usuarios internos (Admin, TecnicoPais)
        var internalRoles = await _db
            .Roles.Where(r => r.Name == ConstantRoles.Admin || r.Name == ConstantRoles.TecnicoPais)
            .ToListAsync();

        var empresa = await _db
            .Empresa.AsNoTracking()
            .Include(x => x.Pais)
            .Where(x => x.Id == empresaId)
            .FirstOrDefaultAsync();

        foreach (var role in internalRoles)
        {
            var usersInRole = await GetUsersInRole(role.Id, empresa.Pais.Id);
            recipients.InternalRecipients.AddRange(usersInRole);
        }

        // Obtener usuarios de la empresa
        var companyUsers = await GetCompanyUsers(empresaId);
        recipients.CompanyRecipients.AddRange(companyUsers);

        return recipients;
    }

    /// <summary>
    /// Retrieves users associated with a specific company
    /// </summary>
    /// <param name="empresaId">ID of the company</param>
    /// <returns>List of users from the company</returns>
    private async Task<List<UsersListVm>> GetCompanyUsers(int empresaId)
    {
        var users = new List<UsersListVm>();
        var encargado = await _db.ApplicationUser.FirstOrDefaultAsync(x =>
            x.EmpresaId == empresaId
        );

        if (encargado != null)
        {
            users.Add(
                new UsersListVm
                {
                    Rol = ConstantRoles.Empresa,
                    FirstName = encargado.FirstName,
                    LastName = encargado.LastName,
                    Email = encargado.Email,
                    Lang = encargado.Lenguage,
                }
            );
        }

        return users;
    }

    /// <summary>
    /// Retrieves users in a specific role for a country
    /// </summary>
    /// <param name="roleId">ID of the role</param>
    /// <param name="paisId">ID of the country</param>
    /// <returns>List of users in the specified role</returns>
    private async Task<List<UsersListVm>> GetUsersInRole(string roleId, int paisId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("Pais", paisId);
        parameters.Add("Role", roleId);

        var users = await Task.FromResult(
            _dapper.GetAll<UsersListVm>(
                "[dbo].[GetUsersByRole]",
                parameters,
                commandType: CommandType.StoredProcedure
            )
        );

        return users.Where(u => u.Notificaciones && u.Active).ToList();
    }

    private async Task RegisterNotificationSentAsync(string userId, int certificacionId)
    {
        var notificacion = new NotificacionesEnviadas
        {
            UserId = userId,
            CertificacionId = certificacionId,
            FechaNotificacion = DateTime.UtcNow,
        };

        _db.NotificacionesEnviadas.Add(notificacion);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Sends notifications when a certification process begins or changes status
    /// </summary>
    /// <param name="idCertificacion">ID of the certification process</param>
    /// <param name="status">Optional status to override company status</param>
    /// <param name="lang">Language code for the notification (default: "es")</param>
    /// <returns>NotificacionVm containing notification data and recipient users</returns>
    public async Task<NotificacionVm> SendNotification(
        int idCertificacion,
        int? status,
        string lang = "es"
    )
    {
        try
        {
            // 1. Obtiene los detalles de la certificación, empresa y país usando tuple deconstruction
            var (certificacion, empresa, paisData) = await GetCertificationDetailsAsync(
                idCertificacion
            );
            if (certificacion == null)
                return null;

            // 2. Obtiene la plantilla de notificación basada en el estado proporcionado o el estado de la empresa
            var notifData = await GetNotificationDataAsync(status ?? empresa.Estado);
            if (notifData == null)
                return null;

            // 3. Actualiza el texto si es una recertificación (cambia "certificación" por "re certificación")
            UpdateNotificationTextForRecertification(
                notifData,
                certificacion.Recertificacion,
                lang
            );

            // 4. Si el idioma no es español, obtiene los textos en el idioma solicitado
            if (lang != "es")
            {
                notifData = SetTextLanguages(notifData, lang);
            }

            // 5. Obtiene la lista de usuarios a notificar
            var usersToNotify = await GetUsersToNotifyAsync(notifData, certificacion, empresa);

            // 6. Envía las notificaciones a todos los usuarios
            await SendNotificationsToAllUsersAsync(
                usersToNotify,
                notifData,
                certificacion,
                empresa,
                paisData
            );

            // 7. Retorna el objeto con los datos de la notificación y los usuarios notificados
            return new NotificacionVm { Data = notifData, Users = usersToNotify };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error sending notification for certification {CertificationId}",
                idCertificacion
            );
            throw;
        }
    }

    /// <summary>
    /// Sends special notifications to a company
    /// </summary>
    /// <param name="idEmpresa">ID of the target company</param>
    /// <param name="notifType">Type of notification to send</param>
    /// <param name="lang">Language code for the notification (default: "es")</param>
    /// <returns>NotificacionVm containing notification data and recipient users</returns>
    public async Task<NotificacionVm> SendNotificacionSpecial(
        int idEmpresa,
        NotificationTypes notifType,
        string lang = "es"
    )
    {
        try
        {
            var empresa =
                await _db.Empresa.FindAsync(idEmpresa)
                ?? throw new KeyNotFoundException($"Empresa {idEmpresa} not found");

            var notifData = await GetNotificationDataAsync((int)notifType);
            if (notifData == null)
                return null;

            if (lang != "es")
            {
                notifData = SetTextLanguages(notifData, lang);
            }

            var usersToNotify = await GetUsersToNotifyForSpecialAsync(notifData, empresa);
            var paisData = await _db.Pais.FindAsync(empresa.PaisId);

            await SendNotificationsToAllUsersAsync(
                usersToNotify,
                notifData,
                null,
                empresa,
                paisData
            );

            return new NotificacionVm { Data = notifData, Users = usersToNotify };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error sending special notification for company {CompanyId}",
                idEmpresa
            );
            throw;
        }
    }

    private async Task<List<UsersListVm>> GetUsersToNotifyForSpecialAsync(
        Notificacion notifData,
        Empresa empresa
    )
    {
        var usersToNotify = new List<UsersListVm>();

        try
        {
            // Obtener rol de empresa
            var empresaRole = await _db
                .Roles.AsNoTracking()
                .FirstAsync(s => s.Name == ConstantRoles.Empresa);

            // Obtener grupos de notificación excluyendo el rol de empresa
            var notificationGroups = notifData
                .NotificationGroups.Where(s => s.RoleId != empresaRole.Id)
                .ToList();

            // Obtener usuarios por rol
            foreach (var item in notificationGroups)
            {
                var dbPara = new DynamicParameters();
                dbPara.Add("Pais", empresa.PaisId);
                dbPara.Add("Role", item.RoleId);

                var users = await Task.FromResult(
                    _dapper.GetAll<UsersListVm>(
                        "[dbo].[GetUsersByRole]",
                        dbPara,
                        commandType: CommandType.StoredProcedure
                    )
                );

                if (users.Any())
                {
                    // Agregar solo usuarios activos con notificaciones habilitadas
                    usersToNotify.AddRange(users.Where(s => s.Notificaciones && s.Active));
                }
            }

            // Agregar usuarios administradores
            var adminUsers = usersToNotify
                .Where(s => s.Rol != ConstantRoles.Asesor && s.Rol != ConstantRoles.Auditor)
                .ToList();
            usersToNotify.AddRange(adminUsers);

            var cuentasEspeciales = await _db
                .NotificationCustomUsers.AsNoTracking()
                .Where(s => s.PaisId == empresa.PaisId || s.Global)
                .ToListAsync();

            // Convertir cuentas especiales a UsersListVm
            var cuentasEspecialesVm = cuentasEspeciales
                .Select(cuenta => new UsersListVm
                {
                    Email = cuenta.Email,
                    FirstName = cuenta.Name,
                    Rol = ConstantRoles.Admin,
                    Lang = "es",
                })
                .ToList();

            usersToNotify.AddRange(cuentasEspecialesVm);

            // Eliminar duplicados basados en Email
            return usersToNotify.GroupBy(u => u.Email).Select(g => g.First()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener usuarios para notificación especial. EmpresaId: {EmpresaId}",
                empresa.Id
            );
            throw;
        }
    }

    private async Task<(
        ProcesoCertificacion certificacion,
        Empresa empresa,
        Pais paisData
    )> GetCertificationDetailsAsync(int idCertificacion)
    {
        // 1. Obtiene la certificación con su empresa relacionada
        var certificacion = await _db
            .ProcesoCertificacion.AsNoTracking()
            .Include(s => s.Empresa)
            .FirstOrDefaultAsync(x => x.Id == idCertificacion);

        if (certificacion == null)
            return (null, null, null);

        // 2. Obtiene los datos del país relacionado
        var paisData = await _db.Pais.FindAsync(certificacion.Empresa.PaisId);

        // 3. Retorna una tupla con todos los datos
        return (certificacion, certificacion.Empresa, paisData);
    }

    // Método que obtiene la plantilla de notificación según el estado
    private async Task<Notificacion> GetNotificationDataAsync(decimal? status)
    {
        // Obtiene la plantilla de notificación incluyendo los grupos de notificación
        return await _db
            .Notificacion.AsNoTracking()
            .Include(x => x.NotificationGroups)
            .FirstOrDefaultAsync(s => s.Status == status);
    }

    // Método que obtiene los usuarios a notificar
    private async Task<List<UsersListVm>> GetUsersToNotifyAsync(
        Notificacion notifData,
        ProcesoCertificacion certificacion,
        Empresa empresa
    )
    {
        var usersToNotify = new List<UsersListVm>();
        // 1. Obtiene el rol de empresa
        var empresaRole = await GetEmpresaRoleAsync();

        // 2. Agrega usuarios administradores
        await AddAdminUsersAsync(usersToNotify, notifData, certificacion, empresaRole.Id);
        // 3. Agrega usuarios de cuentas especiales
        await AddSpecialAccountUsersAsync(usersToNotify, empresa.PaisId.Value);
        // 4. Agrega usuarios con roles específicos (asesores, auditores)
        await AddSpecificRoleUsersAsync(usersToNotify, certificacion);

        // 5. Retorna la lista sin duplicados
        return usersToNotify.Distinct().ToList();
    }

    /// <summary>
    /// Obtiene el rol de empresa del sistema
    /// </summary>
    /// <returns>IdentityRole del rol Empresa</returns>
    /// <exception cref="InvalidOperationException">Si no se encuentra el rol Empresa</exception>
    private async Task<IdentityRole> GetEmpresaRoleAsync()
    {
        return await _db
                .Roles.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Name == ConstantRoles.Empresa)
            ?? throw new InvalidOperationException($"Rol {ConstantRoles.Empresa} no encontrado");
    }

    private async Task<List<UsersListVm>> GetUsersByRoleAndPaisAsync(
        string roleName,
        int paisId,
        string[] additionalRoles = null
    )
    {
        var users = await _db
            .Users.AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.Pais)
            .Where(u =>
                (u.PaisId == paisId || u.UserRoles.Any(r => r.Role.Name == ConstantRoles.Admin))
                && u.Active
                && u.Notificaciones
                && (
                    u.UserRoles.Any(ur => ur.Role.Name == roleName)
                    || (
                        additionalRoles != null
                        && u.UserRoles.Any(ur => additionalRoles.Contains(ur.Role.Name))
                    )
                )
            )
            .Select(u => new UsersListVm
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Rol = u.UserRoles.FirstOrDefault().Role.Name,
                Notificaciones = u.Notificaciones,
                Active = u.Active,
                Lang = u.Lenguage,
                Pais = u.Pais.Name,
            })
            .ToListAsync();

        return users;
    }

    /// <summary>
    /// Agrega usuarios administradores, como Admin y TecnicoPais
    /// </summary>
    /// <param name="usersToNotify">Lista de usuarios a notificar</param>
    /// <param name="notifData">Datos de la notificación</param>
    /// <param name="certificacion">Proceso de certificación</param>
    /// <param name="empresaRoleId">ID del rol de empresa</param>
    private async Task AddAdminUsersAsync(
        List<UsersListVm> usersToNotify,
        Notificacion notifData,
        ProcesoCertificacion certificacion,
        string empresaRoleId
    )
    {
        // Status 1 -> Empresa, TecnicoPais, Asesor, Admin.
        // Status 4 -> Empresa, TecnicoPais, Auditor, Admin.

        var paisId = certificacion.Empresa.PaisId ?? certificacion.Empresa.IdPais;

        var adminUsers = await GetUsersByRoleAndPaisAsync(
            ConstantRoles.Admin,
            paisId,
            new[] { ConstantRoles.TecnicoPais, ConstantRoles.Admin }
        );

        if (adminUsers.Any())
        {
            usersToNotify.AddRange(adminUsers);
            _logger.LogInformation(
                "Agregando {Count} administradores de notificaciones",
                adminUsers.Count
            );
        }
    }

    private async Task<List<UsersListVm>> GetUsersByRoleAndCountryAsync(string roleId, int paisId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("Pais", paisId);
        parameters.Add("Role", roleId);

        var users = await _dapper.GetAllAsync<UsersListVm>(
            GET_USERS_BY_ROLE_SP,
            parameters,
            commandType: CommandType.StoredProcedure
        );
        return users;
    }

    /// <summary>
    /// Agrega usuarios de cuentas especiales a la lista de notificaciones
    /// </summary>
    /// <param name="usersToNotify">Lista de usuarios a notificar</param>
    /// <param name="paisId">ID del país</param>
    private async Task AddSpecialAccountUsersAsync(List<UsersListVm> usersToNotify, int paisId)
    {
        try
        {
            var specialAccounts = await _db
                .NotificationCustomUsers.AsNoTracking()
                .Where(s => s.PaisId == paisId || s.Global)
                .Select(account => new UsersListVm
                {
                    Email = account.Email,
                    FirstName = account.Name,
                    Rol = ConstantRoles.Admin,
                    Lang = "es",
                })
                .ToListAsync();

            if (specialAccounts.Any())
            {
                usersToNotify.AddRange(specialAccounts);
                _logger.LogInformation(
                    "Agregadas {Count} cuentas especiales para país {PaisId}",
                    specialAccounts.Count,
                    paisId
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting special account users for country {PaisId}",
                paisId
            );
        }
    }

    /// <summary>
    /// Agrega usuarios con roles específicos a la lista de notificaciones
    /// </summary>
    /// <param name="usersToNotify">Lista de usuarios a notificar</param>
    /// <param name="certificacion">Proceso de certificación</param>
    private async Task AddSpecificRoleUsersAsync(
        List<UsersListVm> usersToNotify,
        ProcesoCertificacion certificacion
    )
    {
        var paisId = certificacion.Empresa.PaisId ?? certificacion.Empresa.IdPais;
        var specificRoles = new Dictionary<string, (string UserId, string RoleName)>
        {
            { ConstantRoles.Asesor, (certificacion.AsesorId, ConstantRoles.Asesor) },
            { ConstantRoles.Auditor, (certificacion.AuditorId, ConstantRoles.Auditor) },
        };

        foreach (var (role, (userId, roleName)) in specificRoles)
        {
            if (string.IsNullOrEmpty(userId))
                continue;

            var users = await GetUsersByRoleAndPaisAsync(ConstantRoles.Admin, paisId, [roleName]);
            var specificUser = users.FirstOrDefault(u => u.Id == userId);

            if (specificUser != null)
            {
                _logger.LogInformation(
                    "Usuario {Role} {UserId} agregado a la lista de notificaciones",
                    roleName,
                    userId
                );
                usersToNotify.Add(specificUser);
            }
            else
            {
                _logger.LogWarning(
                    "Usuario {Role} {UserId} no encontrado en la lista",
                    roleName,
                    userId
                );
            }
        }
    }

    // Método que envía las notificaciones a todos los usuarios
    private async Task SendNotificationsToAllUsersAsync(
        List<UsersListVm> usersToNotify,
        Notificacion notifData,
        ProcesoCertificacion certificacion,
        Empresa empresa,
        Pais paisData
    )
    {
        // 1. Obtiene el email del remitente desde la configuración
        var senderEmail = _config["EmailSender:UserName"];
        // 2. Obtiene el usuario de la empresa
        var empresaUser = await GetCompanyUserAsync(empresa);

        // 3. Si existe usuario de empresa, envía la notificación
        if (empresaUser != null)
        {
            await SendCompanyNotificationAsync(empresaUser, notifData, empresa, senderEmail);
        }

        // 4. Envía notificaciones a usuarios internos (no empresas)
        foreach (var user in usersToNotify.Where(s => s.Rol != ConstantRoles.Empresa))
        {
            await SendInternalNotificationAsync(user, notifData, empresa, paisData, senderEmail);
        }
    }

    private async Task<UsersListVm> GetCompanyUserAsync(Empresa empresa)
    {
        if (empresa == null)
            return null;

        var encargado = await _db
            .ApplicationUser.AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmpresaId == empresa.Id);

        if (encargado == null)
            return null;

        return new UsersListVm
        {
            Rol = ConstantRoles.Empresa,
            FirstName = empresa.Nombre ?? $"{encargado.FirstName} {encargado.LastName}",
            Email = encargado.Email,
            Lang = encargado.Lenguage,
        };
    }

    private async Task SendCompanyNotificationAsync(
        UsersListVm empresaUser,
        Notificacion notifData,
        Empresa empresa,
        string senderEmail
    )
    {
        var emailContent = await RenderEmailTemplateAsync(
            new NotificacionSigleVm { Data = notifData, User = empresaUser }
        );

        await _emailSender.SendEmailBrevoAsync(
            empresaUser.Email,
            notifData.TituloParaEmpresa,
            emailContent
        );

        // Send to additional company email if exists
        if (!string.IsNullOrEmpty(empresa.Email))
        {
            try
            {
                await _emailSender.SendEmailBrevoAsync(
                    empresa.Email,
                    notifData.TituloParaEmpresa,
                    emailContent
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to send notification to additional company email: {Email}",
                    empresa.Email
                );
            }
        }
    }

    /// <summary>
    /// Sends notification to internal users (admins, country technicians)
    /// </summary>
    /// <param name="recipient">The recipient user</param>
    /// <param name="empresa">Company information</param>
    /// <param name="template">Notification template</param>
    private async Task SendInternalNotificationAsync(
        UsersListVm user,
        Notificacion notifData,
        Empresa empresa,
        Pais paisData,
        string senderEmail
    )
    {
        var emailContent = await RenderEmailTemplateAsync(
            new NotificacionSigleVm { Data = notifData, User = user }
        );

        emailContent = emailContent.Replace(
            "{0}",
            $"{empresa.Nombre} ({paisData?.Name ?? "Unknown"})"
        );

        await _emailSender.SendEmailBrevoAsync(user.Email, notifData.TituloInterno, emailContent);
    }

    private async Task<string> RenderEmailTemplateAsync(NotificacionSigleVm model)
    {
        return await _viewRenderService.RenderToStringAsync("EmailStatusTemplate", model);
    }

    // Helper methods...
    private void UpdateNotificationTextForRecertification(
        Notificacion notifData,
        bool isRecertification,
        string lang
    )
    {
        if (!isRecertification)
            return;

        var (searchText, replaceText) =
            lang == "es"
                ? ("certificación", "re certificación")
                : ("certification", "re certification");

        notifData.TituloParaEmpresa = UpdateText(
            notifData.TituloParaEmpresa,
            searchText,
            replaceText
        );
        notifData.TextoParaEmpresa = UpdateText(
            notifData.TextoParaEmpresa,
            searchText,
            replaceText
        );
        notifData.TextoInterno = UpdateText(notifData.TextoInterno, searchText, replaceText);
        notifData.TituloInterno = UpdateText(notifData.TituloInterno, searchText, replaceText);
    }

    private string UpdateText(string text, string search, string replace)
    {
        return text?.ToLower().Contains(search) == true ? text.Replace(search, replace) : text;
    }

    public Notificacion SetTextLanguages(Notificacion notificationData, string language)
    {
        if (language != "en")
            return notificationData;

        return new Notificacion
        {
            TextoInterno = notificationData.TextoInternoEn ?? notificationData.TextoInterno,
            TextoParaEmpresa =
                notificationData.TextoParaEmpresaEn ?? notificationData.TextoParaEmpresa,
            TituloInterno = notificationData.TituloInternoEn ?? notificationData.TituloInterno,
            TituloParaEmpresa =
                notificationData.TituloParaEmpresaEn ?? notificationData.TituloParaEmpresa,
            NotificationGroups = notificationData.NotificationGroups,
            Status = notificationData.Status,
        };
    }
}
