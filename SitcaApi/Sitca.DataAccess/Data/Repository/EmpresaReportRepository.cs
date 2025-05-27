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

            // Consulta base - Empresas con sus procesos
            var query = _db
                .Empresa.AsNoTracking()
                .Include(e => e.Pais)
                .Include(e => e.Tipologias)
                .ThenInclude(t => t.Tipologia)
                .Include(e => e.Certificaciones.Where(c => c.Enabled))
                .ThenInclude(c => c.Resultados)
                .ThenInclude(r => r.Distintivo)
                .Include(e => e.Certificaciones.Where(c => c.Enabled))
                .ThenInclude(c =>
                    c.ProcesosArchivos.Where(pa =>
                        pa.Enabled && pa.FileTypesCompany == FileCompany.Adhesion
                    )
                )
                .AsSplitQuery();

            // Aplicar filtros
            if (filter.CountryIds != null && filter.CountryIds.Any())
            {
                query = query.Where(e => filter.CountryIds.Contains(e.PaisId ?? 0));
            }

            if (filter.TypologyIds != null && filter.TypologyIds.Any())
            {
                query = query.Where(e =>
                    e.Tipologias.Any(t => filter.TypologyIds.Contains(t.IdTipologia))
                );
            }

            // Para StatusIds filtramos por el patrón de texto directamente
            if (filter.StatusIds != null && filter.StatusIds.Any())
            {
                // Construimos una lista de prefijos de estado para buscar
                // Por ejemplo, si StatusIds = [0, 1], buscaremos Status que empiecen con "0 -" o "1 -"
                var statusPrefixes = filter.StatusIds.Select(id => $"{id} -").ToList();

                query = query.Where(e =>
                    e.Certificaciones.Any(c =>
                        statusPrefixes.Any(prefix => c.Status.StartsWith(prefix))
                    )
                );
            }

            // Para CertificationTypes distinguimos entre Certificación y Homologación
            if (filter.CertificationTypes != null && filter.CertificationTypes.Any())
            {
                bool incluirCertificacion = filter.CertificationTypes.Contains("certificacion");
                bool incluirHomologacion = filter.CertificationTypes.Contains("homologacion");

                if (incluirCertificacion && !incluirHomologacion)
                {
                    query = query.Where(e => !e.EsHomologacion);
                }
                else if (!incluirCertificacion && incluirHomologacion)
                {
                    query = query.Where(e => e.EsHomologacion);
                }
                // Si ambos están incluidos o ninguno, no aplicamos filtro
            }

            // FILTRO: Solo empresas con protocolo (cuando IncludeWithoutProtocol = false)
            if (filter.IncludeWithoutProtocol == false)
            {
                query = query.Where(e =>
                    e.Certificaciones.Any(c =>
                        c.Enabled
                        && c.ProcesosArchivos.Any(pa =>
                            pa.Enabled && pa.FileTypesCompany == FileCompany.Adhesion
                        )
                    )
                );
            }
            // Si IncludeWithoutProtocol es true o null, no se aplica filtro (incluye todas)

            // Manejar el filtro de distintivos y "En Proceso"
            bool incluirDistintivos =
                filter.DistintivoIds != null && filter.DistintivoIds.Any(d => d != -1);
            bool incluirEnProceso =
                filter.DistintivoIds != null && filter.DistintivoIds.Contains(-1);

            if (incluirDistintivos || incluirEnProceso)
            {
                // Construir la expresión con operadores OR para combinar las condiciones
                var distintivosParaFiltrar = filter.DistintivoIds?.Where(d => d != -1).ToList();

                query = query.Where(e =>
                    // Condición 1: Empresas con los distintivos seleccionados
                    (
                        incluirDistintivos
                        && e.Certificaciones.Any(c =>
                            c.Resultados.Any(r =>
                                distintivosParaFiltrar.Contains(r.DistintivoId ?? 0)
                            )
                        )
                        && !e.Certificaciones.Any(c => c.Enabled && !c.Resultados.Any())
                    )
                    ||
                    // Condición 2: Empresas en proceso (sin distintivo)
                    (
                        incluirEnProceso
                        && e.Certificaciones.Any(c => c.Enabled && !c.Resultados.Any())
                    )
                );
            }

            // Contar total de registros para paginación
            var totalItems = await query.CountAsync();
            var totalActives = await query.CountAsync(e => e.Active);
            var totalInactives = totalItems - totalActives;

            // Calcular bloques para paginación
            int blockSize = Math.Max(1, filter.BlockSize);
            int blockNumber = Math.Max(1, filter.BlockNumber);
            int totalBlocks = (int)Math.Ceiling(totalItems / (double)blockSize);

            // Aplicar paginación
            var empresas = await query
                .OrderBy(e => e.Nombre)
                .Skip((blockNumber - 1) * blockSize)
                .Take(blockSize)
                .ToListAsync();

            // Obtener todas las empresas sin paginación para calcular conteos
            var todasEmpresas = await query.ToListAsync();
            // Conteo de empresas por distintivo
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

            // Mapear resultados
            var result = new EmpresaReportResponseDTO
            {
                Items = MapEmpresasToReportItems(empresas, filter.Language),
                TotalCount = totalItems,
                TotalActive = totalActives,
                TotalInactive = totalInactives,
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

    private List<EmpresaReportItemDTO> MapEmpresasToReportItems(
        List<Empresa> empresas,
        string language
    )
    {
        var result = new List<EmpresaReportItemDTO>();

        foreach (var empresa in empresas)
        {
            // Obtener todos los procesos activos
            var procesosActivos = empresa
                .Certificaciones.Where(c => c.Enabled)
                .OrderByDescending(c => c.FechaInicio)
                .ToList();

            // Mapear las tipologías
            var tipologias = empresa
                .Tipologias.Select(t =>
                    language == "es" ? t.Tipologia.Name : t.Tipologia.NameEnglish
                )
                .ToList();

            var tipologiasIds = empresa.Tipologias.Select(t => t.IdTipologia).ToList();

            // Crear ítem básico de empresa
            var item = new EmpresaReportItemDTO
            {
                Id = empresa.Id,
                Nombre = empresa.Nombre,
                Pais = empresa.Pais?.Name ?? string.Empty,
                PaisId = empresa.PaisId ?? 0,
                Responsable = empresa.NombreRepresentante,
                TipologiasList = tipologias,
                TipologiasIds = tipologiasIds,
                Tipologias = string.Join(", ", tipologias),
                TotalProcesos = procesosActivos.Count,
                Activa = empresa.Active,
                Certificacion = empresa.EsHomologacion ? "Homologación" : "Certificación",
                FechaVencimiento = empresa.ResultadoVencimiento,
                // Inicializar colecciones para almacenar información sobre todos los procesos
                Distintivos = new List<string>(),
                DistintivosIds = new List<int>(),
                ProcesosEnProceso = 0,
            };

            // Proceso más reciente para establecer el estado actual
            var procesoMasReciente = procesosActivos.FirstOrDefault();

            if (procesoMasReciente != null)
            {
                // Establecer estado según el proceso más reciente
                int estadoId = 0;
                var statusParts = procesoMasReciente.Status.Split('-');
                if (statusParts.Length > 0 && int.TryParse(statusParts[0].Trim(), out int parsedId))
                {
                    estadoId = parsedId;
                }

                item.EstadoId = estadoId;

                if (Enum.IsDefined(typeof(CertificationStatus), estadoId))
                {
                    var estado = (CertificationStatus)estadoId;
                    item.Estado = estado.ToLocalizedString(language);
                }
                else
                {
                    item.Estado = procesoMasReciente.Status;
                }
            }
            else
            {
                item.Estado = CertificationStatus.Initial.ToLocalizedString(language);
                item.EstadoId = 0;
            }

            // Procesar cada proceso activo para recopilar información de distintivos
            foreach (var proceso in procesosActivos)
            {
                var ultimoResultado = proceso
                    .Resultados.OrderByDescending(r => r.Id)
                    .FirstOrDefault();

                if (
                    ultimoResultado != null
                    && ultimoResultado.DistintivoId.HasValue
                    && ultimoResultado.Distintivo != null
                )
                {
                    var distintivoNombre =
                        language == "es"
                            ? ultimoResultado.Distintivo.Name
                            : ultimoResultado.Distintivo.NameEnglish;

                    item.Distintivos.Add(distintivoNombre);
                    item.DistintivosIds.Add(ultimoResultado.DistintivoId.Value);

                    // Si es el proceso más reciente, establecer como distintivo principal
                    if (proceso == procesoMasReciente)
                    {
                        item.Distintivo = distintivoNombre;
                        item.DistintivoId = ultimoResultado.DistintivoId.Value;
                    }
                }
                else
                {
                    // Proceso en trámite
                    item.ProcesosEnProceso++;

                    // Si es el proceso más reciente y no tiene distintivo, marcar la empresa como en proceso
                    if (proceso == procesoMasReciente)
                    {
                        item.EnProceso = true;
                    }
                }
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
