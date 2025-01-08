using Microsoft.EntityFrameworkCore;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.DataAccess.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;
using Roles = Utilities.Common.Constants.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository
{
    public class ArchivoRepository : Repository<Archivo>, IArchivoRepository
    {
        private readonly ApplicationDbContext _db;

        public ArchivoRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<bool> DeleteFile(int data, ApplicationUser user, string role)
        {
            var archivo = await _db.Archivo.FirstOrDefaultAsync(s => s.Id == data);

            if (role == Roles.Asesor || role == Roles.Auditor)
            {
                if (archivo.UsuarioCargaId != user.Id)
                {
                    return false;
                }
            }

            archivo.Activo = false;
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<List<Archivo>> GetList(ArchivoFilterVm data, ApplicationUser user, string role)
        {
            if (data.type == "pregunta")
            {
                return await _db.Archivo
                  .AsNoTracking()
                  .Where(s => s.CuestionarioItemId == data.idPregunta && s.Activo)
                  .ToListAsync();
            }

            if (data.type == "cuestionario")
            {
                return await _db.Archivo
                  .AsNoTracking()
                  .Where(s => s.CuestionarioItem.CuestionarioId == data.idCuestionario && s.Activo)
                  .ToListAsync();
            }

            return null;
        }

        public List<EnumValueDto> GetTypeFilesCompany()
        {
            return FileCompanyExtensions.GetFileTypes();
        }

        public async Task<bool> SaveFileData(Archivo data)
        {
            var archivo = new Archivo
            {
                Activo = true,
                EmpresaId = data.EmpresaId,
                FechaCarga = DateTime.UtcNow,
                Ruta = data.Ruta,
                Nombre = data.Nombre,
                Tipo = data.Tipo,
                CuestionarioItemId = data.CuestionarioItemId,
                UsuarioCargaId = data.UsuarioCargaId,
                FileTypesCompany = data.FileTypesCompany
            };
            if (!string.IsNullOrEmpty(data.UsuarioId))
            {
                archivo.UsuarioId = data.UsuarioId;
            }

            _db.Archivo.Add(archivo);
            await _db.SaveChangesAsync();

            if (archivo.Nombre == "Protocolo Adhesión")
            {
                var empresa = _db.Empresa.Find(archivo.EmpresaId);
                if (empresa.Estado < 1)
                {
                    empresa.Estado = 1;
                }

                await _db.SaveChangesAsync();
            }

            return true;
        }

    }
}
