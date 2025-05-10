using System.Threading.Tasks;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Data.Repository.IRepository;

public interface IEmpresaReportRepository
{
    /// <summary>
    /// Obtiene un reporte de empresas agrupadas con sus procesos de certificación según los filtros aplicados
    /// </summary>
    Task<Result<EmpresaReportResponseDTO>> GetEmpresaReportAsync(EmpresaReportFilterDTO filter);

    /// <summary>
    /// Obtiene los metadatos para los filtros del reporte de empresas
    /// </summary>
    Task<Result<MetadatosDTO>> GetReportMetadataAsync(string language = "es");
}
