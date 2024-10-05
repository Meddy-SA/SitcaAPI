using Microsoft.EntityFrameworkCore;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.Models;
using Sitca.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository
{
    public class ModulosRepository: Repository<Modulo>, IModuloRepository
    {
        private readonly ApplicationDbContext _db;

        public ModulosRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public List<Modulo> GetList(int? idTipologia = 0)
        {

            idTipologia = idTipologia ?? 0;
            var modulos = _db.Modulo.Include("Tipologia").Where(x => x.Transversal || (x.TipologiaId == idTipologia || idTipologia == 0)).OrderBy(z =>z.Orden).ToList();

            return modulos;
        }

        public Modulo Details(int id)
        {
            //var modulo = _db.Modulo.Include("Preguntas").FirstOrDefault(s =>s.Id == id);

            var modulo = _db.Modulo.Include(i => i.Preguntas).ThenInclude(x =>x.Tipologia).FirstOrDefault(s => s.Id == id);
            
            return modulo;
        }

        public async Task<bool> Edit(Modulo data)
        {
            var modulo = await _db.Modulo.FirstOrDefaultAsync(s => s.Id == data.Id);
            modulo.Nombre = data.Nombre;
            modulo.Orden = data.Orden;
            modulo.Nomenclatura = data.Nomenclatura;

            await _db.SaveChangesAsync();

            return true;
        }

    }
}
