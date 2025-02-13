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
using Sitca.Models.ViewModels;

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

        public async Task<List<UsersListVm>> GetPersonal(int pais, int EmpresaAuditoraId)
        {
            var dbPara = new DynamicParameters();

            dbPara.Add("Pais", pais);
            dbPara.Add("CompanyId", EmpresaAuditoraId);

            var result = await Task.FromResult(
                _dapper.GetAll<UsersListVm>(
                    "[dbo].[GetUserByCompany]",
                    dbPara,
                    commandType: CommandType.StoredProcedure
                )
            );

            return result;
        }

        public async Task<Result<List<UsersListVm>>> GetUsersAsync(
            string query,
            int paisId,
            string role = "All"
        )
        {
            try
            {
                role = (role ?? "All").Trim();
                query = (query ?? string.Empty).Trim();

                // Construir la consulta base
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

                // Obtener los roles de los usuarios
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
