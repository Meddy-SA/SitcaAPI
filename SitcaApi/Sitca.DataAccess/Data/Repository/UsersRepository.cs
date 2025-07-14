using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.Enums;
using Sitca.Models.ViewModels;
using Roles = Utilities.Common.Constants.Roles;

namespace Sitca.DataAccess.Data.Repository
{
    public class UsersRepository : Repository<ApplicationUser>, IUsersRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IDapper _dapper;
        private readonly ILogger<UsersRepository> _logger;

        public UsersRepository(
            ApplicationDbContext db,
            IDapper dapper,
            ILogger<UsersRepository> logger
        )
            : base(db)
        {
            _db = db;
            _dapper = dapper;
            _logger = logger;
        }

        public async Task<bool> SetLanguageAsync(string lang, string user)
        {
            var userFromDb = await _db.Users.FirstOrDefaultAsync(s => s.Email == user);
            var appUser = (ApplicationUser)userFromDb;

            appUser.Lenguage = lang;
            await Context.SaveChangesAsync();

            return true;
        }

        public async Task<Result<List<CompAuditoraListVm>>> GetCompaniesAsync(int paisId)
        {
            try
            {
                var companies = await _db
                    .CompAuditoras.AsNoTracking()
                    .Include(c => c.Pais)
                    .Include(c => c.Usuarios.Where(u => u.Active))
                    .Where(c => (c.PaisId == paisId || paisId == 0) && !c.Special)
                    .Select(c => new CompAuditoraListVm
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Email = c.Email,
                        Telefono = c.Telefono,
                        Direccion = c.Direccion,
                        PaisId = c.PaisId,
                        Pais = c.Pais.Name,
                        Status = c.Status,
                        Representante = c.Representante,
                        NumeroCertificado = c.NumeroCertificado,
                        TotalUsuarios = c.Usuarios.Count,
                        VencimientoConcesion =
                            c.FechaFinConcesion != null
                                ? c.FechaFinConcesion.Value.ToString("dd/MM/yyyy")
                                : null,
                        Usuarios = c
                            .Usuarios.Select(u => new UsersListVm
                            {
                                Id = u.Id,
                                Email = u.Email,
                                FirstName = u.FirstName,
                                LastName = u.LastName,
                                PhoneNumber = u.PhoneNumber ?? string.Empty,
                                Codigo = u.Codigo,
                                NumeroCarnet = u.NumeroCarnet,
                                VencimientoCarnet = u.VencimientoCarnet.ToString(),
                                Active = u.Active,
                                Rol = "ROL", // Si necesitas el rol específico, deberías incluir UserRoles
                                DocumentoIdentidad = u.DocumentoIdentidad,
                                Profesion = u.Profesion,
                                Lang = u.Lenguage,
                            })
                            .ToList(),
                    })
                    .ToListAsync();

                if (!companies.Any())
                {
                    _logger.LogInformation(
                        "No se encontraron compañías auditoras para el país {PaisId}",
                        paisId
                    );
                    return Result<List<CompAuditoraListVm>>.Success(new List<CompAuditoraListVm>());
                }

                return Result<List<CompAuditoraListVm>>.Success(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al obtener las compañías auditoras para el país {PaisId}",
                    paisId
                );
                return Result<List<CompAuditoraListVm>>.Failure(
                    $"Error al obtener las compañías auditoras: {ex.Message}"
                );
            }
        }

        public async Task<Result<List<UsersListVm>>> GetPersonalAsync(
            int paisId,
            int empresaAuditoraId
        )
        {
            try
            {
                var allowedRoles = new[] { Roles.Asesor, Roles.Auditor };

                // Construir la consulta base
                IQueryable<ApplicationUser> query = _db
                    .Users.AsNoTracking()
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .Include(u => u.Pais);

                // Aplicar filtros
                query = query.Where(u =>
                    (paisId == 0 || u.PaisId == paisId)
                    && u.Active
                    && u.UserRoles.Any(ur => allowedRoles.Contains(ur.Role.Name))
                );

                // Aplicar filtro de empresa auditora si es necesario
                if (empresaAuditoraId != 0)
                {
                    query = query.Where(u => u.CompAuditoraId == empresaAuditoraId);
                }

                // Proyección a DTO
                var usersQuery = query.Select(u => new UsersListVm
                {
                    Id = u.Id,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    PaisId = u.PaisId ?? 0,
                    Codigo = u.Codigo,
                    Direccion = u.Direccion,
                    Ciudad = u.Ciudad,
                    Departamento = u.Departamento,
                    FechaIngreso = u.FechaIngreso,
                    NumeroCarnet = u.NumeroCarnet,
                    Rol = u.UserRoles.First().Role.Name,
                    Active = u.Active,
                    CompAuditoraId = u.CompAuditoraId,
                    Pais = u.Pais != null ? u.Pais.Name : string.Empty,
                });

                // Ejecutar la consulta
                var users = await usersQuery.ToListAsync();

                return Result<List<UsersListVm>>.Success(users);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error al obtener los usuarios");
                return Result<List<UsersListVm>>.Failure(
                    $"Error al obtener los usuarios: {exception.Message}"
                );
            }
        }

        // Método base para validaciones comunes
        private async Task<Result<bool>> ValidateCompanyAndUsersAsync(
            int companyId,
            string[] userIds,
            bool checkExistingAssignment = false
        )
        {
            if (companyId <= 0)
                return Result<bool>.Failure("ID de compañía inválido");

            if (userIds == null || !userIds.Any())
                return Result<bool>.Failure("Debe proporcionar al menos un usuario");

            var companyExists = await _db
                .CompAuditoras.AsNoTracking()
                .AnyAsync(c => c.Id == companyId);

            if (!companyExists)
                return Result<bool>.Failure($"No se encontró la compañía con ID {companyId}");

            if (checkExistingAssignment)
            {
                var alreadyAssignedUsers = await _db
                    .Users.AsNoTracking()
                    .Where(u =>
                        userIds.Contains(u.Id)
                        && u.CompAuditoraId != null
                        && u.CompAuditoraId != companyId
                    )
                    .Select(u => new
                    {
                        u.Id,
                        u.CompAuditoraId,
                        u.FirstName,
                        u.LastName,
                    })
                    .ToListAsync();

                if (alreadyAssignedUsers.Any())
                {
                    var companiesInfo = await _db
                        .CompAuditoras.AsNoTracking()
                        .Where(c =>
                            alreadyAssignedUsers.Select(u => u.CompAuditoraId).Contains(c.Id)
                        )
                        .ToDictionaryAsync(c => c.Id, c => c.Name);

                    var conflicts = alreadyAssignedUsers
                        .Select(u =>
                            $"Usuario {u.FirstName} {u.LastName} ya está asignado a {companiesInfo[u.CompAuditoraId.Value]}"
                        )
                        .ToList();

                    return Result<bool>.Failure(
                        $"Algunos usuarios ya están asignados a otras compañías: {string.Join(", ", conflicts)}"
                    );
                }
            }

            return Result<bool>.Success(true);
        }

        // Método genérico para ejecutar operaciones con usuarios
        private async Task<Result<T>> ExecuteUserOperationAsync<T>(
            int companyId,
            string[] userIds,
            Func<IList<ApplicationUser>, Task<T>> operation,
            string operationName
        )
        {
            try
            {
                var strategy = _db.Database.CreateExecutionStrategy();

                return await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _db.Database.BeginTransactionAsync();
                    try
                    {
                        var users = await _db
                            .Users.Where(u => userIds.Contains(u.Id))
                            .ToListAsync();

                        var foundUserIds = users.Select(u => u.Id).ToHashSet();
                        var missingUserIds = userIds.Except(foundUserIds).ToList();

                        if (missingUserIds.Any())
                            return Result<T>.Failure(
                                $"No se encontraron los siguientes usuarios: {string.Join(", ", missingUserIds)}"
                            );

                        var result = await operation(users.Cast<ApplicationUser>().ToList());
                        await _db.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Result<T>.Success(result);
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(
                            ex,
                            "Error de concurrencia al {Operation} usuarios de la compañía {CompanyId}",
                            operationName,
                            companyId
                        );
                        return Result<T>.Failure(
                            $"Error de concurrencia al {operationName} usuarios"
                        );
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(
                            "Error no controlado al {Operation} usuarios de la companía {CompanyId}. {Error}",
                            operationName,
                            companyId,
                            ex.Message
                        );
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error no controlado al {Operation} usuarios de la compañía {CompanyId}",
                    operationName,
                    companyId
                );
                return Result<T>.Failure($"Error al {operationName} usuarios: {ex.Message}");
            }
        }

        public async Task<Result<CompAuditoras>> AssignUserToCompanyAsync(
            int companyId,
            string[] userIds
        )
        {
            var validationResult = await ValidateCompanyAndUsersAsync(companyId, userIds, true);
            if (!validationResult.IsSuccess)
                return Result<CompAuditoras>.Failure(validationResult.Error);

            return await ExecuteUserOperationAsync<CompAuditoras>(
                companyId,
                userIds,
                async users =>
                {
                    var company = await _db
                        .CompAuditoras.Include(c => c.Usuarios)
                        .FirstAsync(c => c.Id == companyId);

                    foreach (var user in users)
                    {
                        user.CompAuditoraId = companyId;
                        _db.Entry(user).State = EntityState.Modified;
                    }

                    _logger.LogInformation(
                        "Asignados {UserCount} usuarios a la compañía {CompanyId}",
                        users.Count,
                        companyId
                    );

                    return company;
                },
                "asignar"
            );
        }

        public async Task<Result<bool>> UnassignUsersFromCompanyAsync(
            int companyId,
            string[] userIds
        )
        {
            var validationResult = await ValidateCompanyAndUsersAsync(companyId, userIds);
            if (!validationResult.IsSuccess)
                return Result<bool>.Failure(validationResult.Error);

            return await ExecuteUserOperationAsync<bool>(
                companyId,
                userIds,
                async users =>
                {
                    var usrs = await Task.FromResult(
                        users.Where(u => u.CompAuditoraId == companyId).ToList()
                    );
                    foreach (var user in usrs)
                    {
                        user.CompAuditoraId = null;
                        _db.Entry(user).State = EntityState.Modified;
                    }

                    _logger.LogInformation(
                        "Desasignados {UserCount} usuarios de la compañía {CompanyId}",
                        users.Count,
                        companyId
                    );

                    return true;
                },
                "desasignar"
            );
        }

        public async Task<Result<List<UsersListVm>>> GetUsersAsync(
            string query,
            int paisId,
            string role = "All",
            string activeFilter = "Todos"
        )
        {
            try
            {
                role = (role ?? "All").Trim();
                query = (query ?? string.Empty).Trim();

                // Construir la consulta base para usuarios locales
                var usersQuery = _db
                    .ApplicationUser.AsNoTracking()
                    .Include(u => u.Pais)
                    .Where(u =>
                        u.EmpresaId == null
                        && (paisId == 0 || u.PaisId == paisId)
                        && (
                            string.IsNullOrEmpty(query)
                            || (u.FirstName.Contains(query) || u.LastName.Contains(query))
                        )
                    );

                // Aplicar filtro de estado activo
                switch (activeFilter?.ToLower())
                {
                    case "activos":
                        usersQuery = usersQuery.Where(u => u.Active);
                        break;
                    case "inactivos":
                        usersQuery = usersQuery.Where(u => !u.Active);
                        break;
                    case "todos":
                    default:
                        // No aplicar filtro, mostrar todos
                        break;
                }

                // Obtener los roles de los usuarios locales
                var userRoles = await (
                    from user in usersQuery
                    join userRole in _db.ApplicationUserRoles on user.Id equals userRole.UserId
                    join rol in _db.ApplicationRole on userRole.RoleId equals rol.Id
                    where role == "All" || rol.Name == role
                    select new UsersListVm
                    {
                        Id = user.Id,
                        Email = user.Email,
                        EmailConfirmed = user.EmailConfirmed.ToString(),
                        PhoneNumber = user.PhoneNumber,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        PaisId = user.PaisId ?? user.Pais.Id,
                        Active = user.Active,
                        Rol = rol.Name,
                        Pais = user.Pais.Name,
                    }
                ).TagWith("GetUsers_Query") // Para facilitar la identificación en logs
                .ToListAsync();

                // Si se está buscando auditores o "All", incluir auditores externos
                if (role == "All" || role == Roles.Auditor)
                {
                    var externalAuditors = await GetExternalAuditorsAsync(paisId, query);
                    userRoles.AddRange(externalAuditors);
                }

                // Agrupar y transformar resultados
                var groupedUsers = AgruparYTransformarUsuarios(userRoles);

                return Result<List<UsersListVm>>.Success(groupedUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error obteniendo usuarios para País: {PaisId}, Rol: {Role}, Query: {Query}",
                    paisId,
                    role,
                    query
                );
                return Result<List<UsersListVm>>.Failure(
                    $"Error al obtener usuarios: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Obtiene los auditores externos aprobados para un país específico
        /// </summary>
        private async Task<List<UsersListVm>> GetExternalAuditorsAsync(int paisId, string query)
        {
            try
            {
                if (paisId == 0)
                    return new List<UsersListVm>();

                var now = DateTime.UtcNow;

                // Obtener auditores externos aprobados para este país
                var externalAuditorsQuery =
                    from request in _db.CrossCountryAuditRequests
                    join auditor in _db.ApplicationUser
                        on request.AssignedAuditorId equals auditor.Id
                    join pais in _db.Pais on auditor.PaisId equals pais.Id
                    where
                        request.RequestingCountryId == paisId
                        && request.Status == CrossCountryAuditRequestStatus.Approved
                        && request.Enabled
                        && request.AssignedAuditorId != null
                        && request.DeadlineDate > now
                        && auditor.Active
                        && (
                            string.IsNullOrEmpty(query)
                            || auditor.FirstName.Contains(query)
                            || auditor.LastName.Contains(query)
                        )
                    select new UsersListVm
                    {
                        Id = auditor.Id,
                        Email = auditor.Email,
                        EmailConfirmed = auditor.EmailConfirmed.ToString(),
                        PhoneNumber = auditor.PhoneNumber,
                        FirstName = auditor.FirstName,
                        LastName = auditor.LastName,
                        PaisId = auditor.PaisId ?? 0,
                        Active = auditor.Active,
                        Rol = Roles.Auditor,
                        Pais = $"Externo - {pais.Name}", // Marcar como externo
                    };

                return await externalAuditorsQuery
                    .TagWith("GetExternalAuditors_Query")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error obteniendo auditores externos para país {PaisId}",
                    paisId
                );
                return new List<UsersListVm>();
            }
        }

        private List<UsersListVm> AgruparYTransformarUsuarios(IEnumerable<UsersListVm> users)
        {
            return users
                .GroupBy(s => s.Email)
                .Select(group => new UsersListVm
                {
                    Email = group.Key,
                    Id = group.First().Id,
                    PhoneNumber = group.First().PhoneNumber,
                    FirstName = group.First().FirstName,
                    LastName = group.First().LastName,
                    Pais = group.First().Pais,
                    PaisId = group.First().PaisId,
                    EmailConfirmed = group.First().EmailConfirmed,
                    Active = group.First().Active,
                    Rol = DetermineUserRole(group.Select(r => r.Rol)),
                })
                .ToList();
        }

        private static string DetermineUserRole(IEnumerable<string> roles)
        {
            var rolesList = roles.Distinct().ToList();

            if (rolesList.Contains("Asesor/Auditor"))
                return "Asesor/Auditor";

            if (rolesList.Contains("Asesor") && rolesList.Contains("Auditor"))
                return "Asesor/Auditor";

            return string.Join("/", rolesList);
        }

        public async Task<UsersListVm> GetUserById(string id)
        {
            var dbPara = new DynamicParameters();

            dbPara.Add("Id", id);

            var result = await Task.FromResult(
                _dapper.Get<UsersListVm>(
                    "[dbo].[GetUserById]",
                    dbPara,
                    commandType: CommandType.StoredProcedure
                )
            );

            return result;
        }
    }

    public class RolesVm
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string ConcurrencyStamp { get; set; }
    }
}
