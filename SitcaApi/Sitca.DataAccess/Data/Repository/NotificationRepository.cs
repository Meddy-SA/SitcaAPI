using Core.Services.Email;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.DataAccess.Services.ViewToString;
using Sitca.Models;
using Sitca.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository
{
    public class NotificationRepository: Repository<Notificacion>, INotificationRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IDapper _dapper;
        private readonly IEmailSender _emailSender;
        private readonly IViewRenderService _viewRenderService;
        private readonly IConfiguration _config;

        public NotificationRepository(ApplicationDbContext db, IDapper dapper, IEmailSender emailSender, IViewRenderService viewRenderService, IConfiguration config) : base(db)
        {
            _db = db;
            _dapper = dapper;
            _emailSender = emailSender;
            _viewRenderService = viewRenderService;
            _config = config;
        }

        public async Task<NotificacionVm> SendNotification(int idCertificacion, int? status, string lang = "es" )
        {
            var empresaRole = _db.Roles.First(s => s.Name == "Empresa");           
            var certificacion = await _db.ProcesoCertificacion.Include(s => s.Empresa).FirstOrDefaultAsync(x => x.Id == idCertificacion);

            var pais = certificacion.Empresa.PaisId;

            var paisData = await _db.Pais.FirstOrDefaultAsync(x => x.Id == pais);

            //obtengo textos para el status y obtengo roles a notificar
            var estado = status?? certificacion.Empresa.Estado;
            
            var notifData = await _db.Notificacion.Include(x => x.NotificationGroups).FirstOrDefaultAsync(s => s.Status == estado);

            if (certificacion.Recertificacion)
            {
                if (lang == "es")
                {                    
                    notifData.TituloParaEmpresa = notifData.TituloParaEmpresa.ToLower().Contains("certificación") ? notifData.TituloParaEmpresa.Replace("certificación", "re certificación") : notifData.TituloParaEmpresa;
                    notifData.TextoParaEmpresa = notifData.TextoParaEmpresa.ToLower().Contains("certificación") ? notifData.TextoParaEmpresa.Replace("certificación", "re certificación") : notifData.TextoParaEmpresa;

                    notifData.TextoInterno = notifData.TextoInterno.ToLower().Contains("certificación") ? notifData.TextoInterno.Replace("certificación", "re certificación") : notifData.TextoInterno;
                    notifData.TituloInterno = notifData.TituloInterno.ToLower().Contains("certificación") ? notifData.TituloInterno.Replace("certificación", "re certificación") : notifData.TituloInterno;
                }
                else
                {
                    notifData.TituloParaEmpresaEn = notifData.TituloParaEmpresaEn.ToLower().Contains("certification") ? notifData.TituloParaEmpresaEn.Replace("certification", "re certification") : notifData.TituloParaEmpresaEn;
                    notifData.TextoParaEmpresaEn = notifData.TextoParaEmpresaEn.ToLower().Contains("certification") ? notifData.TextoParaEmpresaEn.Replace("certification", "re certification") : notifData.TextoParaEmpresaEn;

                    notifData.TextoInternoEn = notifData.TextoInternoEn.ToLower().Contains("certification") ? notifData.TextoInternoEn.Replace("certification", "re certification") : notifData.TextoInternoEn;
                    notifData.TituloInternoEn = notifData.TituloInternoEn.ToLower().Contains("certification") ? notifData.TituloInternoEn.Replace("certification", "re certification") : notifData.TituloInternoEn;
                }
            }

            var UsersByRole = new List<UsersListVm>();
            var UsersToNotify = new List<UsersListVm>();

            if (notifData == null)
            {
                return null;
            }

            if (lang != "es")
            {
                notifData = SetTextLanguages(notifData, lang);
            }

            foreach (var item in notifData.NotificationGroups.Where(s =>s.RoleId != empresaRole.Id))
            {

                var dbPara = new DynamicParameters();

                dbPara.Add("Pais", certificacion.Empresa.PaisId);
                dbPara.Add("Role", item.RoleId);

                var res = await Task.FromResult(_dapper.GetAll<UsersListVm>("[dbo].[GetUsersByRole]", dbPara, commandType: CommandType.StoredProcedure));
                if (res.Any())
                {
                    res = res.Where(s => s.Notificaciones && s.Active).ToList();
                    UsersByRole.AddRange(res);
                }
            }

            var AdminUsers = UsersByRole.Where(s => s.Rol != "Asesor" && s.Rol != "Auditor");
            UsersToNotify.AddRange(AdminUsers);

            //var userSitca = new UsersListVm
            //{
            //    Email = "sitcasiccs@gmail.com",
            //    FirstName = "Sitca",
            //    Rol = "Admin"
            //};

            #region CUENTAS ESPECIALES NOTIFICACION

            var cuentasEspeciales = _db.NotificationCustomUsers.Where(s => s.PaisId == pais || s.Global);

            foreach (var item in cuentasEspeciales)
            {
                var userSitca = new UsersListVm
                {
                    Email = item.Email,
                    FirstName = item.Name,
                    Rol = "Admin",
                    Lang = "es"
                };
                UsersToNotify.Add(userSitca);
            }
            
            #endregion

            //var EmpresaUser = UsersToNotify.Where(s => s.Rol != "Empresa");

            //ASESOR
            if (UsersByRole.Any(s => s.Rol == "Asesor"))
            {
                var asesor = UsersByRole.FirstOrDefault(s => s.Id == certificacion.AsesorId);
                if (asesor != null)
                {
                    UsersToNotify.Add(asesor);
                }                
            }

            //AUDITOR
            if (UsersByRole.Any(s => s.Rol == "Auditor"))
            {
                var auditor = UsersByRole.FirstOrDefault(s => s.Id == certificacion.AuditorId);
                if (auditor != null)
                {
                    UsersToNotify.Add(auditor);
                }

            }

            //var senderEmail = "notificaciones@siccs.info";
            //var senderEmail = "notificaciones@calidadcentroamerica.com";
            var senderEmail = _config["EmailSender:UserName"];

            //EMPRESA
            if (notifData.NotificationGroups.Any(x =>x.RoleId == empresaRole.Id))
            {
                var encargado = await _db.ApplicationUser.FirstOrDefaultAsync(x => x.EmpresaId == certificacion.EmpresaId);
                var empresaUser = new UsersListVm
                {
                    Rol = "Empresa",
                    FirstName = certificacion.Empresa.Nombre ?? encargado.FirstName + " " + encargado.LastName,
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
                    if (!string.IsNullOrEmpty(certificacion.Empresa.Email))
                    {
                        try
                        {
                            var addr = new System.Net.Mail.MailAddress(certificacion.Empresa.Email);
                            _emailSender.SendEmailAsync(senderEmail, certificacion.Empresa.Email, notifData.TituloParaEmpresa, view);
                        }
                        catch
                        {
                            return null;
                        }

                        
                    }
                }
                catch (Exception)
                {
                    
                }
                

                var sasa = _emailSender.SendEmailAsync(senderEmail, empresaUser.Email, notifData.TituloParaEmpresa, view);

            }

            foreach (var item in UsersToNotify.Where(s =>s.Rol != "Empresa"))
            {
                var notificatioItem = new NotificacionSigleVm
                {
                    Data = notifData,
                    User = item
                };

                var view = await _viewRenderService.RenderToStringAsync("EmailStatusTemplate", notificatioItem);
                //view = string.Format(view, certificacion.Empresa.Nombre);
                
                view = view.Replace("{0}", certificacion.Empresa.Nombre + $" ({paisData.Name})");
               
                _emailSender.SendEmailAsync(senderEmail, item.Email, notifData.TituloInterno, view);
            }

            var result = new NotificacionVm
            {
                Data = notifData,
                Users = UsersToNotify
            };

            return result;
        }

        public async Task<NotificacionVm> SendNotificacionSpecial(int idEmpresa, int status, string lang = "es") 
        {
            var empresaRole = _db.Roles.First(s => s.Name == "Empresa");
            var empresa = await _db.Empresa.FirstOrDefaultAsync(x => x.Id == idEmpresa);

            //obtengo textos para el status y obtengo roles a notificar           
            var notifData = await _db.Notificacion.Include(x => x.NotificationGroups).FirstOrDefaultAsync(s => s.Status == status);

            var paisData = await _db.Pais.FirstOrDefaultAsync(x => x.Id == empresa.PaisId);

            var UsersByRole = new List<UsersListVm>();
            var UsersToNotify = new List<UsersListVm>();

            if (notifData == null)
            {
                return null;
            }
            if (lang != "es")
            {
                notifData = SetTextLanguages(notifData, lang);
            }


            foreach (var item in notifData.NotificationGroups.Where(s => s.RoleId != empresaRole.Id))
            {

                var dbPara = new DynamicParameters();

                dbPara.Add("Pais", empresa.PaisId);
                dbPara.Add("Role", item.RoleId);

                var res = await Task.FromResult(_dapper.GetAll<UsersListVm>("[dbo].[GetUsersByRole]", dbPara, commandType: CommandType.StoredProcedure));
                if (res.Any())
                {
                    res = res.Where(s => s.Notificaciones && s.Active).ToList();
                    UsersByRole.AddRange(res);
                }
            }

            var AdminUsers = UsersByRole.Where(s => s.Rol != "Asesor" && s.Rol != "Auditor");
            UsersToNotify.AddRange(AdminUsers);


            #region CUENTAS ESPECIALES NOTIFICACION
            try
            {
                var cuentasEspeciales = _db.NotificationCustomUsers.Where(s => s.PaisId == empresa.PaisId || s.Global);

                foreach (var item in cuentasEspeciales)
                {
                    var userSitca = new UsersListVm
                    {
                        Email = item.Email,
                        FirstName = item.Name,
                        Rol = "Admin",
                        Lang = "es"
                    };
                    UsersToNotify.Add(userSitca);
                }
            }
            catch (Exception)
            {
            
            }


            #endregion

            //var EmpresaUser = UsersToNotify.Where(s => s.Rol != "Empresa");

            //var senderEmail = "notificaciones@calidadcentroamerica.com";
            var senderEmail = _config["EmailSender:UserName"];
            //var senderEmail = "notificaciones@siccs.info";
            //EMPRESA
            if (notifData.NotificationGroups.Any(x => x.RoleId == empresaRole.Id))
            {
                var encargado = await _db.ApplicationUser.FirstOrDefaultAsync(x => x.EmpresaId == empresa.Id);
                var empresaUser = new UsersListVm
                {
                    Rol = "Empresa",
                    FirstName = empresa.Nombre ?? encargado.FirstName + " " + encargado.LastName,
                    Email = string.IsNullOrEmpty(empresa.Email)? encargado.Email: empresa.Email,
                    Lang = encargado.Lenguage
                };
                UsersToNotify.Add(empresaUser);

                var empresaNotificacion = new NotificacionSigleVm
                {
                    Data = notifData,
                    User = empresaUser
                };
                var view = await _viewRenderService.RenderToStringAsync("EmailStatusTemplate", empresaNotificacion);

                _emailSender.SendEmailAsync(senderEmail, empresaUser.Email, notifData.TituloParaEmpresa, view);

            }

            foreach (var item in UsersToNotify.Where(s => s.Rol != "Empresa"))
            {
                var notificatioItem = new NotificacionSigleVm
                {
                    Data = notifData,
                    User = item
                };

                var view = await _viewRenderService.RenderToStringAsync("EmailStatusTemplate", notificatioItem);
                
                //view = view.Replace("{0}", empresa.Nombre);
                view = view.Replace("{0}", empresa.Nombre + $" ({paisData.Name})");

                _emailSender.SendEmailAsync(senderEmail, item.Email, notifData.TituloInterno, view);
            }

            var result = new NotificacionVm
            {
                Data = notifData,
                Users = UsersToNotify
            };

            return result;
        }

        public Notificacion SetTextLanguages(Notificacion notificationData, string language)
        {
            if (language == "en")
            {
                notificationData.TextoInterno = notificationData.TextoInternoEn?? notificationData.TextoInterno;
                notificationData.TextoParaEmpresa = notificationData.TextoParaEmpresaEn?? notificationData.TextoParaEmpresa;
                notificationData.TituloInterno = notificationData.TituloInternoEn?? notificationData.TituloInterno;
                notificationData.TituloParaEmpresa = notificationData.TituloParaEmpresaEn?? notificationData.TituloParaEmpresa;
            }

            return notificationData;
        }
    }
}
