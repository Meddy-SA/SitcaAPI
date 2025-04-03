using Sitca.Models.DTOs;

namespace Sitca.Models.Mappers;

/// <summary>
/// Clase utilitaria para mapear entre entidades y DTOs relacionados con Procesos de Certificación
/// </summary>
public static class ProcesoCertificacionMapper
{
    /// <summary>
    /// Mapea un ProcesoCertificacion a un ProcesoCertificacionDTO
    /// </summary>
    public static ProcesoCertificacionDTO? ToDto(
        this ProcesoCertificacion proceso,
        string userId = ""
    )
    {
        if (proceso == null)
            return null;

        // Paso 1: Obtener el statusId desde el string Status
        byte statusId = 0;
        if (!string.IsNullOrWhiteSpace(proceso.Status))
        {
            _ = byte.TryParse(proceso.Status.Split('-')[0].Trim(), out statusId);
        }

        // Paso 2: Crear el DTO con los datos básicos
        var procesoDto = new ProcesoCertificacionDTO
        {
            Id = proceso.Id,
            NumeroExpediente = proceso.NumeroExpediente,
            Status = proceso.Status,
            StatusId = statusId,
            FechaInicio = proceso.FechaInicio,
            FechaFinalizacion = proceso.FechaFinalizacion,
            FechaSolicitudAuditoria = proceso.FechaSolicitudAuditoria,
            FechaFijadaAuditoria = proceso.FechaFijadaAuditoria,
            FechaVencimiento = proceso.FechaVencimiento,
            Recertificacion = proceso.Recertificacion,
            Cantidad = proceso.Cantidad,
        };

        // Paso 3: Agregar datos de Tipología
        if (proceso.Tipologia != null)
        {
            // Si proceso.Tipologia existe, usamos esos datos
            procesoDto.Tipologia = new TipologiaDTO
            {
                Id = proceso.Tipologia.Id,
                Nombre = proceso.Tipologia.Name,
                NombreIngles = proceso.Tipologia.NameEnglish,
            };
        }
        else
        {
            // Si proceso.Tipologia es null, intentamos obtener la tipología desde la empresa
            var primeraTipologia = proceso.Empresa.Tipologias?.FirstOrDefault()?.Tipologia;

            procesoDto.Tipologia =
                primeraTipologia == null
                    ? null // Si no hay tipologías en la empresa, asignamos null
                    : new TipologiaDTO
                    {
                        Id = primeraTipologia.Id,
                        Nombre = primeraTipologia.Name,
                        NombreIngles = primeraTipologia.NameEnglish,
                    };
        }

        // Paso 4: Crear y agregar datos de Empresa
        var empresaDto = new EmpresaBasicaDTO
        {
            Id = proceso.Empresa.Id,
            Nombre = proceso.Empresa.Nombre,
            NombreRepresentante = proceso.Empresa.NombreRepresentante,
            CargoRepresentante = proceso.Empresa.CargoRepresentante,
            IdNacional = proceso.Empresa.IdNacional,
            Email = proceso.Empresa.Email,
            Telefono = proceso.Empresa.Telefono,
            Direccion = proceso.Empresa.Direccion,
            WebSite = proceso.Empresa.WebSite,
            Ciudad = proceso.Empresa.Ciudad,
            ResultadoActual = proceso.Empresa.ResultadoActual,
            ResultadoVencimiento = proceso.Empresa.ResultadoVencimiento,
            Estado = proceso.Empresa.Estado,
            EsHomologacion = proceso.Empresa.EsHomologacion,
            Activo = proceso.Empresa.Active,
        };

        // Paso 5: Crear y agregar datos de País a la Empresa
        empresaDto.Pais = new PaisDTO
        {
            Id = proceso.Empresa.Pais.Id,
            Nombre = proceso.Empresa.Pais.Name,
        };

        // Asignar la empresa al DTO
        procesoDto.Empresa = empresaDto;

        // Paso 6: Crear y agregar datos del Asesor
        procesoDto.Asesor =
            proceso.AsesorProceso == null
                ? null
                : new UsuarioBasicoDTO
                {
                    Id = proceso.AsesorProceso.Id,
                    NombreCompleto =
                        $"{proceso.AsesorProceso.FirstName} {proceso.AsesorProceso.LastName}".Trim(),
                    Email = proceso.AsesorProceso.Email!,
                    Codigo = proceso.AsesorProceso.Codigo,
                };

        // Paso 7: Crear y agregar datos del Auditor
        procesoDto.Auditor =
            proceso.AuditorProceso == null
                ? null
                : new UsuarioBasicoDTO
                {
                    Id = proceso.AuditorProceso.Id,
                    NombreCompleto =
                        $"{proceso.AuditorProceso.FirstName} {proceso.AuditorProceso.LastName}".Trim(),
                    Email = proceso.AuditorProceso.Email!,
                    Codigo = proceso.AuditorProceso.Codigo,
                };

        // Paso 8: Crear y agregar datos del Usuario Generador
        procesoDto.UserGenerador = new UsuarioBasicoDTO
        {
            Id = proceso.UserGenerador.Id,
            NombreCompleto =
                $"{proceso.UserGenerador.FirstName} {proceso.UserGenerador.LastName}".Trim(),
            Email = proceso.UserGenerador.Email!,
            Codigo = proceso.UserGenerador.Codigo,
        };

        // Paso 9: Crear y agregar lista de Archivos
        var archivos =
            proceso
                .ProcesosArchivos?.Select(a => new ProcesoArchivoDTO
                {
                    Id = a.Id,
                    Nombre = a.Nombre,
                    Ruta = a.Ruta,
                    Tipo = a.Tipo,
                    TipoArchivo = a.FileTypesCompany,
                    NombreTipoArchivo = a.FileTypesCompany.ToString(),
                    FechaCreacion = a.CreatedAt ?? DateTime.UtcNow,
                    CreadoPor = a.CreatedBy,
                    NombreCreador =
                        a.UserCreate == null
                            ? null
                            : $"{a.UserCreate.FirstName} {a.UserCreate.LastName}".Trim(),
                    EsPropio = a.CreatedBy == userId,
                    FileSize = a.FileSize,
                })
                .ToList() ?? new List<ProcesoArchivoDTO>();

        procesoDto.Archivos = archivos;

        // Paso 10: Crear y agregar lista de Cuestionarios
        var cuestionarios =
            proceso
                .Cuestionarios?.Select(a => new CuestionarioBasicoDTO
                {
                    Id = a.Id,
                    Prueba = a.Prueba,
                    FechaRevision = a.FechaRevisionAuditor,
                    FechaInicio = a.FechaInicio,
                    FechaFinalizacion = a.FechaFinalizado,
                    FechaEvaluacion = proceso.FechaFijadaAuditoria.HasValue
                        ? proceso.FechaFijadaAuditoria.Value
                        : a.FechaVisita,
                })
                .ToList() ?? new List<CuestionarioBasicoDTO>();

        procesoDto.Cuestionarios = cuestionarios;

        // Paso 11: Agregar datos de auditoría
        procesoDto.CreadoPor = proceso.CreatedBy;
        procesoDto.FechaCreacion = proceso.CreatedAt;
        procesoDto.ActualizadoPor = proceso.UpdatedBy;
        procesoDto.FechaActualizacion = proceso.UpdatedAt;

        // Retornar el DTO construido
        return procesoDto;
    }

