﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.DataAccess.Extensions;
using Sitca.DataAccess.Middlewares;
using Sitca.DataAccess.Services.Files;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.Enums;
using Sitca.Models.ViewModels;
using Roles = Utilities.Common.Constants.Roles;

namespace Sitca.DataAccess.Data.Repository
{
    public class ArchivoRepository : Repository<Archivo>, IArchivoRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ArchivoRepository> _logger;
        private readonly IFileService _fileService;

        private readonly string PROTOCOLO = "Protocolo Adhesión";

        public ArchivoRepository(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            ILogger<ArchivoRepository> logger,
            IFileService fileService
        )
            : base(db)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<bool> DeleteFile(int data, ApplicationUser user, string role)
        {
            var archivo = await _db.Archivo.FirstOrDefaultAsync(s => s.Id == data);

            if (role == Roles.Asesor || role == Roles.Auditor)
            {
                if (archivo.UsuarioCargaId != user.Id)
                {
                    return false;
                }
            }

            archivo.Activo = false;
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<List<Archivo>> GetList(
            ArchivoFilterVm filter,
            ApplicationUser user,
            string role
        )
        {
            // Validación de parámetros
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "El filtro no puede ser nulo");
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "El usuario no puede ser nulo");
            }

            try
            {
                // Usar un switch para mejorar la legibilidad
                switch (filter.type?.ToLower())
                {
                    case "pregunta" when filter.idPregunta > 0:
                        return await _db
                            .Archivo.AsNoTracking()
                            .Where(a => a.CuestionarioItemId == filter.idPregunta && a.Activo)
                            .ToListAsync();

                    case "cuestionario" when filter.idCuestionario > 0:
                        return await _db
                            .Archivo.AsNoTracking()
                            .Where(a =>
                                a.CuestionarioItem.CuestionarioId == filter.idCuestionario
                                && a.Activo
                            )
                            .Include(a => a.CuestionarioItem) // Incluir para optimizar la carga relacionada
                            .ToListAsync();

                    default:
                        _logger.LogWarning(
                            "Filtro de archivo no válido: {Type}, IdPregunta: {IdPregunta}, IdCuestionario: {IdCuestionario}",
                            filter.type,
                            filter.idPregunta,
                            filter.idCuestionario
                        );
                        return new List<Archivo>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al obtener archivos con filtro: {FilterType}, IdPregunta: {IdPregunta}, IdCuestionario: {IdCuestionario}",
                    filter.type,
                    filter.idPregunta,
                    filter.idCuestionario
                );
                throw new DatabaseException(
                    $"Error al recuperar los archivos con el filtro especificado",
                    ex
                );
            }
        }

        public List<EnumValueDto> GetTypeFilesCompany()
        {
            return FileCompanyExtensions.GetFileTypes();
        }

        public async Task<bool> SaveFileData(Archivo data)
        {
            var archivo = new Archivo
            {
                Activo = true,
                EmpresaId = data.EmpresaId,
                FechaCarga = DateTime.UtcNow,
                Ruta = data.Ruta,
                Nombre = data.Nombre,
                Tipo = data.Tipo,
                CuestionarioItemId = data.CuestionarioItemId,
                UsuarioCargaId = data.UsuarioCargaId,
                FileTypesCompany = data.FileTypesCompany,
            };
            if (!string.IsNullOrEmpty(data.UsuarioId))
            {
                archivo.UsuarioId = data.UsuarioId;
            }

            await _db.Archivo.AddAsync(archivo);
            await _db.SaveChangesAsync();

            if (archivo.Nombre == PROTOCOLO)
            {
                var empresa = _db.Empresa.Find(archivo.EmpresaId);
                if (empresa.Estado < 1)
                {
                    empresa.Estado = 1;
                }

                await _db.SaveChangesAsync();
            }

            return true;
        }

        public async Task<Result<Models.DTOs.UploadRequest>> ValidateAndCreateUploadRequest(
            IFormCollection form,
            ApplicationUser user
        )
        {
            try
            {
                var file = form.Files.First();
                if (file.Length == 0)
                    return Result<UploadRequest>.Failure("El archivo está vacío");

                var roles = await _userManager.GetRolesAsync(user);

                return Result<UploadRequest>.Success(
                    new UploadRequest
                    {
                        File = file,
                        FileName = form["archivo"].ToString(),
                        IsCompanyFile = form["empresa"]
                            .ToString()
                            .Equals("true", StringComparison.OrdinalIgnoreCase),
                        FileType = GetFileType(form["typeFile"].ToString()),
                        DocumentType = form["type"].ToString(),
                        EmpresaId = GetEmpresaId(form["empresaId"].ToString(), user, roles),
                        User = user,
                        Roles = roles,
                        idPregunta = form["idPregunta"].ToString(),
                        idRespuesta = form["idRespuesta"].ToString(),
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando la solicitud de carga");
                return Result<UploadRequest>.Failure("Error validando la solicitud");
            }
        }

        public async Task<Result<FileUploadResponse>> ProcessFileUpload(UploadRequest request)
        {
            try
            {
                // Determinar la subcarpeta según el tipo de documento
                string subfolder = DetermineSubfolder(request);

                // Usar FileService para guardar y optimizar el archivo
                (string relativePath, long fileSize) = await _fileService.SaveFileAsync(
                    request.File,
                    subfolder
                );

                var archivo = await SaveFileRecord(request, relativePath);
                if (archivo == null)
                    return Result<FileUploadResponse>.Failure(
                        "Error al guardar el registro del archivo"
                    );

                return Result<FileUploadResponse>.Success(
                    new FileUploadResponse { DbPath = relativePath, FileId = archivo.Id }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando el archivo");
                return Result<FileUploadResponse>.Failure("Error al procesar el archivo");
            }
        }

        private string DetermineSubfolder(UploadRequest request)
        {
            if (request.IsCompanyFile)
            {
                return $"empresa/{request.EmpresaId}";
            }
            else if (request.DocumentType == "pregunta")
            {
                return $"cuestionario/preguntas/{request.idPregunta}";
            }
            else
            {
                return "general";
            }
        }

        public async Task ProcessAndSaveImage(IFormFile file, string filePath)
        {
            // En producción usar System.Drawing por su mejor rendimiento
            try
            {
                using var image = Image.FromStream(file.OpenReadStream());
                using var imageStream = new MemoryStream();

                var newSize = image.Width;
                int newHeight = image.Height;

                if (image.Width > 1200 && image.Height > 1000)
                {
                    newSize = 700;
                    decimal width = image.Width;
                    decimal proporcional = width / newSize;
                    decimal height = image.Height;
                    newHeight = (int)(height / proporcional);
                }

                using var bitmap = new Bitmap(image, new Size(newSize, newHeight));
                bitmap.Save(imageStream, ImageFormat.Jpeg);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                imageStream.Position = 0;

                var optimizer = new ImageOptimizer();
                var result = optimizer.LosslessCompress(imageStream);
                imageStream.WriteTo(fileStream);
                await imageStream.CopyToAsync(fileStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando imagen con System.Drawing");
                // Fallback a guardado directo si hay error
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
            }
        }

        private async Task<Archivo> SaveFileRecord(UploadRequest request, string relativePath)
        {
            var strategy = _db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                try
                {
                    // Iniciamos una transacción para asegurar la integridad
                    using var transaction = await _db.Database.BeginTransactionAsync();

                    try
                    {
                        var archivo = new Archivo
                        {
                            Activo = true,
                            FechaCarga = DateTime.UtcNow,
                            Ruta = relativePath,
                            Tipo = Path.GetExtension(request.File.FileName),
                            UsuarioCargaId = request.User.Id,
                            Nombre = request.FileName,
                            FileTypesCompany = request.FileType,
                        };

                        if (request.IsCompanyFile)
                        {
                            // Verificar que la empresa existe
                            var empresa = await _db
                                .Empresa.AsNoTracking()
                                .FirstOrDefaultAsync(e => e.Id == request.EmpresaId);
                            if (empresa == null)
                            {
                                throw new InvalidOperationException(
                                    $"La empresa con ID {request.EmpresaId} no existe"
                                );
                            }
                            archivo.EmpresaId = request.EmpresaId;
                        }
                        else if (request.DocumentType == "pregunta")
                        {
                            var preguntaId = int.Parse(request.idPregunta);
                            var cuestionarioItem = await _db
                                .CuestionarioItem.AsNoTracking()
                                .FirstOrDefaultAsync(c => c.Id == preguntaId);
                            if (cuestionarioItem == null)
                            {
                                throw new InvalidOperationException(
                                    $"El CuestionarioItem con ID {preguntaId} no existe"
                                );
                            }
                            archivo.CuestionarioItemId = preguntaId;
                        }

                        await _db.Archivo.AddAsync(archivo);

                        if (
                            request.FileType == FileCompany.Adhesion
                            || request.FileName == PROTOCOLO
                        )
                        {
                            var empresaUpd = await _db.Empresa.FindAsync(request.EmpresaId);

                            if (empresaUpd != null && empresaUpd.Estado < 1)
                            {
                                empresaUpd.Estado = 1;
                                _db.Empresa.Update(empresaUpd);
                            }
                        }
                        await _db.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return archivo;
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
                catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
                {
                    _logger.LogError(
                        ex,
                        "Error de base de datos al guardar archivo. SQL Error: {SqlError}",
                        sqlEx.Message
                    );

                    var errorMessage = sqlEx.Number switch
                    {
                        547 => "Error de referencia: El registro relacionado no existe",
                        2601 => "Ya existe un registro con estos datos",
                        _ => "Error al guardar en la base de datos",
                    };

                    throw new InvalidOperationException(errorMessage, ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar archivo");
                    throw;
                }
            });
        }

        private static async Task SaveFile(IFormFile file, string path)
        {
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        private static bool IsImageFile(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(extension);
        }

        // Método de ayuda para obtener el tipo de archivo
        private static FileCompany GetFileType(string typeFile)
        {
            if (string.IsNullOrEmpty(typeFile) || !int.TryParse(typeFile, out int parsedValue))
                return FileCompany.Informativo;

            return FileCompanyExtensions.GetFileType(parsedValue);
        }

        // Método de ayuda para obtener el ID de empresa
        private static int GetEmpresaId(string idEmp, ApplicationUser user, IList<string> roles)
        {
            if (roles.Contains(Roles.Empresa))
                return user.EmpresaId ?? 0;

            if (!string.IsNullOrEmpty(idEmp) && int.TryParse(idEmp, out int empresaId))
                return empresaId;

            return 0;
        }
    }
}
