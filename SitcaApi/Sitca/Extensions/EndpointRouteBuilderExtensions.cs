using System;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace Sitca.Extensions;

public static class EndpointRouteBuilderExtensions
{
  /// <summary>
  /// Configures the application endpoints including default routes, Razor pages, and Hangfire dashboard
  /// </summary>
  /// <param name="endpoints">The endpoint route builder instance</param>
  /// <param name="configuration">The application configuration</param>
  /// <returns>The endpoint route builder instance</returns>
  /// <remarks>
  /// This method configures:
  /// - Default controller route pattern
  /// - Razor pages routing
  /// - Hangfire dashboard with authentication
  /// </remarks>
  public static IEndpointRouteBuilder ConfigureApplicationEndpoints(
      this IEndpointRouteBuilder endpoints,
      IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(endpoints, nameof(endpoints));
    ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

    // Configure default controller route
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Configure Razor Pages
    endpoints.MapRazorPages();

    // Configure Hangfire Dashboard
    endpoints.MapHangfireDashboard("/hangfire", new DashboardOptions
    {
      Authorization = new[]
        {
                new HangfireCustomBasicAuthenticationFilter
                {
                    User = configuration.GetSection("HangfireSettings:UserName").Value,
                    Pass = configuration.GetSection("HangfireSettings:Password").Value
                }
            },
      IgnoreAntiforgeryToken = true
    });

    return endpoints;
  }

  /// <summary>
  /// Configures API-specific endpoints and versioning
  /// </summary>
  /// <param name="endpoints">The endpoint route builder instance</param>
  /// <returns>The endpoint route builder instance</returns>
  /// <remarks>
  /// Use this method to configure API-specific routes and versioning.
  /// Can be extended to include additional API configuration as needed.
  /// </remarks>
  public static IEndpointRouteBuilder ConfigureApiEndpoints(
      this IEndpointRouteBuilder endpoints)
  {
    ArgumentNullException.ThrowIfNull(endpoints, nameof(endpoints));

    endpoints.MapControllers();

    return endpoints;
  }
}
