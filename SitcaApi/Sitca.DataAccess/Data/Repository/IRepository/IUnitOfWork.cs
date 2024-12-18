﻿using System;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
  public interface IUnitOfWork : IDisposable
  {
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

    void Save();
  }
}
