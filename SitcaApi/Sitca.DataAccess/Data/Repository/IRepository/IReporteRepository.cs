using Sitca.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface IReporteRepository : IRepository<Cuestionario>
    {
        Task<bool> ReporteCertificacion(int cuestionarioId);
    }
}
