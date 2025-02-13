using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.Models;

namespace Sitca.DataAccess.Data.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ILogger<DbInitializer> _logger;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public DbInitializer(
            ILogger<DbInitializer> logger,
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager
        )
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<bool> Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    await _db.Database.MigrateAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error migrating db");
                throw;
            }

            if (await _roleManager.RoleExistsAsync("Admin"))
                return await Task.FromResult(false);
            // Crear rol Admin
            var adminRole = new ApplicationRole("Admin");
            await _roleManager.CreateAsync(adminRole);

            // Crear usuario Admin
            var adminUser = new ApplicationUser
            {
                Email = "leonardoillanez@meddyai.com",
                UserName = "leonardoillanez@meddyai.com",
                EmailConfirmed = true,
            };

            await _userManager.CreateAsync(adminUser, "Password123!");

            // Asignar rol
            var user = await _db
                .Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == "leonardoillanez@meddyai.com");

            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return true;
        }
    }
}
