using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.Models;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository
{
    public class ReportesRepository : Repository<Cuestionario>, IReporteRepository
    {
        private readonly ApplicationDbContext _db;

        public ReportesRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<bool> ReporteCertificacion(int cuestionarioId)
        {
            return await Task.Run(() => true);
        }
    }
}
