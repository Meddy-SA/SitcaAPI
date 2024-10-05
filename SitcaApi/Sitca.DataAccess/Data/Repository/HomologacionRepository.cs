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
using Utilities;

namespace Sitca.DataAccess.Data.Repository
{
    public class HomologacionRepository : Repository<Homologacion>, IHomologacionRepository
    {
        private readonly ApplicationDbContext _db;
        public HomologacionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<bool> BloquearEdicion(int id)
        {
            var homologacion = await _db.Homologacion.FindAsync(id);

            homologacion.EnProcesoSiccs = !homologacion.EnProcesoSiccs;
            await _db.SaveChangesAsync();
            return homologacion.EnProcesoSiccs == true;
        }

        public async Task<int> Create(HomologacionDTO datos, ApplicationUser user)
        {
           
            using var transaction = _db.Database.BeginTransaction();
            var startDate = DateTime.Now;
            var endDate = DateTime.Now;

            try
            {
                var tipologia = await _db.Tipologia.FindAsync(datos.tipologia.Id);

                #region ----CREA EMPRESA----

                var empresa = new Empresa
                {
                    Active = true,
                    EsHomologacion = true,
                    Nombre = datos.nombre,
                    PaisId = user.PaisId,
                    IdPais = user.PaisId??6,
                    Estado = 8,
                    ResultadoVencimiento = datos.fechaVencimiento,
                };

                _db.Empresa.Add(empresa);
                _db.SaveChanges();

                #endregion

                #region ----AGREGA TIPOLOGIA----

                var listaTipologias = new List<TipologiasEmpresa>();


                var TipologiaEmpresa = new TipologiasEmpresa
                {
                    IdEmpresa = empresa.Id,
                    IdTipologia = tipologia.Id,
                };

                listaTipologias.Add(TipologiaEmpresa);

                empresa.Tipologias = listaTipologias;

                _db.SaveChanges();

                #endregion

                #region ----AGREGA CERTIFICACION y ----

                var proceso = new ProcesoCertificacion
                {
                    FechaFinalizacion = datos.fechaVencimiento,
                    FechaInicio = datos.fechaOtorgamiento,
                    TipologiaId = tipologia.Id,
                    EmpresaId = empresa.Id,
                    FechaVencimiento = datos.fechaVencimiento,
                    UserGeneraId = user.Id,
                    Status = "8 - Finalizado",
                };
                _db.ProcesoCertificacion.Add(proceso);
                _db.SaveChanges();

                #endregion

                #region ----AGREGA DISTINTIVO SICCS y ----

                var distintivoSiccs = await _db.Distintivo.FindAsync(datos.distintivoSiccs.id);

                var resultado = new ResultadoCertificacion
                {
                    Aprobado = true,
                    CertificacionId = proceso.Id,
                    DistintivoId = distintivoSiccs.Id,
                    Observaciones = datos.datosProceso,                    
                };

                _db.ResultadoCertificacion.Add(resultado);
                _db.SaveChanges();

                #endregion

                #region ----CREA HOMOLOGACIOM ----

                var homologacion = new Homologacion
                {
                    CertificacionId = proceso.Id,
                    FechaCreacion = DateTime.UtcNow,
                    DistintivoExterno = datos.selloItc.name,
                    EnProcesoSiccs = false,
                    DatosProceso = datos.datosProceso,
                    FechaVencimiento = datos.fechaVencimiento,
                    FechaOtorgamiento = datos.fechaOtorgamiento,
                    Distintivo = user.Lenguage == "es"? distintivoSiccs.Name:distintivoSiccs.NameEnglish,
                    EmpresaId = empresa.Id,
                };

                _db.Homologacion.Add(homologacion);
                _db.SaveChanges();

                #endregion
                transaction.Commit();
                return empresa.Id;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return 0;
            }

        }

        
        public async Task<HomologacionDTO> Details(ApplicationUser appUser,string role, int id)
        {
            var item = await _db.Homologacion
                .Include(s =>s.Empresa)
                .ThenInclude(s => s.Tipologias)
                .ThenInclude(s =>s.Tipologia)
                .Include(s => s.Empresa)
                .ThenInclude(s => s.Archivos)
                .ThenInclude(s =>s.UsuarioCarga)
                .FirstOrDefaultAsync(s => s.Id == id && (s.Empresa.PaisId == appUser.PaisId || role == "Admin"));

            var tipologia = item.Empresa.Tipologias.First().Tipologia;

            var result = new HomologacionDTO
            {
                id = item.Id,
                empresaId = item.EmpresaId,
                fechaOtorgamiento = item.FechaOtorgamiento,
                fechaVencimiento = item.FechaVencimiento,
                nombre = item.Empresa.Nombre,
                enProceso = item.EnProcesoSiccs == true,
                tipologia = new Tipologia
                {
                    Id = tipologia.Id,
                    Name = appUser.Lenguage == "es"? tipologia?.Name: tipologia?.NameEnglish,
                    NameEnglish = tipologia?.NameEnglish,
                },
                datosProceso = item.DatosProceso,
                distintivoSiccs = new CommonVm
                {
                    name = item.Distintivo
                },
                selloItc = new SelloItc
                {
                    name = item.DistintivoExterno
                },
                archivos = item.Empresa.Archivos != null ? item.Empresa.Archivos.Where(s => s.Activo).Select(z => new ArchivoVm
                {
                    Id = z.Id,
                    Nombre = z.Nombre,
                    Ruta = z.Ruta,
                    Tipo = z.Tipo,
                    Cargador = z.UsuarioCarga.FirstName + " " + z.UsuarioCarga.LastName,
                    FechaCarga = z.FechaCarga.ToUtc(),
                    Propio = true
                }).ToList() : null
            };

            return result;
        }

