using System.Threading.Tasks;

namespace Sitca.DataAccess.Services.JobsService
{
    public interface IJobsServices
    {
        Task<bool> EnviarRecordatorio();

        Task<bool> NotificarVencimientoCarnets();
    }
}
