using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sitca.DataAccess.Services;

namespace Sitca
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      var host = CreateHostBuilder(args).Build();

      // Inicializar usuarios
      using (var scope = host.Services.CreateScope())
      {
        var services = scope.ServiceProvider;
        var dbUserInitializer = services.GetRequiredService<DbUserInitializer>();
        await dbUserInitializer.InitializeUsersAsync();
      }

      await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
              webBuilder.UseStartup<Startup>();
            });
  }
}
