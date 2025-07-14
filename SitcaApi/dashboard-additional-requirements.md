# Dashboard Backend API Requirements - Roles Adicionales

Este documento complementa `dashboard-backend-requirements.md` con los endpoints específicos para los roles restantes del sistema SITCA.

## 📊 Roles Cubiertos en Este Documento

- **Empresa**: Dashboard centrado en el proceso de certificación
- **Consultor**: Dashboard de gestión de proyectos de consultoría
- **CTC**: Dashboard ejecutivo para el Consejo Técnico Centroamericano
- **Empresa Auditora**: Dashboard de gestión de auditores (pendiente de implementación)

## 🔧 Endpoints Requeridos

### 1. Dashboard Empresa

**Endpoint:** `GET /api/dashboard/empresa-statistics`

**Descripción:** Datos específicos para empresas en proceso de certificación o certificadas.

**Headers:**
```
Authorization: Bearer {jwt_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "value": {
    "certificacionActual": {
      "id": 1,
      "nivel": "azul",
      "estado": "active",
      "fechaEmision": "2024-01-13T00:00:00Z",
      "fechaVencimiento": "2026-01-13T00:00:00Z",
      "numeroCertificado": "SICCS-AZ-2024-001",
      "puntuacion": 85,
      "diasVigencia": 730
    },
    "tareasPendientes": [
      {
        "id": 1,
        "titulo": "Actualizar Manual de Procedimientos",
        "descripcion": "Revisar y actualizar el manual según nuevas normativas",
        "tipo": "documentation",
        "prioridad": "alta",
        "estado": "pending",
        "fechaVencimiento": "2025-01-20T00:00:00Z",
        "asignadoPor": "María González (Asesora)",
        "progreso": 0,
        "requisitos": ["Manual actualizado", "Aprobación del equipo"]
      }
    ],
    "documentosRecientes": [
      {
        "id": 1,
        "titulo": "Manual de Procedimientos v2.1",
        "tipo": "Manual",
        "fechaSubida": "2025-01-11T00:00:00Z",
        "estado": "approved",
        "url": "/documents/manual-v2.1.pdf"
      }
    ],
    "proximasActividades": [
      {
        "tipo": "auditoria",
        "descripcion": "Auditoría de seguimiento programada",
        "fecha": "2025-01-25T10:00:00Z",
        "auditor": "Carlos Mendez"
      }
    ],
    "estadisticasGenerales": {
      "procesoCompletado": 75,
      "documentosAprobados": 12,
      "capacitacionesCompletadas": 8,
      "proximaEvaluacion": "2025-07-13T00:00:00Z"
    }
  }
}
```

### 2. Dashboard Consultor

**Endpoint:** `GET /api/dashboard/consultor-statistics`

**Descripción:** Datos para consultores que realizan proyectos de asesoría y capacitación.

**Response:**
```json
{
  "isSuccess": true,
  "value": {
    "proyectosActivos": [
      {
        "id": 1,
        "titulo": "Implementación Sistema de Gestión de Calidad",
        "cliente": "Hotel Paradise Resort",
        "descripcion": "Consultoría especializada para implementar SGC según normas SICCS",
        "estado": "in_progress",
        "tipo": "certification",
        "prioridad": "alta",
        "fechaInicio": "2024-12-01T00:00:00Z",
        "fechaFin": "2025-02-15T00:00:00Z",
        "porcentajeCompletado": 65,
        "presupuesto": 15000,
        "proximoHito": "Revisión de documentación",
        "fechaProximoHito": "2025-01-20T00:00:00Z"
      }
    ],
    "reunionesProgramadas": [
      {
        "id": 1,
        "titulo": "Revisión de Avances SGC",
        "cliente": "Hotel Paradise Resort",
        "proyectoId": 1,
        "fechaHora": "2025-01-15T14:00:00Z",
        "duracion": 120,
        "ubicacion": "Oficinas del hotel",
        "tipo": "presencial",
        "estado": "scheduled",
        "agenda": ["Revisión de documentos", "Evaluación de implementación", "Próximos pasos"]
      }
    ],
    "informesPendientes": [
      {
        "id": 1,
        "titulo": "Informe de Progreso - SGC",
        "proyectoId": 1,
        "cliente": "Hotel Paradise Resort",
        "tipo": "progress",
        "estado": "draft",
        "fechaVencimiento": "2025-01-20T00:00:00Z",
        "porcentajeCompletado": 60
      }
    ],
    "estadisticas": {
      "totalProyectos": 3,
      "consultoriasActivas": 2,
      "proyectosCompletados": 8,
      "informesPendientes": 2,
      "facturaciónMensual": 25000,
      "horasFacturadas": 120,
      "clientesSatisfechos": 4.8
    }
  }
}
```

