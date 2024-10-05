using System;
using System.Collections.Generic;
using System.Text;
using Core.Services.Email;
using Microsoft.Extensions.Configuration;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Services.ViewToString;

namespace Sitca.DataAccess.Data.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        private readonly IDapper _dapper;
        private readonly IEmailSender _emailSender;
        private readonly IViewRenderService _viewRenderService;
        private readonly IConfiguration _config;

        public UnitOfWork(ApplicationDbContext db, IDapper dapper, IEmailSender emailSender, IViewRenderService viewRenderService, IConfiguration config)
        {
            _db = db;
            _dapper = dapper;
            _emailSender = emailSender;
            _viewRenderService = viewRenderService;
            _config = config;
            ItemTemplate = new ItemTemplateRepository(_db);
            Empresa = new EmpresaRepository(_db);
            Modulo = new ModulosRepository(_db);
            Pregunta = new PreguntasRepository(_db);
            Archivo = new ArchivoRepository(_db);

            Notificacion = new NotificationRepository(_db, _dapper, _emailSender, _viewRenderService,config);
            ProcesoCertificacion = new CertificacionRepository(_db, _config);
            Reportes = new ReportesRepository(_db);
            Users = new UsersRepository(_db, _dapper);
            Capacitaciones = new CapacitacionesRepository(_db);
            CompañiasAuditoras = new CompañiasAuditorasRepository(_db);
            Tipologias = new TipologiaRepository(_db);
            Homologacion = new HomologacionRepository(_db);
        }

        public IItemTemplateRepository ItemTemplate { get; private set; }

        public IEmpresaRepository Empresa { get; private set; }

        public IModuloRepository Modulo { get; private set; }

        public IPreguntasRepository Pregunta { get; private set; }

        public IArchivoRepository Archivo { get; private set; }

        public IUsersRepository Users { get; private set; }

        public ICertificacionRepository ProcesoCertificacion { get; private set; }

        public IReporteRepository Reportes { get; private set; }
        public INotificationRepository Notificacion { get; private set; }

        public ICapacitacionesRepository Capacitaciones { get; private set; }

        public ICompañiasAuditorasRepository CompañiasAuditoras { get; private set; }

        public ITipologiaRepository Tipologias { get; private set; }

        public IHomologacionRepository Homologacion { get; private set; }

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
