using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository.IRepository;

public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    // Repositorios existentes
    IItemTemplateRepository ItemTemplate { get; }
    IEmpresaRepository Empresa { get; }
    IModuloRepository Modulo { get; }
    IPreguntasRepository Pregunta { get; }
    IArchivoRepository Archivo { get; }
    IUsersRepository Users { get; }
    ICertificacionRepository ProcesoCertificacion { get; }
    IReporteRepository Reportes { get; }
    ICapacitacionesRepository Capacitaciones { get; }
    ICompañiasAuditorasRepository CompañiasAuditoras { get; }
    ITipologiaRepository Tipologias { get; }
    IHomologacionRepository Homologacion { get; }
    IAuthRepository Auth { get; }
    IProcesoRepository Proceso { get; }
    IEmpresasRepository Empresas { get; }
    IProcesoArchivosRepository ProcesoArchivos { get; }
    IEmpresaReportRepository EmpresaReport { get; }
    IAuthenticationRepository Authentication { get; }
    ICrossCountryAuditRequestRepository CrossCountryAuditRequest { get; }

    // Métodos síncronos
    int SaveChanges();
    int SaveChanges(bool acceptAllChangesOnSuccess);

    // Métodos asíncronos
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default
    );

    // Método para operaciones con ejecución resiliente
    Task<TResult> ExecuteWithResiliencyAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default
    );
}
