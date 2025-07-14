# Dashboard Backend API Requirements

Este documento especifica los endpoints y estructuras de datos necesarios para el nuevo dashboard modular de SITCA.

## 📊 Dashboard Overview

El nuevo dashboard está organizado por roles y requiere datos específicos para cada tipo de usuario:

- **Admin/Técnico País**: Vista completa del sistema con KPIs, gráficos y actividades
- **ATP**: Vista de soporte con tareas delegadas
- **Asesor/Auditor**: Vista de workload personal
- **Consultor**: Vista de proyectos de consultoría
- **Empresa**: Vista del proceso de certificación
- **CTC**: Vista ejecutiva con reportes avanzados
- **Empresa Auditora**: Vista de gestión de auditores

## 🔧 Endpoints Requeridos

### 1. Dashboard Statistics (Admin/Técnico País)

**Endpoint:** `GET /api/dashboard/admin-statistics`

**Descripción:** Retorna estadísticas principales del sistema para usuarios Admin y Técnico País.

**Headers:**
```
Authorization: Bearer {jwt_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "value": {
    "totalEmpresas": 451,
    "totalCompleted": 252,
    "totalInProcess": 24, 
    "totalPending": 175,
    "empresasPorPais": [
      {
        "name": "Guatemala",
        "count": 183,
        "countryId": 1
      },
      {
        "name": "El Salvador", 
        "count": 143,
        "countryId": 2
      },
      {
        "name": "Honduras",
        "count": 47,
        "countryId": 3
      },
      {
        "name": "Belice",
        "count": 25,
        "countryId": 4
      },
      {
        "name": "Nicaragua",
        "count": 53,
        "countryId": 5
      }
    ],
    "empresasPorTipologia": [
      {
        "name": "Alojamiento",
        "count": 183,
        "tipologiaId": 1
      },
      {
        "name": "Restaurantes", 
        "count": 116,
        "tipologiaId": 2
      },
      {
        "name": "Operadoras de Turismo",
        "count": 93,
        "tipologiaId": 3
      },
      {
        "name": "Empresas de Transporte y Rent a car",
        "count": 28,
        "tipologiaId": 4
      },
      {
        "name": "Agencias de Viajes",
        "count": 31,
        "tipologiaId": 5
      }
    ]
  }
}
```

### 2. Recent Activities

**Endpoint:** `GET /api/dashboard/recent-activities`

**Descripción:** Retorna las actividades recientes del sistema.

**Query Parameters:**
- `limit` (opcional): Número máximo de actividades (default: 10)
- `offset` (opcional): Offset para paginación (default: 0)

**Response:**
```json
{
  "isSuccess": true,
  "value": [
    {
      "id": 1,
      "title": "Nueva empresa registrada",
      "description": "Hotel Paradise se registró para certificación",
      "timestamp": "2025-01-13T10:30:00Z",
      "type": "company_registered",
      "icon": "business",
      "priority": "info",
      "companyId": 152,
      "companyName": "Hotel Paradise"
    },
    {
      "id": 2,
      "title": "Certificación completada", 
      "description": "Restaurante El Buen Sabor obtuvo Distintivo Verde",
      "timestamp": "2025-01-13T08:39:00Z",
      "type": "certification_completed",
      "icon": "verified",
      "priority": "success",
      "companyId": 89,
      "companyName": "Restaurante El Buen Sabor",
      "distintivo": "Distintivo Verde"
    },
    {
      "id": 3,
      "title": "Auditoría programada",
      "description": "Auditoría para Hotel Costa Azul programada para mañana",
      "timestamp": "2025-01-13T06:39:00Z",
      "type": "audit_scheduled",
      "icon": "event",
      "priority": "warning",
      "companyId": 78,
      "companyName": "Hotel Costa Azul",
      "auditDate": "2025-01-14T09:00:00Z"
    }
  ]
}
```

### 3. System Status

**Endpoint:** `GET /api/dashboard/system-status`

**Descripción:** Retorna el estado actual del sistema.

**Response:**
```json
{
  "isSuccess": true,
  "value": {
    "apiStatus": {
      "status": "online",
      "responseTime": "45ms",
      "lastCheck": "2025-01-13T11:45:00Z"
    },
    "databaseStatus": {
      "status": "connected",
      "connectionCount": 12,
      "lastCheck": "2025-01-13T11:45:00Z"
    },
    "lastBackup": {
      "timestamp": "2025-01-13T02:00:00Z",
      "status": "success",
      "size": "2.4GB"
    },
    "serverHealth": {
      "cpuUsage": 23.5,
      "memoryUsage": 67.2,
      "diskUsage": 45.8
    }
  }
}
```

