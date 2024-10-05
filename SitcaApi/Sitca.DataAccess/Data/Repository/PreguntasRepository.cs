using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
