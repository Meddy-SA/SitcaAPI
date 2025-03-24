using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;

namespace Sitca.Controllers
{
    [Route("api/procesos/{procesoId}/archivos")]
    [ApiController]
    [Authorize]
    public class ProcesoArchivosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ProcesoArchivosController> _logger;

        public ProcesoArchivosController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ILogger<ProcesoArchivosController> logger
        )
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los archivos asociados a un proceso
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(Result<List<ProcesoArchivoDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Result<List<ProcesoArchivoDTO>>>> GetArchivos(
            int procesoId,
            [FromQuery] FiltrarArchivosProcesoDTO filtro
        )
        {
            try
            {
                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null)
                    return Unauthorized();

                // Asegurar que el filtro use el ID de proceso de la ruta
                if (filtro == null)
                    filtro = new FiltrarArchivosProcesoDTO();

                filtro.ProcesoCertificacionId = procesoId;

                var result = await _unitOfWork.ProcesoArchivos.GetArchivosByProcesoIdAsync(
                    filtro,
                    appUser.Id
                );
                return this.HandleResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al obtener archivos del proceso: {ProcesoId}",
                    procesoId
                );
                return StatusCode(
                    500,
                    Result<List<ProcesoArchivoDTO>>.Failure("Error interno del servidor")
                );
            }
        }

        /// <summary>
        /// Obtiene un archivo específico por su ID
        /// </summary>
        [HttpGet("{archivoId}")]
        [ProducesResponseType(typeof(Result<ProcesoArchivoDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Result<ProcesoArchivoDTO>>> GetArchivoById(
            int procesoId,
            int archivoId
        )
        {
            try
            {
                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null)
                    return Unauthorized();

                var result = await _unitOfWork.ProcesoArchivos.GetArchivoByIdAsync(
                    archivoId,
                    appUser.Id
                );

                return this.HandleResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al obtener archivo {ArchivoId} del proceso {ProcesoId}",
                    archivoId,
                    procesoId
                );
                return StatusCode(
                    500,
                    Result<ProcesoArchivoDTO>.Failure("Error interno del servidor")
                );
            }
        }

        /// <summary>
        /// Añade un nuevo archivo al proceso
        /// </summary>
        [HttpPost("upload")]
        [ProducesResponseType(typeof(Result<int>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [RequestSizeLimit(50 * 1024 * 1024)] // Limitar a 50MB
        public async Task<ActionResult<Result<int>>> AddArchivo(int procesoId)
        {
            try
            {
                if (!Request.HasFormContentType || !Request.Form.Files.Any())
                    return BadRequest(
                        Result<FileUploadResponse>.Failure("No se encontró ningún archivo")
                    );
                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null)
                    return Unauthorized();

                // Asegurar que el DTO use el ID de proceso de la ruta
                var result = await _unitOfWork.ProcesoArchivos.AddArchivoAsync(
                    Request.Form,
                    appUser.Id,
                    procesoId
                );

                if (result.IsSuccess)
                {
                    // Retornar 201 Created con la ubicación del recurso creado
                    return CreatedAtAction(
                        nameof(GetArchivoById),
                        new { procesoId = procesoId, archivoId = result.Value },
                        result
                    );
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al añadir archivo al proceso {ProcesoId}", procesoId);
                return StatusCode(500, Result<int>.Failure("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Actualiza la información de un archivo
        /// </summary>
        [HttpPut("{archivoId}")]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Result<bool>>> UpdateArchivo(
            int procesoId,
            int archivoId,
            [FromBody] ActualizarArchivoProcesoDTO dto
        )
        {
            try
            {
                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null)
                    return Unauthorized();

                // Verificar que el archivo pertenece al proceso indicado
                var archivo = await _unitOfWork.ProcesoArchivos.GetFirstOrDefault(a =>
                    a.Id == archivoId && a.ProcesoCertificacionId == procesoId && a.Enabled == true
                );

                if (archivo == null)
                {
                    return NotFound(
                        Result<bool>.Failure(
                            "El archivo no existe o no pertenece al proceso especificado"
                        )
                    );
                }

                var result = await _unitOfWork.ProcesoArchivos.UpdateArchivoAsync(
                    archivoId,
                    dto,
                    appUser.Id
                );
                return this.HandleResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al actualizar archivo {ArchivoId} del proceso {ProcesoId}",
                    archivoId,
                    procesoId
                );
                return StatusCode(500, Result<bool>.Failure("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Elimina (desactiva) un archivo
        /// </summary>
        [HttpDelete("{archivoId}")]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Result<bool>>> DeleteArchivo(int procesoId, int archivoId)
        {
            try
            {
                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null)
                    return Unauthorized();

                // Verificar que el archivo pertenece al proceso indicado
                var archivo = await _unitOfWork.ProcesoArchivos.GetFirstOrDefault(a =>
                    a.Id == archivoId && a.ProcesoCertificacionId == procesoId && a.Enabled == true
                );

                if (archivo == null)
                {
                    string msg =
                        appUser.Lenguage == "es"
                            ? "El archivo no existe o no pertenece al proceso especificado"
                            : "The file does not exist or does not belong to the specified process";
                    return NotFound(Result<bool>.Failure(msg));
                }

                var result = await _unitOfWork.ProcesoArchivos.DeleteArchivoAsync(
                    archivoId,
                    appUser.Id
                );
                return this.HandleResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al eliminar archivo {ArchivoId} del proceso {ProcesoId}",
                    archivoId,
                    procesoId
                );
                return StatusCode(500, Result<bool>.Failure("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Descarga un archivo específico
        /// </summary>
        [HttpGet("{archivoId}/descargar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DescargarArchivo(int archivoId)
        {
            try
            {
                var appUser = await this.GetCurrentUserAsync(_userManager);
                if (appUser == null)
                    return Unauthorized();

                // Obtener información del archivo
                var archivoResult = await _unitOfWork.ProcesoArchivos.GetArchivoByIdAsync(
                    archivoId,
                    appUser.Id
                );

                if (!archivoResult.IsSuccess)
                {
                    return NotFound(Result<bool>.Failure("Archivo no encontrado"));
                }

                var archivo = archivoResult.Value;

                // Obtener la ruta física del archivo
                var rutaResult = await _unitOfWork.ProcesoArchivos.GetArchivoRutaFisicaAsync(
                    archivoId
                );

                if (!rutaResult.IsSuccess)
                {
                    return NotFound(Result<bool>.Failure(rutaResult.Error));
                }

                // Determinar el tipo MIME
                var contentType = GetContentType(archivo.Tipo);

                // Devolver el archivo físico
                return PhysicalFile(rutaResult.Value, contentType, archivo.Nombre);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al descargar archivo {ArchivoId} del proceso",
                    archivoId
                );
                return StatusCode(500, Result<bool>.Failure("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Determina el tipo MIME basado en la extensión del archivo
        /// </summary>
        private string GetContentType(string fileExtension)
        {
            return fileExtension.ToLower() switch
            {
                "pdf" => "application/pdf",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "doc" => "application/msword",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "xls" => "application/vnd.ms-excel",
                "pptx" =>
                    "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "ppt" => "application/vnd.ms-powerpoint",
                "txt" => "text/plain",
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "gif" => "image/gif",
                "zip" => "application/zip",
                "rar" => "application/x-rar-compressed",
                "7z" => "application/x-7z-compressed",
                _ => "application/octet-stream", // Tipo por defecto para archivos binarios
            };
        }
    }
}
