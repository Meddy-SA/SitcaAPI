using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Sitca.DataAccess.Services.Files;

public interface IFileService
{
    string GetFullPath();

    /// <summary>
    /// Verifica si el tipo de archivo está permitido
    /// </summary>
    bool IsFileTypeAllowed(IFormFile file);

    /// <summary>
    /// Guarda un archivo con optimización según el tipo
    /// </summary>
    Task<(string FilePath, long FileSize)> SaveFileAsync(
        IFormFile file,
        string subfolder = "",
        CancellationToken cancellationToken = default
    );

    (string FilePath, long FileSize) CopyFileAsync(
        string sourceRelativePath,
        string targetSubfolder,
        string newFileName = null
    );
}
