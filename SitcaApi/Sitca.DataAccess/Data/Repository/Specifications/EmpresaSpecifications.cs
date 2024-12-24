using System.Linq;
using Microsoft.EntityFrameworkCore;
using Sitca.DataAccess.Data.Repository.Constants;
using Sitca.Models;

namespace Sitca.DataAccess.Data.Repository.Specifications;

public static class EmpresaSpecifications
{
    public static IQueryable<ProcesoCertificacion> ForAsesor(IQueryable<ProcesoCertificacion> query, string userId) =>
          query.Include(g => g.Empresa)
              .ThenInclude(x => x.Tipologias)
              .Where(s => s.AsesorId == userId &&
                         s.Status != StatusConstants.GetLocalizedStatus("es") &&
                         !s.Empresa.EsHomologacion);

    public static IQueryable<ProcesoCertificacion> ForAuditor(IQueryable<ProcesoCertificacion> query, string userId) =>
        query.Include(g => g.Empresa)
            .ThenInclude(x => x.Tipologias)
            .Where(s => s.AuditorId == userId &&
                       s.Status != StatusConstants.GetLocalizedStatus("es"));

    public static IQueryable<ProcesoCertificacion> ForCTC(IQueryable<ProcesoCertificacion> query, int paisId) =>
        query.Include(g => g.Empresa)
            .ThenInclude(x => x.Tipologias)
            .Where(s => s.Empresa.PaisId == paisId &&
                       s.Empresa.Estado > 5 &&
                       s.Empresa.Estado < 8 &&
                       !s.Status.Contains("8"));

    public static IQueryable<ProcesoCertificacion> ForConsultor(IQueryable<ProcesoCertificacion> query, int paisId) =>
        query.Include(g => g.Empresa)
            .ThenInclude(x => x.Tipologias)
            .Where(s => s.Empresa.PaisId == paisId);
}
