using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(Sitca.Areas.Identity.IdentityHostingStartup))]
namespace Sitca.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}