    /// <summary>
    /// Mapea una lista de ProcesoCertificacion a una lista de ProcesoCertificacionListaDTO
    /// </summary>
    public static List<ProcesoCertificacionListaDTO> ToListDto(
        this IEnumerable<ProcesoCertificacion> procesos
    )
    {
        if (procesos == null)
            return new List<ProcesoCertificacionListaDTO>();

        return procesos
            .Select(p => new ProcesoCertificacionListaDTO
            {
                Id = p.Id,
                NumeroExpediente = p.NumeroExpediente,
                Status = p.Status,
                FechaInicio = p.FechaInicio,
                FechaVencimiento = p.FechaVencimiento,
                Recertificacion = p.Recertificacion,

                NombreEmpresa = p.Empresa?.Nombre ?? string.Empty,
                EmpresaId = p.EmpresaId,

                CantidadArchivos = p.ProcesosArchivos?.Count ?? 0,

                NombreAsesor =
                    p.AsesorProceso == null
                        ? null
                        : $"{p.AsesorProceso.FirstName} {p.AsesorProceso.LastName}".Trim(),

                NombreAuditor =
                    p.AuditorProceso == null
                        ? null
                        : $"{p.AuditorProceso.FirstName} {p.AuditorProceso.LastName}".Trim(),

                NombrePais = p.Empresa?.Pais?.Name ?? string.Empty,
                PaisId = p.Empresa?.PaisId ?? 0,

                NombreTipologia = p.Tipologia?.Name ?? null,

                UltimaActualizacion = p.UpdatedAt ?? p.CreatedAt,
            })
            .ToList();
    }

