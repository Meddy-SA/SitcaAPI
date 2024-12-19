using Sitca.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;

namespace Sitca.DataAccess.Data.Repository
{
    public class ItemTemplateRepository : Repository<ItemTemplate>, IItemTemplateRepository
    {
        private readonly ApplicationDbContext _db;

        public ItemTemplateRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public IEnumerable<SelectListItem> GetITemForDropDown()
        {
            return _db.ItemTemplate.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
        }

        public async Task<List<ItemTemplate>> GetAllAsync()
        {
            return await _db.ItemTemplate.ToListAsync();
        }
    }
}
