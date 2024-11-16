using System.Threading.Tasks;
using Sitca.DataAccess.Data.Repository.Constants;
using Sitca.Models;
using Sitca.Models.ViewModels;

namespace Sitca.DataAccess.Services.Notification;

public interface INotificationService
{
  Task<bool> HasBeenNotifiedAsync(string userId, int certificacionId);
  Task SendExpirationNotificationAsync(ApplicationUser user, CertificacionDetailsVm certification);
  Task<NotificacionVm> SendNotification(int idCertificacion, int? status, string lang = "es");
  Task<NotificacionVm> SendNotificacionSpecial(int idEmpresa, NotificationTypes notificationType, string lang = "es");
}
