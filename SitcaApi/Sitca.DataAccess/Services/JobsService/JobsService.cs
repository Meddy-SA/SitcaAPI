using Core.Services.Email;
using Dapper;
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

namespace Sitca.DataAccess.Services.JobsService;

public class JobsService : IJobsServices
{
  private readonly ApplicationDbContext _db;
  private readonly IEmailSender _emailSender;
  private readonly IViewRenderService _viewRenderService;
  private readonly IDapper _dapper;
  private readonly IConfiguration _config;
  private readonly ILogger<JobsService> _logger;

  public JobsService(
      ApplicationDbContext db,
      IEmailSender emailSender,
      IViewRenderService viewRenderService,
      IDapper dapper,
      IConfiguration config,
      ILogger<JobsService> logger)
  {
    _db = db ?? throw new ArgumentNullException(nameof(db));
    _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
    _viewRenderService = viewRenderService ?? throw new ArgumentNullException(nameof(viewRenderService));
    _dapper = dapper ?? throw new ArgumentNullException(nameof(dapper));
    _config = config ?? throw new ArgumentNullException(nameof(config));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public async Task<bool> EnviarRecordatorio()
  {
    try
    {
      _logger.LogInformation("Iniciando proceso de envío de recordatorios");

      // Cargar datos necesarios de una sola vez
      var limitDate = DateTime.Now.AddMonths(6);
      var senderEmail = _config["EmailSender:SenderEmail"];

      // Obtener empresas que necesitan notificación
      var empresas = await _db.Empresa
          .Where(z => z.ResultadoVencimiento != null &&
                     z.ResultadoVencimiento < limitDate &&
                     z.Estado == 8 &&
                     z.FechaAutoNotif == null)
          .ToListAsync();

      if (!empresas.Any())
      {
        _logger.LogInformation("No hay empresas que requieran notificación");
        return true;
      }

      // Cargar plantilla de notificación
      var notifDataMain = await GetNotificationTemplate(NotificationTypes.Expiracion);

      if (notifDataMain == null)
      {
        _logger.LogError("No se encontró la plantilla de notificación principal");
        return false;
      }

      // Cargar cuentas especiales
      var cuentasEspeciales = await _db.NotificationCustomUsers
        .AsNoTracking()
        .ToListAsync();

      foreach (var empresa in empresas)
      {
        try
        {
          await ProcesarNotificacionEmpresa(
              empresa,
              notifDataMain,
              cuentasEspeciales,
              senderEmail);

          empresa.FechaAutoNotif = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
          _logger.LogError(ex,
              "Error procesando notificaciones para empresa {EmpresaId}: {EmpresaNombre}",
              empresa.Id,
              empresa.Nombre);
        }
      }

      await _db.SaveChangesAsync();
      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error general en EnviarRecordatorio");
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

  private async Task ProcesarNotificacionEmpresa(
      Empresa empresa,
      Notificacion notifDataMain,
      List<NotificationCustomUsers> cuentasEspeciales,
      string senderEmail)
  {
    var notifData = CrearNotificacionBase(notifDataMain);

    // Aplicar idioma si es necesario
    if (empresa.IdPais == 1)
    {
      notifData = SetTextLanguages(notifData, "en");
    }

    // 1. Notificar a cuentas especiales
    await NotificarCuentasEspeciales(
        empresa,
        notifData,
        cuentasEspeciales,
        senderEmail);

    // 2. Notificar a la empresa si no es homologación
    if (!empresa.EsHomologacion)
    {
      await NotificarEmpresa(
          empresa,
          notifData,
          senderEmail);
    }

    // 3. Notificar a usuarios SITCA
    await NotificarUsuariosSITCA(
        empresa,
        notifData,
        senderEmail);
  }

  private async Task NotificarCuentasEspeciales(
      Empresa empresa,
      Notificacion notifData,
      List<NotificationCustomUsers> cuentasEspeciales,
      string senderEmail)
  {
    var cuentasRelevantes = cuentasEspeciales
        .Where(s => s.PaisId == empresa.PaisId || s.Global);

    foreach (var cuenta in cuentasRelevantes)
    {
      try
      {
        var userSitca = new UsersListVm
        {
          Email = cuenta.Email,
          FirstName = cuenta.Name,
          Rol = "Admin"
        };

        var notificacion = new NotificacionSigleVm
        {
          Data = notifData,
          User = userSitca
        };

        var contenidoEmail = await _viewRenderService
            .RenderToStringAsync("EmailStatusTemplate", notificacion);

        await _emailSender.SendEmailBrevoAsync(
            cuenta.Email,
            notifData.TituloInterno,
            contenidoEmail);

        _logger.LogInformation(
            "Notificación enviada a cuenta especial {Email} para empresa {EmpresaId}",
            cuenta.Email,
            empresa.Id);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,
            "Error enviando notificación a cuenta especial {Email}",
            cuenta.Email);
      }
    }
  }

  private async Task NotificarEmpresa(
      Empresa empresa,
      Notificacion notifData,
      string senderEmail)
  {
    var encargado = await _db.ApplicationUser
      .AsNoTracking()
      .FirstOrDefaultAsync(x => x.EmpresaId == empresa.Id);

    if (encargado == null)
    {
      _logger.LogWarning(
          "No se encontró encargado para la empresa {EmpresaId}",
          empresa.Id);
      return;
    }

    var empresaUser = new UsersListVm
    {
      Rol = "Empresa",
      FirstName = empresa.Nombre ?? $"{encargado.FirstName} {encargado.LastName}",
      Email = encargado.Email,
      Lang = encargado.Lenguage
    };

    var notificacion = new NotificacionSigleVm
    {
      Data = notifData,
      User = empresaUser
    };

    var contenidoEmail = await _viewRenderService
        .RenderToStringAsync("EmailStatusTemplate", notificacion);

    // Enviar al email del encargado
    await _emailSender.SendEmailBrevoAsync(
        empresaUser.Email,
        notifData.TituloParaEmpresa,
        contenidoEmail);

    // Enviar al email adicional de la empresa si existe
    if (!string.IsNullOrEmpty(empresa.Email))
    {
      try
      {
        await _emailSender.SendEmailBrevoAsync(
            empresa.Email,
            notifData.TituloParaEmpresa,
            contenidoEmail);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,
            "Error enviando notificación al email adicional de la empresa {Email}",
            empresa.Email);
      }
    }
  }

  private async Task NotificarUsuariosSITCA(
      Empresa empresa,
      Notificacion notifData,
      string senderEmail)
  {
    // Obtener nombre del país
    var paisNombre = await _db.Pais
        .AsNoTracking()
        .Where(p => p.Id == empresa.PaisId)
        .Select(p => p.Name)
        .FirstOrDefaultAsync() ?? "País no especificado";

    var roles = await _db.Roles
        .Where(s => s.Name == "Admin" || s.Name == "TecnicoPais")
        .ToListAsync();

    foreach (var role in roles)
    {
      var dbPara = new DynamicParameters();
      dbPara.Add("Pais", empresa.PaisId);
      dbPara.Add("Role", role.Id);

      var usuarios = await Task.FromResult(_dapper.GetAll<UsersListVm>(
          "[dbo].[GetUsersByRole]",
          dbPara,
          commandType: CommandType.StoredProcedure));

      foreach (var usuario in usuarios.Where(s => s.Notificaciones && s.Active))
      {
        try
        {
          var notificacion = new NotificacionSigleVm
          {
            Data = notifData,
            User = usuario
          };

          var contenidoEmail = await _viewRenderService
              .RenderToStringAsync("EmailStatusTemplate", notificacion);
          contenidoEmail = contenidoEmail.Replace("{0}", $"{empresa.Nombre} ({paisNombre})");

          await _emailSender.SendEmailBrevoAsync(
              usuario.Email,
              notifData.TituloInterno,
              contenidoEmail);

          _logger.LogInformation(
              "Notificación enviada a usuario SITCA {Email} para empresa {EmpresaId}",
              usuario.Email,
              empresa.Id);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex,
              "Error enviando notificación a usuario SITCA {Email}",
              usuario.Email);
        }
      }
    }
  }

