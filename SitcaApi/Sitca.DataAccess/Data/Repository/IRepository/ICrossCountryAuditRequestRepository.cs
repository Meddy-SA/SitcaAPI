using System.Collections.Generic;
using System.Threading.Tasks;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.Enums;

namespace Sitca.DataAccess.Data.Repository.IRepository;

public interface ICrossCountryAuditRequestRepository : IRepository<CrossCountryAuditRequest>
{
    Task<Result<CrossCountryAuditRequestDTO>> CreateAsync(
        CreateCrossCountryAuditRequestDTO dto,
        ApplicationUser requestingUser
    );
    Task<Result<List<CrossCountryAuditRequestDTO>>> GetForCountryAsync(
        int countryId,
        CrossCountryAuditRequestStatus? status,
        string userId
    );
    Task<Result<CrossCountryAuditRequestDTO>> ApproveAsync(
        int requestId,
        ApproveCrossCountryAuditRequestDTO dto,
        ApplicationUser approvingUser
    );
    Task<Result<CrossCountryAuditRequestDTO>> RejectAsync(
        int requestId,
        RejectCrossCountryAuditRequestDTO dto,
        ApplicationUser rejectingUser
    );
    Task<Result<CrossCountryAuditRequestDTO>> RevokeAsync(
        int requestId,
        ApplicationUser revokingUser
    );
    Task<Result<CrossCountryAuditRequestDTO>> GetByIdAsync(int requestId, ApplicationUser user);
    Task<Result<bool>> CanAssignAuditorToCompanyAsync(string auditorId, int companyId);
    Task<List<string>> GetApprovedExternalAuditorIdsForCountryAsync(int countryId);
    Task<bool> VerifyUserHasAuditorRoleAsync(string userId);
    Task<bool> VerifyUserHasAsesorRoleAsync(string userId, int? companyCountryId);
}
