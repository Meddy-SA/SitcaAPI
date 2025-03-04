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
    public static ProcesoCertificacionDTO? ToDto(this ProcesoCertificacion proceso)
    {
        if (proceso == null)
            return null;

        byte statusId = 0;
        if (!string.IsNullOrWhiteSpace(proceso.Status))
        {
            _ = byte.TryParse(proceso.Status.Split('-')[0].Trim(), out statusId);
        }

        return new ProcesoCertificacionDTO
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

            Tipologia =
                proceso.Tipologia == null
                    ? null
                    : new TipologiaDTO
                    {
                        Id = proceso.Tipologia.Id,
                        Nombre = proceso.Tipologia.Name,
                        NombreIngles = proceso.Tipologia.NameEnglish,
                    },

            Empresa = new EmpresaBasicaDTO
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
                Pais = new PaisDTO
                {
                    Id = proceso.Empresa.Pais.Id,
                    Nombre = proceso.Empresa.Pais.Name,
                },
            },

            Asesor =
                proceso.AsesorProceso == null
                    ? null
                    : new UsuarioBasicoDTO
                    {
                        Id = proceso.AsesorProceso.Id,
                        NombreCompleto =
                            $"{proceso.AsesorProceso.FirstName} {proceso.AsesorProceso.LastName}".Trim(),
                        Email = proceso.AsesorProceso.Email!,
                        Codigo = proceso.AsesorProceso.Codigo,
                    },

            Auditor =
                proceso.AuditorProceso == null
                    ? null
                    : new UsuarioBasicoDTO
                    {
                        Id = proceso.AuditorProceso.Id,
                        NombreCompleto =
                            $"{proceso.AuditorProceso.FirstName} {proceso.AuditorProceso.LastName}".Trim(),
                        Email = proceso.AuditorProceso.Email!,
                        Codigo = proceso.AuditorProceso.Codigo,
                    },

            UserGenerador = new UsuarioBasicoDTO
            {
                Id = proceso.UserGenerador.Id,
                NombreCompleto =
                    $"{proceso.UserGenerador.FirstName} {proceso.UserGenerador.LastName}".Trim(),
                Email = proceso.UserGenerador.Email!,
                Codigo = proceso.UserGenerador.Codigo,
            },

            Archivos =
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
                    })
                    .ToList() ?? new List<ProcesoArchivoDTO>(),

            CreadoPor = proceso.CreatedBy,
            FechaCreacion = proceso.CreatedAt,
            ActualizadoPor = proceso.UpdatedBy,
            FechaActualizacion = proceso.UpdatedAt,
        };
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
    public static ProcesoArchivoDTO? ToDto(this ProcesoArchivos archivo)
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
}
