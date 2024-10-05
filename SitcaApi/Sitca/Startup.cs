using Sitca.DataAccess.Data;
using Sitca.DataAccess.Data.Initializer;
using Sitca.DataAccess.Data.Repository;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Services.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitca.Areas.Identity;
using DinkToPdf.Contracts;
using DinkToPdf;
using Sitca.DataAccess.Services.Pdf;
using Sitca.DataAccess.Services.ViewToString;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using System.IO;
using Hangfire;
using Hangfire.MemoryStorage;
using Sitca.DataAccess.Services.JobsService;
using HangfireBasicAuthenticationFilter;
using Sitca.Middlewares;

namespace Sitca
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;

    }

    public IConfiguration Configuration { get; }



    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {

      services.AddCors(options =>
      {
        options.AddPolicy("CorsPolicy",
                  builder => builder.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader());
      });

      #region HANGFIRE
      services.AddHangfire(config => config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
      .UseSimpleAssemblyNameTypeSerializer()
      .UseDefaultTypeSerializer()
      .UseMemoryStorage()
      );

      services.AddHangfireServer();

      services.AddScoped<IJobsServices, JobsService>();

      #endregion


      services.AddDbContext<ApplicationDbContext>(options =>
          options.UseSqlServer(
              Configuration.GetConnectionString("DefaultConnection")));

      services.Configure<FormOptions>(o =>
      {
        o.ValueLengthLimit = int.MaxValue;
        o.MultipartBodyLengthLimit = int.MaxValue;
        o.MemoryBufferThreshold = int.MaxValue;
      });

      #region Identity
      //no compatible con JWT
      //19. cambiamos el identity default por custom para poder modificar
      //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
      //    .AddEntityFrameworkStores<ApplicationDbContext>();

      services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

      services.Configure<IdentityOptions>(options =>
      {
        //https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-5.0
        // Default Lockout settings.
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;

        // Password settings.
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;

        options.User.RequireUniqueEmail = true;

        //options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        //options.Cookie.Name = "YourAppCookieName";
        //options.Cookie.HttpOnly = true;
        //options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        //options.LoginPath = "/Identity/Account/Login";
        //// ReturnUrlParameter requires 
        ////using Microsoft.AspNetCore.Authentication.Cookies;
        //options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
        //options.SlidingExpiration = true;
      });

      //Register dapper in scope    
      services.AddScoped<IDapper, Dapperr>();

      //services.AddTransient<IEmailSender, EmailSender>(); //no funciona con la sobrecarga

      //services.AddTransient<IEmailSender,DataAccess.Services.Email.EmailSender>(i =>
      //    new DataAccess.Services.Email.EmailSender(
      //         Configuration["EmailSender:Host"],
      //         Configuration.GetValue<int>("EmailSender:Port"),
      //         Configuration.GetValue<bool>("EmailSender:EnableSSL"),
      //         Configuration["EmailSender:UserName"],
      //             Configuration["EmailSender:Password"]
      //    )
      //);

      services.AddTransient<Core.Services.Email.IEmailSender, DataAccess.Services.Email.EmailSender>(i =>
           new DataAccess.Services.Email.EmailSender(
                Configuration
           )
      );


      #endregion


      #region JWT
      //no compatible con login con identity
      services.AddAuthentication(opt =>
      {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(options =>
      {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Token:Key"])),
          ValidIssuer = Configuration["Token:Issuer"],
          ValidateIssuer = true,
          ValidateAudience = false,
        };
      });

      services.AddScoped<IJWTTokenGenerator, JWTTokenGenerator>();

      services.AddAuthorization();

      #endregion


      #region repos
      services.AddScoped<IUnitOfWork, UnitOfWork>();
      #endregion


      //DbInitializer
      services.AddScoped<IDbInitializer, DbInitializer>();

      //services.AddControllersWithViews();
      services.AddControllersWithViews().AddNewtonsoftJson().AddRazorRuntimeCompilation().AddControllersAsServices();


      services.AddRazorPages();

      //services.AddCors();






      //pdf 
      services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
      services.AddScoped<IReportService, ReportService>();

      //view to string
      services.AddScoped<IViewRenderService, ViewRenderService>();

      //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-5.0
      //services.AddAuthorization(options =>
      //{
      //    options.FallbackPolicy = new AuthorizationPolicyBuilder()
      //        .RequireAuthenticatedUser()
      //        .Build();
      //});







      //21?
      //services.ConfigureApplicationCookie(options =>
      //{
      //    options.LoginPath = $"/Identity/Account/Login";

      //    options.LogoutPath = $"/Identity/Account/Logout";

      //    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";

      //});


    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbInitializer dbInit, IRecurringJobManager recurringJobManager, IServiceProvider serviceProvider)
    {
      //app.UseOptions();

      //cors angular App
      app.UseCors(options =>
      options
      //.WithOrigins(origins)
      .AllowAnyOrigin()
      .AllowAnyMethod()
      .AllowAnyHeader());

      app.UseDeveloperExceptionPage();
      app.UseDatabaseErrorPage();

      if (env.IsDevelopment())
      {

      }
      else
      {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }
      app.UseHttpsRedirection();
      app.UseStaticFiles();
      app.UseStaticFiles(new StaticFileOptions()
      {
        FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Resources")),
        RequestPath = new PathString("/Resources")
      });


      //app.UseStaticFiles(new StaticFileOptions
      //{
      //    FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "Resources")),
      //    RequestPath = "/Resources"
      //});


      //app.UseSqlServer(ConnectionString, x => x.UseNetTopologySuite();

      var origins = new string[] { "http://localhost:4200", "https://web.postman.co/" };


      app.UseRouting();

      //Agrega automaticamente roles y user Admin
      // dbInit.Initialize();

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllerRoute(
                  name: "default",
                  pattern: "{controller=Home}/{action=Index}/{id?}");
        endpoints.MapRazorPages();
        endpoints.MapHangfireDashboard("/hangfire", new DashboardOptions
        {
          Authorization = new[]
                  {
                        new HangfireCustomBasicAuthenticationFilter{
                            User = Configuration.GetSection("HangfireSettings:UserName").Value,
                            Pass = Configuration.GetSection("HangfireSettings:Password").Value
                        }
              },
          IgnoreAntiforgeryToken = true,

        });
      });

      #region hangfire
      app.UseHangfireDashboard();

      //recurringJobManager.Enqueue(() => _unitOfWork.Empresa.EnviarRecordatorio());
      //recurringJobManager.AddOrUpdate("test", () => _unitOfWork.Empresa.EnviarRecordatorio(), Cron.Minutely);


      recurringJobManager.AddOrUpdate("NotificacionVencimiento", () => serviceProvider.GetService<IJobsServices>().EnviarRecordatorio(), Cron.Hourly);

      recurringJobManager.AddOrUpdate("NotificacionVencimientoCarnets", () => serviceProvider.GetService<IJobsServices>().NotificarVencimientoCarnets(), Cron.Hourly);

      #endregion

    }
  }
}
