using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;

namespace Sitca.DataAccess.Services.CompanyQuery;

public interface ICompanyQueryBuilder
{
    IQueryable<Empresa> BuildGeneralQuery(bool includeHomologacion);
    IQueryable<Empresa> BuildRoleBasedQuery(ApplicationUser user, string role);
    IQueryable<Empresa> ApplyFilters(
        IQueryable<Empresa> query,
        CompanyFilterDTO filter,
        int? distintiveId = null
    );
    Task<List<EmpresaVm>> BuildAndExecuteProjection(IQueryable<Empresa> query, string language);
}
