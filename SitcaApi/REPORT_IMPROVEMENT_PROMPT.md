# SITCA - Report Improvement Prompt

## Contexto del Sistema

SITCA (Sistema Integrado de Certificación) es una plataforma Web API desarrollada en ASP.NET Core 8.0 que gestiona procesos de certificación, auditorías y cumplimiento para empresas en países centroamericanos. El sistema maneja operaciones multi-tenant con soporte para diferentes países, idiomas (español/inglés) y roles de usuario.

## Cambio de Paradigma en los Reportes

### Situación Anterior
- Los reportes estaban **centrados en las empresas** (Company-Centered)
- La estructura organizativa priorizaba la vista por empresa
- Los datos se agrupaban principalmente por entidad Empresa

### Nueva Estructura Requerida
- Los reportes deben estar **centrados en los procesos de certificación** (Process-Centered)
- La entidad principal es `ProcesoCertificacion`
- Necesidad de reorganizar la información desde la perspectiva del proceso

## Entidades Clave Relacionadas

### ProcesoCertificacion (Entidad Principal)
- **Id**: Identificador único del proceso
- **EmpresaId**: Relación con la empresa certificada
- **AuditorId**: Auditor asignado al proceso
- **TipoCertificacionId**: Tipo de certificación (ej: ISO, HACCP, etc.)
- **Estado**: Estado actual del proceso (En Proceso, Completado, Rechazado, etc.)
- **FechaInicio/FechaFin**: Timeline del proceso
- **Porcentaje**: Progreso del proceso

### Relaciones Importantes
1. **Cuestionario**: Múltiples cuestionarios por proceso
   - Respuestas de evaluación
   - Estado de revisión
   - Puntajes y calificaciones

2. **Auditor**: Responsable del proceso
   - Asignaciones
   - Revisiones realizadas
   - Tiempo de respuesta

3. **Empresa**: Entidad siendo certificada
   - Información de contacto
   - Historial de certificaciones
   - Documentación asociada

4. **Hallazgos**: Observaciones del proceso
   - Criticidad
   - Estado de resolución
   - Acciones correctivas

## Métricas y KPIs Sugeridos para Reportes por Proceso

### 1. Métricas de Eficiencia del Proceso
- Tiempo promedio de certificación por tipo
- Procesos completados vs en progreso por periodo
- Tasa de éxito/rechazo por tipo de certificación
- Cuello de botella en etapas del proceso

### 2. Métricas de Calidad
- Promedio de hallazgos por proceso
- Tiempo de resolución de hallazgos críticos
- Tasa de re-certificación exitosa
- Puntaje promedio de cuestionarios por proceso

### 3. Métricas de Auditor
- Carga de trabajo por auditor (procesos asignados)
- Tiempo promedio de revisión por auditor
- Tasa de aprobación por auditor
- Especialización por tipo de certificación

### 4. Métricas Comparativas
- Comparación entre países (multi-tenant)
- Tendencias por sector/industria
- Benchmarking entre tipos de certificación
- Evolución temporal de los procesos

## Preguntas para Generar Ideas de Mejora

1. **Visualización de Datos**
   - ¿Qué tipo de gráficos serían más útiles para mostrar el progreso de múltiples procesos simultáneos?
   - ¿Cómo visualizar la línea de tiempo de un proceso con sus hitos críticos?
   - ¿Qué dashboards necesitan los diferentes roles (Admin, Auditor, Empresa)?

2. **Agrupación y Filtrado**
   - ¿Cómo agrupar procesos por estado, tipo, auditor, país, periodo?
   - ¿Qué filtros avanzados necesitan los usuarios?
   - ¿Cómo implementar búsquedas predictivas por proceso?

3. **Exportación y Formatos**
   - ¿Qué formatos de exportación son necesarios (PDF, Excel, CSV)?
   - ¿Qué plantillas de reporte predefinidas serían útiles?
   - ¿Cómo generar reportes automáticos programados?

4. **Análisis Predictivo**
   - ¿Cómo predecir la duración estimada de un proceso basado en históricos?
   - ¿Qué alertas tempranas se pueden generar para procesos en riesgo?
   - ¿Cómo identificar patrones de éxito/fracaso?

5. **Integración y Notificaciones**
   - ¿Qué notificaciones automáticas necesitan los stakeholders?
   - ¿Cómo integrar los reportes con el sistema de notificaciones existente (Hangfire)?
   - ¿Qué APIs necesitan exponerse para reportes externos?

## Consideraciones Técnicas

### Tecnologías Disponibles
- **Generación PDF**: iText, GemBox.Document, wkhtmltopdf
- **Queries Complejos**: Dapper para consultas optimizadas
- **Jobs Programados**: Hangfire para generación automática
- **Caché**: Considerar implementar caché para reportes frecuentes

### Optimización de Consultas
- Uso de índices apropiados para consultas por ProcesoCertificacionId
- Considerar vistas materializadas para reportes complejos
- Implementar paginación eficiente para grandes volúmenes

### Estructura de Datos Sugerida para Reportes

```csharp
public class ProcesoReportViewModel
{
    public int ProcesoId { get; set; }
    public string TipoCertificacion { get; set; }
    public string Estado { get; set; }
    public decimal Progreso { get; set; }
    public TimeSpan DuracionActual { get; set; }
    public int TotalHallazgos { get; set; }
    public int HallazgosPendientes { get; set; }
    public decimal PuntajePromedio { get; set; }
    // Métricas adicionales...
}
```

## Prompt para Generar Ideas

"Basándome en el sistema SITCA de certificación que ahora debe generar reportes centrados en procesos en lugar de empresas, necesito ideas innovadoras para:

1. **Diseñar nuevos reportes** que aprovechen la estructura de ProcesoCertificacion como entidad principal
2. **Crear visualizaciones** que muestren el flujo y progreso de múltiples procesos simultáneos
3. **Implementar métricas** que ayuden a identificar cuellos de botella y oportunidades de mejora en el proceso de certificación
4. **Desarrollar análisis comparativos** entre diferentes tipos de certificación, auditores y países
5. **Generar alertas inteligentes** basadas en patrones históricos de los procesos

Considera que el sistema maneja multi-tenancy por país, diferentes roles de usuario, y que los procesos pueden tener múltiples cuestionarios, hallazgos y documentos asociados. Los reportes deben ser útiles para administradores, auditores y empresas, cada uno con sus propias necesidades de información."

## Estructura de Implementación Sugerida

```
/Sitca/Controllers/Reports/
├── ProcessReportController.cs       # Endpoint principal de reportes
├── ProcessMetricsController.cs      # Métricas y KPIs
└── ProcessExportController.cs       # Exportación a diferentes formatos

/Sitca.DataAccess/Repositories/Reports/
├── ProcessReportRepository.cs       # Queries optimizados con Dapper
└── ProcessMetricsRepository.cs      # Cálculo de métricas

/Sitca.Models/ViewModels/Reports/
├── ProcessReportViewModel.cs        # Modelos de vista para reportes
├── ProcessMetricsViewModel.cs       # Modelos para métricas
└── ProcessFilterViewModel.cs        # Filtros y parámetros
```

## Siguiente Paso

Utiliza este prompt para generar ideas específicas de reportes, considerando:
- La nueva estructura centrada en procesos
- Las necesidades de los diferentes stakeholders
- Las capacidades técnicas del sistema
- La escalabilidad y performance requeridas