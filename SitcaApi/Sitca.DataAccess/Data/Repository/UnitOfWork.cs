using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Services.CompanyQuery;
using Sitca.DataAccess.Services.Cuestionarios;
using Sitca.DataAccess.Services.Files;
using Sitca.DataAccess.Services.Notification;
using Sitca.DataAccess.Services.Token;
using Sitca.DataAccess.Services.ViewToString;
using Sitca.Models;

namespace Sitca.DataAccess.Data.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;
    private readonly IDapper _dapper;
    private readonly IEmailSender _emailSender;
    private readonly IFileService _fileService;
    private readonly INotificationService _notificationService;
    private readonly ICompanyQueryBuilder _queryBuilder;
    private readonly IViewRenderService _viewRenderService;
    private readonly IConfiguration _config;
    private readonly IJWTTokenGenerator _jwtToken;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICuestionarioReaperturaService _cuestionarioService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<UnitOfWork> _logger;
    private bool _disposed;

    public UnitOfWork(
        ApplicationDbContext db,
        IDapper dapper,
        IEmailSender emailSender,
        IFileService fileService,
        INotificationService notificationService,
        ICompanyQueryBuilder queryBuilder,
        IViewRenderService viewRenderService,
        IConfiguration config,
        IJWTTokenGenerator jwtToken,
        UserManager<ApplicationUser> userManager,
        ICuestionarioReaperturaService cuestionarioService,
        ILoggerFactory loggerFactory
    )
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _dapper = dapper ?? throw new ArgumentNullException(nameof(dapper));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _notificationService =
            notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _queryBuilder = queryBuilder ?? throw new ArgumentNullException(nameof(queryBuilder));
        _viewRenderService =
            viewRenderService ?? throw new ArgumentNullException(nameof(viewRenderService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _jwtToken = jwtToken ?? throw new ArgumentNullException(nameof(jwtToken));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _cuestionarioService =
            cuestionarioService ?? throw new ArgumentNullException(nameof(cuestionarioService));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = _loggerFactory.CreateLogger<UnitOfWork>();

        InitializeRepositories();
    }

    // Inicialización de los repositorios
    private void InitializeRepositories()
    {
        ItemTemplate = new ItemTemplateRepository(_db);
        Empresa = new EmpresaRepository(
            _db,
            _notificationService,
            _queryBuilder,
            _loggerFactory.CreateLogger<EmpresaRepository>()
        );
        Auth = new AuthRepository(
            _userManager,
            _jwtToken,
            _emailSender,
            _config,
            _viewRenderService,
            Empresa,
            _loggerFactory.CreateLogger<AuthRepository>()
        );
        Modulo = new ModulosRepository(_db);
        Pregunta = new PreguntasRepository(_db);
        Archivo = new ArchivoRepository(
            _db,
            _userManager,
            _loggerFactory.CreateLogger<ArchivoRepository>(),
            _fileService
        );
        ProcesoCertificacion = new CertificacionRepository(
            _db,
            _config,
            _cuestionarioService,
            _loggerFactory.CreateLogger<CertificacionRepository>()
        );
        Reportes = new ReportesRepository(_db);
        Users = new UsersRepository(_db, _dapper, _loggerFactory.CreateLogger<UsersRepository>());
        Capacitaciones = new CapacitacionesRepository(_db);
        CompañiasAuditoras = new CompañiasAuditorasRepository(_db);
        Tipologias = new TipologiaRepository(_db);
        Homologacion = new HomologacionRepository(
            _db,
            _loggerFactory.CreateLogger<HomologacionRepository>()
        );
        Proceso = new ProcesoRepository(
            _db,
            _config,
            _loggerFactory.CreateLogger<ProcesoRepository>()
        );
        Empresas = new EmpresasRepository(
            _db,
            _notificationService,
            _queryBuilder,
            _loggerFactory.CreateLogger<EmpresasRepository>()
        );
        ProcesoArchivos = new ProcesoArchivosRepository(
            _db,
            _fileService,
            _config,
            _loggerFactory.CreateLogger<ProcesoArchivosRepository>()
        );
    }

    // Propiedades Repositorios
    public IItemTemplateRepository ItemTemplate { get; private set; }
    public IEmpresaRepository Empresa { get; private set; }
    public IModuloRepository Modulo { get; private set; }
    public IPreguntasRepository Pregunta { get; private set; }
    public IArchivoRepository Archivo { get; private set; }
    public IUsersRepository Users { get; private set; }
    public ICertificacionRepository ProcesoCertificacion { get; private set; }
    public IReporteRepository Reportes { get; private set; }
    public ICapacitacionesRepository Capacitaciones { get; private set; }
    public ICompañiasAuditorasRepository CompañiasAuditoras { get; private set; }
    public ITipologiaRepository Tipologias { get; private set; }
    public IHomologacionRepository Homologacion { get; private set; }
    public IAuthRepository Auth { get; private set; }
    public IProcesoRepository Proceso { get; private set; }
    public IEmpresasRepository Empresas { get; private set; }
    public IProcesoArchivosRepository ProcesoArchivos { get; private set; }

    // Implementación de IUnitOfWork
    public int SaveChanges() => _db.SaveChanges();

    public int SaveChanges(bool acceptAllChangesOnSuccess) =>
        _db.SaveChanges(acceptAllChangesOnSuccess);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _db.SaveChangesAsync(cancellationToken);

    public async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default
    ) => await _db.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

    // Implementación del patrón Strategy para operaciones resilientes
    public async Task<TResult> ExecuteWithResiliencyAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default
    )
    {
        var strategy = _db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing operation with resiliency strategy");
                throw;
            }
        });
    }

    // Para mantener compatibilidad con métodos legacy
    public void Save() => SaveChanges();

    // Implementación de IDisposable
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Implementación de IAsyncDisposable
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            _disposed = true;
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_db is not null)
        {
            await _db.DisposeAsync();
        }
    }
}
