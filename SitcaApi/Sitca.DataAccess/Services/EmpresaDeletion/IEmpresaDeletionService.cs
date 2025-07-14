using System.Collections.Generic;
using System.Threading.Tasks;
using Sitca.Models;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Services.EmpresaDeletion;

public interface IEmpresaDeletionService
{
    /// <summary>
    /// Elimina una empresa y todas sus entidades relacionadas de forma segura
    /// </summary>
    /// <param name="empresaId">ID de la empresa a eliminar</param>
    /// <param name="paisId">ID del país del usuario que realiza la eliminación</param>
    /// <param name="userRole">Rol del usuario que realiza la eliminación</param>
    /// <returns>Resultado de la operación con detalles del proceso</returns>
    Task<Result<EmpresaDeletionResult>> DeleteEmpresaWithRelatedEntitiesAsync(
        int empresaId, 
        int paisId, 
        string userRole
    );

    /// <summary>
    /// Verifica si una empresa puede ser eliminada y retorna información sobre las dependencias
    /// </summary>
    /// <param name="empresaId">ID de la empresa a verificar</param>
    /// <param name="paisId">ID del país del usuario</param>
    /// <param name="userRole">Rol del usuario</param>
    /// <returns>Resultado con información sobre las dependencias encontradas</returns>
    Task<Result<EmpresaDeletionInfo>> CanDeleteEmpresaAsync(
        int empresaId, 
        int paisId, 
        string userRole
    );
}

/// <summary>
/// Información sobre las dependencias de una empresa antes de eliminarla
/// </summary>
public class EmpresaDeletionInfo
{
    public int EmpresaId { get; set; }
    public string EmpresaNombre { get; set; } = string.Empty;
    public bool CanDelete { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public int TotalProcesos { get; set; }
    public int TotalCuestionarios { get; set; }
    public int TotalArchivos { get; set; }
    public int TotalUsuarios { get; set; }
    public int TotalHomologaciones { get; set; }
}

/// <summary>
/// Resultado detallado de la eliminación de una empresa
/// </summary>
public class EmpresaDeletionResult
{
    public int EmpresaId { get; set; }
    public string EmpresaNombre { get; set; } = string.Empty;
    public bool Success { get; set; }
    public List<string> DeletedEntities { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public int ProcessesDeleted { get; set; }
    public int QuestionnairesDeleted { get; set; }
    public int FilesDeleted { get; set; }
    public int UsersDeleted { get; set; }
    public int HomologacionesDeleted { get; set; }
}