### 4. ATP Dashboard Data

**Endpoint:** `GET /api/dashboard/atp-statistics`

**Descripción:** Datos específicos para usuarios ATP (Apoyo Técnico País).

**Response:**
```json
{
  "isSuccess": true,
  "value": {
    "tareasAsignadas": 12,
    "tareasCompletadas": 8,
    "tareasPendientes": 4,
    "empresasAsignadas": [
      {
        "empresaId": 123,
        "nombreEmpresa": "Hotel Ejemplo",
        "estadoProceso": "En Revisión",
        "fechaAsignacion": "2025-01-10T00:00:00Z",
        "prioridad": "alta"
      }
    ],
    "actividadesRecientes": [
      {
        "tipo": "revision_completada",
        "descripcion": "Revisión de documentos completada para Hotel ABC",
        "timestamp": "2025-01-13T09:30:00Z"
      }
    ]
  }
}
```

### 5. Asesor/Auditor Dashboard Data

**Endpoint:** `GET /api/dashboard/asesor-auditor-statistics`

**Descripción:** Datos para usuarios Asesor y Auditor.

**Response:**
```json
{
  "isSuccess": true,
  "value": {
    "procesosAsignados": 5,
    "certificacionesPendientes": 3,
    "auditoriasEsteDate": [
      {
        "empresaId": 89,
        "nombreEmpresa": "Hotel Costa del Sol",
        "fechaAuditoria": "2025-01-15T10:00:00Z",
        "tipo": "auditoria_inicial"
      }
    ],
    "estadisticasPersonales": {
      "certificacionesCompletadas": 45,
      "promedioTiempoCertificacion": 30,
      "satisfaccionClientes": 4.8
    }
  }
}
```

## 🔐 Autenticación y Autorización

Todos los endpoints requieren:
1. **Token JWT válido** en el header `Authorization: Bearer {token}`
2. **Permisos de rol apropiados** verificados server-side

### Roles y Permisos:
- `is:admin` - Acceso completo a dashboard admin
- `is:tecnico-pais` - Acceso a dashboard admin (vista limitada)
- `is:atp` - Acceso a dashboard ATP
- `is:asesor` - Acceso a dashboard asesor
- `is:auditor` - Acceso a dashboard auditor
- `is:consultor` - Acceso a dashboard consultor
- `is:empresa` - Acceso a dashboard empresa
- `is:ctc` - Acceso a dashboard CTC
- `is:empresa-auditora` - Acceso a dashboard empresa auditora

## 📝 Consideraciones de Implementación

### 1. Performance
- Implementar **caché** para estadísticas que no cambian frecuentemente
- Usar **paginación** para actividades recientes
- **Índices** en tablas para consultas por país y tipología

### 2. Real-time Updates (Opcional)
- Considerar **WebSockets** o **Server-Sent Events** para actualizaciones en tiempo real
- Especialmente útil para actividades recientes y estados del sistema

### 3. Filtros y Parámetros
- Agregar filtros por **rango de fechas** en actividades
- Permitir filtrar estadísticas por **país específico** para técnicos país

### 4. Error Handling
- Retornar códigos HTTP apropiados (200, 401, 403, 500)
- Estructura de error consistente:
```json
{
  "isSuccess": false,
  "error": {
    "code": "UNAUTHORIZED",
    "message": "Token inválido o expirado"
  }
}
```

## 🚀 Prioridad de Implementación

1. **Alta Prioridad:**
   - `/api/dashboard/admin-statistics` (necesario para Admin/Técnico País)
   - `/api/dashboard/recent-activities` (timeline de actividades)

2. **Media Prioridad:**
   - `/api/dashboard/system-status` (estado del sistema)
   - `/api/dashboard/atp-statistics` (para completar rol ATP)

3. **Baja Prioridad:**
   - Dashboards específicos para otros roles (se pueden implementar gradualmente)


---

**Notas adicionales:**
- Los datos de ejemplo mostrados son referenciales
- Ajustar nombres de campos según convenciones del backend existente
- Considerar versionado de API (`/api/v1/dashboard/...`)
