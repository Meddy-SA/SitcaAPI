using Sitca.Models;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface IReporteRepository : IRepository<Cuestionario>
    {
        Task<bool> ReporteCertificacion(int cuestionarioId);
    }
}
