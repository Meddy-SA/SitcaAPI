using System;
using System.Text;
using Core.Services.Email;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Sitca.DataAccess.Data;
using Sitca.DataAccess.Data.Initializer;
using Sitca.DataAccess.Data.Repository;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Services.CompanyQuery;
using Sitca.DataAccess.Services.Cuestionarios;
using Sitca.DataAccess.Services.Dashboard;
using Sitca.DataAccess.Services.Email;
using Sitca.DataAccess.Services.EmpresaDeletion;
using Sitca.DataAccess.Services.ProcesosDeletion;
using Sitca.DataAccess.Services.Files;
using Sitca.DataAccess.Services.JobsService;
using Sitca.DataAccess.Services.Notification;
using Sitca.DataAccess.Services.Pdf;
using Sitca.DataAccess.Services.ProcessQuery;
using Sitca.DataAccess.Services.Token;
using Sitca.DataAccess.Services.Url;
using Sitca.DataAccess.Services.ViewToString;
using Sitca.Middlewares;
using Sitca.Models;

namespace Sitca.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null
                    );
                }
            )
            .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.MultipleCollectionIncludeWarning))
        );

        // Configuración de Dapper
        services.AddScoped<IDapper, Dapperr>();

        return services;
    }

    public static IServiceCollection ConfigureIdentity(this IServiceCollection services)
    {
        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Password settings
                options.Password = new PasswordOptions
                {
                    RequireDigit = false, // Requerir un número
                    RequiredLength = 6, // ✓ Longitud mínima
                    RequireUppercase = false, // Requiere mayúsculas
                    RequireLowercase = false, // Requiere minusculas
                    RequireNonAlphanumeric = false, // Caracteres especiales
                    RequiredUniqueChars = 1, // Mínimo de caracteres únicos
                };

                // Lockout settings
                options.Lockout = new LockoutOptions
                {
                    DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5),
                    MaxFailedAccessAttempts = 5,
                    AllowedForNewUsers = true,
                };

                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn = new SignInOptions
                {
                    RequireConfirmedEmail = false,
                    RequireConfirmedAccount = false,
                    RequireConfirmedPhoneNumber = false,
                };
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<LocalizedIdentityErrorDescriber>();

        return services;
    }

    public static IServiceCollection ConfigureAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                    ClockSkew = TimeSpan.Zero,
                };
            });

        services.AddScoped<IJWTTokenGenerator, JWTTokenGenerator>();

        return services;
    }

    public static IServiceCollection ConfigureHangfire(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHangfire(config =>
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseDefaultTypeSerializer()
                .UseMemoryStorage()
        );

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 2;
            options.Queues = new[] { "default", "notifications", "recurring" };
        });

        services.AddScoped<IJobsServices, JobsService>();

        return services;
    }

    public static IServiceCollection ConfigureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Email Service
        services.AddHttpClient("BrevoClient");
        services.Configure<Models.DTOs.EmailConfiguration>(configuration.GetSection("EmailSender"));
        services.AddTransient<IEmailSender, EmailSender>();

        // Python API HttpClient
        services.AddHttpClient(
            "PythonApi",
            client =>
            {
                // Configuración base del cliente HTTP
                var pythonApiUrl = configuration["CorsSettings:pythonURL"];
                if (!string.IsNullOrEmpty(pythonApiUrl))
                {
                    // Asegurar que la URL incluya la ruta base de la API y termine con /
                    if (!pythonApiUrl.EndsWith("/"))
                    {
                        pythonApiUrl += "/";
                    }

                    client.BaseAddress = new Uri(pythonApiUrl);
                }

                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
                );
                client.Timeout = TimeSpan.FromSeconds(30);
            }
        );

        // URLs
        services.AddScoped<IUrlService, UrlService>();

        // Notifications
        services.AddScoped<INotificationService, NotificationService>();

        // PDF Services
        services.AddScoped<IReportService, ITextReportService>();

        // View Render Service
        services.AddScoped<IViewRenderService, ViewRenderService>();

        // DB Initializer
        services.AddScoped<IDbInitializer, DbInitializer>();

        // Configurar Cuestionario
        services.AddScoped<ICuestionarioReaperturaService, CuestionarioReaperturaService>();

        // Servicio de Company
        services.AddScoped<ICompanyQueryBuilder, CompanyQueryBuilder>();

        // Servicio de Procesos
        services.AddScoped<IProcessQueryBuilder, ProcessQueryBuilder>();
        // Servicio de Eliminación de Empresas
        services.AddScoped<IEmpresaDeletionService, EmpresaDeletionService>();
        // Servicio de Eliminación de Procesos
        services.AddScoped<IProcesosDeletionService, ProcesosDeletionService>();

        // Servicios de Manejo de Archivos.
        services.AddScoped<IFileService, FileService>();

        // Servicio Manejo de Iconos.
        services.AddSingleton<IIconService, IconService>();

        // Dashboard Services
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IRoleDashboardService, RoleDashboardService>();

        return services;
    }

    public static IServiceCollection ConfigureCors(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var allowedOrigins = configuration
            .GetSection("CorsSettings:AllowedOrigins")
            .Get<string[]>();

        services.AddCors(options =>
        {
            options.AddPolicy(
                "cors",
                builder =>
                {
                    builder
                        .WithOrigins(allowedOrigins ?? Array.Empty<string>())
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .SetIsOriginAllowedToAllowWildcardSubdomains();
                }
            );
        });

        return services;
    }
}
