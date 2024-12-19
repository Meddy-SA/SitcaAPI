using System.Threading.Tasks;
using Sitca.Models;
using Sitca.Models.ViewModels;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface INotificationRepository : IRepository<Notificacion>
    {
        Task<NotificacionVm> SendNotification(int idCertificacion, int? status, string lang);
        Task<NotificacionVm> SendNotificacionSpecial(int idEmpresa, int status, string lang);
    }
}
