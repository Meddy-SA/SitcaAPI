using Core.Services.Email;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sitca.DataAccess.Data;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Services.ViewToString;
using Sitca.Models;
using Sitca.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Services.JobsService
{
    public class JobsService: IJobsServices
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        private readonly IViewRenderService _viewRenderService;
        private readonly IDapper _dapper;
        private readonly IConfiguration _config;

        public JobsService(ApplicationDbContext db, IEmailSender emailSender, IViewRenderService viewRenderService, IDapper dapper, IConfiguration config)
        {
            _db = db;
            _emailSender = emailSender;
            _viewRenderService = viewRenderService;
            _dapper = dapper;
            _config = config;
        }

        public async Task<bool> EnviarRecordatorio()
        {
            var limitDate = DateTime.Now.AddMonths(6);

            //La empresa tiene un distintivo que vence en menos de 6 meses y que no han sido notificadas
            var empresas = _db.Empresa.Where(z => z.ResultadoVencimiento != null && z.ResultadoVencimiento < limitDate && z.Estado == 8 && z.FechaAutoNotif == null).ToList();
            
            var UsersToNotify = new List<UsersListVm>();

            var notifDataMain = await _db.Notificacion.Include(x => x.NotificationGroups).FirstOrDefaultAsync(s => s.Status == -10);

            
            //var senderEmail = "notificaciones@siccs.info";
            //var senderEmail = "notificaciones@calidadcentroamerica.com";
            var senderEmail = _config["EmailSender:UserName"];

            #region CUENTAS ESPECIALES NOTIFICACION

            var cuentasEspeciales = _db.NotificationCustomUsers.ToList();            

            #endregion

            foreach (var item in empresas)
            {
                var notifData = new Notificacion
                {
                    NotificationGroups = notifDataMain.NotificationGroups,
                    TextoInterno = notifDataMain.TextoInterno,
                    TextoInternoEn = notifDataMain.TextoInternoEn,
                    TextoParaEmpresa = notifDataMain.TextoParaEmpresa,
                    TextoParaEmpresaEn = notifDataMain.TextoParaEmpresaEn,
                    TituloInterno = notifDataMain.TituloInterno,
                    TituloInternoEn = notifDataMain.TituloInternoEn,
                    TituloParaEmpresa = notifDataMain.TituloParaEmpresa,
                    TituloParaEmpresaEn = notifDataMain.TituloParaEmpresaEn,
                    Status = notifDataMain.Status,
                };

                if (item.IdPais == 1)
                {
                    //belize textos en ingles
                    notifData = SetTextLanguages(notifData, "en");
                }

                foreach (var cuenta in cuentasEspeciales.Where(s => s.PaisId == item.PaisId || s.Global))
                {
                    var userSitca = new UsersListVm
                    {
                        Email = cuenta.Email,
                        FirstName = cuenta.Name,
                        Rol = "Admin"
                    };

                    var sitcaNotificacion = new NotificacionSigleVm
                    {
                        Data = notifData,
                        User = userSitca
                    };
                    var SitcaView = await _viewRenderService.RenderToStringAsync("EmailStatusTemplate", sitcaNotificacion);

                    try
                    {
                        var addr = new System.Net.Mail.MailAddress(item.Email);
                        _emailSender.SendEmailAsync(senderEmail, cuenta.Email, notifData.TituloInterno, SitcaView);
                    }
                    catch
                    {

                    }

                }

                #region Cuentas empresa

                if (!item.EsHomologacion)
                {
                    var encargado = await _db.ApplicationUser.FirstOrDefaultAsync(x => x.EmpresaId == item.Id);
                    var empresaUser = new UsersListVm
                    {
                        Rol = "Empresa",
                        FirstName = item.Nombre ?? encargado.FirstName + " " + encargado.LastName,
                        Email = encargado.Email,
                        Lang = encargado.Lenguage
                    };

                    UsersToNotify.Add(empresaUser);

                    var empresaNotificacion = new NotificacionSigleVm
                    {
                        Data = notifData,
                        User = empresaUser
                    };
                    var view = await _viewRenderService.RenderToStringAsync("EmailStatusTemplate", empresaNotificacion);

                    try
                    {
                        //envia tambien al mail que agrego la empresa
                        if (!string.IsNullOrEmpty(item.Email))
                        {
                            try
                            {
                                var addr = new System.Net.Mail.MailAddress(item.Email);
                                _emailSender.SendEmailAsync(senderEmail, item.Email, notifData.TituloParaEmpresa, view);
                            }
                            catch
                            {

                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                    _emailSender.SendEmailAsync(senderEmail, empresaUser.Email, notifData.TituloParaEmpresa, view);
                }

                
                item.FechaAutoNotif = DateTime.UtcNow;
                #endregion

                #region Cuentas Sitca

                var Pais = item.PaisId;

                var dbPara = new DynamicParameters();

                var roles = _db.Roles.Where(s => s.Name == "Admin" || s.Name == "TecnicoPais");

                var UsersByRole = new List<UsersListVm>();                

                foreach (var role in roles)
                {
                    dbPara.Add("Pais", Pais);
                    dbPara.Add("Role", roles.First(s => s.Name == role.Name).Id);

                    var res = await Task.FromResult(_dapper.GetAll<UsersListVm>("[dbo].[GetUsersByRole]", dbPara, commandType: CommandType.StoredProcedure));
                    if (res.Any())
                    {
                        res = res.Where(s => s.Notificaciones && s.Active).ToList();
                        UsersByRole.AddRange(res);
                    }
                }

                foreach (var sitcaUser in UsersByRole)
                {
                    var notificatioItem = new NotificacionSigleVm
                    {
                        Data = notifData,
                        User = sitcaUser
                    };

                    var view1 = await _viewRenderService.RenderToStringAsync("EmailStatusTemplate", notificatioItem);                    
                    view1 = view1.Replace("{0}", item.Nombre);

                    _emailSender.SendEmailAsync(senderEmail, sitcaUser.Email, notifData.TituloInterno, view1);
                }

                #endregion
            }

            try
            {
                _db.SaveChanges();
            }
            catch (Exception e)
            {
            }
            
            return true;
        }

        public async Task<bool> NotificarVencimientoCarnets()
        {
            var vencimiento = DateTime.UtcNow.AddMonths(6);

            //carnets que no han sido notificados y que estan por vencer en los proximos meses
            var porVencer = await _db.ApplicationUser.Where(s => (s.VencimientoCarnet != null && s.VencimientoCarnet < vencimiento) && s.AvisoVencimientoCarnet == null).ToListAsync();
            var UsersToNotify = new List<UsersListVm>();

            var notifDataMain = await _db.Notificacion.Include(x => x.NotificationGroups).FirstOrDefaultAsync(s => s.Status == -15);
            var senderEmail = _config["EmailSender:UserName"];
            var paises = await _db.Pais.ToListAsync();

            foreach (var item in porVencer)
            {
                var notifData = new Notificacion
                {
                    NotificationGroups = notifDataMain.NotificationGroups,
                    TextoInterno = notifDataMain.TextoInterno,
                    TextoInternoEn = notifDataMain.TextoInternoEn,
                    TextoParaEmpresa = notifDataMain.TextoParaEmpresa,
                    TextoParaEmpresaEn = notifDataMain.TextoParaEmpresaEn,
                    TituloInterno = notifDataMain.TituloInterno,
                    TituloInternoEn = notifDataMain.TituloInternoEn,
                    TituloParaEmpresa = notifDataMain.TituloParaEmpresa,
                    TituloParaEmpresaEn = notifDataMain.TituloParaEmpresaEn,
                    Status = notifDataMain.Status,
                };

                #region Cuentas Sitca


                var Pais = item.PaisId;
                var dbPara = new DynamicParameters();
                var roles = _db.Roles.Where(s => s.Name == "Admin" || s.Name == "TecnicoPais");
                var UsersByRole = new List<UsersListVm>();

                var personaPrincipal = new UsersListVm
                {
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    PaisId = item.PaisId??0,
                    Email = item.Email,
                };
                UsersByRole.Add(personaPrincipal);

                foreach (var role in roles)
                {
                    dbPara.Add("Pais", Pais);
                    dbPara.Add("Role", roles.First(s => s.Name == role.Name).Id);

                    var res = await Task.FromResult(_dapper.GetAll<UsersListVm>("[dbo].[GetUsersByRole]", dbPara, commandType: CommandType.StoredProcedure));
                    if (res.Any())
                    {
                        res = res.Where(s => s.Notificaciones && s.Active).ToList();
                        UsersByRole.AddRange(res);
                    }
                }

                foreach (var sitcaUser in UsersByRole)
                {
                    var notificatioItem = new NotificacionSigleVm
                    {
                        Data = notifData,
                        User = sitcaUser
                    };

                    var view1 = await _viewRenderService.RenderToStringAsync("EmailStatusTemplate", notificatioItem);
                    view1 = view1.Replace("{user}", item.FirstName + " (" + paises.FirstOrDefault(s =>s.Id == item.PaisId).Name + ")") ;
                    view1 = view1.Replace("{fecha}", item.VencimientoCarnet.Value.ToString("dd/MM/yyyy"));

                    _emailSender.SendEmailAsync(senderEmail, sitcaUser.Email, notifData.TituloInterno, view1);
                }

                #endregion
            }



            return true; 
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
}
