using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.Models;

namespace Sitca.DataAccess.Data.Repository
{
    class PreguntasRepository : Repository<Pregunta>, IPreguntasRepository
    {
        private readonly ApplicationDbContext _db;

        public PreguntasRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
