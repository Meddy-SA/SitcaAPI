using Microsoft.EntityFrameworkCore;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.Models;
using Sitca.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository
{
    public class CompañiasAuditorasRepository: Repository<CompAuditoras>, ICompañiasAuditorasRepository
    {
        private readonly ApplicationDbContext _db;

        public CompañiasAuditorasRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<bool> Save(CompAuditoras data)
        {
            //agregar tipologia
            try
            {
                if (data.Id > 0)
                {                    
                    var item = await _db.CompAuditoras.FindAsync(data.Id);
                    item.Name = data.Name;
                    item.PaisId = data.PaisId;
                    item.Telefono = data.Telefono;
                    item.Email = data.Email;
                    item.Direccion = data.Direccion;
                    item.FechaFinConcesion = data.FechaFinConcesion;
                    item.FechaInicioConcesion = data.FechaInicioConcesion;
                    item.Tipo = data.Tipo;
                    item.Representante = data.Representante;
                    item.NumeroCertificado = data.NumeroCertificado;
                    item.Status = data.Status;
                    await _db.SaveChangesAsync();
                    return true;
                }

                var nuevaComp = new CompAuditoras
                {
                    Direccion = data.Direccion,
                    Email = data.Email,
                    FechaInicioConcesion = data.FechaInicioConcesion,
                    FechaFinConcesion = data.FechaFinConcesion,
                    Representante = data.Representante,
                    NumeroCertificado = data.NumeroCertificado,
                    Tipo = data.Tipo,
                    Name = data.Name,
                    PaisId = data.PaisId,
                    Telefono = data.Telefono,
                    Status = true
                };

                _db.CompAuditoras.Add(nuevaComp);
                await _db.SaveChangesAsync();
                return true;                                
            }
            catch (Exception)
            {
                return false;
            }            
        }
    }
}