  public async Task<bool> NotificarVencimientoCarnets()
  {
    try
    {
      var fechaActual = DateTime.UtcNow;
      var senderEmail = _config["EmailSender:SenderEmail"];

      var notifDataMain = await _db.Notificacion
        .AsNoTracking()
        .Include(x => x.NotificationGroups)
        .FirstOrDefaultAsync(s => s.Status == -15);

      if (notifDataMain == null)
      {
        _logger.LogWarning("No se encontró la plantilla de notificación para vencimientos de carnet");
        return false;
      }

      var paises = await _db.Pais
        .AsNoTracking()
        .ToDictionaryAsync(p => p.Id, p => p.Name);

      // Obtener todos los usuarios activos con fecha de vencimiento de carnet
      var usuariosConVencimiento = await _db.ApplicationUser
        .Where(u => u.VencimientoCarnet != null && u.Active)
        .ToListAsync();

      if (!usuariosConVencimiento.Any())
      {
        _logger.LogInformation("No hay usuarios con carnets por vencer");
        return true;
      }

      var notificacionesEnviadas = 0;

      foreach (var usuario in usuariosConVencimiento)
      {
        try
        {
          var diasHastaVencimiento = (usuario.VencimientoCarnet.Value - fechaActual).Days;
          var debeNotificar = false;
          var periodoNotificacion = "";

          // Determinar si debe notificar según el período
          if (diasHastaVencimiento <= 180 && diasHastaVencimiento > 90) // 6 meses
          {
            // Verificar si no se ha notificado en los últimos 7 días
            if (usuario.AvisoVencimientoCarnet == null || 
                (fechaActual - usuario.AvisoVencimientoCarnet.Value).Days >= 7)
            {
              debeNotificar = true;
              periodoNotificacion = "6 meses";
            }
          }
          else if (diasHastaVencimiento <= 90 && diasHastaVencimiento > 30) // 3 meses
          {
            // Verificar si no se ha notificado en los últimos 7 días
            if (usuario.AvisoVencimientoCarnet == null || 
                (fechaActual - usuario.AvisoVencimientoCarnet.Value).Days >= 7)
            {
              debeNotificar = true;
              periodoNotificacion = "3 meses";
            }
          }
          else if (diasHastaVencimiento <= 30 && diasHastaVencimiento > 0) // 1 mes
          {
            // Verificar si no se ha notificado en los últimos 3 días
            if (usuario.AvisoVencimientoCarnet == null || 
                (fechaActual - usuario.AvisoVencimientoCarnet.Value).Days >= 3)
            {
              debeNotificar = true;
              periodoNotificacion = "1 mes";
            }
          }
          else if (diasHastaVencimiento <= 0) // Vencido o día de vencimiento
          {
            // Verificar si no se ha notificado el día de vencimiento
            if (usuario.AvisoVencimientoCarnet == null || 
                usuario.AvisoVencimientoCarnet.Value.Date < usuario.VencimientoCarnet.Value.Date)
            {
              debeNotificar = true;
              periodoNotificacion = diasHastaVencimiento == 0 ? "hoy" : "vencido";
            }
          }

          if (debeNotificar)
          {
            await NotificarVencimientoCarnetUsuario(
                usuario,
                notifDataMain,
                senderEmail,
                paises.GetValueOrDefault(usuario.PaisId ?? 0, "País no especificado"),
                periodoNotificacion
                );
            usuario.AvisoVencimientoCarnet = DateTime.UtcNow;
            notificacionesEnviadas++;
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error al notificar vencimiento de carnet para usuario {Email}", usuario.Email);
        }
      }

      // Guardamos los cambios al final
      if (notificacionesEnviadas > 0)
      {
        await _db.SaveChangesAsync();
      }
      
      _logger.LogInformation("Proceso de notificación de carnets completado. {Count} usuarios notificados", notificacionesEnviadas);
      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error al notificar carnets");
      throw;
    }
  }

  private async Task NotificarVencimientoCarnetUsuario(
      ApplicationUser user,
      Notificacion notifDataMain,
      string senderEmail,
      string country,
      string periodoNotificacion)
  {
    var notifData = CrearNotificacionBase(notifDataMain);
    var destinatarios = await ObtenerDestinatariosNotificacion(user);

    foreach (var destinatario in destinatarios)
    {
      try
      {
        var notificationViewModel = new NotificacionSigleVm
        {
          Data = notifData,
          User = destinatario
        };

        var contenidoEmail = await _viewRenderService
          .RenderToStringAsync(
              "EmailStatusTemplate",
              notificationViewModel
              );

        contenidoEmail = contenidoEmail
          .Replace("{user}", $"{user.FirstName} {user.LastName} ({country})")
          .Replace("{fecha}", user.VencimientoCarnet.Value.ToString("dd/MM/yyyy"))
          .Replace("{periodo}", periodoNotificacion);

        // Personalizar el asunto según el período
        var asunto = notifData.TituloInterno;
        if (periodoNotificacion == "vencido")
        {
          asunto = $"URGENTE: {asunto} - Carnet vencido";
        }
        else if (periodoNotificacion == "hoy")
        {
          asunto = $"URGENTE: {asunto} - Vence hoy";
        }
        else
        {
          asunto = $"{asunto} - Vence en {periodoNotificacion}";
        }

        await _emailSender.SendEmailBrevoAsync(
            destinatario.Email,
            asunto,
            contenidoEmail
            );

        _logger.LogInformation(
            "Notificación de vencimiento de carnet enviada a {Email} para usuario {Usuario} - Período: {Periodo}",
            destinatario.Email,
            user.Email,
            periodoNotificacion);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,
            "Error al enviar notificación a {Email} para usuario {Usuario}",
            destinatario.Email,
            user.Email);
      }
    }
  }

