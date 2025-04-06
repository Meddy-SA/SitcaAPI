using System.Collections.Generic;
using System.Threading.Tasks;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;

namespace Sitca.DataAccess.Data.Repository.IRepository;

public interface IEmpresasRepository : IRepository<Empresa>
{
    Task<Result<MetadatosDTO>> GetMetadataAsync(string language);

    Task<Result<bool>> ActualizarDatosEmpresaAsync(
        EmpresaBasicaDTO datosEmpresa,
        ApplicationUser user,
        string role
    );

    Task<Result<List<ProcesoArchivoDTO>>> GetFilesByCompanyAsync(int empresaId);

    Task<BlockResult<ProcesoCertificacionVm>> GetProcesosCompaniesBlockAsync(
        FilterCompanyDTO filtro
    );
}
