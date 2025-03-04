using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.DataAccess.Extensions;
using Sitca.DataAccess.Services.Files;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.Enums;
using Sitca.Models.Mappers;

namespace Sitca.DataAccess.Data.Repository
{
    public class ProcesoArchivosRepository : Repository<ProcesoArchivos>, IProcesoArchivosRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;
        private readonly IConfiguration _config;
        private readonly ILogger<ProcesoArchivosRepository> _logger;

        public ProcesoArchivosRepository(
            ApplicationDbContext db,
            IFileService fileService,
            IConfiguration configuration,
            ILogger<ProcesoArchivosRepository> logger
        )
            : base(db)
        {
            _db = db;
            _fileService = fileService;
            _config = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los archivos de un proceso de certificación
        /// </summary>
        public async Task<Result<List<ProcesoArchivoDTO>>> GetArchivosByProcesoIdAsync(
            FiltrarArchivosProcesoDTO filtro
        )
        {
            try
            {
                if (filtro == null)
                {
                    return Result<List<ProcesoArchivoDTO>>.Failure("El filtro es requerido");
                }

                // Verificar que el proceso existe
                var procesoExiste = await _db.ProcesoCertificacion.AnyAsync(p =>
                    p.Id == filtro.ProcesoCertificacionId && p.Enabled == true
                );

                if (!procesoExiste)
                {
                    return Result<List<ProcesoArchivoDTO>>.Failure(
                        $"No se encontró el proceso con ID {filtro.ProcesoCertificacionId}"
                    );
                }

                // Construir la consulta
                IQueryable<ProcesoArchivos> query = _db
                    .ProcesoArchivos.AsNoTracking()
                    .Include(a => a.UserCreate)
                    .Where(a =>
                        a.ProcesoCertificacionId == filtro.ProcesoCertificacionId
                        && a.Enabled == true
                    );

                // Aplicar filtro por tipo de archivo
                if (filtro.TipoArchivo.HasValue)
                {
                    query = query.Where(a => a.FileTypesCompany == filtro.TipoArchivo.Value);
                }

                // Aplicar filtro por texto
                if (!string.IsNullOrWhiteSpace(filtro.TextoBusqueda))
                {
                    var texto = filtro.TextoBusqueda.Trim().ToLower();
                    query = query.Where(a => a.Nombre.ToLower().Contains(texto));
                }

                // Ordenar por fecha de creación descendente (más recientes primero)
                query = query.OrderByDescending(a => a.CreatedAt);

                // Ejecutar la consulta
                var archivos = await query.ToListAsync();

                // Mapear a DTOs
                var archivosDto = archivos.Select(a => a.ToDto()).ToList();

                return Result<List<ProcesoArchivoDTO>>.Success(archivosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al obtener archivos del proceso {ProcesoId}",
                    filtro?.ProcesoCertificacionId
                );

                return Result<List<ProcesoArchivoDTO>>.Failure(
                    $"Error al obtener archivos: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Obtiene un archivo específico por su ID
        /// </summary>
        public async Task<Result<ProcesoArchivoDTO>> GetArchivoByIdAsync(int id)
        {
            try
            {
                var archivo = await _db
                    .ProcesoArchivos.AsNoTracking()
                    .Include(a => a.UserCreate)
                    .FirstOrDefaultAsync(a => a.Id == id && a.Enabled == true);

                if (archivo == null)
                {
                    return Result<ProcesoArchivoDTO>.Failure(
                        $"No se encontró el archivo con ID {id}"
                    );
                }

                return Result<ProcesoArchivoDTO>.Success(archivo.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener archivo {ArchivoId}", id);
                return Result<ProcesoArchivoDTO>.Failure($"Error al obtener archivo: {ex.Message}");
            }
        }

        /// <summary>
        /// Agrega un nuevo archivo a un proceso de certificación
        /// </summary>
        public async Task<Result<int>> AddArchivoAsync(
            IFormCollection form,
            string userId,
            int procesoId,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var resultDto = GenerarDtoProcesarArchivo(procesoId, form);
                if (!resultDto.IsSuccess)
                {
                    return Result<int>.Failure(resultDto.Error);
                }

                var dto = resultDto.Value;
                // Validar DTO
                if (dto == null)
                {
                    return Result<int>.Failure("Los datos del archivo son requeridos");
                }

                // Verificar que el proceso existe
                var procesoExiste = await _db.ProcesoCertificacion.AnyAsync(p =>
                    p.Id == dto.ProcesoCertificacionId && p.Enabled == true
                );

                if (!procesoExiste)
                {
                    return Result<int>.Failure(
                        $"No se encontró el proceso con ID {dto.ProcesoCertificacionId}"
                    );
                }

                // Utilizar una estrategia de ejecución para manejo de reintentos
                var strategy = _db.Database.CreateExecutionStrategy();

                return await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _db.Database.BeginTransactionAsync(
                        cancellationToken
                    );
                    try
                    {
                        // Generar un nombre único para el archivo si no se proporciona uno
                        var nombreArchivo = !string.IsNullOrWhiteSpace(dto.Nombre)
                            ? dto.Nombre
                            : $"Archivo-{DateTime.Now:yyyyMMdd-HHmmss}";

                        // Guardar físicamente el archivo usando FileService
                        var rutaRelativa = await GuardarArchivoFisicoAsync(
                            dto.Archivo,
                            dto.ProcesoCertificacionId,
                            cancellationToken
                        );

                        // Crear la entidad de archivo
                        var nuevoArchivo = new ProcesoArchivos
                        {
                            ProcesoCertificacionId = dto.ProcesoCertificacionId,
                            Nombre = nombreArchivo,
                            Ruta = rutaRelativa,
                            Tipo = Path.GetExtension(dto.Archivo.FileName).Replace(".", ""),
                            FileTypesCompany = dto.TipoArchivo,
                            CreatedBy = userId,
                            CreatedAt = DateTime.UtcNow,
                            Enabled = true,
                        };

                        // Guardar en base de datos
                        await _db.ProcesoArchivos.AddAsync(nuevoArchivo);
                        await _db.SaveChangesAsync(cancellationToken);

                        await transaction.CommitAsync(cancellationToken);

                        _logger.LogInformation(
                            "Archivo añadido exitosamente al proceso {ProcesoId}. ID Archivo: {ArchivoId}",
                            dto.ProcesoCertificacionId,
                            nuevoArchivo.Id
                        );

                        return Result<int>.Success(nuevoArchivo.Id);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        throw new Exception($"Error al guardar el archivo: {ex.Message}", ex);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al añadir archivo al proceso {ProcesoId}",
                    form["procesoId"].ToString()
                );

                return Result<int>.Failure($"Error al añadir archivo: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza la información de un archivo existente
        /// </summary>
        public async Task<Result<bool>> UpdateArchivoAsync(
            int id,
            ActualizarArchivoProcesoDTO dto,
            string userId
        )
        {
            try
            {
                // Validar DTO
                if (dto == null)
                {
                    return Result<bool>.Failure(
                        "Los datos para actualizar el archivo son requeridos"
                    );
                }

                var archivo = await _db.ProcesoArchivos.FirstOrDefaultAsync(a =>
                    a.Id == id && a.Enabled == true
                );

                if (archivo == null)
                {
                    return Result<bool>.Failure($"No se encontró el archivo con ID {id}");
                }

                // Actualizar propiedades
                if (!string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    archivo.Nombre = dto.Nombre;
                }

                if (dto.TipoArchivo.HasValue)
                {
                    archivo.FileTypesCompany = dto.TipoArchivo.Value;
                }

                // Actualizar metadatos de auditoría
                archivo.UpdatedBy = userId;
                archivo.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                _logger.LogInformation("Archivo {ArchivoId} actualizado exitosamente", id);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar archivo {ArchivoId}", id);
                return Result<bool>.Failure($"Error al actualizar archivo: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina lógicamente un archivo
        /// </summary>
        public async Task<Result<bool>> DeleteArchivoAsync(int id, string userId)
        {
            try
            {
                var archivo = await _db.ProcesoArchivos.FirstOrDefaultAsync(a =>
                    a.Id == id && a.Enabled == true
                );

                if (archivo == null)
                {
                    return Result<bool>.Failure($"No se encontró el archivo con ID {id}");
                }

                // Eliminación lógica
                archivo.Enabled = false;
                archivo.UpdatedBy = userId;
                archivo.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                _logger.LogInformation("Archivo {ArchivoId} eliminado exitosamente", id);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar archivo {ArchivoId}", id);
                return Result<bool>.Failure($"Error al eliminar archivo: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene la ruta física de un archivo para su descarga
        /// </summary>
        public async Task<Result<string>> GetArchivoRutaFisicaAsync(int id)
        {
            try
            {
                var archivo = await _db
                    .ProcesoArchivos.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == id && a.Enabled == true);

                if (archivo == null)
                {
                    return Result<string>.Failure($"No se encontró el archivo con ID {id}");
                }

                // Obtener la ruta base de almacenamiento
                var rutaPlataforma = _fileService.GetFullPath();
                var rutaCompleta = Path.Combine(rutaPlataforma, archivo.Ruta);

                // Verificar que el archivo existe físicamente
                if (!File.Exists(rutaCompleta))
                {
                    _logger.LogWarning(
                        "El archivo físico {ArchivoRuta} no existe en el sistema de archivos",
                        rutaCompleta
                    );

                    return Result<string>.Failure(
                        $"El archivo físico no se encuentra en el sistema"
                    );
                }

                return Result<string>.Success(rutaCompleta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ruta física del archivo {ArchivoId}", id);
                return Result<string>.Failure($"Error al obtener ruta del archivo: {ex.Message}");
            }
        }

        /// <summary>
        /// Guarda físicamente un archivo en el sistema de archivos
        /// </summary>
        private async Task<string> GuardarArchivoFisicoAsync(
            IFormFile file,
            int procesoCertificacionId,
            CancellationToken cancellationToken = default
        )
        {
            // Usar el subfolder específico para este proceso
            var subfolder = $"procesos/{procesoCertificacionId}";

            // Utilizar FileService para guardar el archivo con optimización
            return await _fileService.SaveFileAsync(file, subfolder, cancellationToken);
        }

        /// <summary>
        /// Genera el dto SubirArchivoProcesoDTO con los datos del Form.
        /// </summary>
        private Result<SubirArchivoProcesoDTO> GenerarDtoProcesarArchivo(
            int procesoId,
            IFormCollection form
        )
        {
            try
            {
                var file = form.Files.First();
                if (file.Length == 0)
                    return Result<SubirArchivoProcesoDTO>.Failure("El archivo está vacío");

                var uploadName = ContentDispositionHeaderValue
                    .Parse(file.ContentDisposition)
                    .FileName.Trim('"');
                var extension = Path.GetExtension(uploadName);

                return Result<SubirArchivoProcesoDTO>.Success(
                    new SubirArchivoProcesoDTO
                    {
                        ProcesoCertificacionId = procesoId,
                        Nombre = form["nombre"].ToString(),
                        Tipo = extension,
                        TipoArchivo = GetFileType(form["tipoArchivo"].ToString()),
                        Archivo = file,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando la solicitud de carga");
                return Result<SubirArchivoProcesoDTO>.Failure("Error validando la solicitud");
            }
        }

        private static FileCompany GetFileType(string typeFile)
        {
            if (string.IsNullOrEmpty(typeFile) || !int.TryParse(typeFile, out int parsedValue))
                return FileCompany.Informativo;

            return FileCompanyExtensions.GetFileType(parsedValue);
        }
    }
}