  private async Task<List<UsersListVm>> ObtenerDestinatariosNotificacion(ApplicationUser usuario)
  {
    var destinatarios = new List<UsersListVm>();

    // Agregar el usuario principal
    destinatarios.Add(new UsersListVm
    {
      FirstName = usuario.FirstName,
      LastName = usuario.LastName,
      PaisId = usuario.PaisId ?? 0,
      Email = usuario.Email,
    });

    // Obtener administradores y técnicos del país
    var roles = await _db.Roles
        .AsNoTracking()
        .Where(s => s.Name == "Admin" || s.Name == "TecnicoPais")
        .ToListAsync();

    foreach (var role in roles)
    {
      var dbPara = new DynamicParameters();
      dbPara.Add("Pais", usuario.PaisId);
      dbPara.Add("Role", role.Id);

      var usuariosRol = await Task.FromResult(
          _dapper.GetAll<UsersListVm>(
              "[dbo].[GetUsersByRole]",
              dbPara,
              commandType: CommandType.StoredProcedure
          )
      );

      if (usuariosRol.Any())
      {
        destinatarios.AddRange(
            usuariosRol.Where(s => s.Notificaciones && s.Active)
        );
      }
    }

    return destinatarios;
  }

  private Notificacion CrearNotificacionBase(Notificacion origen)
  {
    return new Notificacion
    {
      NotificationGroups = origen.NotificationGroups,
      TextoInterno = origen.TextoInterno,
      TextoInternoEn = origen.TextoInternoEn,
      TextoParaEmpresa = origen.TextoParaEmpresa,
      TextoParaEmpresaEn = origen.TextoParaEmpresaEn,
      TituloInterno = origen.TituloInterno,
      TituloInternoEn = origen.TituloInternoEn,
      TituloParaEmpresa = origen.TituloParaEmpresa,
      TituloParaEmpresaEn = origen.TituloParaEmpresaEn,
      Status = origen.Status,
    };
  }

  public Notificacion SetTextLanguages(Notificacion notificationData, string language)
  {
    if (language == "en")
    {
      notificationData.TextoInterno = notificationData.TextoInternoEn ?? notificationData.TextoInterno;
      notificationData.TextoParaEmpresa = notificationData.TextoParaEmpresaEn ?? notificationData.TextoParaEmpresa;
      notificationData.TituloInterno = notificationData.TituloInternoEn ?? notificationData.TituloInterno;
      notificationData.TituloParaEmpresa = notificationData.TituloParaEmpresaEn ?? notificationData.TituloParaEmpresa;
    }

    return notificationData;
  }
}