        public async Task<List<HomologacionDTO>> List(int country)
        {
            var homologaciones = await _db.Homologacion
                .Include(s => s.Empresa)
                .ThenInclude(s =>s.Tipologias)
                .Where(s => s.Empresa.PaisId == country).ToListAsync();

            var tipologias = await _db.Tipologia.ToListAsync();


            return homologaciones.Select(s => new HomologacionDTO
            {
                id = s.Id,
                empresaId = s.EmpresaId,
                fechaOtorgamiento = s.FechaOtorgamiento,
                fechaVencimiento = s.FechaVencimiento,
                nombre = s.Empresa.Nombre,
                tipologia = tipologias.First(x => x.Id == s.Empresa.Tipologias.First().IdTipologia),
                datosProceso = s.DatosProceso,
                distintivoSiccs = new CommonVm {
                    name = s.Distintivo
                },
                selloItc = new SelloItc
                {
                    name = s.DistintivoExterno
                }
            }).ToList();
            

        }

        public async Task<bool> Update(ApplicationUser appUser, string role, HomologacionDTO datos)
        {

            var distintivosSiccs = await _db.Distintivo.ToListAsync();

            var item = await _db.Homologacion
                .Include(s =>s.Empresa)
                .ThenInclude(s => s.Tipologias)
                .ThenInclude(s => s.Tipologia)
                .Include(s =>s.Certificacion)
                .ThenInclude(s =>s.Resultados).FirstOrDefaultAsync(s => s.Id == datos.id && (s.Empresa.PaisId == appUser.PaisId || role == "Admin"));

            if (item.EnProcesoSiccs == true)
            {
                return false;
            }

            item.Empresa.Nombre = datos.nombre;                      
            item.FechaUltimaEdicion = DateTime.UtcNow;
            item.DatosProceso = datos.datosProceso;
           
            var certificacionActual = item.Empresa.Certificaciones.OrderByDescending(s => s.Id).First();

            if (item.FechaOtorgamiento != datos.fechaOtorgamiento)
            {
                item.FechaOtorgamiento = datos.fechaOtorgamiento;
                certificacionActual.FechaInicio = datos.fechaOtorgamiento;
            }

            if (item.FechaVencimiento != datos.fechaVencimiento)
            {
                item.FechaVencimiento = datos.fechaVencimiento;
                certificacionActual.FechaVencimiento = datos.fechaVencimiento;
            }

            _db.SaveChanges();

            var tipologia = item.Empresa.Tipologias.First().Tipologia;
            
            if (datos.selloItc.name != item.DistintivoExterno)
            {
                //hay que modificar el distintivo
                item.DistintivoExterno = datos.selloItc.name;
                item.Distintivo = datos.distintivoSiccs.name;


                item.Empresa.ResultadoActual = datos.distintivoSiccs.name;
                var resultadoActual = item.Empresa.Certificaciones.OrderByDescending(s => s.Id).First().Resultados.FirstOrDefault();

                var nuevoDistintivo = distintivosSiccs.First(s => s.Name == datos.distintivoSiccs.name || s.NameEnglish == datos.distintivoSiccs.name);

                resultadoActual.DistintivoId = nuevoDistintivo.Id;

                _db.SaveChanges();
            }

            //actualizar la tipologia
            if (datos.tipologia.Id != tipologia.Id)
            {

                item.Empresa.Tipologias.Clear();
                await _db.SaveChangesAsync();

                #region ----AGREGA TIPOLOGIA----

                var listaTipologias = new List<TipologiasEmpresa>();


                var TipologiaEmpresa = new TipologiasEmpresa
                {
                    IdEmpresa = item.EmpresaId,
                    IdTipologia = datos.tipologia.Id,
                };

                listaTipologias.Add(TipologiaEmpresa);

                item.Empresa.Tipologias = listaTipologias;

                _db.SaveChanges();

                #endregion
            }            
            return true;
        }
    }
}
