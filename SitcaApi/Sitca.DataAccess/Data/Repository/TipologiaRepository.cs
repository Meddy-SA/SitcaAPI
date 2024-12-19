using Microsoft.EntityFrameworkCore;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.Models;
using Sitca.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository
{
    public class TipologiaRepository : Repository<Tipologia>, ITipologiaRepository
    {
        private readonly ApplicationDbContext _db;

        public TipologiaRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<List<CommonVm>> SelectList(string lang = "es")
        {
            var tipologias = await _db.Tipologia.Select(x => new CommonVm
            {
                id = x.Id,
                name = lang == "es" ? x.Name : x.NameEnglish,
                isSelected = false
            }).ToListAsync();

            return tipologias;
        }
    }
}
