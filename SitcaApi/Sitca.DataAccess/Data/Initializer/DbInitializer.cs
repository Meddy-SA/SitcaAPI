using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sitca.DataAccess.Data.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;            
        }

        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception e)
            {

                throw;
            }

            if (_db.Roles.Any(r => r.Name == "Admin")) return;
            _roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();

            _userManager.CreateAsync(new IdentityUser {
                Email = "fedehit_2@hotmail.com",
                UserName = "fedehit_2@hotmail.com",
                EmailConfirmed = true,
            },"123456").GetAwaiter().GetResult();

            IdentityUser user = _db.Users.Where(u => u.Email == "fedehit_2@hotmail.com").FirstOrDefault();
            _userManager.AddToRoleAsync(user, "Admin");
        }
    }
}
