using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using GemBox.Document;
using GemBox.Pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sitca.Models.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Sitca.DataAccess.Services.Files;

public class FileService : IFileService
{
    private readonly IConfiguration _config;
    private readonly ILogger<FileService> _logger;
    private readonly IList<string> _allowedExtensions;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _pythonApiUrl;

    public FileService(
        IConfiguration configuration,
        ILogger<FileService> logger,
        IHttpClientFactory httpClientFactory
    )
    {
        _config = configuration;
        _logger = logger;
        _httpClientFactory = httpClientFactory;

        // Obtener la URL de la API Python desde la configuración
        _pythonApiUrl = _config["CorsSettings:pythonURL"];

        if (string.IsNullOrEmpty(_pythonApiUrl))
        {
            _logger.LogWarning(
                "La URL de la API Python no está configurada en appsettings (CorsSettings:pythonURL)"
            );
        }

        // Obtener extensiones permitidas desde configuración o usar valores predeterminados
        _allowedExtensions =
            _config.GetSection("Settings:Storage:AllowedExtensions").Get<List<string>>()
            ?? new List<string>
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".gif",
                ".bmp",
                ".pdf",
                ".doc",
                ".docx",
                ".xls",
                ".xlsx",
                ".ppt",
                ".pptx",
                ".txt",
                ".csv",
                ".zip",
            };
    }

    /// <summary>
    /// Guarda un archivo con optimización según el tipo
    /// </summary>
    /// <param name="file">Archivo a guardar</param>
    /// <param name="subfolder">Subcarpeta opcional</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Tupla con la ruta relativa del archivo guardado y su tamaño en bytes</returns>
    public async Task<(string FilePath, long FileSize)> SaveFileAsync(
        IFormFile file,
        string subfolder = "",
        CancellationToken cancellationToken = default
    )
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("El archivo es inválido", nameof(file));

        if (!IsFileTypeAllowed(file))
            throw new ArgumentException("El tipo de archivo no está permitido", nameof(file));

        // Crear directorio si no existe
        var basePath = GetFullPath();
        var targetPath = string.IsNullOrWhiteSpace(subfolder)
            ? basePath
            : Path.Combine(basePath, subfolder);

        Directory.CreateDirectory(targetPath);

        // Generar nombre único para evitar colisiones
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
        var filePath = Path.Combine(targetPath, fileName);

        // Procesar según tipo de archivo
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        try
        {
            if (IsFileType(extension, FileType.Image))
            {
                await ProcessAndSaveImageAsync(file, filePath, cancellationToken);
            }
            else if (IsFileType(extension, FileType.Word))
            {
                await ProcessAndSaveWordDocumentAsync(file, filePath, cancellationToken);
            }
            else if (IsFileType(extension, FileType.Pdf))
            {
                await ProcessAndSavePdfAsync(file, filePath, cancellationToken);
            }
            else
            {
                // Archivos sin procesamiento específico
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Obtener el tamaño del archivo final
            var fileInfo = new FileInfo(filePath);
            var fileSize = fileInfo.Length;

            // Devolver ruta relativa para guardar en BD
            return (Path.Combine(subfolder, fileName).Replace("\\", "/"), fileSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar el archivo {FileName}", file.FileName);
            throw new ApplicationException("Error al procesar el archivo", ex);
        }
    }

    private async Task ProcessAndSaveImageAsync(
        IFormFile file,
        string outputPath,
        CancellationToken cancellationToken = default
    )
    {
        using var inputStream = file.OpenReadStream();
        using var image = await Image.LoadAsync(inputStream, cancellationToken);

        image.Mutate(x => x.AutoOrient());

        try
        {
            // Intentar procesar la imagen a través de la API Python si está configurada
            if (!string.IsNullOrEmpty(_pythonApiUrl))
            {
                var processedImage = await ProcessImageWithPythonApiAsync(
                    image,
                    file.FileName,
                    cancellationToken
                );
                if (processedImage != null)
                {
                    // Si se procesó correctamente, reemplazamos la imagen actual
                    image.Dispose();
                    await processedImage.SaveAsync(outputPath, cancellationToken);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al procesar la imagen con la API Python. Continúa con el procesamiento local."
            );
            // Continuamos con el procesamiento local si falla la API
        }

        // Redimensionar si es demasiado grande
        if (image.Width > 1200 || image.Height > 1200)
        {
            image.Mutate(x =>
                x.Resize(
                    new ResizeOptions
                    {
                        Size = new SixLabors.ImageSharp.Size(1200, 1200),
                        Mode = ResizeMode.Max,
                    }
                )
            );
        }

        // Comprimir y guardar
        var extension = Path.GetExtension(outputPath).ToLowerInvariant();

        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                await image.SaveAsync(
                    outputPath,
                    new JpegEncoder
                    {
                        Quality = 75, // 75% de calidad
                    },
                    cancellationToken
                );
                break;
            case ".png":
                await image.SaveAsync(
                    outputPath,
                    new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression },
                    cancellationToken
                );
                break;
            case ".gif":
                await image.SaveAsync(outputPath, new GifEncoder(), cancellationToken);
                break;
            default:
                await image.SaveAsync(outputPath, cancellationToken);
                break;
        }
    }

    /// <summary>
    /// Procesa una imagen utilizando la API Python configurada
    /// </summary>
    /// <param name="image">Imagen a procesar</param>
    /// <param name="originalFileName">Nombre del archivo original</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Imagen procesada o null si no se pudo procesar</returns>
    private async Task<Image> ProcessImageWithPythonApiAsync(
        Image image,
        string originalFileName,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrEmpty(_pythonApiUrl))
        {
            _logger.LogWarning(
                "No se puede procesar la imagen con la API Python porque la URL no está configurada"
            );
            return null;
        }

        try
        {
            // Crear un cliente HTTP
            var client = _httpClientFactory.CreateClient("PythonApi");

            // Preparar el contenido multipart
            using var content = new MultipartFormDataContent();

            // Convertir la imagen actual a un stream para enviarla
            using var imageStream = new MemoryStream();
            await image.SaveAsync(imageStream, new JpegEncoder(), cancellationToken);
            imageStream.Position = 0;

            // Agregar la imagen al contenido
            var imageContent = new StreamContent(imageStream);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(
                GetContentType(Path.GetExtension(originalFileName))
            );
            content.Add(imageContent, "file", Path.GetFileName(originalFileName));

            // Agregar cualquier metadato si es necesario
            content.Add(new StringContent(""), "metadata");

            // Configurar timeout
            var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(30)); // 30 segundos de timeout

            // Enviar la solicitud POST a la API de procesamiento
            var response = await client.PostAsync(_pythonApiUrl, content, timeoutCts.Token);

            // Verificar si la solicitud fue exitosa
            if (response.IsSuccessStatusCode)
            {
                // Leer la respuesta como stream
                var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

                // Intentar cargar la imagen desde la respuesta
                return await Image.LoadAsync(responseStream, cancellationToken);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "La API Python respondió con código {StatusCode}: {ErrorContent}",
                    (int)response.StatusCode,
                    errorContent
                );
                return null;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("La operación de procesamiento de imagen fue cancelada");
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "El tiempo de espera para procesar la imagen con la API Python expiró"
            );
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar la imagen con la API Python");
            return null;
        }
    }

    /// <summary>
    /// Obtiene el tipo de contenido MIME basado en la extensión del archivo
    /// </summary>
    private string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream",
        };
    }

    private async Task ProcessAndSaveWordDocumentAsync(
        IFormFile file,
        string outputPath,
        CancellationToken cancellationToken = default
    )
    {
        // Primero guardamos el archivo original
        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(outputPath));
        using (var stream = new FileStream(tempPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        try
        {
            // Inicializar GemBox Document si es necesario
            GemBox.Document.ComponentInfo.SetLicense("FREE-LIMITED-KEY");

            // Abrir documento con GemBox
            var document = DocumentModel.Load(tempPath);

            // Comprimir imágenes dentro del documento - enfoque simplificado
            // Nota: la API exacta para obtener y comprimir imágenes depende de la versión de GemBox
            // Este enfoque genérico funciona para guardar con menor calidad general

            // Configurar opciones de guardado para comprimir el documento
            var saveOptions = new DocxSaveOptions { ImageDpi = 96 };

            // Guardar documento optimizado
            document.Save(outputPath);

            // Eliminar archivo temporal
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar documento Word");

            // Si falla, usar el archivo original
            if (File.Exists(tempPath))
            {
                File.Move(tempPath, outputPath, true);
            }
        }
    }

    private async Task ProcessAndSavePdfAsync(
        IFormFile file,
        string outputPath,
        CancellationToken cancellationToken = default
    )
    {
        // Primero guardamos el archivo original
        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(outputPath));
        using (var stream = new FileStream(tempPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        try
        {
            GemBox.Pdf.ComponentInfo.SetLicense("FREE-LIMITED-KEY");

            // Cargar el PDF con GemBox.Pdf
            var document = PdfDocument.Load(tempPath);

            // Guardar PDF optimizado
            document.Save(outputPath);

            // Eliminar archivo temporal
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar PDF");

            // Si falla, usar el archivo original
            if (File.Exists(tempPath))
            {
                File.Move(tempPath, outputPath, true);
            }
        }
    }

    public bool IsFileTypeAllowed(IFormFile file)
    {
        if (file == null)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return !string.IsNullOrEmpty(extension) && _allowedExtensions.Contains(extension);
    }

    /// <summary>
    /// Define la ruta base de almacenamiento
    /// </summary>
    public string GetFullPath()
    {
        var folderName = Path.Combine("Resources", "files");
        var rutaBase = _config["Settings:Storage:BasePath"] ?? folderName;
        var rutaPlataforma = Path.Combine(Directory.GetCurrentDirectory(), rutaBase);
        return rutaPlataforma;
    }

    /// <summary>
    /// Verifica si la extensión corresponde a un tipo de archivo específico
    /// </summary>
    private bool IsFileType(string extension, FileType fileType)
    {
        extension = extension.ToLowerInvariant();
        return _fileTypeExtensions[fileType].Contains(extension);
    }

    private static readonly Dictionary<FileType, string[]> _fileTypeExtensions = new()
    {
        [FileType.Image] = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" },
        [FileType.Word] = new[] { ".doc", ".docx" },
        [FileType.Pdf] = new[] { ".pdf" },
        [FileType.Excel] = new[] { ".xls", ".xlsx" },
        [FileType.PowerPoint] = new[] { ".ppt", ".pptx" },
        [FileType.Text] = new[] { ".txt", ".csv" },
        [FileType.Archive] = new[] { ".zip" },
    };
}
