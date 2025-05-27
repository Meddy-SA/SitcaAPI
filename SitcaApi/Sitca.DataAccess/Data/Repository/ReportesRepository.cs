using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.DataAccess.Data.Repository.Repository;
using Sitca.DataAccess.Middlewares;
using Sitca.DataAccess.Services.Files;
using Sitca.DataAccess.Services.Pdf;
using Sitca.DataAccess.Services.Url;
using Sitca.DataAccess.Services.ViewToString;
using Sitca.Models;
using Sitca.Models.ViewModels;

namespace Sitca.DataAccess.Data.Repository
{
    public class ReportesRepository : Repository<Cuestionario>, IReporteRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ReportesRepository> _logger;
        private readonly IViewRenderService _viewRenderService;
        private readonly IReportService _reportService;
        private readonly IIconService _iconService;
        private readonly IUrlService _urlService;
        private IUnitOfWork _unitOfWork;

        public ReportesRepository(
            ApplicationDbContext db,
            ILogger<ReportesRepository> logger,
            IViewRenderService viewRenderService,
            IReportService reportService,
            IIconService iconService,
            IUrlService urlService
        )
            : base(db)
        {
            _db = db;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _viewRenderService =
                viewRenderService ?? throw new ArgumentNullException(nameof(viewRenderService));
            _reportService =
                reportService ?? throw new ArgumentNullException(nameof(reportService));
            _iconService = iconService ?? throw new ArgumentNullException(nameof(iconService));
            _urlService = urlService ?? throw new ArgumentNullException(nameof(urlService));
        }

        public void SetUnitOfWork(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<bool> ReporteCertificacion(int cuestionarioId)
        {
            return await Task.Run(() => true);
        }

        /// <inheritdoc/>
        public async Task<byte[]> GenerarReporteCertificacion(
            int cuestionarioId,
            ApplicationUser user,
            string role
        )
        {
            if (cuestionarioId <= 0)
                throw new ArgumentException(
                    "El ID del cuestionario debe ser mayor que cero",
                    nameof(cuestionarioId)
                );

            if (user == null)
                throw new ArgumentNullException(nameof(user), "El usuario no puede ser nulo");

            try
            {
                // Preparar datos completos para el reporte
                var resultados = await PrepararDatosCertificacion(cuestionarioId, user, role);

                // Seleccionar la plantilla adecuada según si es prueba o certificación final
                var vista = resultados.Prueba ? "ReporteAsesoria" : "ReporteCertificacionV2";

                // Renderizar la vista a HTML
                var viewHtml = await _viewRenderService.RenderToStringAsync(vista, resultados);

                // Generar PDF a partir del HTML
                var pdfBytes = _reportService.GeneratePdfReport(viewHtml);

                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al generar reporte de certificación para cuestionario: {CuestionarioId}",
                    cuestionarioId
                );
                throw new DatabaseException(
                    "Error al generar el reporte de certificación",
                    ex
                ).WithEntity("Cuestionario");
            }
        }

        /// <inheritdoc/>
        public async Task<CuestionarioDetailsVm> PrepararDatosCertificacion(
            int cuestionarioId,
            ApplicationUser user,
            string role
        )
        {
            try
            {
                // Obtener datos básicos del cuestionario
                var resultados = await _unitOfWork.ProcesoCertificacion.GetCuestionario(
                    cuestionarioId,
                    user,
                    role
                );

                if (resultados == null)
                {
                    throw new KeyNotFoundException(
                        $"No se encontró el cuestionario con ID: {cuestionarioId}"
                    );
                }

                // Asignar valores base
                resultados.path = _urlService.GenerateBaseUrl();
                resultados.Lang = user.Lenguage;

                // Obtener IDs de respuestas para consultas posteriores
                var respuestasIds = resultados
                    .Modulos.SelectMany(m =>
                        m.Items.Where(i => i.IdRespuesta.HasValue).Select(i => i.IdRespuesta.Value)
                    )
                    .ToArray();

                var archivoFilter = new ArchivoFilterVm
                {
                    idCuestionario = cuestionarioId,
                    type = "cuestionario",
                };

                // Ejecutar las consultas secuencialmente
                var observaciones = await _unitOfWork.ProcesoCertificacion.GetListObservaciones(
                    respuestasIds
                );
                var archivos = await _unitOfWork.Archivo.GetList(archivoFilter, user, role);

                // Crear un diccionario de observaciones para búsqueda eficiente
                var observacionesPorRespuesta = observaciones.ToDictionary(
                    o => o.IdRespuesta,
                    o => o.Observaciones
                );

                // Agrupar archivos por CuestionarioItemId para búsqueda eficiente
                var archivosPorItem = archivos
                    .GroupBy(a => a.CuestionarioItemId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Procesar módulos y asignar recursos
                EnriquecerModulosConRecursos(
                    resultados.Modulos,
                    archivosPorItem,
                    observacionesPorRespuesta
                );

                return resultados;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al preparar datos de certificación para cuestionario: {CuestionarioId}",
                    cuestionarioId
                );
                throw;
            }
        }

        /// <summary>
        /// Enriquece los módulos con archivos y observaciones
        /// </summary>
        private void EnriquecerModulosConRecursos(
            IEnumerable<ModulosVm> modulos,
            Dictionary<int?, List<Archivo>> archivosPorItem,
            Dictionary<int, string> observacionesPorRespuesta
        )
        {
            foreach (var modulo in modulos)
            {
                var preguntas = modulo.Items.Where(item => item.Type == "pregunta");

                foreach (var pregunta in preguntas)
                {
                    // Solo procesar si hay un ID de respuesta válido
                    if (!pregunta.IdRespuesta.HasValue)
                        continue;

                    var idRespuesta = pregunta.IdRespuesta.Value;

                    // Asignar archivos si existen para este item
                    if (archivosPorItem.TryGetValue(idRespuesta, out var archivosItem))
                    {
                        pregunta.Archivos = archivosItem;

                        // Asignar iconos a los archivos
                        foreach (var archivo in pregunta.Archivos)
                        {
                            archivo.Base64Str = _iconService.GetIconForFileType(archivo.Tipo);
                        }
                    }
                    else
                    {
                        pregunta.Archivos = new List<Archivo>();
                    }

                    // Asignar observación si existe
                    pregunta.Observacion = observacionesPorRespuesta.TryGetValue(
                        idRespuesta,
                        out var observacion
                    )
                        ? observacion
                        : string.Empty;
                }
            }
        }
    }
}
