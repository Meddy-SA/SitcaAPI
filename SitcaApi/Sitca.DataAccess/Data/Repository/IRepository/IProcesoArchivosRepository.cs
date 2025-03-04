using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Sitca.Models;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface IProcesoArchivosRepository : IRepository<ProcesoArchivos>
    {
        /// <summary>
        /// Obtiene todos los archivos de un proceso de certificación
        /// </summary>
        Task<Result<List<ProcesoArchivoDTO>>> GetArchivosByProcesoIdAsync(
            FiltrarArchivosProcesoDTO filtro
        );

        /// <summary>
        /// Obtiene un archivo específico por su ID
        /// </summary>
        Task<Result<ProcesoArchivoDTO>> GetArchivoByIdAsync(int id);

        /// <summary>
        /// Agrega un nuevo archivo a un proceso de certificación
        /// </summary>
        Task<Result<int>> AddArchivoAsync(
            IFormCollection form,
            string userId,
            int procesoId,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Actualiza la información de un archivo existente
        /// </summary>
        Task<Result<bool>> UpdateArchivoAsync(
            int id,
            ActualizarArchivoProcesoDTO dto,
            string userId
        );

        /// <summary>
        /// Elimina lógicamente un archivo
        /// </summary>
        Task<Result<bool>> DeleteArchivoAsync(int id, string userId);

        /// <summary>
        /// Obtiene la ruta física de un archivo para su descarga
        /// </summary>
        Task<Result<string>> GetArchivoRutaFisicaAsync(int id);
    }
}