### 3. Dashboard CTC (Consejo Técnico Centroamericano)

**Endpoint:** `GET /api/dashboard/ctc-statistics`

**Descripción:** Vista ejecutiva para el Consejo Técnico Centroamericano con datos regionales.

**Response:**
```json
{
  "isSuccess": true,
  "value": {
    "evaluacionesPendientes": [
      {
        "id": 1,
        "nombreEmpresa": "Hotel Intercontinental Guatemala",
        "pais": "Guatemala",
        "tipoEvaluacion": "certification",
        "estado": "pending",
        "fechaEnvio": "2025-01-08T00:00:00Z",
        "evaluador": "Dr. María González",
        "nivel": "azul",
        "prioridad": "alta"
      }
    ],
    "certificacionesRecientes": [
      {
        "id": 1,
        "nombreEmpresa": "Hotel Paradise Belize",
        "pais": "Belice",
        "nivel": "verde",
        "fechaEmision": "2025-01-06T00:00:00Z",
        "fechaVencimiento": "2027-01-06T00:00:00Z",
        "numeroCertificado": "SICCS-BZ-2024-001",
        "estado": "active"
      }
    ],
    "estadisticasRegionales": {
      "totalCertificaciones": 847,
      "certificacionesPorPais": {
        "Guatemala": 186,
        "Costa Rica": 203,
        "Panamá": 142,
        "Honduras": 98,
        "El Salvador": 127,
        "Belice": 45,
        "Nicaragua": 46
      },
      "certificacionesPorNivel": {
        "azul": 421,
        "rojo": 298,
        "verde": 128
      },
      "evaluacionesPendientes": 23,
      "certificacionesPorVencer": 15,
      "nuevasCertificacionesMes": 12,
      "tasaAprobacion": 87.5
    },
    "alertasImportantes": [
      {
        "tipo": "expiracion",
        "mensaje": "15 certificaciones vencen en los próximos 30 días",
        "prioridad": "alta"
      },
      {
        "tipo": "evaluacion_pendiente",
        "mensaje": "3 evaluaciones pendientes por más de 15 días",
        "prioridad": "media"
      }
    ],
    "tendenciasCertificacion": [
      {
        "mes": "2024-12",
        "nuevas": 15,
        "renovadas": 8,
        "suspendidas": 2
      },
      {
        "mes": "2025-01",
        "nuevas": 12,
        "renovadas": 10,
        "suspendidas": 1
      }
    ]
  }
}
```

### 4. Dashboard Empresa Auditora

**Endpoint:** `GET /api/dashboard/empresa-auditora-statistics`

**Descripción:** Datos para empresas auditoras que gestionan auditores y procesos de auditoría.

**Response:**
```json
{
  "isSuccess": true,
  "value": {
    "auditoresActivos": [
      {
        "id": 1,
        "nombre": "Carlos Mendez",
        "especialidades": ["Alojamiento", "Restauración"],
        "paises": ["Guatemala", "Honduras"],
        "certificacionVigente": true,
        "fechaVencimientoCertificacion": "2025-12-15T00:00:00Z",
        "auditoriasPendientes": 3,
        "calificacionPromedio": 4.8,
        "estadoDisponibilidad": "disponible"
      }
    ],
    "auditoriasProgamadas": [
      {
        "id": 1,
        "empresa": "Hotel Costa del Sol",
        "auditor": "Carlos Mendez",
        "fecha": "2025-01-18T09:00:00Z",
        "tipo": "auditoria_inicial",
        "nivel": "azul",
        "duracion": 480,
        "estado": "programada",
        "pais": "Guatemala"
      }
    ],
    "auditoriasCompletadas": [
      {
        "id": 1,
        "empresa": "Restaurante El Pescador",
        "auditor": "Ana Morales",
        "fechaCompletada": "2025-01-10T00:00:00Z",
        "resultado": "aprobado",
        "puntuacion": 92,
        "nivel": "verde",
        "informeEntregado": true
      }
    ],
    "estadisticas": {
      "totalAuditores": 15,
      "auditoresActivos": 12,
      "auditoriasEsteDate": 8,
      "auditoriasCompletadasMes": 23,
      "tasaAprobacion": 89.5,
      "tiempoPromedioAuditoria": 6,
      "certificacionesVigentes": 14,
      "ingresosMes": 45000
    },
    "alertasGestion": [
      {
        "tipo": "certificacion_vencimiento",
        "mensaje": "3 auditores con certificación por vencer en 60 días",
        "prioridad": "media"
      },
      {
        "tipo": "sobrecarga",
        "mensaje": "2 auditores con más de 5 auditorías asignadas",
        "prioridad": "alta"
      }
    ],
    "distribucionTrabajo": [
      {
        "auditor": "Carlos Mendez",
        "auditoriasPendientes": 3,
        "auditoriasMes": 5,
        "disponibilidad": "limitada"
      },
      {
        "auditor": "Ana Morales", 
        "auditoriasPendientes": 2,
        "auditoriasMes": 4,
        "disponibilidad": "disponible"
      }
    ]
  }
}
```

