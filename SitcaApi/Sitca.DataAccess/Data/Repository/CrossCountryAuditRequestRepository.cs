using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.Enums;
using Roles = Utilities.Common.Constants.Roles;

namespace Sitca.DataAccess.Data.Repository;

public class CrossCountryAuditRequestRepository
    : Repository<CrossCountryAuditRequest>,
        ICrossCountryAuditRequestRepository
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<CrossCountryAuditRequestRepository> _logger;

    public CrossCountryAuditRequestRepository(
        ApplicationDbContext db,
        ILogger<CrossCountryAuditRequestRepository> logger
    )
        : base(db)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<CrossCountryAuditRequestDTO>> CreateAsync(
        CreateCrossCountryAuditRequestDTO dto,
        ApplicationUser requestingUser
    )
    {
        try
        {
            // Validación de usuario
            if (requestingUser == null)
                return Result<CrossCountryAuditRequestDTO>.Failure("Usuario no autenticado");

            if (!requestingUser.PaisId.HasValue)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    "El usuario no está asociado a un país"
                );

            // Validar que el país solicitante y el aprobador sean diferentes
            if (requestingUser.PaisId.Value == dto.ApprovingCountryId)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    "No se pueden solicitar auditores del mismo país"
                );

            // Validar que el país aprobador existe
            var approvingCountry = await _db.Pais.FindAsync(dto.ApprovingCountryId);
            if (approvingCountry == null)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    $"No se encontró el país con ID {dto.ApprovingCountryId}"
                );

            // Crear la nueva solicitud
            var request = new CrossCountryAuditRequest
            {
                RequestingCountryId = requestingUser.PaisId.Value,
                ApprovingCountryId = dto.ApprovingCountryId,
                Status = CrossCountryAuditRequestStatus.Pending,
                NotesRequest = dto.NotesRequest,
                CreatedBy = requestingUser.Id,
                CreatedAt = DateTime.UtcNow,
                Enabled = true,
            };

            _db.CrossCountryAuditRequests.Add(request);
            await _db.SaveChangesAsync();

            // Cargar relaciones para el DTO de respuesta
            await _db.Entry(request).Reference(r => r.RequestingCountry).LoadAsync();
            await _db.Entry(request).Reference(r => r.ApprovingCountry).LoadAsync();

            // Mapear a DTO
            var responseDto = new CrossCountryAuditRequestDTO
            {
                Id = request.Id,
                RequestingCountryId = request.RequestingCountryId,
                RequestingCountryName = request.RequestingCountry.Name,
                ApprovingCountryId = request.ApprovingCountryId,
                ApprovingCountryName = request.ApprovingCountry.Name,
                Status = request.Status.ToString(),
                AssignedAuditorId = null,
                AssignedAuditorName = null,
                DeadlineDate = null,
                NotesRequest = request.NotesRequest,
                NotesApproval = null,
                CreatedAt = request.CreatedAt ?? DateTime.UtcNow,
                CreatedBy = request.CreatedBy ?? requestingUser.Id,
                CreatedByName = $"{requestingUser.FirstName} {requestingUser.LastName}",
            };

            return Result<CrossCountryAuditRequestDTO>.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear solicitud de auditoría cruzada");
            return Result<CrossCountryAuditRequestDTO>.Failure(
                $"Error al crear solicitud: {ex.Message}"
            );
        }
    }

    public async Task<Result<List<CrossCountryAuditRequestDTO>>> GetForCountryAsync(
        int countryId,
        CrossCountryAuditRequestStatus? status,
        string userId,
        string? countryRole = null
    )
    {
        try
        {
            // Construir la consulta base
            var query = _db
                .CrossCountryAuditRequests.Include(r => r.RequestingCountry)
                .Include(r => r.ApprovingCountry)
                .Include(r => r.AssignedAuditor)
                .Include(r => r.UserCreate)
                .AsNoTracking()
                .Where(r => r.Enabled);

            // Filtrar según el rol del país
            if (string.Equals(countryRole, "requesting", StringComparison.OrdinalIgnoreCase))
            {
                // Solo solicitudes CREADAS por este país
                query = query.Where(r => r.RequestingCountryId == countryId);
            }
            else if (string.Equals(countryRole, "approving", StringComparison.OrdinalIgnoreCase))
            {
                // Solo solicitudes que este país debe APROBAR/RECHAZAR
                query = query.Where(r => r.ApprovingCountryId == countryId);
            }
            else
            {
                // Comportamiento por defecto: ambas
                query = query.Where(r =>
                    r.RequestingCountryId == countryId || r.ApprovingCountryId == countryId
                );
            }

            // Aplicar filtro opcional por estado
            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            // Ordenar por fecha de creación (más reciente primero)
            query = query.OrderByDescending(r => r.CreatedAt);

            // Ejecutar la consulta
            var requests = await query.ToListAsync();

            // Mapear a DTOs
            var dtos = requests
                .Select(r => new CrossCountryAuditRequestDTO
                {
                    Id = r.Id,
                    RequestingCountryId = r.RequestingCountryId,
                    RequestingCountryName = r.RequestingCountry.Name,
                    ApprovingCountryId = r.ApprovingCountryId,
                    ApprovingCountryName = r.ApprovingCountry.Name,
                    Status = r.Status.ToString(),
                    AssignedAuditorId = r.AssignedAuditorId,
                    AssignedAuditorName =
                        r.AssignedAuditor != null
                            ? $"{r.AssignedAuditor.FirstName} {r.AssignedAuditor.LastName}"
                            : null,
                    DeadlineDate = r.DeadlineDate,
                    NotesRequest = r.NotesRequest,
                    NotesApproval = r.NotesApproval,
                    CreatedAt = r.CreatedAt ?? DateTime.UtcNow,
                    CreatedBy = r.CreatedBy ?? userId,
                    CreatedByName =
                        r.UserCreate != null
                            ? $"{r.UserCreate.FirstName} {r.UserCreate.LastName}"
                            : null,
                })
                .ToList();

            return Result<List<CrossCountryAuditRequestDTO>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener solicitudes de auditoría cruzada para el país {CountryId}",
                countryId
            );
            return Result<List<CrossCountryAuditRequestDTO>>.Failure(
                $"Error al obtener solicitudes: {ex.Message}"
            );
        }
    }

    public async Task<Result<CrossCountryAuditRequestDTO>> ApproveAsync(
        int requestId,
        ApproveCrossCountryAuditRequestDTO dto,
        ApplicationUser approvingUser
    )
    {
        try
        {
            // Validar usuario
            if (approvingUser == null || !approvingUser.PaisId.HasValue)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    "Usuario no autenticado o sin país asignado"
                );

            // Cargar la solicitud con todas sus relaciones
            var request = await _db
                .CrossCountryAuditRequests.Include(r => r.RequestingCountry)
                .Include(r => r.ApprovingCountry)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Enabled);

            if (request == null)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    $"No se encontró la solicitud con ID {requestId}"
                );

            // Verificar que el usuario pertenece al país aprobador
            if (approvingUser.PaisId != request.ApprovingCountryId)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    "No tienes permisos para aprobar esta solicitud"
                );

            // Verificar que la solicitud está en estado pendiente
            if (request.Status != CrossCountryAuditRequestStatus.Pending)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    $"La solicitud no está en estado pendiente (estado actual: {request.Status})"
                );

            // Verificar que el auditor asignado pertenece al país aprobador
            var assignedAuditor = await _db.ApplicationUser.FirstOrDefaultAsync(u =>
                u.Id == dto.AssignedAuditorId && u.PaisId == request.ApprovingCountryId
            );

            if (assignedAuditor == null)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    "El auditor seleccionado no existe o no pertenece al país aprobador"
                );

            // Verificar que la fecha límite es futura
            if (dto.DeadlineDate <= DateTime.UtcNow)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    "La fecha límite debe ser futura"
                );

            // Actualizar la solicitud
            request.Status = CrossCountryAuditRequestStatus.Approved;
            request.AssignedAuditorId = dto.AssignedAuditorId;
            request.DeadlineDate = dto.DeadlineDate;
            request.NotesApproval = dto.NotesApproval;
            request.UpdatedBy = approvingUser.Id;
            request.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Mapear a DTO
            var responseDto = new CrossCountryAuditRequestDTO
            {
                Id = request.Id,
                RequestingCountryId = request.RequestingCountryId,
                RequestingCountryName = request.RequestingCountry.Name,
                ApprovingCountryId = request.ApprovingCountryId,
                ApprovingCountryName = request.ApprovingCountry.Name,
                Status = request.Status.ToString(),
                AssignedAuditorId = request.AssignedAuditorId,
                AssignedAuditorName = $"{assignedAuditor.FirstName} {assignedAuditor.LastName}",
                DeadlineDate = request.DeadlineDate,
                NotesRequest = request.NotesRequest,
                NotesApproval = request.NotesApproval,
                CreatedAt = request.CreatedAt ?? DateTime.UtcNow,
                CreatedBy = request.CreatedBy ?? "",
                CreatedByName =
                    request.UserCreate != null
                        ? $"{request.UserCreate.FirstName} {request.UserCreate.LastName}"
                        : null,
            };

            return Result<CrossCountryAuditRequestDTO>.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al aprobar solicitud de auditoría cruzada {RequestId}",
                requestId
            );
            return Result<CrossCountryAuditRequestDTO>.Failure(
                $"Error al aprobar solicitud: {ex.Message}"
            );
        }
    }

    public async Task<Result<CrossCountryAuditRequestDTO>> RejectAsync(
        int requestId,
        RejectCrossCountryAuditRequestDTO dto,
        ApplicationUser rejectingUser
    )
    {
        try
        {
            // Validaciones similares a la aprobación
            if (rejectingUser == null || !rejectingUser.PaisId.HasValue)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    "Usuario no autenticado o sin país asignado"
                );

            var request = await _db
                .CrossCountryAuditRequests.Include(r => r.RequestingCountry)
                .Include(r => r.ApprovingCountry)
                .Include(r => r.UserCreate)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Enabled);

            if (request == null)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    $"No se encontró la solicitud con ID {requestId}"
                );

            if (rejectingUser.PaisId != request.ApprovingCountryId)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    "No tienes permisos para rechazar esta solicitud"
                );

            if (request.Status != CrossCountryAuditRequestStatus.Pending)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    $"La solicitud no está en estado pendiente (estado actual: {request.Status})"
                );

            // Actualizar la solicitud
            request.Status = CrossCountryAuditRequestStatus.Rejected;
            request.NotesApproval = dto.NotesApproval;
            request.UpdatedBy = rejectingUser.Id;
            request.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Mapear a DTO
            var responseDto = new CrossCountryAuditRequestDTO
            {
                Id = request.Id,
                RequestingCountryId = request.RequestingCountryId,
                RequestingCountryName = request.RequestingCountry.Name,
                ApprovingCountryId = request.ApprovingCountryId,
                ApprovingCountryName = request.ApprovingCountry.Name,
                Status = request.Status.ToString(),
                AssignedAuditorId = null,
                AssignedAuditorName = null,
                DeadlineDate = null,
                NotesRequest = request.NotesRequest,
                NotesApproval = request.NotesApproval,
                CreatedAt = request.CreatedAt ?? DateTime.UtcNow,
                CreatedBy = request.CreatedBy ?? "",
                CreatedByName =
                    request.UserCreate != null
                        ? $"{request.UserCreate.FirstName} {request.UserCreate.LastName}"
                        : null,
            };

            return Result<CrossCountryAuditRequestDTO>.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al rechazar solicitud de auditoría cruzada {RequestId}",
                requestId
            );
            return Result<CrossCountryAuditRequestDTO>.Failure(
                $"Error al rechazar solicitud: {ex.Message}"
            );
        }
    }

    public async Task<Result<CrossCountryAuditRequestDTO>> RevokeAsync(
        int requestId,
        ApplicationUser revokingUser
    )
    {
        try
        {
            // Validaciones
            if (revokingUser == null || !revokingUser.PaisId.HasValue)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    "Usuario no autenticado o sin país asignado"
                );

            var request = await _db
                .CrossCountryAuditRequests.Include(r => r.RequestingCountry)
                .Include(r => r.ApprovingCountry)
                .Include(r => r.AssignedAuditor)
                .Include(r => r.UserCreate)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Enabled);

            if (request == null)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    $"No se encontró la solicitud con ID {requestId}"
                );

            // Verificar permisos - debe ser del país solicitante o del país aprobador
            if (
                revokingUser.PaisId != request.RequestingCountryId
                && revokingUser.PaisId != request.ApprovingCountryId
            )
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    "No tienes permisos para revocar esta solicitud"
                );

            // Verificar que la solicitud está en estado aprobado
            if (request.Status != CrossCountryAuditRequestStatus.Approved)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    $"Solo se pueden revocar solicitudes aprobadas (estado actual: {request.Status})"
                );

            // Actualizar la solicitud
            request.Status = CrossCountryAuditRequestStatus.Revoked;
            request.UpdatedBy = revokingUser.Id;
            request.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Mapear a DTO
            var responseDto = new CrossCountryAuditRequestDTO
            {
                Id = request.Id,
                RequestingCountryId = request.RequestingCountryId,
                RequestingCountryName = request.RequestingCountry.Name,
                ApprovingCountryId = request.ApprovingCountryId,
                ApprovingCountryName = request.ApprovingCountry.Name,
                Status = request.Status.ToString(),
                AssignedAuditorId = request.AssignedAuditorId,
                AssignedAuditorName =
                    request.AssignedAuditor != null
                        ? $"{request.AssignedAuditor.FirstName} {request.AssignedAuditor.LastName}"
                        : null,
                DeadlineDate = request.DeadlineDate,
                NotesRequest = request.NotesRequest,
                NotesApproval = request.NotesApproval,
                CreatedAt = request.CreatedAt ?? DateTime.UtcNow,
                CreatedBy = request.CreatedBy ?? "",
                CreatedByName =
                    request.UserCreate != null
                        ? $"{request.UserCreate.FirstName} {request.UserCreate.LastName}"
                        : null,
            };

            return Result<CrossCountryAuditRequestDTO>.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al revocar solicitud de auditoría cruzada {RequestId}",
                requestId
            );
            return Result<CrossCountryAuditRequestDTO>.Failure(
                $"Error al revocar solicitud: {ex.Message}"
            );
        }
    }

    public async Task<Result<CrossCountryAuditRequestDTO>> CancelAsync(
        int requestId,
        ApplicationUser cancellingUser
    )
    {
        try
        {
            // Validar usuario
            if (cancellingUser == null || !cancellingUser.PaisId.HasValue)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    "Usuario no autenticado o sin país asignado"
                );

            var request = await _db
                .CrossCountryAuditRequests.Include(r => r.RequestingCountry)
                .Include(r => r.ApprovingCountry)
                .Include(r => r.UserCreate)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Enabled);

            if (request == null)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    $"No se encontró la solicitud con ID {requestId}"
                );

            // Solo el país solicitante puede cancelar
            if (cancellingUser.PaisId != request.RequestingCountryId)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    "Solo el país solicitante puede cancelar esta solicitud"
                );

            // Solo solicitudes pendientes pueden ser canceladas
            if (request.Status != CrossCountryAuditRequestStatus.Pending)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    $"Solo se pueden cancelar solicitudes pendientes (estado actual: {request.Status})"
                );

            // Actualizar la solicitud
            request.Status = CrossCountryAuditRequestStatus.Cancelled;
            request.UpdatedBy = cancellingUser.Id;
            request.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Mapear a DTO
            var responseDto = new CrossCountryAuditRequestDTO
            {
                Id = request.Id,
                RequestingCountryId = request.RequestingCountryId,
                RequestingCountryName = request.RequestingCountry.Name,
                ApprovingCountryId = request.ApprovingCountryId,
                ApprovingCountryName = request.ApprovingCountry.Name,
                Status = request.Status.ToString(),
                AssignedAuditorId = null,
                AssignedAuditorName = null,
                DeadlineDate = null,
                NotesRequest = request.NotesRequest,
                NotesApproval = request.NotesApproval,
                CreatedAt = request.CreatedAt ?? DateTime.UtcNow,
                CreatedBy = request.CreatedBy ?? "",
                CreatedByName =
                    request.UserCreate != null
                        ? $"{request.UserCreate.FirstName} {request.UserCreate.LastName}"
                        : null,
            };

            return Result<CrossCountryAuditRequestDTO>.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al cancelar solicitud de auditoría cruzada {RequestId}",
                requestId
            );
            return Result<CrossCountryAuditRequestDTO>.Failure(
                $"Error al cancelar solicitud: {ex.Message}"
            );
        }
    }

    public async Task<Result<int>> GetPendingCountForApproverAsync(int countryId)
    {
        try
        {
            var count = await _db
                .CrossCountryAuditRequests.AsNoTracking()
                .Where(r =>
                    r.ApprovingCountryId == countryId
                    && r.Status == CrossCountryAuditRequestStatus.Pending
                    && r.Enabled
                )
                .CountAsync();

            return Result<int>.Success(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener conteo de solicitudes pendientes para el país {CountryId}",
                countryId
            );
            return Result<int>.Failure($"Error al obtener conteo: {ex.Message}");
        }
    }

    public async Task<Result<CrossCountryAuditRequestDTO>> GetByIdAsync(
        int requestId,
        ApplicationUser user
    )
    {
        try
        {
            // Validar usuario
            if (user == null || !user.PaisId.HasValue)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    "Usuario no autenticado o sin país asignado"
                );

            // Cargar la solicitud con sus relaciones
            var request = await _db
                .CrossCountryAuditRequests.Include(r => r.RequestingCountry)
                .Include(r => r.ApprovingCountry)
                .Include(r => r.AssignedAuditor)
                .Include(r => r.UserCreate)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Enabled);

            if (request == null)
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    $"No se encontró la solicitud con ID {requestId}"
                );

            // Verificar que el usuario tiene acceso a esta solicitud (país solicitante o aprobador)
            if (
                user.PaisId != request.RequestingCountryId
                && user.PaisId != request.ApprovingCountryId
            )
                return Result<CrossCountryAuditRequestDTO>.Failure(
                    "No tienes permisos para ver esta solicitud"
                );

            // Mapear a DTO
            var dto = new CrossCountryAuditRequestDTO
            {
                Id = request.Id,
                RequestingCountryId = request.RequestingCountryId,
                RequestingCountryName = request.RequestingCountry.Name,
                ApprovingCountryId = request.ApprovingCountryId,
                ApprovingCountryName = request.ApprovingCountry.Name,
                Status = request.Status.ToString(),
                AssignedAuditorId = request.AssignedAuditorId,
                AssignedAuditorName =
                    request.AssignedAuditor != null
                        ? $"{request.AssignedAuditor.FirstName} {request.AssignedAuditor.LastName}"
                        : null,
                DeadlineDate = request.DeadlineDate,
                NotesRequest = request.NotesRequest,
                NotesApproval = request.NotesApproval,
                CreatedAt = request.CreatedAt ?? DateTime.UtcNow,
                CreatedBy = request.CreatedBy ?? "",
                CreatedByName =
                    request.UserCreate != null
                        ? $"{request.UserCreate.FirstName} {request.UserCreate.LastName}"
                        : null,
            };

            return Result<CrossCountryAuditRequestDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener solicitud de auditoría cruzada {RequestId}",
                requestId
            );
            return Result<CrossCountryAuditRequestDTO>.Failure(
                $"Error al obtener solicitud: {ex.Message}"
            );
        }
    }

    public async Task<Result<bool>> CanAssignAuditorToCompanyAsync(string auditorId, int companyId)
    {
        try
        {
            // Verificar si el auditor está asignado a alguna solicitud de auditoría cruzada aprobada
            var crossAuditRequest = await _db
                .CrossCountryAuditRequests.AsNoTracking()
                .Include(r => r.RequestingCountry)
                .Include(r => r.ApprovingCountry)
                .FirstOrDefaultAsync(r =>
                    r.AssignedAuditorId == auditorId
                    && r.Status == CrossCountryAuditRequestStatus.Approved
                    && r.Enabled
                );

            // Si no hay solicitud, el auditor es local
            if (crossAuditRequest == null)
            {
                // Para auditores locales, verificar que pertenecen al mismo país que la empresa
                var company = await _db.Empresa.FindAsync(companyId);
                if (company == null)
                    return Result<bool>.Failure("No se encontró la empresa especificada");

                var auditor = await _db.ApplicationUser.FindAsync(auditorId);
                if (auditor == null)
                    return Result<bool>.Failure("No se encontró el auditor especificado");

                if (auditor.PaisId != company.PaisId)
                    return Result<bool>.Failure(
                        "El auditor local debe pertenecer al mismo país que la empresa"
                    );

                return Result<bool>.Success(true);
            }

            // Si hay solicitud, verificar si la fecha límite ha pasado
            if (
                crossAuditRequest.DeadlineDate.HasValue
                && crossAuditRequest.DeadlineDate.Value < DateTime.UtcNow
            )
            {
                _logger.LogWarning(
                    "Intento de asignar auditor externo {AuditorId} cuya colaboración venció el {DeadlineDate}",
                    auditorId,
                    crossAuditRequest.DeadlineDate.Value
                );
                return Result<bool>.Failure(
                    $"La colaboración de este auditor externo venció el {crossAuditRequest.DeadlineDate.Value:dd/MM/yyyy}. "
                        + $"No puede ser asignado a nuevas empresas."
                );
            }

            // Obtener el país de la empresa
            var targetCompany = await _db.Empresa.FindAsync(companyId);
            if (targetCompany == null)
                return Result<bool>.Failure("No se encontró la empresa especificada");

            // Verificar si la empresa pertenece al país solicitante de la auditoría cruzada
            if (targetCompany.PaisId != crossAuditRequest.RequestingCountryId)
            {
                _logger.LogWarning(
                    "Intento de asignar auditor externo {AuditorId} de {ApprovingCountry} a empresa {CompanyId} de país {CompanyCountry}, "
                        + "pero la colaboración es solo para {RequestingCountry}",
                    auditorId,
                    crossAuditRequest.ApprovingCountry.Name,
                    companyId,
                    targetCompany.PaisId,
                    crossAuditRequest.RequestingCountry.Name
                );
                return Result<bool>.Failure(
                    $"Este auditor externo de {crossAuditRequest.ApprovingCountry.Name} solo puede ser asignado "
                        + $"a empresas de {crossAuditRequest.RequestingCountry.Name}."
                );
            }

            _logger.LogInformation(
                "Validación exitosa para auditor externo {AuditorId} de {ApprovingCountry} "
                    + "para empresa {CompanyId} de {RequestingCountry}. Vence: {DeadlineDate}",
                auditorId,
                crossAuditRequest.ApprovingCountry.Name,
                companyId,
                crossAuditRequest.RequestingCountry.Name,
                crossAuditRequest.DeadlineDate
            );

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al verificar si el auditor {AuditorId} puede ser asignado a la empresa {CompanyId}",
                auditorId,
                companyId
            );

            return Result<bool>.Failure(
                $"Error al validar la asignación del auditor: {ex.Message}"
            );
        }
    }

    public async Task<List<string>> GetApprovedExternalAuditorIdsForCountryAsync(int countryId)
    {
        try
        {
            var now = DateTime.UtcNow;

            // Obtener IDs de auditores externos aprobados para este país
            var auditorIds = await _db
                .CrossCountryAuditRequests.AsNoTracking()
                .Where(r =>
                    r.RequestingCountryId == countryId
                    && r.Status == CrossCountryAuditRequestStatus.Approved
                    && r.Enabled
                    && r.AssignedAuditorId != null
                    && r.DeadlineDate > now
                )
                .Select(r => r.AssignedAuditorId!)
                .ToListAsync();

            return auditorIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener auditores externos para el país {CountryId}",
                countryId
            );
            return new List<string>();
        }
    }

    /// <summary>
    /// Verifica que un usuario tenga rol de auditor y esté activo
    /// </summary>
    public async Task<bool> VerifyUserHasAuditorRoleAsync(string userId)
    {
        try
        {
            var userWithRole = await _db
                .ApplicationUser.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.Active);

            if (userWithRole == null)
                return false;

            return userWithRole.UserRoles.Any(ur =>
                ur.Role.Name == Roles.Auditor || ur.Role.Name == Roles.AsesorAuditor
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando rol de auditor para usuario {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Verifica que un usuario tenga rol de asesor y pertenezca al país correcto
    /// </summary>
    public async Task<bool> VerifyUserHasAsesorRoleAsync(string userId, int? companyCountryId)
    {
        try
        {
            var userWithRole = await _db
                .ApplicationUser.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.Active);

            if (userWithRole == null)
                return false;

            // Verificar rol
            var hasAsesorRole = userWithRole.UserRoles.Any(ur =>
                ur.Role.Name == Roles.Asesor || ur.Role.Name == Roles.AsesorAuditor
            );

            if (!hasAsesorRole)
                return false;

            // Para asesores, deben ser del mismo país que la empresa
            if (companyCountryId.HasValue && userWithRole.PaisId != companyCountryId.Value)
                return false;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando rol de asesor para usuario {UserId}", userId);
            return false;
        }
    }
}
