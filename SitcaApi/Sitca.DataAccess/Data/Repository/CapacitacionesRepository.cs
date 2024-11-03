using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.Models;
using System;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository
{
  public class CapacitacionesRepository : Repository<Capacitaciones>, ICapacitacionesRepository
  {
    private readonly ApplicationDbContext _db;

    public CapacitacionesRepository(ApplicationDbContext db) : base(db)
    {
      _db = db;
    }

    public async Task<bool> SaveCapacitacion(Capacitaciones data)
    {
      var archivo = new Capacitaciones
      {
        Activo = true,
        FechaCarga = DateTime.UtcNow,
        Ruta = data.Ruta,
        Nombre = data.Nombre,
        Tipo = data.Tipo,
        Descripcion = data.Descripcion,
        UsuarioCargaId = data.UsuarioCargaId
      };
      _db.Capacitaciones.Add(archivo);
      await _db.SaveChangesAsync();

      return true;
    }

    public async Task<bool> DeleteFile(int id)
    {
      var capacitacion = await _db.Capacitaciones.FindAsync(id);
      if (capacitacion == null)
      {
        return false;
      }

      _db.Capacitaciones.Remove(capacitacion);
      await _db.SaveChangesAsync();

      return true;
    }

  }
}
