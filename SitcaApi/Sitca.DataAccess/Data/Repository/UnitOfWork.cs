using Core.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Services.Cuestionarios;
using Sitca.DataAccess.Services.Notification;
using Sitca.DataAccess.Services.Token;
using Sitca.DataAccess.Services.ViewToString;
using Sitca.Models;

namespace Sitca.DataAccess.Data.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        private readonly IDapper _dapper;
        private readonly IEmailSender _emailSender;
        private readonly INotificationService _notificationService;
        private readonly IViewRenderService _viewRenderService;
        private readonly IConfiguration _config;
        private readonly IJWTTokenGenerator _jwtToken;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICuestionarioReaperturaService _cuestionarioService;

        private readonly ILoggerFactory _loggerFactory;
        public UnitOfWork(
            ApplicationDbContext db,
            IDapper dapper,
            IEmailSender emailSender,
            INotificationService notificationService,
            IViewRenderService viewRenderService,
            IConfiguration config,
            IJWTTokenGenerator jwtToken,
            UserManager<ApplicationUser> userManager,
            ICuestionarioReaperturaService cuestionarioService,
            ILoggerFactory loggerFactory)
        {
            _db = db;
            _dapper = dapper;
            _emailSender = emailSender;
            _notificationService = notificationService;
            _viewRenderService = viewRenderService;
            _config = config;
            _jwtToken = jwtToken;
            _userManager = userManager;
            _cuestionarioService = cuestionarioService;
            _loggerFactory = loggerFactory;

            ItemTemplate = new ItemTemplateRepository(_db);
            Empresa = new EmpresaRepository(
                _db,
                _notificationService,
                _loggerFactory.CreateLogger<EmpresaRepository>()
            );
            Auth = new AuthRepository(
                _userManager,
                _jwtToken,
                _emailSender,
                _config,
                _viewRenderService,
                _loggerFactory.CreateLogger<AuthRepository>()
            );
            Modulo = new ModulosRepository(_db);
            Pregunta = new PreguntasRepository(_db);
            Archivo = new ArchivoRepository(_db);

            ProcesoCertificacion = new CertificacionRepository(
                _db,
                _config,
                _cuestionarioService,
                _loggerFactory.CreateLogger<CertificacionRepository>()
            );
            Reportes = new ReportesRepository(_db);
            Users = new UsersRepository(
                _db,
                _dapper,
                _loggerFactory.CreateLogger<UsersRepository>()
            );
            Capacitaciones = new CapacitacionesRepository(_db);
            CompañiasAuditoras = new CompañiasAuditorasRepository(_db);
            Tipologias = new TipologiaRepository(_db);
            Homologacion = new HomologacionRepository(
                _db,
                _loggerFactory.CreateLogger<HomologacionRepository>()
            );
        }

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

        public void Dispose()
        {
            _db.Dispose();
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
