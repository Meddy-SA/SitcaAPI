using Dapper;
using Microsoft.EntityFrameworkCore;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.Models;
using Sitca.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Sitca.DataAccess.Data.Repository
{
    public class UsersRepository : Repository<ApplicationUser>, IUsersRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IDapper _dapper;

        public UsersRepository(ApplicationDbContext db, IDapper dapper) : base(db)
        {
            _db = db;
            _dapper = dapper;
            //https://www.c-sharpcorner.com/article/using-dapper-in-asp-net-core-web-api/
            //https://www.freecodespot.com/blog/using-dapper-in-asp-net-core-web-api/
        }


        public async Task<bool> SetLanguageAsync(string lang, string user)
        {

            var userFromDb = await _db.Users.FirstOrDefaultAsync(s => s.Email == user);
            var appUser = (ApplicationUser)userFromDb;            

            appUser.Lenguage = lang;
            await Context.SaveChangesAsync();

            return true;
        }

        public async Task<List<UsersListVm>> GetPersonal( int pais, int EmpresaAuditoraId)
        {            
            var dbPara = new DynamicParameters();

            dbPara.Add("Pais", pais);
            dbPara.Add("CompanyId", EmpresaAuditoraId);
           
            var result = await Task.FromResult(_dapper.GetAll<UsersListVm>("[dbo].[GetUserByCompany]", dbPara, commandType: CommandType.StoredProcedure));

            return result;
        }


        public async Task<List<UsersListVm>> GetUsersAsync(string query ,int pais, string Role)
        {
            //var result = await Task.FromResult(_dapper.GetAll<RolesVm>($"Select * from [AspNetRoles]", null, commandType: CommandType.Text));

            if (string.IsNullOrEmpty(Role))
            {
                Role = "All";
            }

            //var result = _db.ApplicationUser.Where(x => x.PaisId == pais   && x. (x.FirstName.Contains(query) || x.LastName.Contains(query)));

            var dbPara = new DynamicParameters();            

            dbPara.Add("Pais", pais);
            dbPara.Add("Role", Role);
            dbPara.Add("Name", query);
            var result2 = await Task.FromResult(_dapper.GetAll<UsersListVm>("[dbo].[GetUsers]", dbPara, commandType: CommandType.StoredProcedure));

            var res3 = result2.GroupBy(s => s.Email).Select(x => new UsersListVm
            {
                Email = x.Key,
                Id = x.First().Id,
                PhoneNumber = x.First().PhoneNumber,
                FirstName = x.First().FirstName,
                LastName = x.First().LastName,
                Pais = x.First().Pais,
                PaisId = x.First().PaisId,
                EmailConfirmed = x.First().EmailConfirmed,
                Active = x.First().Active,
                Rol = string.Join('/', x.Select(m => m.Rol))
            }).ToList();

            return res3;
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
