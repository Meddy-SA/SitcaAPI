using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Sitca.DataAccess.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
  public ApplicationDbContext CreateDbContext(string[] args)
  {
    var currentDir = Directory.GetCurrentDirectory();
    var projectRoot = Directory.GetParent(currentDir)?.FullName;
    var mainProjectPath = Path.Combine(projectRoot!, "Sitca");

    Console.WriteLine($"Buscando appsettings.json en: {mainProjectPath}");

    var configuration = new ConfigurationBuilder()
        .SetBasePath(mainProjectPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
      throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection' en appsettings.json");
    }

    builder.UseSqlServer(connectionString);

    return new ApplicationDbContext(builder.Options);
  }
}

