using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.Models;
using System;
using System.Linq;

namespace Sitca.DataAccess.Data.Initializer
{
  public class DbInitializer : IDbInitializer
  {
    private readonly ILogger<DbInitializer> _logger;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DbInitializer(
        ILogger<DbInitializer> logger,
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
      _logger = logger;
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
        _logger.LogError(e, "Error migrating db");
        throw;
      }

      if (_db.Roles.Any(r => r.Name == "Admin")) return;
      _roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();

      _userManager.CreateAsync(new ApplicationUser
      {
        Email = "leonardoillanez@meddyai.com",
        UserName = "leonardoillanez@meddyai.com",
        EmailConfirmed = true,
      }, "Password123!").GetAwaiter().GetResult();

      ApplicationUser user = _db.Users
        .AsNoTracking()
        .Where(u => u.Email == "leonardoillanez@meddyai.com")
        .FirstOrDefault();
      _userManager.AddToRoleAsync(user, "Admin");
    }
  }
}
