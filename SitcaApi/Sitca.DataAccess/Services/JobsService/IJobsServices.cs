using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Services.JobsService
{
    public interface IJobsServices
    {
        Task<bool> EnviarRecordatorio();

        Task<bool> NotificarVencimientoCarnets();
    }
}