## 🔐 Autenticación y Autorización

### Roles y Permisos Específicos:
- `is:empresa` - Acceso limitado a datos propios de certificación
- `is:consultor` - Acceso a proyectos de consultoría asignados
- `is:ctc` - Acceso completo a datos regionales y estadísticas
- `is:empresa-auditora` - Acceso a gestión de auditores y auditorías

## 📊 Endpoints de Soporte Adicionales

### 1. Actividades Específicas por Rol

**Endpoint:** `GET /api/dashboard/activities/{role}`

**Query Parameters:**
- `limit` (opcional): Número máximo de actividades (default: 10)
- `days` (opcional): Días hacia atrás para filtrar (default: 7)

### 2. Notificaciones por Dashboard

**Endpoint:** `GET /api/dashboard/notifications/{role}`

**Response:**
```json
{
  "isSuccess": true,
  "value": [
    {
      "id": 1,
      "tipo": "task_due",
      "titulo": "Tarea por vencer",
      "mensaje": "El manual de procedimientos debe entregarse en 2 días",
      "prioridad": "alta",
      "fechaCreacion": "2025-01-13T09:00:00Z",
      "leida": false,
      "accionesDisponibles": ["mark_read", "view_task", "extend_deadline"]
    }
  ]
}
```

### 3. Métricas de Performance

**Endpoint:** `GET /api/dashboard/metrics/{role}`

**Descripción:** Métricas específicas de rendimiento por rol.

## 📝 Consideraciones de Implementación Específicas

### 1. Empresa Dashboard
- **Filtros por estado de proceso**: Permitir filtrar tareas por estado
- **Calendario de vencimientos**: Vista calendario para fechas importantes
- **Subida de documentos**: Integración con sistema de gestión documental
- **Comunicación con asesor**: Sistema de mensajería integrado

### 2. Consultor Dashboard
- **Gestión de tiempo**: Tracking de horas por proyecto
- **Facturación**: Integración con sistema de facturación
- **Recursos compartidos**: Biblioteca de documentos y plantillas
- **Reportes automáticos**: Generación automática de informes de progreso

### 3. CTC Dashboard
- **Exportación de datos**: Excel, PDF para reportes ejecutivos
- **Filtros avanzados**: Por país, nivel, rango de fechas
- **Alertas configurables**: Notificaciones personalizables
- **Comparativas históricas**: Datos de años anteriores

### 4. Empresa Auditora Dashboard
- **Calendario de auditorías**: Vista de calendario integrada
- **Gestión de disponibilidad**: Sistema de asignación automática
- **Tracking de calificaciones**: Historial de performance por auditor
- **Facturación por auditoría**: Módulo de facturación específico

## 🚀 Prioridad de Implementación

### Alta Prioridad (Próximas 2 semanas):
1. **Empresa Dashboard** - `/api/dashboard/empresa-statistics`
2. **Consultor Dashboard** - `/api/dashboard/consultor-statistics`

### Media Prioridad (Siguiente mes):
3. **CTC Dashboard** - `/api/dashboard/ctc-statistics`
4. **Notificaciones por rol** - `/api/dashboard/notifications/{role}`

### Baja Prioridad (Próximos 2 meses):
5. **Empresa Auditora Dashboard** - `/api/dashboard/empresa-auditora-statistics`
6. **Métricas de performance** - `/api/dashboard/metrics/{role}`

## 📋 Campos Específicos Requeridos

### Para Empresa:
- Estado actual de certificación
- Progreso del proceso
- Documentos pendientes y aprobados
- Próximas fechas importantes
- Comunicaciones con asesor

### Para Consultor:
- Proyectos activos y su estado
- Facturación y horas trabajadas
- Reuniones programadas
- Informes pendientes
- Evaluación de clientes

### Para CTC:
- Estadísticas regionales
- Evaluaciones pendientes por evaluar
- Tendencias de certificación
- Alertas del sistema
- Datos comparativos

### Para Empresa Auditora:
- Estado de auditores (certificaciones, disponibilidad)
- Auditorías programadas y completadas
- Facturación por auditorías
- Distribución de carga de trabajo
- Métricas de calidad

---

**Notas:**
- Todos los endpoints mantienen la estructura de respuesta estándar con `isSuccess` y `value`
- Las fechas siguen formato ISO 8601
- Los roles deben validarse server-side para cada endpoint
- Considerar caché para datos que no cambian frecuentemente
- Implementar paginación donde sea apropiado