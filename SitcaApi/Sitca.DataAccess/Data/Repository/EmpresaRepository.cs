using Microsoft.EntityFrameworkCore;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.Models;
using Utilities;
using Sitca.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Sitca.DataAccess.Data.Repository
{
    public class EmpresaRepository: Repository<Empresa>, IEmpresaRepository
    {
        private readonly ApplicationDbContext _db;

        public EmpresaRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public bool ActualizarDatos(EmpresaUpdateVm datos, string user, string role)
        {            
            var empresaId = datos.Id;
            if (role == "Empresa")
            {
                var userDB = _db.ApplicationUser.FirstOrDefault(s => s.UserName == user);
                empresaId = userDB.EmpresaId??datos.Id;
            }

            var empresa = _db.Empresa.Include("Tipologias").FirstOrDefault(s => s.Id == empresaId);

            empresa.Direccion = datos.Direccion;
            empresa.IdNacional = datos.IdNacionalRepresentante;
            empresa.Nombre = datos.Nombre;
            empresa.NombreRepresentante = datos.Responsable;
            //no se deberia poder modificar el pais???
            if (role == "Empresa" && empresa.Estado < 2)
            {
                empresa.PaisId = datos.Pais.id;
            }            
            empresa.CargoRepresentante = datos.CargoRepresentante;
            empresa.Ciudad = datos.Ciudad;
            empresa.Telefono = datos.Telefono;
            empresa.WebSite = datos.Website;
            empresa.Email = datos.Email;

            if (datos.Tipologias.Any(s => s.isSelected))
            {
                empresa.Tipologias.Clear();
                Context.SaveChanges();
                var listaTipologias = new List<TipologiasEmpresa>();

                foreach (var item in datos.Tipologias.Where(s =>s.isSelected))
                {
                    var TipologiaEmpresa = new TipologiasEmpresa
                    {
                        IdEmpresa = empresa.Id,
                        IdTipologia = item.id,
                    };

                    listaTipologias.Add(TipologiaEmpresa);
                }
                empresa.Tipologias = listaTipologias;
            }

            Context.SaveChanges();
            

            return true;
        }

        public int SaveEmpresa(RegisterVm model)
        {

            var empresa = new Empresa
            {
                Active = true,
                IdPais = model.country,
                PaisId = model.country,
                Nombre = model.empresa,
                NombreRepresentante = model.representante,
                Estado = 0
            };

            _db.Empresa.Add(empresa);
            _db.SaveChanges();

            var listaTipologias = new List<TipologiasEmpresa>();

            foreach (var item in model.tipologias.Where(s => s.isSelected))
            {                
                var TipologiaEmpresa = new TipologiasEmpresa
                {         
                    IdEmpresa = empresa.Id,
                    IdTipologia = item.id,       
                };

                listaTipologias.Add(TipologiaEmpresa);
            }

            empresa.Tipologias = listaTipologias;
           
            _db.SaveChanges();

            return empresa.Id;
        }

        

        public List<EmpresaVm> GetList(int? idEstado,int idPais = 0 , int idTipologia = 0, string q = null, string leng = "es")
        {
            var empresas = _db.Empresa.Include(x =>x.Tipologias).Include(s =>s.Certificaciones).Where(x => x.Nombre != null
            && (x.PaisId == idPais || idPais == 0)
            && !x.EsHomologacion
            && (x.Estado == idEstado || idEstado == null)
            && ( idTipologia == 0  || (x.Tipologias.Any(z => z.IdTipologia == idTipologia)))
            );
            
            if (q != null)
            {
                empresas = empresas.Where(s => s.Nombre.Contains(q));
            }
                        
            var result = empresas.Select(x => new EmpresaVm { 
                Nombre = x.Nombre,
                Certificacion = x.Certificaciones.Any()? x.Certificaciones.First().Id.ToString():null,
                Pais = x.Pais.Name,
                Id = x.Id,
                Recertificacion = x.Certificaciones.Count > 1 || x.Certificaciones.Any(s =>s.Recertificacion),
                Status = x.Estado.ToString(),
                Responsable = _db.ApplicationUser.FirstOrDefault(s =>s.EmpresaId == x.Id).FirstName,
                Tipologias = x.Tipologias.Any()? x.Tipologias.Select(z => leng == "es"? z.Tipologia.Name: z.Tipologia.NameEnglish).ToList():null
            }).ToList();


            foreach (var item in result)
            {
                item.Status = item.Status.ToDecimal().GetEstado(leng);
            }

            return result;
        }

        public List<EmpresaVm> GetListXVencerReporte(FiltroEmpresaReporteVm data)
        {
            bool? homologacion = data.homologacion == "-1" ? true : data.homologacion == "1"? false: (bool?)null ;

            var meses = data.meses ?? 1;
            var dueDate = DateTime.Now.AddMonths(meses);
            var empresas = _db.Empresa.Include(x => x.Tipologias).Where(x => x.Nombre != null
             && (x.PaisId == data.country || data.country == 0)
              && (x.EsHomologacion == homologacion || homologacion == null)
             //&& (x.Estado == 8)
             && (x.ResultadoVencimiento != null && x.ResultadoVencimiento < dueDate)
             && (data.tipologia == 0 || data.tipologia == null || (x.Tipologias.Any(z => z.IdTipologia == data.tipologia)))
            );

            var result = empresas.Select(x => new EmpresaVm
            {
                Nombre = x.Nombre,
                Pais = x.Pais.Name,
                Id = x.Id,
                Status = x.Estado.ToString(),
                Vencimiento = x.ResultadoVencimiento.ToStringArg(),
                Responsable = _db.ApplicationUser.FirstOrDefault(s => s.EmpresaId == x.Id).FirstName,
                Certificacion = x.ResultadoActual,
                Tipologias = x.Tipologias.Any() ? x.Tipologias.Select(z => z.Tipologia.Name).ToList() : null
            }).ToList();

            foreach (var item in result)
            {
                item.Status = item.Status.ToDecimal().GetEstado(data.lang);
            }

           
            return result;
        }

        public List<EmpresaVm> GetListRenovacionReporte(FiltroEmpresaReporteVm data)
        {
                     
            var empresas = _db.Empresa.Include(x => x.Tipologias).Where(x => x.Nombre != null
             && (x.PaisId == data.country || data.country == 0)
             && (x.Estado < 8 && x.Estado > 0)
             && (x.ResultadoVencimiento != null)
             && (x.Certificaciones.Count > 1)
             && (data.tipologia == 0 || data.tipologia == null || (x.Tipologias.Any(z => z.IdTipologia == data.tipologia)))
            );

            var result = empresas.Select(x => new EmpresaVm
            {
                Nombre = x.Nombre,
                Pais = x.Pais.Name,
                Id = x.Id,
                Status = x.Estado.ToString(),
                Vencimiento = x.ResultadoVencimiento.ToStringArg(),
                Responsable = _db.ApplicationUser.FirstOrDefault(s => s.EmpresaId == x.Id).FirstName,
                Certificacion = x.ResultadoActual,
                Tipologias = x.Tipologias.Any() ?
                data.lang == "es" ? x.Tipologias.Select(z => z.Tipologia.Name).ToList() : x.Tipologias.Select(z => z.Tipologia.NameEnglish).ToList() 
                : null
            }).ToList();

            foreach (var item in result)
            {
                item.Status = item.Status.ToDecimal().GetEstado(data.lang);
            }


            return result;
        }

        public List<EmpresaVm> GetListReporte(FiltroEmpresaReporteVm data)
        {
            bool? homologacion = data.homologacion == "-1" ? true : data.homologacion == "1" ? false : (bool?)null;

            var empresas = _db.Empresa.Include(x => x.Tipologias).Where(x => x.Nombre != null
             && (x.PaisId == data.country || data.country == 0)
             && (x.Estado == data.estado || data.estado == -1)
             && (x.EsHomologacion == homologacion || homologacion == null)
             && (data.tipologia == 0 || data.tipologia == null || (x.Tipologias.Any(z => z.IdTipologia == data.tipologia)))
            );            

            var result = empresas.Select(x => new EmpresaVm
            {
                Nombre = x.Nombre,
                Pais = x.Pais.Name,
                Id = x.Id,
                Status = x.Estado.ToString(),
                Responsable = _db.ApplicationUser.FirstOrDefault(s => s.EmpresaId == x.Id).FirstName,
                Certificacion = x.Estado > 1 ?
                                 x.Estado == 8 ? !x.Certificaciones.OrderByDescending(x =>x.Id).First().Resultados.First().Aprobado? "No Certificado"
                                : data.lang == "es" ? x.Certificaciones.OrderByDescending(x => x.Id).First().Resultados.First().Distintivo.Name : x.Certificaciones.OrderByDescending(x => x.Id).First().Resultados.First().Distintivo.NameEnglish
                                : data.lang == "es" ? "En Proceso" : "In Process"
                                : data.lang == "es" ? "No comenzada" :"Not started",
                Tipologias = x.Tipologias.Any() ? data.lang == "es"? x.Tipologias.Select(z => z.Tipologia.Name).ToList() : x.Tipologias.Select(z => z.Tipologia.NameEnglish).ToList() : null
            }).ToList();

            foreach (var item in result)
            {
                item.Status = item.Status.ToDecimal().GetEstado(data.lang);
                //if(item.Certificacion == "Green Badge")
                //{
                //    if (item.Certificacion.ToLower() == data.certificacion.ToLower())
                //    {
                //        var a = 1;
                //    }
                //}
            }

            if (data.certificacion != "Todas")
            {
                result = result.Where(s => s.Certificacion.ToLower() == data.certificacion.ToLower()).ToList();
            }

            return result;
        }

        public List<EmpresaPersonalVm> GetListReportePersonal(FiltroEmpresaReporteVm data)
        {

            var empresas = _db.Empresa.Include(z =>z.Pais).Include(s =>s.Certificaciones).Where(x => x.Nombre != null
             && (x.PaisId == data.country || data.country == 0)
             && (x.Estado == data.estado || data.estado == -1));

            var listaEmpresas = new List<EmpresaPersonalVm>();
            foreach (var item in empresas.Where(s =>s.Estado > 0))
            {
                try
                {
                    var obj = new EmpresaPersonalVm();
                    obj.Nombre = item.Nombre;
                    obj.Pais = item.Pais.Name;
                    obj.Status = item.Estado.GetEstado(data.lang);
                    var cert = item.Certificaciones.OrderByDescending(s => s.Id).First();

                    if (cert.AsesorId != null)
                    {
                        var asesor = _db.ApplicationUser.Find(cert.AsesorId);
                        obj.Asesor = new CommonUserVm
                        {
                            id = cert.AsesorId,
                            email = asesor.Email,
                            fullName = asesor.FirstName + " " + asesor.LastName,
                            codigo = asesor.Codigo
                        };

                    }

                    if (cert.AuditorId != null)
                    {

                        var auditor = _db.ApplicationUser.Find(cert.AuditorId);
                        obj.Auditor = new CommonUserVm
                        {
                            id = cert.AuditorId,
                            email = auditor.Email,
                            fullName = auditor.FirstName + " " + auditor.LastName,
                            codigo = auditor.Codigo
                        };

                    }

                    if (cert.UserGeneraId != null)
                    {
                        var tecnicoPais = _db.ApplicationUser.Find(cert.UserGeneraId);
                        obj.TecnicoPais = new CommonUserVm
                        {
                            id = cert.UserGeneraId,
                            email = tecnicoPais.Email,
                            fullName = tecnicoPais.FirstName + " " + tecnicoPais.LastName,
                            codigo = tecnicoPais.Codigo
                        };

                    }

                    listaEmpresas.Add(obj);
                }
                catch (Exception e)
                {
                    var a = item;
                    
                }
                

            }         
            return listaEmpresas;
        }

        public async Task< List<EmpresaVm> > ListForRole(ApplicationUser user, string role)
        {           

            if (role == "Asesor")
            {
                var empresas = _db.ProcesoCertificacion.Include(g =>g.Empresa).ThenInclude(x =>x.Tipologias)
                    .Where(s => s.AsesorId == user.Id && s.Status != "8 - Finalizado" && !s.Empresa.EsHomologacion).Select(x => new EmpresaVm
                    {
                        Id = x.EmpresaId,
                        Status = Utilities.Utilities.GetEstado(x.Empresa.Estado, user.Lenguage),
                        Pais = x.Empresa.Pais.Name,
                        Nombre = x.Empresa.Nombre,
                        Recertificacion = x.Recertificacion,
                        Tipologias = x.Empresa.Tipologias.Any() ? x.Empresa.Tipologias.Select(z => z.Tipologia.Name).ToList() : null,
                        Responsable = x.Empresa.NombreRepresentante,                        
                    }).ToListAsync();

                return await empresas;
            }

            if (role == "Auditor")
            {
                var empresas = _db.ProcesoCertificacion.Include("Empresa")
                    .Where(s => s.AuditorId == user.Id && s.Status != "8 - Finalizado").Select(x => new EmpresaVm
                    {
                        Id = x.EmpresaId,
                        Status = Utilities.Utilities.GetEstado(x.Empresa.Estado, user.Lenguage),
                        Pais = x.Empresa.Pais.Name,
                        Nombre = x.Empresa.Nombre,
                        Recertificacion = x.Recertificacion,
                        Tipologias =  x.Empresa.Tipologias.Any() ? user.Lenguage == "es" ? x.Empresa.Tipologias.Select(z => z.Tipologia.Name).ToList(): x.Empresa.Tipologias.Select(z => z.Tipologia.NameEnglish).ToList() : null,
                        Responsable = x.Empresa.NombreRepresentante,
                    }).ToListAsync();

                return await empresas;
            }

            if (role == "CTC")
            {                
                var empresas = _db.ProcesoCertificacion.Include("Empresa")
                    .Where(s => s.Empresa.PaisId == user.PaisId && s.Empresa.Estado > 5 && s.Empresa.Estado < 8 && !s.Status.Contains("8")).Select(x => new EmpresaVm
                    {
                        Id = x.EmpresaId,
                        Status = Utilities.Utilities.GetEstado(x.Empresa.Estado, user.Lenguage),
                        Pais = x.Empresa.Pais.Name,
                        Recertificacion = x.Recertificacion,
                        Nombre = x.Empresa.Nombre,
                        Tipologias = x.Empresa.Tipologias.Any() ? user.Lenguage == "es" ? x.Empresa.Tipologias.Select(z => z.Tipologia.Name).ToList() : x.Empresa.Tipologias.Select(z => z.Tipologia.NameEnglish).ToList() : null,
                        Responsable = x.Empresa.NombreRepresentante,
                    }).ToListAsync();

                return await empresas;
            }


            return null;
        }

        public async Task<bool> SolicitaAuditoria(int idEmpresa)
        {
            try
            {
                var procesoCertificacion = await _db.ProcesoCertificacion.OrderByDescending(s => s.Id).FirstOrDefaultAsync(s => s.EmpresaId == idEmpresa);
                procesoCertificacion.FechaSolicitudAuditoria = DateTime.UtcNow;

                //var empresa = await _db.Empresa.FindAsync(idEmpresa);
                //empresa.Estado = 3;

                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }           
        }

        public async Task<List<EstadisticaItemVm>> EnCertificacion(int idPais, string lenguage)
        {
            try
            {
                //Empresas que estan en asesoria
                var empresasPorTipologia = _db.Tipologia.Include(s =>s.Empresas).Select(x => new EstadisticaItemVm
                {
                    Id = x.Id,
                    Name = lenguage == "es"? x.Name:x.NameEnglish,
                    Count = x.Empresas.Where(s => s.Empresa.Estado > 0 && s.Empresa.Estado < 4 
                        && (s.Empresa.PaisId == idPais || idPais == 0  )).Count()
                });

               
                return await empresasPorTipologia.ToListAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<EmpresasCalificadas>> EvaluadasEnCtc(int idPais, string language)
        {
            try
            {
                var distintivos = await  _db.Distintivo.ToListAsync();

                var query  = await _db.ProcesoCertificacion
                    .Include(u => u.AuditorProceso)
                    .Include(z => z.AsesorProceso)
                    .Include(x =>x.Empresa)
                    .ThenInclude(s => s.Tipologias)
                    .ThenInclude(s =>s.Tipologia)
                    .Include(m =>m.Resultados)
                    .Where(s => s.Status.Contains("8") && s.Empresa.PaisId == idPais).ToListAsync();

                var noCertificado = language == "es" ? "No Certificado" : "Not Certified";
                
                var result = query.Select(x => new EmpresasCalificadas
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,
                    Name = x.Empresa.Nombre,
                    Aprobado = x.Resultados.First().Aprobado,
                    Asesor = x.AsesorProceso != null? new CommonUserVm
                    {
                        fullName = x.AsesorProceso.FirstName + " " + x.AsesorProceso.LastName,
                        codigo = x.AsesorProceso.Codigo,
                        id = x.AsesorId
                    }:null,
                    Auditor = x.AuditorProceso != null ? new CommonUserVm
                    {
                        fullName = x.AuditorProceso.FirstName + " " + x.AuditorProceso.LastName,
                        codigo = x.AuditorProceso.Codigo,
                        id = x.AuditorId
                    } : null,
                    Tipologia = new CommonVm{ 
                        name = language == "es"? x.Empresa.Tipologias.First().Tipologia.Name: x.Empresa.Tipologias.First().Tipologia.NameEnglish,
                    },
                    Observaciones = x.Resultados.First().Observaciones,
                    FechaDictamen = x.FechaFinalizacion.Value.AddHours(-6).ToStringArg(),
                    Distintivo = x.Resultados.First().Aprobado ? language == "es" ? distintivos.FirstOrDefault(u => u.Id == x.Resultados.First().DistintivoId).Name : distintivos.FirstOrDefault(u => u.Id == x.Resultados.First().DistintivoId).NameEnglish : noCertificado,                    
                    NumeroDictamen = x.Resultados.First().NumeroDictamen,
                }).ToList();
                
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<List<EstadisticaItemVm>> EstadisticasCtc(int idPais, string lang)
        {
            try
            {
                //Empresas que estan en asesoria
                var empresasPorTipologia = _db.Tipologia.Include(s => s.Empresas).Select(x => new EstadisticaItemVm
                {
                    Id = x.Id,
                    Name = lang == "es"? x.Name:x.NameEnglish,
                    Count = x.Empresas.Where(s => s.Empresa.Active && s.Empresa.Estado > 0 && s.Empresa.Estado > 5 && s.Empresa.Estado < 8
                        && (s.Empresa.PaisId == idPais || idPais == 0)).Count()
                });

                return await empresasPorTipologia.ToListAsync();
            }
            catch (Exception)
            {
                return null;
            }

        }

        public async Task<EmpresaUpdateVm> Data(int empresaId, string userId)
        {
            var user = await _db.ApplicationUser.FindAsync(userId);

            var empresa = await _db.Empresa.Include(p =>p.Pais).Include(t => t.Tipologias).Include(x =>x.Archivos).ThenInclude(c =>c.UsuarioCarga).FirstOrDefaultAsync(s => s.Id == empresaId && s.Active);

            

            var allTipologias = await _db.Tipologia.ToListAsync();
            var result = new EmpresaUpdateVm
            {   Language = user.Lenguage,
                Nombre = empresa.Nombre,
                Pais = new CommonVm {
                    id = empresa.Pais.Id,
                    name = empresa.Pais.Name
                },
                Id = empresa.Id,
                ResultadoSugerido = empresa.ResultadoSugerido,
                Estado = empresa.Estado ?? 0,
                CargoRepresentante = empresa.CargoRepresentante,
                Ciudad = empresa.Ciudad,
                Telefono = empresa.Telefono,
                Responsable = empresa.NombreRepresentante,
                Direccion = empresa.Direccion,
                Website = empresa.WebSite,
                Email = empresa.Email,
                IdNacionalRepresentante = empresa.IdNacional,
                Tipologias = allTipologias.Select(x => new CommonVm
                {
                    name = user.Lenguage == "es"? x.Name:x.NameEnglish,
                    id = x.Id,
                    isSelected = empresa.Tipologias.Any(z => z.IdTipologia == x.Id)
                }).ToList(),
                MesHoy = DateTime.Now.ToString("MMMM", CultureInfo.CreateSpecificCulture("es")),
                Archivos = empresa.Archivos != null? empresa.Archivos.Where(s =>s.Activo).Select(z => new ArchivoVm {
                    Id = z.Id,
                    Nombre = z.Nombre,
                    Ruta = z.Ruta,
                    Tipo = z.Tipo,
                    Cargador = z.UsuarioCarga.FirstName + " " + z.UsuarioCarga.LastName,
                    FechaCarga = z.FechaCarga.ToUtc(),
                    Propio = z.UsuarioCargaId == userId
                }).ToList():null
            };

            var noCertificado = user.Lenguage == "es" ? "No certificado" : "Not certified";

            var certificaciones =  _db.ProcesoCertificacion.Include(i => i.Resultados).ThenInclude(it => it.Distintivo).Where(s => s.EmpresaId == empresaId).Select(x => new CertificacionDetailsVm {
                Status = Utilities.Utilities.CambiarIdiomaEstado(x.Status,user.Lenguage),
                FechaInicio = x.FechaInicio.ToUtc(),
                FechaFin =  x.FechaFinalizacion.ToUtc(),
                Expediente = x.NumeroExpediente,
                Recertificacion = x.Recertificacion,
                Asesor = new CommonUserVm
                {
                    email = x.AsesorProceso.Email,
                    fullName = x.AsesorProceso.FirstName + " " + x.AsesorProceso.LastName,
                    id = x.AsesorId,
                    phone = x.AsesorProceso.PhoneNumber,
                    codigo = x.AsesorProceso.NumeroCarnet
                },
                Auditor = x.AuditorId != null? new CommonUserVm
                {
                    email = x.AuditorProceso.Email,
                    fullName = x.AuditorProceso.FirstName + " " + x.AuditorProceso.LastName,
                    id = x.AuditorId,
                    phone = x.AuditorProceso.PhoneNumber,
                    codigo = x.AuditorProceso.NumeroCarnet
                } :null,
                Generador = new CommonUserVm
                {
                    email = x.UserGenerador.Email,
                    fullName = x.UserGenerador.FirstName + " " + x.UserGenerador.LastName,
                    id = x.UserGeneraId
                },
                Resultado = x.Resultados.Any()?x.Resultados.First().Aprobado? user.Lenguage == "es"? x.Resultados.First().Distintivo.Name: x.Resultados.First().Distintivo.NameEnglish: noCertificado : "",
                FechaVencimiento = x.FechaVencimiento.ToStringArg(),
                Id = x.Id
            }).ToListAsync();
            
            result.Certificaciones = await certificaciones;

            if (result.Certificaciones.Any())
            {
                result.CertificacionActual = result.Certificaciones.OrderByDescending(s => s.Id).First();
                if (result.CertificacionActual.FechaVencimiento != null)
                {
                    var dueDate = DateTime.Now.AddMonths(6);
                    var vencimientoSello = result.CertificacionActual.FechaVencimiento.ToDateArg();
                    if (vencimientoSello < dueDate)
                    {
                        result.CertificacionActual.alertaVencimiento = true;
                    }
                }
            }

            return result;
        }
       

        public EstadisticasVm Estadisticas(string lang)
        {
            var empresasPorPais = _db.Pais.Select(x => new EstadisticaItemVm
            {
                Id = x.Id,
                Name = (x.Name == "Republica Dominicana" && lang == "en") ? "Dominican Republic":x.Name,
                Count = x.Empresas.Count(s =>s.Active)
            });

            var empresasPorTipologia = _db.Tipologia.Select(x => new EstadisticaItemVm
            {
                Id = x.Id,
                Name = lang == "es"? x.Name: x.NameEnglish,
                Count = x.Empresas.Count()
            });

            var result = new EstadisticasVm
            {
                EmpresasPorPais = empresasPorPais,
                EmpresasPorTipologia = empresasPorTipologia
            };

            return result;
        }

        public async Task<UrlResult> Delete(int id, int paisId, string role)
        {
            try
            {
                var empresa = _db.Empresa.Find(id);

                if (role != "Admin")
                {
                    if (empresa.PaisId != paisId){
                        return new UrlResult
                        {
                            Success = false,
                            Message = "Error 403 Forbidden"
                        };
                    }                    
                }

                var homologaciones = _db.Homologacion.Where(s => s.EmpresaId == id);
                var proceso = _db.ProcesoCertificacion.Where(s => s.EmpresaId == id);
                var cuestionarios = _db.CuestionarioItem.Include(s => s.Archivos).Where(s => s.Cuestionario.IdEmpresa == id);


                foreach (var item in cuestionarios)
                {
                    if (item.Archivos.Any())
                    {
                        _db.Archivo.RemoveRange(item.Archivos);
                    }

                    var obs = _db.CuestionarioItemObservaciones.Where(s => s.CuestionarioItemId == item.Id);
                    if (obs.Any())
                    {
                        _db.CuestionarioItemObservaciones.RemoveRange(obs);
                    }

                   
                }
                await _db.SaveChangesAsync();

                _db.CuestionarioItem.RemoveRange(cuestionarios);
                _db.SaveChanges();

                var otrosarchivos = _db.Archivo.Where(s => s.EmpresaId == id);
                _db.Archivo.RemoveRange(otrosarchivos);
                await _db.SaveChangesAsync();


                var cuestionariosObj = _db.Cuestionario.Where(s => s.IdEmpresa == id);
                _db.Cuestionario.RemoveRange(cuestionariosObj);
                _db.SaveChanges();
                foreach (var item in proceso)
                {
                    var resultado = _db.ResultadoCertificacion.Where(s => s.CertificacionId == item.Id);
                    _db.ResultadoCertificacion.RemoveRange(resultado);
                }
                await _db.SaveChangesAsync();

                _db.ProcesoCertificacion.RemoveRange(proceso);
                await _db.SaveChangesAsync();

                _db.Empresa.Remove(empresa);
                await _db.SaveChangesAsync();                
            }
            catch (Exception e)
            {

                return new UrlResult
                {
                    Success = false,
                    Message = e.InnerException + " " + e.Message 
                };
            }


            return new UrlResult
            {
                Success = true                
            };
        }

        public async Task<ResponseListadoExterno> GetCertificadasParaExterior(ListadoExternoFiltro filtro)
        {
            var countries = await _db.Pais.ToListAsync();
            var pais = 0;
            if (!string.IsNullOrEmpty(filtro.Pais))
            {
                var paisObj = countries.FirstOrDefault(s => s.Name.ToLower() == filtro.Pais.ToLower());
                if (paisObj != null)
                {
                    pais = paisObj.Id;
                }
            }

            try
            {
                //empresas que estan en recertificacion teniendo un solo proceso (importadas) o empresas que tienen un proceso finalizado y con fechavencimiento valida
                var empresasImportadas = await _db.Empresa
                    .Include(s => s.Certificaciones)
                    .Include(s =>s.Tipologias)
                    .ThenInclude(s =>s.Tipologia)
                    .Where(s => (s.PaisId == pais || pais == 0) 
                        && ((s.Certificaciones.Count == 1 && s.Certificaciones.Any(x => x.Recertificacion)) 
                        || s.Certificaciones.Any(s => s.FechaVencimiento != null && s.FechaVencimiento > DateTime.Now.Date))).ToListAsync();

                return new ResponseListadoExterno
                {
                    Success = true,
                    Data = empresasImportadas.Select(x => new ListadoExterno
                    {
                        Id = x.Id.ToString(),
                        Nombre = x.Nombre,
                        Pais = x.Pais.Name,
                        Tipologia = x.Tipologias.First().Tipologia.Name
                    }).ToList()                    
                };
            }
            catch (Exception e)
            {

                return new ResponseListadoExterno
                {
                    Success = false,
                    Message = e.InnerException.ToString()

                };
            }
            

            
        }
    }
}
