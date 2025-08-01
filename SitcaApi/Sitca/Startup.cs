using System;
using System.Collections.Generic;
using System.IO;
using FluentValidation;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Sitca.DataAccess.Data.Initializer;
using Sitca.DataAccess.Services;
using Sitca.DataAccess.Services.JobsService;
using Sitca.Extensions;
using Sitca.Validators;

namespace Sitca
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .ConfigureCors(Configuration)
                .ConfigureDatabase(Configuration) // Primero database.
                .ConfigureIdentity() // Luego identity.
                .ConfigureAuthentication(Configuration) // Luego autenticación.
                .ConfigureHangfire(Configuration)
                .ConfigureServices(Configuration) // Después los demás servicios.
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft
                        .Json
                        .ReferenceLoopHandling
                        .Ignore;
                    options.SerializerSettings.NullValueHandling = Newtonsoft
                        .Json
                        .NullValueHandling
                        .Ignore;
                })
                .AddRazorRuntimeCompilation()
                .AddControllersAsServices();

            services.Configure<FormOptions>(ConfigureFormOptions);
            services.AddValidatorsFromAssemblyContaining<LoginDtoValidator>();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddScoped<DbUserInitializer>();

            if (Environment.IsDevelopment())
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "SITCA API", Version = "v1" });
                    
                    // Configure JWT Authentication
                    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                        Description = "Please enter JWT with Bearer into field",
                        Name = "Authorization",
                        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });
                    
                    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                    {
                        {
                            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                            {
                                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                                {
                                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                        }
                    });
                });
            }

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IDbInitializer dbInit,
            IRecurringJobManager recurringJobManager,
            IServiceProvider serviceProvider
        )
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection()
                .UseStaticFiles()
                .UseStaticFiles(ConfigureStaticFiles())
                .UseRouting()
                .UseCors("cors")
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.ConfigureApplicationEndpoints(Configuration);
                    endpoints.ConfigureApiEndpoints();
                })
                .UseHangfireDashboard();
            ConfigureHangfireJobs(recurringJobManager, serviceProvider);
        }

        private static void ConfigureFormOptions(FormOptions options)
        {
            options.ValueLengthLimit = int.MaxValue;
            options.MultipartBodyLengthLimit = int.MaxValue;
            options.MemoryBufferThreshold = int.MaxValue;
        }

        private static StaticFileOptions ConfigureStaticFiles()
        {
            return new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "Resources")
                ),
                RequestPath = new PathString("/Resources"),
            };
        }

        private static void ConfigureHangfireJobs(
            IRecurringJobManager recurringJobManager,
            IServiceProvider serviceProvider
        )
        {
            recurringJobManager.AddOrUpdate(
                "NotificacionVencimiento",
                () => serviceProvider.GetService<IJobsServices>().EnviarRecordatorio(),
                Cron.Hourly
            );

            recurringJobManager.AddOrUpdate(
                "NotificacionVencimientoCarnets",
                () => serviceProvider.GetService<IJobsServices>().NotificarVencimientoCarnets(),
                Cron.Hourly
            );
        }
    }
}
