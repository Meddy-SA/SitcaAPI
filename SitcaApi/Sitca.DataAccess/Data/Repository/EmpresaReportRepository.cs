using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Extensions;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.Enums;
using Sitca.Models.ViewModels;

namespace Sitca.DataAccess.Data.Repository;

public class EmpresaReportRepository : IEmpresaReportRepository
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<EmpresaReportRepository> _logger;

    public EmpresaReportRepository(ApplicationDbContext db, ILogger<EmpresaReportRepository> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<EmpresaReportResponseDTO>> GetEmpresaReportAsync(
        EmpresaReportFilterDTO filter
    )
    {
        try
        {
            // Validación del filtro
            filter ??= new EmpresaReportFilterDTO();
            filter.Language ??= "es";

            // Consulta base - Procesos de certificación con sus empresas y relaciones
            var query = _db
                .ProcesoCertificacion.AsNoTracking()
                .Include(p => p.Empresa)
                .ThenInclude(e => e.Pais)
                .Include(p => p.Empresa)
                .ThenInclude(e => e.Tipologias)
                .ThenInclude(t => t.Tipologia)
                .Include(p => p.Resultados)
                .ThenInclude(r => r.Distintivo)
                .Include(p => p.ProcesosArchivos.Where(pa => pa.Enabled))
                .Include(p => p.Tipologia)
                .Where(p => p.Enabled) // Solo procesos activos
                .AsSplitQuery();

            // Aplicar filtros
            if (filter.CountryIds != null && filter.CountryIds.Any())
            {
                query = query.Where(p => filter.CountryIds.Contains(p.Empresa.PaisId ?? 0));
            }

            if (filter.TypologyIds != null && filter.TypologyIds.Any())
            {
                query = query.Where(p =>
                    p.Empresa.Tipologias.Any(t => filter.TypologyIds.Contains(t.IdTipologia))
                );
            }

            // Para StatusIds filtramos por el patrón de texto directamente
            if (filter.StatusIds != null && filter.StatusIds.Any())
            {
                // Construimos una lista de prefijos de estado para buscar
                // Por ejemplo, si StatusIds = [0, 1], buscaremos Status que empiecen con "0 -" o "1 -"
                var statusPrefixes = filter.StatusIds.Select(id => $"{id} -").ToList();

                query = query.Where(p =>
                    statusPrefixes.Any(prefix => p.Status.StartsWith(prefix))
                );
            }

            // Para CertificationTypes distinguimos entre Certificación y Homologación
            if (filter.CertificationTypes != null && filter.CertificationTypes.Any())
            {
                bool incluirCertificacion = filter.CertificationTypes.Contains("certificacion");
                bool incluirHomologacion = filter.CertificationTypes.Contains("homologacion");

                if (incluirCertificacion && !incluirHomologacion)
                {
                    query = query.Where(p => !p.Empresa.EsHomologacion);
                }
                else if (!incluirCertificacion && incluirHomologacion)
                {
                    query = query.Where(p => p.Empresa.EsHomologacion);
                }
                // Si ambos están incluidos o ninguno, no aplicamos filtro
            }

            // FILTRO: Solo procesos con protocolo (cuando IncludeWithoutProtocol = false)
            if (filter.IncludeWithoutProtocol == false)
            {
                query = query.Where(p =>
                    p.ProcesosArchivos.Any(pa =>
                        pa.Enabled && pa.FileTypesCompany == FileCompany.Adhesion
                    )
                );
            }
            // Si IncludeWithoutProtocol es true o null, no se aplica filtro (incluye todos)

            // Manejar el filtro de distintivos y "En Proceso"
            bool incluirDistintivos =
                filter.DistintivoIds != null && filter.DistintivoIds.Any(d => d != -1);
            bool incluirEnProceso =
                filter.DistintivoIds != null && filter.DistintivoIds.Contains(-1);

            if (incluirDistintivos || incluirEnProceso)
            {
                // Construir la expresión con operadores OR para combinar las condiciones
                var distintivosParaFiltrar = filter.DistintivoIds?.Where(d => d != -1).ToList();

                query = query.Where(p =>
                    // Condición 1: Procesos con los distintivos seleccionados
                    (
                        incluirDistintivos
                        && p.Resultados.Any(r =>
                            distintivosParaFiltrar.Contains(r.DistintivoId ?? 0)
                        )
                    )
                    ||
                    // Condición 2: Procesos en trámite (sin distintivo)
                    (
                        incluirEnProceso
                        && !p.Resultados.Any()
                    )
                );
            }

            // Contar total de registros para paginación
            var totalItems = await query.CountAsync();
            var totalActives = await query.CountAsync(p => p.Empresa.Active);
            var totalInactives = totalItems - totalActives;

            // Calcular bloques para paginación
            int blockSize = Math.Max(1, filter.BlockSize);
            int blockNumber = Math.Max(1, filter.BlockNumber);
            int totalBlocks = (int)Math.Ceiling(totalItems / (double)blockSize);

            // Aplicar paginación
            var procesos = await query
                .OrderBy(p => p.Empresa.Nombre)
                .ThenByDescending(p => p.FechaInicio)
                .Skip((blockNumber - 1) * blockSize)
                .Take(blockSize)
                .ToListAsync();

            // Obtener todos los procesos sin paginación para calcular conteos
            var todosProcesos = await query.ToListAsync();
            // Conteo de procesos por distintivo
            var distintivosCount = new Dictionary<int, int>();
            var distintivosNameCount = new Dictionary<string, int>();

            // Obtener todos los distintivos para tener sus nombres
            var distintivos = await _db
                .Distintivo.Where(d => d.Activo)
                .ToDictionaryAsync(
                    d => d.Id,
                    d => filter.Language == "es" ? d.Name : d.NameEnglish
                );

            // Inicializar contadores en cero para todos los distintivos
            foreach (var distintivo in distintivos)
            {
                distintivosCount[distintivo.Key] = 0;
                distintivosNameCount[distintivo.Value] = 0;
            }

            // Calcular contadores de distintivos
            foreach (var proceso in todosProcesos)
            {
                var ultimoResultado = proceso.Resultados
                    .OrderByDescending(r => r.Id)
                    .FirstOrDefault();
                    
                if (ultimoResultado?.DistintivoId != null && distintivos.ContainsKey(ultimoResultado.DistintivoId.Value))
                {
                    distintivosCount[ultimoResultado.DistintivoId.Value]++;
                    var nombreDistintivo = distintivos[ultimoResultado.DistintivoId.Value];
                    distintivosNameCount[nombreDistintivo]++;
                }
            }
            
            // Contar procesos en trámite
            var totalEnProceso = todosProcesos.Count(p => !p.Resultados.Any());
            
            // Contar empresas únicas
            var empresasUnicas = todosProcesos.Select(p => p.EmpresaId).Distinct().Count();

            // Mapear resultados
            var result = new EmpresaReportResponseDTO
            {
                Items = MapProcesosToReportItems(procesos, filter.Language),
                TotalCount = totalItems,
                TotalActive = totalActives,
                TotalInactive = totalInactives,
                TotalEnProceso = totalEnProceso,
                DistintivosCount = distintivosCount,
                DistintivosNameCount = distintivosNameCount,
                totalUniqueCompanies = empresasUnicas,
                CurrentBlock = blockNumber,
                TotalBlocks = totalBlocks,
                BlockSize = blockSize,
            };

            return Result<EmpresaReportResponseDTO>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener el reporte de empresas con filtro {@Filter}",
                filter
            );
            return Result<EmpresaReportResponseDTO>.Failure(
                $"Error interno al generar reporte: {ex.Message}"
            );
        }
    }

    public async Task<Result<MetadatosDTO>> GetReportMetadataAsync(string language = "es")
    {
        try
        {
            var result = new MetadatosDTO
            {
                Paises = await _db
                    .Pais.Where(p => p.Active)
                    .OrderBy(p => p.Name)
                    .Select(p => new CommonVm
                    {
                        id = p.Id,
                        name = p.Name,
                        isSelected = false,
                    })
                    .ToListAsync(),

                Tipologias = await _db
                    .Tipologia.Where(t => t.Active)
                    .OrderBy(t => language == "es" ? t.Name : t.NameEnglish)
                    .Select(t => new CommonVm
                    {
                        id = t.Id,
                        name = language == "es" ? t.Name : t.NameEnglish,
                        isSelected = false,
                    })
                    .ToListAsync(),

                Estados = Enum.GetValues(typeof(CertificationStatus))
                    .Cast<CertificationStatus>()
                    .Select(s => new CommonVm
                    {
                        id = (int)s,
                        name = s.ToLocalizedString(language),
                        isSelected = false,
                    })
                    .ToList(),

                Distintivos = await _db
                    .Distintivo.Where(d => d.Activo)
                    .OrderBy(d => language == "es" ? d.Name : d.NameEnglish)
                    .Select(d => new CommonVm
                    {
                        id = d.Id,
                        name = language == "es" ? d.Name : d.NameEnglish,
                        isSelected = false,
                    })
                    .ToListAsync(),
            };

            // Añadir la opción "En Proceso" para los distintivos
            result.Distintivos.Add(
                new CommonVm
                {
                    id = -1,
                    name = language == "es" ? "En Proceso" : "In Process",
                    isSelected = false,
                }
            );

            return Result<MetadatosDTO>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener metadatos para el reporte de empresas");
            return Result<MetadatosDTO>.Failure($"Error al obtener metadatos: {ex.Message}");
        }
    }

    private List<EmpresaReportItemDTO> MapProcesosToReportItems(
        List<ProcesoCertificacion> procesos,
        string language
    )
    {
        var result = new List<EmpresaReportItemDTO>();

        foreach (var proceso in procesos)
        {
            var empresa = proceso.Empresa;
            
            // Mapear las tipologías de la empresa
            var tipologias = empresa
                .Tipologias.Select(t =>
                    language == "es" ? t.Tipologia.Name : t.Tipologia.NameEnglish
                )
                .ToList();

            var tipologiasIds = empresa.Tipologias.Select(t => t.IdTipologia).ToList();

            // Obtener el último resultado del proceso
            var ultimoResultado = proceso.Resultados
                .OrderByDescending(r => r.Id)
                .FirstOrDefault();

            // Extraer estado del proceso
            int estadoId = 0;
            var statusParts = proceso.Status.Split('-');
            if (statusParts.Length > 0 && int.TryParse(statusParts[0].Trim(), out int parsedId))
            {
                estadoId = parsedId;
            }

            string estado;
            if (Enum.IsDefined(typeof(CertificationStatus), estadoId))
            {
                var estadoEnum = (CertificationStatus)estadoId;
                estado = estadoEnum.ToLocalizedString(language);
            }
            else
            {
                estado = proceso.Status;
            }

            // Crear ítem del proceso
            var item = new EmpresaReportItemDTO
            {
                // Datos de la empresa
                EmpresaId = empresa.Id,
                NombreEmpresa = empresa.Nombre,
                Pais = empresa.Pais?.Name ?? string.Empty,
                PaisId = empresa.PaisId ?? 0,
                Responsable = empresa.NombreRepresentante,
                TipologiasList = tipologias,
                TipologiasIds = tipologiasIds,
                Tipologias = string.Join(", ", tipologias),
                EmpresaActiva = empresa.Active,
                
                // Datos del proceso
                ProcesoId = proceso.Id,
                Estado = estado,
                EstadoId = estadoId,
                TipoCertificacion = empresa.EsHomologacion ? "Homologación" : "Certificación",
                FechaInicioProceso = proceso.FechaInicio,
                FechaFinProceso = proceso.FechaFinalizacion,
                NumeroExpediente = proceso.NumeroExpediente,
                FechaAuditoria = proceso.FechaSolicitudAuditoria,
                FechaAuditoriaFin = proceso.FechaFijadaAuditoria,
                
                // Distintivo del proceso
                EnProceso = ultimoResultado == null || !ultimoResultado.DistintivoId.HasValue,
            };

            // Establecer información del distintivo si existe
            if (ultimoResultado?.DistintivoId.HasValue == true && ultimoResultado.Distintivo != null)
            {
                item.Distintivo = language == "es" 
                    ? ultimoResultado.Distintivo.Name 
                    : ultimoResultado.Distintivo.NameEnglish;
                item.DistintivoId = ultimoResultado.DistintivoId.Value;
                item.FechaVencimientoDistintivo = proceso.FechaVencimiento;
            }

            result.Add(item);
        }

        return result;
    }
}

// Helper para convertir status de texto a int
public static class StatusConverter
{
    public static int ConvertStatusTextToInt(string status)
    {
        if (string.IsNullOrEmpty(status))
            return 0;

        // El formato esperado es: "0 - Inicial", "1 - Para Asesorar", etc.
        var parts = status.Split('-');
        if (parts.Length > 0 && int.TryParse(parts[0].Trim(), out int result))
        {
            return result;
        }

        return 0;
    }
}
