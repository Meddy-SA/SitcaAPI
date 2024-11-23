using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.DataAccess.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
        ILogger<UsersRepository> logger) : base(db)
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

      var result = await Task.FromResult(_dapper.GetAll<UsersListVm>("[dbo].[GetUserByCompany]", dbPara, commandType: CommandType.StoredProcedure));

      return result;
    }


    public async Task<Result<List<UsersListVm>>> GetUsersAsync(string query, int paisId, string role = "All")
    {
      try
      {
        // Preparar parámetros para el SP
        var parameters = CreateUserQueryParameters(query, paisId, role);

        // Ejecutar SP y obtener resultados
        var users = await ExecuteUserQueryAsync(parameters);

        // Agrupar y transformar resultados
        var groupedUsers = AgruparYTransformarUsuarios(users);

        return Result<List<UsersListVm>>.Success(groupedUsers);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error obteniendo usuarios para País: {PaisId}, Rol: {Role}, Query: {Query}",
            paisId, role, query);
        return Result<List<UsersListVm>>.Failure($"Error al obtener usuarios: {ex.Message}");
      }
    }

    private DynamicParameters CreateUserQueryParameters(string query, int paisId, string role)
    {
      return new DynamicParameters()
        .AddParameter("Pais", paisId)
        .AddParameter("Role", role)
        .AddParameter("Name", query);
    }

    private async Task<IEnumerable<UsersListVm>> ExecuteUserQueryAsync(DynamicParameters parameters)
    {
      const string SP_NAME = "[dbo].[GetUsers]";
      return await Task.FromResult(
          _dapper.GetAll<UsersListVm>(
              SP_NAME,
              parameters,
              commandType: CommandType.StoredProcedure
          )
      );
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
            Rol = string.Join('/', group.Select(m => m.Rol))
          })
          .ToList();
    }

    public async Task<UsersListVm> GetUserById(string id)
    {
      var dbPara = new DynamicParameters();

      dbPara.Add("Id", id);

      var result = await Task.FromResult(_dapper.Get<UsersListVm>("[dbo].[GetUserById]", dbPara, commandType: CommandType.StoredProcedure));

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
