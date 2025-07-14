using System.Collections.Generic;
using System.Threading.Tasks;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Services.ProcesosDeletion;

public interface IProcesosDeletionService
{
    Task<Result<ProcesosDeletionInfo>> CanDeleteProcesoAsync(
        int procesoId,
        int paisId,
        string userRole
    );
    Task<Result<ProcesosDeletionResult>> DeleteProcesoWithRelatedEntitiesAsync(
        int procesoId,
        int paisId,
        string userRole
    );
}

public class ProcesosDeletionInfo
{
    public int ProcesoId { get; set; }
    public string ProcesoExpediente { get; set; } = string.Empty;
    public int TotalCuestionarios { get; set; }
    public int TotalResultados { get; set; }
    public int TotalArchivos { get; set; }
    public int TotalHomologaciones { get; set; }
    public bool CanDelete { get; set; }
    public List<string> Dependencies { get; set; } = new();
}

public class ProcesosDeletionResult
{
    public int ProcesoId { get; set; }
    public string ProcesoExpediente { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int QuestionnairesDeleted { get; set; }
    public int ResultsDeleted { get; set; }
    public int FilesDeleted { get; set; }
    public int HomologacionesDeleted { get; set; }
    public List<string> DeletedEntities { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