    /// <summary>
    /// Crea un nuevo ProcesoCertificacion a partir de CrearProcesoCertificacionDTO
    /// </summary>
    public static ProcesoCertificacion CrearDesdeDto(
        this CrearProcesoCertificacionDTO dto,
        string userId
    )
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        return new ProcesoCertificacion
        {
            EmpresaId = dto.EmpresaId,
            AsesorId = dto.AsesorId,
            TipologiaId = dto.TipologiaId,
            NumeroExpediente = dto.NumeroExpediente ?? string.Empty,
            Recertificacion = dto.Recertificacion,
            Status = "0 - Inicial", // Estado inicial por defecto
            FechaInicio = DateTime.UtcNow,
            Cantidad = dto.Cantidad ?? 0,
            UserGeneraId = userId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            Enabled = true,
        };
    }

    /// <summary>
    /// Actualiza un ProcesoCertificacion con datos de un ActualizarProcesoCertificacionDTO
    /// </summary>
    public static void ActualizarDesdeDto(
        this ProcesoCertificacion proceso,
        ActualizarProcesoCertificacionDTO dto,
        string userId
    )
    {
        if (proceso == null || dto == null)
            return;

        // Solo actualizamos propiedades que no son null en el DTO
        if (!string.IsNullOrWhiteSpace(dto.AsesorId))
            proceso.AsesorId = dto.AsesorId;

        if (!string.IsNullOrWhiteSpace(dto.AuditorId))
            proceso.AuditorId = dto.AuditorId;

        if (dto.TipologiaId.HasValue)
            proceso.TipologiaId = dto.TipologiaId;

        if (!string.IsNullOrWhiteSpace(dto.NumeroExpediente))
            proceso.NumeroExpediente = dto.NumeroExpediente;

        if (!string.IsNullOrWhiteSpace(dto.Status))
            proceso.Status = dto.Status;

        if (dto.FechaSolicitudAuditoria.HasValue)
            proceso.FechaSolicitudAuditoria = dto.FechaSolicitudAuditoria;

        if (dto.FechaFijadaAuditoria.HasValue)
            proceso.FechaFijadaAuditoria = dto.FechaFijadaAuditoria;

        if (dto.FechaFinalizacion.HasValue)
            proceso.FechaFinalizacion = dto.FechaFinalizacion;

        if (dto.FechaVencimiento.HasValue)
            proceso.FechaVencimiento = dto.FechaVencimiento;

        if (dto.Cantidad.HasValue)
            proceso.Cantidad = dto.Cantidad;

        // Actualizamos timestamps de auditoría
        proceso.UpdatedAt = DateTime.UtcNow;
        proceso.UpdatedBy = userId;
    }

    /// <summary>
    /// Mapea un ProcesoArchivos a un ProcesoArchivoDTO
    /// </summary>
    public static ProcesoArchivoDTO? ToDto(this ProcesoArchivos archivo, string userId)
    {
        if (archivo == null)
            return null;

        return new ProcesoArchivoDTO
        {
            Id = archivo.Id,
            Nombre = archivo.Nombre,
            Ruta = archivo.Ruta,
            Tipo = archivo.Tipo,
            TipoArchivo = archivo.FileTypesCompany ?? Enums.FileCompany.Informativo,
            NombreTipoArchivo = archivo.FileTypesCompany?.ToString() ?? "Informativo",
            FechaCreacion = archivo.CreatedAt ?? DateTime.UtcNow,
            CreadoPor = archivo.CreatedBy,
            NombreCreador =
                archivo.UserCreate == null
                    ? null
                    : $"{archivo.UserCreate.FirstName} {archivo.UserCreate.LastName}".Trim(),
            EsPropio = archivo.CreatedBy == userId,
            FileSize = archivo.FileSize,
        };
    }

    /// <summary>
    /// Mapea un Archivo a un ProcesoArchivoDTO
    /// </summary>
    public static ProcesoArchivoDTO? ToDto(this Archivo archivo)
    {
        if (archivo == null)
            return null;

        return new ProcesoArchivoDTO
        {
            Id = archivo.Id,
            Nombre = archivo.Nombre,
            Ruta = archivo.Ruta,
            Tipo = archivo.Tipo,
            TipoArchivo = archivo.FileTypesCompany ?? Enums.FileCompany.Informativo,
            NombreTipoArchivo = archivo.FileTypesCompany?.ToString() ?? "Informativo",
            FechaCreacion = archivo.FechaCarga,
            CreadoPor = archivo.UsuarioCargaId,
            NombreCreador =
                archivo.UsuarioCarga == null
                    ? null
                    : $"{archivo.UsuarioCarga.FirstName} {archivo.UsuarioCarga.LastName}".Trim(),
            EsPropio = false,
            EsEmpresa = true,
        };
    }

    /// <summary>
    /// Crea un ProcesoArchivos a partir de SubirArchivoBase64DTO
    /// </summary>
    public static ProcesoArchivos CrearDesdeDto(
        this SubirArchivoProcesoDTO dto,
        string userId,
        string rutaArchivo
    )
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        return new ProcesoArchivos
        {
            ProcesoCertificacionId = dto.ProcesoCertificacionId,
            Nombre = dto.Nombre ?? "",
            Ruta = rutaArchivo,
            Tipo = dto.Tipo ?? "",
            FileTypesCompany = dto.TipoArchivo,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            Enabled = true,
        };
    }

    /// <summary>
    /// Crea un ProcesoArchivos a partir de un Archivo
    /// </summary>
    public static ProcesoArchivos ConvertirDesdeArchivo(
        this Archivo archivo,
        int procesoCertificacionId,
        string userId
    )
    {
        if (archivo == null)
            throw new ArgumentNullException(nameof(archivo));

        return new ProcesoArchivos
        {
            ProcesoCertificacionId = procesoCertificacionId,
            Nombre = archivo.Nombre,
            Ruta = archivo.Ruta,
            Tipo = archivo.Tipo,
            FileTypesCompany = archivo.FileTypesCompany ?? Enums.FileCompany.Informativo,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            // Mantener información original cuando sea posible
            UpdatedAt = null,
            UpdatedBy = null,
            Enabled = true,
        };
    }

    /// <summary>
    /// Crea un ProcesoArchivos a partir de un Archivo preservando metadatos originales
    /// </summary>
    public static ProcesoArchivos ConvertirDesdeArchivoPreservandoMetadata(
        this Archivo archivo,
        int procesoCertificacionId,
        string userId
    )
    {
        if (archivo == null)
            throw new ArgumentNullException(nameof(archivo));

        return new ProcesoArchivos
        {
            ProcesoCertificacionId = procesoCertificacionId,
            Nombre = archivo.Nombre,
            Ruta = archivo.Ruta,
            Tipo = archivo.Tipo,
            FileTypesCompany = archivo.FileTypesCompany ?? Enums.FileCompany.Informativo,
            CreatedAt = archivo.FechaCarga,
            CreatedBy = archivo.UsuarioCargaId ?? userId,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = userId,
            Enabled = true,
        };
    }
}
