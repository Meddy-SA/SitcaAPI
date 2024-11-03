using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.DataAccess.Extensions;
using Sitca.Models;
using Sitca.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace Sitca.DataAccess.Data.Repository
{
  class CertificacionRepository : Repository<ProcesoCertificacion>, ICertificacionRepository
  {
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<CertificacionRepository> _logger;

    public CertificacionRepository(ApplicationDbContext db, IConfiguration configuration, ILogger<CertificacionRepository> logger) : base(db)
    {
      _db = db;
      _config = configuration;
      _logger = logger;
    }

    public async Task<bool> UpdateNumeroExp(CertificacionDetailsVm data)
    {
      //agregar tipologia
      try
      {
        var certificacion = await _db.ProcesoCertificacion.FindAsync(data.Id);

        if (certificacion != null)
        {
          certificacion.NumeroExpediente = data.Expediente;
        }

        await _db.SaveChangesAsync();
      }
      catch (Exception)
      {
        return false;
      }

      return true;
    }

    public async Task<bool> SaveCalificacion(SaveCalificacionVm data, ApplicationUser appUser, string role)
    {
      //agregar tipologia
      try
      {
        var certificacion = await _db.ProcesoCertificacion.Include(s => s.Empresa).FirstOrDefaultAsync(x => x.Id == data.idProceso);
        var empresa = certificacion.Empresa;
        certificacion.FechaFinalizacion = DateTime.UtcNow;

        if (data.aprobado)
        {
          certificacion.FechaVencimiento = DateTime.UtcNow.AddYears(2);
          empresa.ResultadoVencimiento = certificacion.FechaVencimiento;
          var distintivo = await _db.Distintivo.FindAsync(data.distintivoId);
          empresa.ResultadoActual = appUser.Lenguage == "es" ? distintivo.Name : distintivo.NameEnglish;
        }

        var resultado = new ResultadoCertificacion
        {
          Aprobado = data.aprobado,
          DistintivoId = data.aprobado ? data.distintivoId : (int?)null,
          CertificacionId = data.idProceso,
          NumeroDictamen = data.Dictamen,
          Observaciones = data.Observaciones
        };
        _db.ResultadoCertificacion.Add(resultado);

        var newStatus = new CertificacionStatusVm
        {
          CertificacionId = data.idProceso,
          Status = "8 - Finalizado"
        };
        await ChangeStatus(newStatus, appUser, role);

        await _db.SaveChangesAsync();
      }
      catch (Exception)
      {
        return false;
      }

      return true;
    }

    public async Task<int> AsignaAuditor(AsignaAuditoriaVm data, ApplicationUser appUser, string role)
    {
      //agregar tipologia
      try
      {
        var proceso = await _db.ProcesoCertificacion.FirstOrDefaultAsync(s => s.EmpresaId == data.EmpresaId && s.FechaFinalizacion == null);

        proceso.AuditorId = data.AuditorId;
        proceso.FechaFijadaAuditoria = data.Fecha.ToDateUniversal();
        proceso.FechaSolicitudAuditoria = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var nuevoEstado = new CertificacionStatusVm
        {
          CertificacionId = proceso.Id,
          Status = "4 - Para Auditar",
        };

        await ChangeStatus(nuevoEstado, appUser, role);
        return proceso.Id;
      }
      catch
      {
        return 0;
      }


    }

    public async Task<CuestionarioDetailsMinVm> GenerarCuestionario(CuestionarioCreateVm data, string userGenerador, string role)
    {
      try
      {

        var cuestionario = new Cuestionario
        {
          FechaInicio = DateTime.UtcNow,
          FechaGenerado = DateTime.UtcNow,
          AsesorId = data.AsesorId,
          IdEmpresa = data.EmpresaId,
          TipologiaId = data.TipologiaId,
          IdTipologia = data.TipologiaId,
          Prueba = role == "Asesor",
          ProcesoCertificacionId = data.CertificacionId,
        };

        _db.Cuestionario.Add(cuestionario);

        var proceso = await _db.ProcesoCertificacion.FindAsync(data.CertificacionId);

        if (role == "Asesor")
        {
          var empresa = await _db.Empresa.FirstOrDefaultAsync(s => s.Id == data.EmpresaId);
          empresa.Estado = 2;
          proceso.TipologiaId = cuestionario.TipologiaId;
          proceso.Status = "2 - Asesoria en Proceso";
        }
        if (role == "Auditor")
        {
          var empresa = await _db.Empresa.FirstOrDefaultAsync(s => s.Id == data.EmpresaId);
          empresa.Estado = 5;

          proceso.Status = "5 - Auditoria en Proceso";
          cuestionario.AuditorId = proceso.AuditorId;
        }
        await _db.SaveChangesAsync();

        var result = new CuestionarioDetailsMinVm
        {
          Id = cuestionario.Id,
          Asesor = new CommonUserVm
          {
            id = data.AsesorId,
            email = "",
          },
          Empresa = new CommonVm
          {
            id = data.EmpresaId,
          },
          Prueba = cuestionario.Prueba,
          Tipologia = new CommonVm
          {
            id = data.TipologiaId,
          }
        };


        return result;
      }
      catch (Exception)
      {
        return null;
      }


    }

    public async Task<int> ComenzarProceso(CertificacionVm data, string userGenerador)
    {
      try
      {
        var otrosProcesos = _db.ProcesoCertificacion.Any(s => s.EmpresaId == data.EmpresaId);
        var proceso = new ProcesoCertificacion
        {
          AsesorId = data.Asesor,
          EmpresaId = data.EmpresaId,
          Status = "1 - Para Asesorar",
          FechaInicio = DateTime.UtcNow,
          UserGeneraId = userGenerador,
          Recertificacion = otrosProcesos,
        };

        _db.ProcesoCertificacion.Add(proceso);
        await _db.SaveChangesAsync();

        var empresa = _db.Empresa.First(s => s.Id == proceso.EmpresaId);
        empresa.Estado = 1;
        empresa.FechaAutoNotif = null;
        await _db.SaveChangesAsync();
        return proceso.Id;
      }
      catch (Exception)
      {

        return 0;
      }

    }


    public async Task<bool> CambiarAuditor(CambioAuditor data)
    {
      var proceso = await _db.ProcesoCertificacion.FindAsync(data.idProceso);

      if (data.auditor)
      {
        proceso.AuditorId = data.userId;
        var cuestionarios = _db.Cuestionario.Where(s => s.ProcesoCertificacionId == proceso.Id && !s.Prueba && s.FechaFinalizado == null);
        foreach (var item in cuestionarios)
        {
          item.AuditorId = proceso.AuditorId;
        }
      }
      else
      {
        proceso.AsesorId = data.userId;
        var cuestionarios = _db.Cuestionario.Where(s => s.ProcesoCertificacionId == proceso.Id && !s.Prueba && s.FechaFinalizado == null);
        foreach (var item in cuestionarios)
        {
          item.AsesorId = proceso.AsesorId;
        }
      }
      await _db.SaveChangesAsync();

      return true;
    }

    public async Task<List<CuestionarioDetailsMinVm>> GetCuestionariosList(int idEmpresa, ApplicationUser user, string role)
    {

      var cuestionarios = await _db.Cuestionario.Where(s => s.IdEmpresa == idEmpresa).Select(s => new CuestionarioDetailsMinVm
      {
        FechaEvaluacion = s.Certificacion == null ? s.FechaVisita.ToStringArg() : s.Certificacion.FechaFijadaAuditoria.Value.ToStringArg(),
        FechaFin = s.FechaFinalizado.ToUtc(),
        FechaInicio = s.FechaInicio.ToUtc(),
        Tipologia = new CommonVm
        {
          name = user.Lenguage == "es" ? s.Tipologia.Name : s.Tipologia.NameEnglish
        },
        Id = s.Id,
        Prueba = s.Prueba,
        IdCertificacion = s.ProcesoCertificacionId ?? 0
      }).ToListAsync();

      return cuestionarios;
    }

    public async Task<CuestionarioNoCumpleVm> GetNoCumplimientos(int id, ApplicationUser user, string role)
    {
      var cuestionario = await _db.Cuestionario.Include("Tipologia").FirstOrDefaultAsync(s => s.Id == id);
      var empresa = _db.Empresa.Find(cuestionario.IdEmpresa);

      var lenguage = user.Lenguage;

      var respuestas = _db.CuestionarioItem.Where(s => s.CuestionarioId == id).ToList();
      var proceso = await _db.ProcesoCertificacion.Include(x => x.AsesorProceso).FirstOrDefaultAsync(x => x.Id == cuestionario.ProcesoCertificacionId);

      if ((role == "Asesor" && proceso.AsesorId != user.Id) || (role == "Auditor" && proceso.AuditorId != user.Id))
      {
        return null;
      }

      var result = new CuestionarioNoCumpleVm
      {
        Id = cuestionario.Id,
        Prueba = cuestionario.Prueba,
        Expediente = proceso.NumeroExpediente,
        FechaFinalizacion = cuestionario.FechaFinalizado.ToStringArg(),
        Tipologia = new CommonVm
        {
          id = cuestionario.IdTipologia,
          name = cuestionario.Tipologia.Name
        },
        Empresa = new CommonVm
        {
          id = cuestionario.IdEmpresa,
          name = empresa.Nombre
        },
        Asesor = new CommonUserVm
        {
          codigo = proceso.AsesorProceso.Codigo,
          fullName = proceso.AsesorProceso.FirstName + " " + proceso.AsesorProceso.LastName
        }
      };

      var bioseguridad = _config["Settings:bioseguridad"] == "true";


      var pregs = await _db.Pregunta.Where(s => (s.TipologiaId == proceso.TipologiaId || s.TipologiaId == null) && (bioseguridad || s.ModuloId < 11)).OrderBy(z => z.Modulo.Orden).ThenBy(y => y.SeccionModuloId).ThenBy(s => s.SubtituloSeccionId)
          .Select(k => new CuestionarioItemVm
          {
            Id = k.Id,
            NoAplica = k.NoAplica,
            Text = lenguage == "es" ? k.Texto : k.Text,
            Order = Int32.Parse(k.Orden),
            Nomenclatura = k.Nomenclatura,
            Obligatoria = k.Obligatoria,
          }).ToListAsync();

      foreach (var pregunta in pregs)
      {
        if (respuestas.Any(s => s.PreguntaId == pregunta.Id))
        {
          var resp = respuestas.First(s => s.PreguntaId == pregunta.Id);
          pregunta.Result = resp.Resultado;
          pregunta.IdRespuesta = resp.Id;
        }
      }

      result.Preguntas = pregs;
      return result;
    }

    public async Task<CuestionarioDetailsVm> GetCuestionario(int id, ApplicationUser user, string role)
    {
      var cuestionario = await _db.Cuestionario
        .Include("Tipologia")
        .FirstOrDefaultAsync(s => s.Id == id);

      var empresa = await _db.Empresa.FindAsync(cuestionario.IdEmpresa);

      var respuestas = await _db.CuestionarioItem
        .Include(s => s.Archivos)
        .Where(s => s.CuestionarioId == id)
        .ToListAsync();

      var proceso = await _db.ProcesoCertificacion.FindAsync(cuestionario.ProcesoCertificacionId);

      if ((role == "Asesor" && proceso.AsesorId != user.Id) || (role == "Auditor" && proceso.AuditorId != user.Id))
      {
        return null;
      }

      var result = new CuestionarioDetailsVm
      {
        Id = cuestionario.Id,
        Prueba = cuestionario.Prueba,
        Pais = empresa.PaisId.GetValueOrDefault(),
        Expediente = proceso.NumeroExpediente,
        Recertificacion = proceso.Recertificacion,
        FechaFinalizacion = cuestionario.FechaFinalizado.ToStringArg(),
        Tipologia = new CommonVm
        {
          id = cuestionario.IdTipologia,
          name = cuestionario.Tipologia.Name
        },
        Empresa = new CommonVm
        {
          id = cuestionario.IdEmpresa,
          name = empresa.Nombre
        },
        Modulos = new List<ModulosVm>(),

      };

      if (!cuestionario.Prueba)
      {
        var auditor = await _db.ApplicationUser.FindAsync(proceso.AuditorId);
        result.Auditor = new CommonUserVm
        {
          fullName = $"{auditor?.FirstName} {auditor?.LastName}".Trim(),
          codigo = auditor.NumeroCarnet
        };
      }
      else
      {
        var asesor = await _db.ApplicationUser.FindAsync(proceso.AsesorId);
        result.Asesor = new CommonUserVm
        {
          fullName = asesor.FirstName + " " + asesor.LastName,
          codigo = asesor.NumeroCarnet
        };
      }


      var modulos = await _db.Modulo
        .Include(s => s.Secciones)
        .ThenInclude(s => s.SubtituloSeccion)
        .Where(s => s.TipologiaId == cuestionario.IdTipologia || s.TipologiaId == null)
        .OrderBy(m => m.Orden)
        .ToListAsync();

      if (_config.GetBool("Settings:bioseguridad"))
      {
        //si no esta activo el modulo de bioseguridad, quitarlo del cuestionario
        modulos = modulos
          .Where(s => s.Id < 11)
          .OrderBy(m => m.Orden).ToList();
      }

      try
      {
        int? none = null;

        foreach (var modulo in modulos)
        {
          var preguntasC = new List<CuestionarioItemVm>();

          if (modulo.Secciones.Any())
          {
            #region SECCIONES
            foreach (var seccion in modulo.Secciones
                .Where(s => s.TipologiaId == cuestionario.IdTipologia || s.TipologiaId == null)
                .OrderBy(x => x.Orden))
            {
              var itemSecc = new CuestionarioItemVm
              {
                Nomenclatura = seccion.Nomenclatura,
                Order = Int32.Parse(seccion.Orden),
                Text = user.Lenguage == "en" ? seccion.NameEnglish : seccion.Name,
                Type = "seccion",
              };
              preguntasC.Add(itemSecc);

              #region SUBTITULOS
              foreach (var subtitulo in seccion.SubtituloSeccion)
              {
                var itemSub = new CuestionarioItemVm
                {
                  Nomenclatura = subtitulo.Nomenclatura,
                  Order = Int32.Parse(subtitulo.Orden),
                  Text = user.Lenguage == "en" ? subtitulo.NameEnglish : subtitulo.Name,
                  Type = "subtitulo",
                };
                preguntasC.Add(itemSub);

                var preguntasSubs = await GetPreguntas(modulo.Id, seccion.Id, subtitulo.Id, user.Lenguage);
                if (preguntasSubs.Any())
                {
                  preguntasC.AddRange(preguntasSubs);
                }

              }
              #endregion



              var preguntas = await GetPreguntas(modulo.Id, seccion.Id, none, user.Lenguage);
              if (preguntas.Any())
              {
                preguntasC.AddRange(preguntas);
              }

            }

            #endregion
          }
          else
          {
            var preguntas = await GetPreguntas(modulo.Id, none, none, user.Lenguage);
            if (preguntas.Any())
            {
              preguntasC.AddRange(preguntas);
            }
          }

          result.Modulos.Add(
              new ModulosVm
              {
                Nomenclatura = modulo.Nomenclatura,
                Orden = modulo.Orden,
                Nombre = user.Lenguage == "es" ? modulo.Nombre : modulo.EnglishName,
                Id = modulo.Id,
                Items = preguntasC,
              });
        }

        foreach (var mod in result.Modulos)
        {
          foreach (var pregunta in mod.Items.Where(s => s.Type == "pregunta"))
          {
            if (respuestas.Any(s => s.PreguntaId == pregunta.Id))
            {
              var resp = respuestas.First(s => s.PreguntaId == pregunta.Id);
              pregunta.Result = resp.Resultado;
              pregunta.IdRespuesta = resp.Id;
              pregunta.TieneArchivos = resp.Archivos.Any(s => s.Activo);
            }
          }
        }

        #region cumplimientos y resultados
        var cumplimientos = _db.Cumplimiento.Where(s => s.TipologiaId == cuestionario.IdTipologia || s.TipologiaId == null);


        foreach (var modulo in result.Modulos)
        {
          modulo.Resultados = new ResultadosModuloVm();

          //cambio para solo tener en cuenta las evaluadas y no las N/A
          modulo.Resultados.TotalObligatorias = modulo.Items.Count(s => s.Type == "pregunta" && s.Obligatoria && s.Result < 2);
          modulo.Resultados.TotalComplementarias = modulo.Items.Count(s => s.Type == "pregunta" && !s.Obligatoria && s.Result < 2);

          //cambio para solo tener en cuenta las cumplidas y no las N/A
          modulo.Resultados.ObligCumple = modulo.Items.Count(s => s.Type == "pregunta" && s.Obligatoria && s.Result == 1);
          modulo.Resultados.ComplementCumple = modulo.Items.Count(s => s.Type == "pregunta" && !s.Obligatoria && s.Result == 1);



          modulo.Resultados.PorcComplementCumple = modulo.Resultados.TotalComplementarias > 0 ? modulo.Resultados.ComplementCumple * 100 / modulo.Resultados.TotalComplementarias : 0;
          modulo.Resultados.PorcObligCumple = modulo.Resultados.TotalObligatorias > 0 ? modulo.Resultados.ObligCumple * 100 / modulo.Resultados.TotalObligatorias : 0;

          //var porcentajeComplement = complementCumplidas * 100 / totalComplement;
          var noCertificado = user.Lenguage == "es" ? "No certificado" : "Not certified";
          modulo.Resultados.ResultModulo = noCertificado;


          //no tener en cuenta por ahora el modulo de bioseguridad
          if (modulo.Id < 11)
          {
            if (modulo.Resultados.TotalObligatorias == modulo.Resultados.ObligCumple)
            {
              var cumple = cumplimientos.FirstOrDefault(s => s.ModuloId == modulo.Id && modulo.Resultados.PorcComplementCumple > s.PorcentajeMinimo && modulo.Resultados.PorcComplementCumple < (s.PorcentajeMaximo + 1));
              if (cumple != null)
              {
                var distintivo = _db.Distintivo.First(x => x.Id == cumple.DistintivoId);
                modulo.Resultados.ResultModulo = user.Lenguage == "es" ? distintivo.Name : distintivo.NameEnglish;
              }
            }
          }
          else
          {
            modulo.Resultados.ResultModulo = "-";
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al cargar resultados");
      }

      #endregion


      return result;
    }

    public async Task<List<CuestionarioItemVm>> GetPreguntas(int idModulo, int? idSeccion, int? idSubtitulo, string lang = "es")
    {
      IQueryable<Pregunta> query = _db.Pregunta;

      if (idSubtitulo.HasValue && idSubtitulo.Value > 0)
      {
        query = query.Where(s => s.SubtituloSeccionId == idSubtitulo);
      }
      else if (idSeccion.HasValue && idSeccion.Value > 0)
      {
        query = query.Where(s => s.SeccionModuloId == idSeccion && s.SubtituloSeccionId == null);
      }
      else
      {
        query = query.Where(s => s.ModuloId == idModulo && s.SeccionModuloId == null && s.SubtituloSeccionId == null);
      }

      return await query.Select(x => new CuestionarioItemVm
      {
        Id = x.Id,
        Nomenclatura = x.Nomenclatura,
        Text = lang == "es" ? x.Texto : x.Text,
        Order = int.Parse(x.Orden),
        Result = 0,
        Type = "pregunta",
        NoAplica = x.NoAplica,
        Obligatoria = x.Obligatoria,
        IdRespuesta = 0
      }).ToListAsync();
    }

    public async Task<bool> ChangeStatus(CertificacionStatusVm data, ApplicationUser appUser, string role)
    {
      //0 - Inicial
      //1 - Para Asesorar
      //2 - Asesoria en Proceso
      //3 - Asesoria Finalizada
      //4 - Para Auditar
      //5 - Auditoria en Proceso
      //6 - Auditoria Finalizada
      //7 - En revisión de CTC
      //8 - Finalizado
      var certificacion = await _db.ProcesoCertificacion.Include("Empresa").FirstOrDefaultAsync(s => s.Id == data.CertificacionId);
      certificacion.Status = data.Status;

      var status = Int32.Parse(data.Status[0].ToString());
      certificacion.Empresa.Estado = status;

      await _db.SaveChangesAsync();

      return true;
    }



    public async Task<List<HistorialVm>> GetHistory(int idCuestionario)
    {
      var cuestionario = await _db.Cuestionario.Include("Items").FirstOrDefaultAsync(s => s.Id == idCuestionario);

      var resultados = new List<HistorialVm>();

      var archivos = await _db.Archivo.Where(s => s.CuestionarioItem.CuestionarioId == idCuestionario && s.Activo).ToListAsync();

      var TotalPreguntas = _db.Pregunta.Count(s => (s.TipologiaId == cuestionario.TipologiaId || s.TipologiaId == null) && (!s.Nomenclatura.StartsWith("mb-")));
      var TotalPorcentaje = 0;
      foreach (var item in cuestionario.Items.GroupBy(s => s.FechaActualizado))
      {
        var archivosCount = archivos.Count(s => s.FechaCarga.Date == item.Key);
        var currentPorcentaje = item.Count() * 100 / TotalPreguntas;
        TotalPorcentaje += currentPorcentaje;
        var fila = new HistorialVm
        {
          Fecha = item.Key.ToStringArg(),
          Cantidad = item.Count(),
          Archivos = archivosCount,
          Porcentaje = currentPorcentaje,
          PorcentajeAcumulado = TotalPorcentaje
        };

        resultados.Add(fila);
      }

      return resultados;
    }

    public async Task<int> SavePregunta(CuestionarioItemVm obj, ApplicationUser appUser, string role)
    {
      var itemCuestionario = await _db.CuestionarioItem.FirstOrDefaultAsync(s => s.CuestionarioId == obj.CuestionarioId && s.PreguntaId == obj.Id);
      if (itemCuestionario != null)
      {
        //ya existe, solo editar el result
        itemCuestionario.Resultado = obj.Result ?? 0;
        await _db.SaveChangesAsync();

        return itemCuestionario.Id;
      }

      var nuevoItem = new CuestionarioItem
      {
        Obligatorio = obj.Obligatoria,
        Nomenclatura = obj.Nomenclatura,
        PreguntaId = obj.Id,
        Texto = obj.Text,
        Resultado = obj.Result ?? 0,
        CuestionarioId = obj.CuestionarioId,
        FechaActualizado = DateTime.Now.Date,
      };

      _db.CuestionarioItem.Add(nuevoItem);
      await _db.SaveChangesAsync();

      //agregar historico de acciones


      return nuevoItem.Id;
    }


    public Task<bool> IsCuestionarioCompleto(CuestionarioDetailsVm data)
    {
      bool isCompleto = data.Modulos
        .SelectMany(x => x.Items)
        .Where(s => s.Type == "pregunta")
        .All(s => s.Result != null && s.Result != 0);

      return Task.FromResult(isCompleto);
    }

    public async Task<int> FinCuestionario(int idCuestionario, ApplicationUser appUser, string role)
    {
      var cuestionario = await _db.Cuestionario.FirstOrDefaultAsync(s => s.Id == idCuestionario);

      cuestionario.FechaFinalizado = DateTime.UtcNow;
      cuestionario.Resultado = 1;

      var certificacion = cuestionario.ProcesoCertificacionId;

      await _db.SaveChangesAsync();

      var status = new CertificacionStatusVm
      {
        CertificacionId = cuestionario.ProcesoCertificacionId ?? 0,
        Status = role == "Asesor" ? "3 - Asesoria Finalizada" : "6 - Auditoria Finalizada"
      };



      await ChangeStatus(status, appUser, role);
      if (role == "Auditor")
      {
        var res = await SaveResultadoSugerido(idCuestionario, appUser, role);
      }

      return cuestionario.ProcesoCertificacionId ?? 0;
    }

    public async Task<bool> SaveResultadoSugerido(int idCuestionario, ApplicationUser appUser, string role)
    {
      var cuestionario = await _db.Cuestionario.FirstOrDefaultAsync(s => s.Id == idCuestionario);
      var empresa = _db.Empresa.Find(cuestionario.IdEmpresa);

      try
      {
        var resultados = await GetCuestionario(idCuestionario, appUser, role);

        if (resultados.Modulos.Any(s => s.Resultados.ResultModulo == "No Certificado"))
        {
          var noCertificado = appUser.Lenguage == "es" ? "No Certificado" : "Not Certified";
          empresa.ResultadoSugerido = noCertificado;
          await _db.SaveChangesAsync();
          return true;
        }

        var distintivos = _db.Distintivo.ToList();
        var listaDistintivosPorModulo = new List<Distintivo>();

        //no tener en cuenta el modulo bioseguridad
        foreach (var item in resultados.Modulos.Where(s => s.Id < 11))
        {
          listaDistintivosPorModulo.Add(distintivos.First(s => s.Name == item.Resultados.ResultModulo || s.NameEnglish == item.Resultados.ResultModulo));
        }

        empresa.ResultadoSugerido = appUser.Lenguage == "es" ? listaDistintivosPorModulo.OrderByDescending(s => s.Importancia).First().Name : listaDistintivosPorModulo.OrderByDescending(s => s.Importancia).First().NameEnglish;
        await _db.SaveChangesAsync();
        return true;

      }
      catch (Exception e)
      {

        var a = e;
        return false;
      }
    }


    public async Task<bool> CalcularResultado(int idCuestionario)
    {
      //var Cuestionario = await _db.Cuestionario.Include("CuestionarioItem").FirstOrDefaultAsync(s => s.Id == idCuestionario);

      var cuestionario = await GetCuestionario(idCuestionario, null, "");

      foreach (var modulo in cuestionario.Modulos)
      {

        var totalOblig = modulo.Items.Count(s => s.Type == "pregunta" && s.Obligatoria);
        var obligatoriasCumplidas = modulo.Items.Count(s => s.Type == "pregunta" && s.Obligatoria && s.Result == 1);

        var totalComplement = modulo.Items.Count(s => s.Type == "pregunta" && !s.Obligatoria);
        var complementCumplidas = modulo.Items.Count(s => s.Type == "pregunta" && !s.Obligatoria && s.Result == 1);

        decimal porcentajeOblig = Math.Floor((decimal)(obligatoriasCumplidas * 100 / totalOblig));
        decimal porcentajeComplement = Math.Floor((decimal)(complementCumplidas * 100 / totalComplement));
      }
      return true;
    }

    public Task<List<CommonVm>> GetStatusList(string lang)
    {
      //0 - Inicial
      //1 - Para Asesorar
      //2 - Asesoria en Proceso
      //3 - Asesoria Finalizada
      //4 - Para Auditar
      //5 - Auditoria en Proceso
      //6 - Auditoria Finalizada
      //7 - En revisión de CTC
      //8 - Finalizado
      var statuses = new List<CommonVm>
    {
        new CommonVm { id = 0, name = lang == "es" ? "Inicial" : "Initial" },
        new CommonVm { id = 1, name = lang == "es" ? "Para Asesorar" : "To be Advised" },
        new CommonVm { id = 2, name = lang == "es" ? "Asesoria en Proceso" : "In Advising Process" },
        new CommonVm { id = 3, name = lang == "es" ? "Asesoria Finalizada" : "Advising Finalized" },
        new CommonVm { id = 4, name = lang == "es" ? "Para Auditar" : "To be Audited" },
        new CommonVm { id = 5, name = lang == "es" ? "Auditoria en Proceso" : "In Auditing Process" },
        new CommonVm { id = 6, name = lang == "es" ? "Auditoria Finalizada" : "Auditing Finalized" },
        new CommonVm { id = 7, name = lang == "es" ? "En revisión de CTC" : "Under CTC Review" },
        new CommonVm { id = 8, name = lang == "es" ? "Finalizado" : "Ended" }
    };

      return Task.FromResult(statuses);
    }

    public async Task<List<CommonVm>> GetDistintivos(string lang)
    {
      var distintivos = await _db.Distintivo.OrderBy(s => s.Importancia).Where(s => s.Activo).Select(x => new CommonVm
      {
        id = x.Id,
        name = lang == "es" ? x.Name : x.NameEnglish,
      }).ToListAsync();

      return distintivos;
    }

    public async Task<bool> SaveObservaciones(ApplicationUser user, ObservacionesDTO data)
    {
      var observacion = await _db.CuestionarioItemObservaciones.FirstOrDefaultAsync(s => s.CuestionarioItemId == data.IdRespuesta);
      if (observacion != null)
      {
        observacion.Observaciones = data.Observaciones;
        await _db.SaveChangesAsync();
        return true;
      }

      var nuevaObservacion = new CuestionarioItemObservaciones
      {
        CuestionarioItemId = data.IdRespuesta,
        Date = DateTime.UtcNow,
        Observaciones = data.Observaciones,
        UsuarioCargaId = user.Id
      };

      _db.CuestionarioItemObservaciones.Add(nuevaObservacion);
      await _db.SaveChangesAsync();
      return true;
    }

    public async Task<ObservacionesDTO> GetObservaciones(int idRespuesta, ApplicationUser user, string role)
    {
      var observacion = await _db.CuestionarioItemObservaciones.FirstOrDefaultAsync(s => s.CuestionarioItemId == idRespuesta);
      if (observacion == null)
      {
        return null;
      }

      return new ObservacionesDTO
      {
        IdRespuesta = idRespuesta,
        Observaciones = observacion.Observaciones
      };
    }

    public async Task<RegistroHallazgos> ReporteHallazgos(int CuestionarioId, ApplicationUser user, string role)
    {
      var cuestionario = await _db.Cuestionario.FirstOrDefaultAsync(s => s.Id == CuestionarioId);
      var empresa = await _db.Empresa.FindAsync(cuestionario.IdEmpresa);
      var auditorDB = await _db.Users.FindAsync(cuestionario.AuditorId);
      ApplicationUser appUser = (ApplicationUser)auditorDB;

      var result = new RegistroHallazgos
      {
        Empresa = empresa.Nombre,
        Generador = appUser.FirstName + " " + appUser.LastName,
        HallazgosItems = new List<HallazgosDTO>()
      };

      var cuestionariosItems = _db.CuestionarioItem.Include(s => s.CuestionarioItemObservaciones).Where(s => s.CuestionarioId == CuestionarioId && s.Resultado == -1);

      foreach (var item in cuestionariosItems)
      {
        var referencia = item.Nomenclatura;
        var orden = new Version("1.1");
        if (item.Nomenclatura.Contains("mb"))
        {
          orden = new Version(item.Nomenclatura.Replace("mb-", ""));
        }
        else
        {
          orden = new Version(item.Nomenclatura);
        }


        var hallazgo = new HallazgosDTO
        {
          Descripcion = item.CuestionarioItemObservaciones.Any() ? item.CuestionarioItemObservaciones.First().Observaciones : "",
          Obligatorio = item.Obligatorio ? "Si" : "No",
          Referencia = item.Nomenclatura,
          ReferenciaOrden = orden,
          //Modulo = item.Nomenclatura.Contains("mb")? "BIO": (orden.Major - 3).ToString(),
          Modulo = item.Nomenclatura.Contains("mb") ? "BIO" : (orden.Major - 3).ToString(),
        };
        result.HallazgosItems.Add(hallazgo);
      }

      if (result.HallazgosItems.Any())
      {
        result.HallazgosItems = result.HallazgosItems.OrderBy(s => s.ReferenciaOrden).ToList();
      }

      return result;
    }

    public async Task<List<ObservacionesDTO>> GetListObservaciones(IEnumerable<int> ItemIds)
    {

      var items = await _db.CuestionarioItemObservaciones.Where(s => ItemIds.Contains(s.CuestionarioItemId)).Select(x => new ObservacionesDTO
      {
        Observaciones = x.Observaciones,
        IdRespuesta = x.CuestionarioItemId
      }).ToListAsync();


      return items;
    }

    public async Task<bool> ConvertirARecertificacion(ApplicationUser user, EmpresaVm data)
    {

      var certificacion = Int32.Parse(data.Certificacion);
      var proceso = await _db.ProcesoCertificacion.FirstOrDefaultAsync(s => s.Id == certificacion && s.EmpresaId == data.Id);

      if (proceso == null)
      {
        return false;
      }

      proceso.Recertificacion = true;
      return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> ReAbrirCuestionario(ApplicationUser user, int cuestionarioId)
    {
      var cuestionario = await _db.Cuestionario.Include(s => s.Certificacion)
          .FirstOrDefaultAsync(s => s.Id == cuestionarioId && (s.Certificacion.Status.StartsWith("6 - ") || s.Certificacion.Status.StartsWith("7 - ")));

      if (cuestionario == null)
      {
        return false;
      }

      //reabrir (colocar en 5)
      using var transaction = _db.Database.BeginTransaction();
      try
      {
        cuestionario.Resultado = 0;
        cuestionario.FechaFinalizado = null;

        cuestionario.Certificacion.FechaFinalizacion = null;
        cuestionario.Certificacion.Status = (cuestionario.Certificacion.Status == "6 - Auditoria Finalizada" || cuestionario.Certificacion.Status == "7 - En revisión de CTC") ? "5 - Auditoria en Proceso" : "5 - Auditing underway";

        var empresa = await _db.Empresa.FirstOrDefaultAsync(s => s.Id == cuestionario.Certificacion.EmpresaId);
        empresa.Estado = 5;

        await _db.SaveChangesAsync();

        transaction.Commit();
      }
      catch (Exception)
      {
        transaction.Rollback();
      }

      return true;
    }
  }
}
