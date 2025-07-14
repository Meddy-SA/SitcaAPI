# Manual Técnico del Backend - SITCA 2.0

## Tabla de Contenidos

1. [Introducción](#introducción)
2. [Arquitectura del Proyecto](#arquitectura-del-proyecto)
3. [Tecnologías y Dependencias](#tecnologías-y-dependencias)
4. [Estructura del Proyecto](#estructura-del-proyecto)
5. [Modelos de Base de Datos](#modelos-de-base-de-datos)
6. [API y Controladores](#api-y-controladores)
7. [Configuración y Despliegue](#configuración-y-despliegue)
8. [Comandos Esenciales](#comandos-esenciales)
9. [Patrones de Desarrollo](#patrones-de-desarrollo)
10. [Consideraciones de Seguridad](#consideraciones-de-seguridad)

## Introducción

SITCA (Sistema Integrado de Certificación) es una aplicación Web API desarrollada en ASP.NET Core 8.0 para la gestión de certificaciones, auditorías y cumplimiento normativo para empresas de turismo en países de Centroamérica. El sistema maneja operaciones multi-tenant con soporte para diferentes países, idiomas (español/inglés) y roles de usuario.

### Características Principales

- **Multi-tenancy**: Separación por países
- **Multi-idioma**: Soporte para español e inglés
- **Gestión de certificaciones**: Proceso completo de certificación turística
- **Sistema de auditorías**: Auditorías internas y cruzadas entre países
- **Gestión de archivos**: Sistema robusto de manejo de documentos
- **Reportes PDF**: Generación dinámica de documentos
- **Notificaciones**: Sistema de notificaciones por email y en aplicación
- **Trabajos en segundo plano**: Procesamiento asíncrono con Hangfire

## Arquitectura del Proyecto

El proyecto sigue los principios de **Clean Architecture** organizado en 4 proyectos principales:

### Diagrama de Capas

```
┌─────────────────────────────────────┐
│           Sitca (Web API)           │
│     Controllers, Middleware,       │
│     Authentication, Validators     │
├─────────────────────────────────────┤
│        Sitca.Models (Domain)       │
│    Entities, DTOs, ViewModels,     │
│         Enums, Constants           │
├─────────────────────────────────────┤
│    Sitca.DataAccess (Data Layer)   │
│   Repository, DbContext, Services, │
│       Migrations, Configurations   │
├─────────────────────────────────────┤
│      Utilities (Shared Layer)      │
│    Authorization, Constants,       │
│        Helpers, Extensions         │
└─────────────────────────────────────┘
```

### Principios Arquitectónicos

1. **Separación de Responsabilidades**: Cada capa tiene responsabilidades específicas
2. **Inversión de Dependencias**: Las capas superiores no dependen de las inferiores
3. **Patrón Repository**: Abstracción del acceso a datos
4. **Unit of Work**: Gestión transaccional consistente
5. **Dependency Injection**: Inyección de dependencias nativa de ASP.NET Core

## Tecnologías y Dependencias

### Framework Principal

- **.NET 8.0**: Framework base
- **ASP.NET Core 8.0**: Web API framework
- **Entity Framework Core 8.0.8**: ORM principal
- **ASP.NET Core Identity**: Sistema de autenticación y autorización

### Base de Datos

- **SQL Server**: Base de datos principal
- **Dapper**: ORM ligero para consultas complejas
- **Entity Framework Migrations**: Gestión de esquema de base de datos

### Autenticación y Seguridad

- **JWT Bearer Tokens**: Autenticación basada en tokens
- **ASP.NET Core Identity**: Gestión de usuarios y roles
- **Role-based Authorization**: Autorización basada en roles

### Procesamiento de Archivos

- **Magick.NET**: Procesamiento de imágenes
- **GemBox.Document**: Procesamiento de documentos Office
- **GemBox.Pdf**: Manipulación de archivos PDF
- **iText**: Generación avanzada de PDFs
- **wkhtmltopdf**: Conversión HTML a PDF

### Trabajos en Segundo Plano

- **Hangfire**: Procesamiento de trabajos en segundo plano
- **Hangfire.SqlServer**: Persistencia de trabajos en SQL Server

### Comunicaciones

- **Brevo (Sendinblue)**: Servicio de email SMTP
- **Newtonsoft.Json**: Serialización JSON

### Validación

- **FluentValidation**: Validación fluida de modelos

### Librerías Adicionales

```xml
<!-- Principales dependencias del proyecto -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.8" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.8" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.8" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.8" />
<PackageReference Include="Hangfire.Core" Version="1.8.14" />
<PackageReference Include="Hangfire.SqlServer" Version="1.8.14" />
<PackageReference Include="Dapper" Version="2.1.35" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="Magick.NET-Q16-AnyCPU" Version="13.10.0" />
<PackageReference Include="GemBox.Document" Version="3.7.0.1061" />
<PackageReference Include="itext7" Version="8.0.5" />
```

## Estructura del Proyecto

### Sitca (Web API Principal)

```
Sitca/
├── Controllers/               # 21 controladores API
│   ├── AuthController.cs      # Autenticación y gestión de usuarios
│   ├── EmpresaController.cs   # Gestión de empresas
│   ├── CertificacionController.cs  # Procesos de certificación
│   ├── FileManagerController.cs    # Gestión de archivos
│   └── ...
├── Areas/Identity/            # Páginas de ASP.NET Core Identity
├── Extensions/                # Métodos de extensión
├── Middlewares/              # Middleware personalizado
├── Validators/               # Validadores FluentValidation
├── Views/                    # Vistas Razor (MVC)
├── Resources/Files/          # Almacenamiento de archivos
├── wwwroot/                  # Recursos estáticos
├── Program.cs                # Punto de entrada
├── Startup.cs               # Configuración de servicios
└── appsettings.json         # Configuración de aplicación
```

### Sitca.DataAccess (Capa de Datos)

```
Sitca.DataAccess/
├── Data/
│   ├── DataContext.cs        # DbContext principal
│   ├── Repository/           # Implementación del patrón Repository
│   └── DbInitializer.cs      # Inicializador de base de datos
├── Migrations/               # 50+ migraciones EF Core
├── Services/                 # Servicios de negocio
│   ├── EmailService/         # Servicio de email
│   ├── PdfService/          # Generación de PDFs
│   ├── FileService/         # Gestión de archivos
│   ├── NotificationService/ # Sistema de notificaciones
│   └── JobsService/         # Trabajos en segundo plano
├── Configurations/          # Configuraciones EF Core
├── Extensions/              # Métodos de extensión
└── Helpers/                 # Clases auxiliares
```

### Sitca.Models (Modelos de Dominio)

```
Sitca.Models/
├── DTOs/                    # Data Transfer Objects
├── ViewModels/              # Modelos para vistas
├── Enums/                   # Enumeraciones del sistema
├── Mappers/                 # Mapeadores de objetos
├── Constants/               # Constantes del sistema
└── [Entity Models]          # Entidades de dominio
    ├── ApplicationUser.cs
    ├── Empresa.cs
    ├── ProcesoCertificacion.cs
    ├── Cuestionario.cs
    └── ...
```

### Utilities (Utilidades Compartidas)

```
Utilities/
├── Authorization/           # Políticas de autorización
├── Constants/              # Constantes compartidas
├── Extensions/             # Métodos de extensión
└── Helpers/               # Clases auxiliares
```

### Scripts de Base de Datos

```
Scripts/
├── 00_ExecuteAll.sql       # Script maestro
├── 01_BaseData.sql         # Datos base
├── 02_TestCompanies.sql    # Empresas de prueba
├── 03_AdvisorUser.sql      # Usuarios asesores
├── 04_CertificationProcesses.sql  # Procesos de certificación
└── 05_Questionnaires.sql   # Cuestionarios
```

## Modelos de Base de Datos

### Entidades Principales

#### Gestión de Usuarios

**ApplicationUser** (Hereda de IdentityUser)
```csharp
public class ApplicationUser : IdentityUser
{
    // Información personal
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Codigo { get; set; }
    public string Direccion { get; set; }
    
    // Información profesional
    public string NumeroCarnet { get; set; }
    public string Profesion { get; set; }
    public string Nacionalidad { get; set; }
    public string DocumentoIdentidad { get; set; }
    
    // Propiedades del sistema
    public bool Active { get; set; }
    public bool Notificaciones { get; set; }
    public string Lenguage { get; set; }
    
    // Relaciones
    public int? PaisId { get; set; }
    public Pais Pais { get; set; }
    public int? CompAuditoraId { get; set; }
    public CompAuditoras CompAuditora { get; set; }
}
```

#### Multi-tenancy

**Pais** (Países para multi-tenancy)
```csharp
public class Pais
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool Active { get; set; }
    
    // Relaciones
    public virtual ICollection<Empresa> Empresas { get; set; }
    public virtual ICollection<ApplicationUser> Users { get; set; }
}
```

#### Entidades de Negocio Core

**Empresa** (Empresas a certificar)
```csharp
public class Empresa : AuditableEntity
{
    // Información básica
    public string Nombre { get; set; }
    public string NombreRepresentante { get; set; }
    public string CargoRepresentante { get; set; }
    
    // Ubicación
    public string Ciudad { get; set; }
    public string Calle { get; set; }
    public string Direccion { get; set; }
    public double? Longitud { get; set; }
    public double? Latitud { get; set; }
    
    // Contacto
    public string Email { get; set; }
    public string Telefono { get; set; }
    public string WebSite { get; set; }
    
    // Datos de negocio
    public string IdNacional { get; set; }
    public int Estado { get; set; }
    public int? ResultadoSugerido { get; set; }
    public int? ResultadoActual { get; set; }
    
    // Relaciones
    public int PaisId { get; set; }
    public Pais Pais { get; set; }
    public virtual ICollection<TipologiasEmpresa> Tipologias { get; set; }
    public virtual ICollection<Archivo> Archivos { get; set; }
    public virtual ICollection<ProcesoCertificacion> Certificaciones { get; set; }
}
```

**ProcesoCertificacion** (Proceso de certificación principal)
```csharp
public class ProcesoCertificacion : AuditableEntity
{
    // Identificación del proceso
    public string NumeroExpediente { get; set; }
    public int Status { get; set; }
    
    // Fechas del proceso
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
    public DateTime? FechaSolicitudAuditoria { get; set; }
    public DateTime? FechaFijadaAuditoria { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    
    // Propiedades del proceso
    public bool Recertificacion { get; set; }
    public int? Cantidad { get; set; }
    
    // Relaciones clave
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; }
    public string AsesorProcesoId { get; set; }
    public ApplicationUser AsesorProceso { get; set; }
    public string AuditorProcesoId { get; set; }
    public ApplicationUser AuditorProceso { get; set; }
    public int TipologiaId { get; set; }
    public Tipologia Tipologia { get; set; }
    
    // Colecciones
    public virtual ICollection<ResultadoCertificacion> Resultados { get; set; }
    public virtual ICollection<Cuestionario> Cuestionarios { get; set; }
    public virtual ICollection<ProcesoArchivos> ProcesosArchivos { get; set; }
}
```

#### Sistema de Cuestionarios y Evaluación

**Tipologia** (Tipos de negocio turístico)
```csharp
public class Tipologia
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string NameEnglish { get; set; }
    public bool Active { get; set; }
    
    // Relaciones
    public virtual ICollection<TipologiasEmpresa> Empresas { get; set; }
    public virtual ICollection<Cuestionario> Cuestionarios { get; set; }
    public virtual ICollection<Modulo> Modulos { get; set; }
}
```

**Modulo** (Módulos de evaluación)
```csharp
public class Modulo
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string EnglishName { get; set; }
    public bool Transversal { get; set; }
    public int Orden { get; set; }
    public string Nomenclatura { get; set; }
    
    // Relaciones
    public int TipologiaId { get; set; }
    public Tipologia Tipologia { get; set; }
    public virtual ICollection<SeccionModulo> Secciones { get; set; }
    public virtual ICollection<Pregunta> Preguntas { get; set; }
}
```

**Pregunta** (Preguntas individuales)
```csharp
public class Pregunta
{
    public int Id { get; set; }
    public string Texto { get; set; }        // Español
    public string Text { get; set; }         // Inglés
    public bool NoAplica { get; set; }
    public bool Obligatoria { get; set; }
    public int Status { get; set; }
    public string Nomenclatura { get; set; }
    public int Orden { get; set; }
    
    // Relaciones jerárquicas
    public int TipologiaId { get; set; }
    public Tipologia Tipologia { get; set; }
    public int ModuloId { get; set; }
    public Modulo Modulo { get; set; }
    public int? SeccionModuloId { get; set; }
    public SeccionModulo SeccionModulo { get; set; }
    public int? SubtituloSeccionId { get; set; }
    public SubtituloSeccion SubtituloSeccion { get; set; }
}
```

**Cuestionario** (Instancia de evaluación)
```csharp
public class Cuestionario
{
    public int Id { get; set; }
    
    // Fechas del cuestionario
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaGenerado { get; set; }
    public DateTime? FechaVisita { get; set; }
    public DateTime? FechaFinalizado { get; set; }
    public DateTime? FechaRevisionAuditor { get; set; }
    
    // Propiedades
    public bool Prueba { get; set; }
    public double? Resultado { get; set; }
    
    // Usuarios asignados
    public string AsesorId { get; set; }
    public string AuditorId { get; set; }
    public string TecnicoPaisId { get; set; }
    
    // Relaciones
    public int TipologiaId { get; set; }
    public Tipologia Tipologia { get; set; }
    public int ProcesoCertificacionId { get; set; }
    public ProcesoCertificacion Certificacion { get; set; }
}
```

#### Gestión de Archivos

**Archivo** (Sistema de archivos general)
```csharp
public class Archivo
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Ruta { get; set; }
    public string Tipo { get; set; }
    public DateTime FechaCarga { get; set; }
    public FileTypesCompany FileTypesCompany { get; set; }
    
    // Relaciones
    public string UsuarioCargaId { get; set; }
    public ApplicationUser UsuarioCarga { get; set; }
    public string UsuarioId { get; set; }
    public ApplicationUser Usuario { get; set; }
    public int? CuestionarioItemId { get; set; }
    public CuestionarioItem CuestionarioItem { get; set; }
    public int? EmpresaId { get; set; }
    public Empresa Empresa { get; set; }
}
```

#### Auditorías Cruzadas

**CrossCountryAuditRequest** (Solicitudes de auditoría entre países)
```csharp
public class CrossCountryAuditRequest : AuditableEntity
{
    // Países involucrados
    public int RequestingCountryId { get; set; }
    public Pais RequestingCountry { get; set; }
    public int ApprovingCountryId { get; set; }
    public Pais ApprovingCountry { get; set; }
    
    // Estado y asignación
    public CrossCountryAuditRequestStatus Status { get; set; }
    public string AssignedAuditorId { get; set; }
    public ApplicationUser AssignedAuditor { get; set; }
    public DateTime? DeadlineDate { get; set; }
    
    // Notas
    public string NotesRequest { get; set; }
    public string NotesApproval { get; set; }
}
```

### Clase Base Auditable

**AuditableEntity** (Base para auditoría)
```csharp
public abstract class AuditableEntity
{
    public bool Enabled { get; set; } = true;
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navegación
    public virtual ApplicationUser UserCreate { get; set; }
}
```

### Enumeraciones Principales

**CertificationStatus** (Estados de certificación)
```csharp
public enum CertificationStatus
{
    Initial = 1,
    ToBeAdvised = 2,
    AdvisingInProcess = 3,
    AdvisingFinalized = 4,
    ToBeAudited = 5,
    AuditingInProcess = 6,
    AuditingFinalized = 7,
    UnderCTCReview = 8,
    Ended = 9
}
```

**FileCompany** (Tipos de documentos empresariales)
```csharp
public enum FileCompany
{
    Informativo = 1,
    Adhesion = 2,
    AuditoraCTC = 3,
    ComprosimoConfidencialidad = 4,
    DeclaracionJurada = 5,
    SolicitudCertificacion = 6
    // ... más tipos
}
```

### Relaciones Clave del Modelo

1. **Multi-tenancy**: `Pais` → `ApplicationUser`, `Empresa`
2. **Certificación**: `Empresa` → `ProcesoCertificacion` → `Cuestionario`
3. **Evaluación**: `Tipologia` → `Modulo` → `Pregunta` → `CuestionarioItem`
4. **Archivos**: `Archivo` puede relacionarse con `Empresa`, `CuestionarioItem`, etc.
5. **Auditoría**: `AuditableEntity` proporciona trazabilidad en todas las entidades importantes

## API y Controladores

### Arquitectura de API

La API sigue principios RESTful con 21 controladores organizados por dominio funcional:

#### Controladores de Autenticación
- **AuthController**: Gestión completa de usuarios y autenticación
- **AuthenticationController**: Información específica de países
- **AccountController**: Controlador base (implementación mínima)

#### Controladores de Negocio Core
- **EmpresaController**: Gestión básica de empresas
- **EmpresasController**: Operaciones extendidas de empresas
- **CertificacionController**: Procesos de certificación
- **ProcesoCertificacionController**: Operaciones específicas de procesos
- **CuestionarioController**: Gestión de cuestionarios

#### Controladores de Archivos y Reportes
- **FileManagerController**: Gestión de archivos con optimización
- **ProcesoArchivosController**: Archivos específicos de procesos
- **ReportPdfController**: Generación de reportes PDF

#### Controladores Especializados
- **CrossCountryAuditController**: Auditorías entre países
- **HomologacionController**: Procesos de homologación
- **CompaniaAuditoraController**: Empresas auditoras
- **CapacitacionesController**: Gestión de capacitaciones
- **ProfesionalesController**: Directorio de profesionales

### Endpoints Principales por Funcionalidad

#### Autenticación y Usuarios
```http
POST   /auth/login                    # Autenticación de usuario
POST   /auth/register                 # Registro de usuario
POST   /auth/renewToken              # Renovación de token
GET    /auth/GetUsers                # Listado de usuarios
POST   /auth/save-user               # Crear usuario (Admin/TecnicoPais)
PUT    /auth/update-user             # Actualizar usuario
GET    /auth/GetRoles                # Roles disponibles
```

#### Gestión de Empresas
```http
GET    /api/Empresa/{idPais}         # Empresas por país
POST   /api/Empresa/list            # Listado filtrado
GET    /api/Empresa/Details/{id}    # Detalles de empresa
POST   /api/Empresa/ActualizarDatos # Actualizar datos
DELETE /api/Empresa/{id}            # Eliminar empresa
```

#### Procesos de Certificación
```http
POST   /api/Certificacion/GenerarCuestionario     # Generar cuestionario
GET    /api/Certificacion/GetCuestionario         # Obtener cuestionario
POST   /api/Certificacion/FinCuestionario         # Finalizar cuestionario
POST   /api/Certificacion/SaveCalificacion        # Guardar calificaciones
POST   /api/Certificacion/CambiarAuditor          # Cambiar auditor asignado
```

#### Gestión de Archivos
```http
POST   /api/FileManager                           # Subir archivos
GET    /api/FileManager/DeleteFile               # Eliminar archivos
POST   /api/FileManager/GetFiles                 # Obtener listado
POST   /api/FileManager/UploadMyFile             # Subir archivos personales
```

#### Generación de Reportes
```http
GET    /api/reports/empresas/{empresaId}/recomendacion-ctc    # PDF recomendación CTC
GET    /api/reports/empresas/{empresaId}/dictamen-tecnico     # PDF dictamen técnico
GET    /api/reports/cuestionarios/{cuestionarioId}/certificacion  # Reporte certificación
GET    /api/reports/users/current/declaracion-jurada         # Declaración jurada
```

### Patrones de Autorización

#### JWT Bearer Authentication
```csharp
[Authorize] // Requiere autenticación JWT
public class EmpresaController : ControllerBase
```

#### Autorización Basada en Roles
```csharp
[Authorize(Roles = "Admin,TecnicoPais")]
[Authorize(Roles = AuthorizationPolicies.Empresa.AdminTecnico)]
[Authorize(Roles = Constants.Roles.Admin)]
```

#### Roles del Sistema
- **Admin**: Administradores del sistema
- **TecnicoPais**: Técnicos por país
- **Auditor**: Auditores de certificación
- **Asesor**: Asesores/Consultores
- **Empresa**: Usuarios de empresas
- **CTC**: Comités técnicos
- **ATP**: Proveedores de asistencia técnica

### Modelos de Respuesta Estándar

#### Resultado Genérico
```csharp
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T Value { get; set; }
    public string Error { get; set; }
}
```

#### Respuesta API
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
}
```

### Características Técnicas

#### Procesamiento de Archivos
- Optimización automática de imágenes con ImageMagick
- Validación de tipos y tamaños de archivo
- Redimensionamiento automático para rendimiento
- Soporte para múltiples formatos (PDF, Office, imágenes)

#### Generación de PDFs
- Conversión HTML a PDF con wkhtmltopdf
- Plantillas dinámicas basadas en datos
- Soporte multi-idioma en documentos
- Inyección de contenido personalizado

#### Consideraciones de Rendimiento
- Paginación por bloques para grandes conjuntos de datos
- Operaciones asíncronas en toda la API
- Consultas optimizadas vía patrón Repository
- Procesamiento optimizado para archivos grandes

## Configuración y Despliegue

### Archivo de Configuración Principal (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SitcaDb;Trusted_Connection=true"
  },
  "Jwt": {
    "Issuer": "https://localhost:44367/",
    "Audience": "https://localhost:44367/",
    "SecretKey": "[SECRET_KEY]"
  },
  "AllowedHosts": "*",
  "AllowedOrigins": ["http://localhost:4200", "https://localhost:4200"],
  "Brevo": {
    "ApiKey": "[BREVO_API_KEY]",
    "SenderEmail": "noreply@sitca.org",
    "SenderName": "SITCA System"
  },
  "Hangfire": {
    "ConnectionString": "[HANGFIRE_CONNECTION]"
  },
  "FileStorage": {
    "BasePath": "Resources/Files",
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx"]
  }
}
```

### Variables de Entorno

#### Variables de Desarrollo
```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:44367;http://localhost:5000
```

#### Variables de Producción
```bash
ASPNETCORE_ENVIRONMENT=Production
SITCA_CONNECTION_STRING="[PRODUCTION_DB_CONNECTION]"
SITCA_JWT_SECRET="[PRODUCTION_JWT_SECRET]"
SITCA_BREVO_API_KEY="[PRODUCTION_BREVO_KEY]"
```

### Configuración de Servicios (Startup.cs)

#### Servicios de Base de Datos
```csharp
services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connectionString));

services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();
```

#### Configuración JWT
```csharp
services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
```

#### Configuración Hangfire
```csharp
services.AddHangfire(configuration =>
    configuration.UseSqlServerStorage(connectionString));
services.AddHangfireServer();
```

#### CORS
```csharp
services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins(allowedOrigins)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});
```

### Requisitos del Sistema

#### Servidor de Aplicación
- **.NET 8.0 Runtime**: Requerido para ejecutar la aplicación
- **IIS o Kestrel**: Servidor web (IIS para Windows, Kestrel multiplataforma)
- **SQL Server 2019+**: Base de datos (puede ser LocalDB para desarrollo)

#### Dependencias Externas
- **wkhtmltopdf**: Binarios nativos para generación de PDF
  - Windows: `wkhtmltopdf.exe`
  - Linux: `wkhtmltopdf`
  - macOS: `wkhtmltopdf`

#### Almacenamiento
- **Espacio en disco**: Mínimo 2GB para archivos de usuario
- **Estructura de carpetas**: `Resources/Files/` con permisos de escritura

### Proceso de Despliegue

#### 1. Preparación de Base de Datos
```bash
# Aplicar migraciones
dotnet ef database update --project Sitca.DataAccess --startup-project Sitca

# Ejecutar scripts de datos iniciales
sqlcmd -S [SERVER] -d [DATABASE] -i Scripts/00_ExecuteAll.sql
```

#### 2. Publicación de Aplicación
```bash
# Publicar para producción
dotnet publish -c Release -o ./publish

# Copiar archivos al servidor
# Configurar IIS/Kestrel
# Configurar variables de entorno
```

#### 3. Configuración de Servicios
```bash
# Configurar servicio Windows (opcional)
sc create SitcaApi binPath="dotnet Sitca.dll"

# O configurar como servicio systemd en Linux
```

#### 4. Verificación Post-Despliegue
- Verificar conectividad a base de datos
- Probar endpoints de salud
- Verificar funcionamiento de Hangfire
- Probar generación de PDFs
- Verificar envío de emails

## Comandos Esenciales

### Desarrollo

#### Construcción y Ejecución
```bash
# Construir toda la solución
dotnet build

# Ejecutar la API (puerto predeterminado: https://localhost:44367)
dotnet run --project Sitca/Sitca.csproj

# Ejecutar con entorno específico
ASPNETCORE_ENVIRONMENT=Development dotnet run --project Sitca/Sitca.csproj
```

#### Gestión de Base de Datos
```bash
# Agregar nueva migración
dotnet ef migrations add [NombreMigracion] --project Sitca.DataAccess --startup-project Sitca

# Actualizar base de datos con migraciones
dotnet ef database update --project Sitca.DataAccess --startup-project Sitca

# Remover última migración (si no se ha aplicado)
dotnet ef migrations remove --project Sitca.DataAccess --startup-project Sitca

# Generar script SQL de migración
dotnet ef migrations script --project Sitca.DataAccess --startup-project Sitca
```

#### Depuración y Pruebas
```bash
# Ejecutar en modo debug
dotnet run --project Sitca --configuration Debug

# Limpiar y reconstruir
dotnet clean && dotnet build

# Verificar estado de la aplicación
curl https://localhost:44367/api/health
```

### Producción

#### Publicación
```bash
# Publicar para producción
dotnet publish -c Release

# Publicar para plataforma específica
dotnet publish -c Release -r win-x64 --self-contained true
dotnet publish -c Release -r linux-x64 --self-contained true
```

#### Monitoreo
```bash
# Ver logs de aplicación
tail -f /var/log/sitca/application.log

# Verificar estado de servicios
systemctl status sitca-api
```

### Utilidades de Base de Datos

#### Backup y Restore
```sql
-- Backup de base de datos
BACKUP DATABASE SitcaDb TO DISK = 'C:\Backups\SitcaDb.bak'

-- Restore de base de datos
RESTORE DATABASE SitcaDb FROM DISK = 'C:\Backups\SitcaDb.bak'
```

#### Índices Pendientes (Performance)
```sql
-- Índices recomendados para optimización
CREATE NONCLUSTERED INDEX IX_AspNetUsers_PaisId_Active_Notificaciones 
ON AspNetUsers(PaisId, Active, Notificaciones) 
INCLUDE (Email, FirstName, LastName, Lenguage);

CREATE INDEX IX_Cuestionario_ProcesoCertificacionId_Prueba 
ON Cuestionario(ProcesoCertificacionId, Prueba)
INCLUDE (FechaRevisionAuditor);
```

## Patrones de Desarrollo

### Patrón Repository

#### Interfaz Repository
```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<IEnumerable<T>> GetWithSpecificationAsync(ISpecification<T> spec);
}
```

#### Unit of Work
```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<Empresa> Empresas { get; }
    IRepository<ProcesoCertificacion> ProcesosCertificacion { get; }
    IRepository<Cuestionario> Cuestionarios { get; }
    // ... otros repositorios
    
    Task<int> CompleteAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

### Patrón Service Layer

#### Servicio de Negocio
```csharp
public class CertificacionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    
    public async Task<Result<Cuestionario>> GenerarCuestionarioAsync(
        int empresaId, int tipologiaId, string auditorId)
    {
        try
        {
            // Lógica de negocio
            var cuestionario = new Cuestionario
            {
                EmpresaId = empresaId,
                TipologiaId = tipologiaId,
                AuditorId = auditorId,
                FechaGenerado = DateTime.UtcNow
            };
            
            await _unitOfWork.Cuestionarios.AddAsync(cuestionario);
            await _unitOfWork.CompleteAsync();
            
            // Notificación automática
            await _notificationService.NotificarNuevoCuestionarioAsync(cuestionario);
            
            return Result<Cuestionario>.Success(cuestionario);
        }
        catch (Exception ex)
        {
            return Result<Cuestionario>.Failure(ex.Message);
        }
    }
}
```

### Patrón Specification

#### Especificación para Consultas Complejas
```csharp
public class EmpresasPorPaisYEstadoSpec : ISpecification<Empresa>
{
    private readonly int _paisId;
    private readonly int _estado;
    
    public EmpresasPorPaisYEstadoSpec(int paisId, int estado)
    {
        _paisId = paisId;
        _estado = estado;
    }
    
    public Expression<Func<Empresa, bool>> Criteria =>
        empresa => empresa.PaisId == _paisId && empresa.Estado == _estado;
        
    public List<Expression<Func<Empresa, object>>> Includes =>
        new List<Expression<Func<Empresa, object>>>
        {
            empresa => empresa.Pais,
            empresa => empresa.Tipologias,
            empresa => empresa.Certificaciones
        };
}
```

### Middleware Personalizado

#### Middleware de Manejo de Errores
```csharp
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción no manejada");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            BusinessException => new { error = exception.Message, statusCode = 400 },
            ValidationException => new { error = exception.Message, statusCode = 422 },
            _ => new { error = "Error interno del servidor", statusCode = 500 }
        };
        
        context.Response.StatusCode = response.statusCode;
        await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
}
```

### Extensiones de Controller

#### Extensiones para Manejo de Usuario Actual
```csharp
public static class ControllerExtensions
{
    public static async Task<ApplicationUser> GetCurrentUserAsync(
        this ControllerBase controller, UserManager<ApplicationUser> userManager)
    {
        var email = controller.User.FindFirst(ClaimTypes.Email)?.Value;
        return await userManager.FindByEmailAsync(email);
    }
    
    public static async Task<(ApplicationUser user, string role)> GetCurrentUserWithRoleAsync(
        this ControllerBase controller, UserManager<ApplicationUser> userManager)
    {
        var user = await controller.GetCurrentUserAsync(userManager);
        var roles = await userManager.GetRolesAsync(user);
        return (user, roles.FirstOrDefault());
    }
    
    public static IActionResult HandleResponse<T>(this ControllerBase controller, Result<T> result)
    {
        return result.IsSuccess 
            ? controller.Ok(new ApiResponse<T> { Success = true, Data = result.Value })
            : controller.BadRequest(new ApiResponse<T> { Success = false, Message = result.Error });
    }
}
```

### Trabajos en Segundo Plano con Hangfire

#### Servicio de Trabajos
```csharp
public class JobsService
{
    public void ProgramarRecordatorioAuditoria(int procesoId, DateTime fechaRecordatorio)
    {
        BackgroundJob.Schedule(() => 
            EnviarRecordatorioAuditoria(procesoId), fechaRecordatorio);
    }
    
    [JobDisplayName("Recordatorio de Auditoría - Proceso {0}")]
    public async Task EnviarRecordatorioAuditoria(int procesoId)
    {
        // Lógica de envío de recordatorio
        var proceso = await _unitOfWork.ProcesosCertificacion.GetByIdAsync(procesoId);
        await _emailService.EnviarRecordatorioAsync(proceso);
    }
    
    [Cron("0 9 * * MON")] // Cada lunes a las 9 AM
    public async Task GenerarReporteSemanal()
    {
        // Generar reporte semanal automático
        var reporte = await _reportService.GenerarReporteSemanalAsync();
        await _emailService.EnviarReporteSemanalAsync(reporte);
    }
}
```

## Consideraciones de Seguridad

### Autenticación y Autorización

#### JWT Token Security
- **Secreto del Token**: Usar claves fuertes y rotarlas periódicamente
- **Tiempo de Expiración**: Tokens con tiempo de vida limitado (24 horas)
- **Renovación Automática**: Mecanismo de renovación de tokens
- **Revocación**: Capacidad de invalidar tokens comprometidos

#### Protección de Endpoints
```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> EliminarEmpresa(int id)
{
    // Verificación adicional de propiedad del recurso
    var empresa = await _unitOfWork.Empresas.GetByIdAsync(id);
    var currentUser = await this.GetCurrentUserAsync(_userManager);
    
    if (empresa.PaisId != currentUser.PaisId && !User.IsInRole("Admin"))
    {
        return Forbid("No tiene permisos para eliminar esta empresa");
    }
    
    // Proceder con eliminación
}
```

### Validación de Datos

#### Validación de Entrada
```csharp
public class EmpresaValidator : AbstractValidator<EmpresaUpdateVm>
{
    public EmpresaValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");
            
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Formato de email inválido")
            .When(x => !string.IsNullOrEmpty(x.Email));
            
        RuleFor(x => x.Telefono)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Formato de teléfono inválido")
            .When(x => !string.IsNullOrEmpty(x.Telefono));
    }
}
```

#### Sanitización de Archivos
```csharp
public class FileValidationService
{
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };
    private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB
    
    public async Task<bool> ValidarArchivoAsync(IFormFile file)
    {
        // Validar extensión
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return false;
            
        // Validar tamaño
        if (file.Length > _maxFileSize)
            return false;
            
        // Validar contenido (magic numbers)
        using var stream = file.OpenReadStream();
        var buffer = new byte[4];
        await stream.ReadAsync(buffer, 0, 4);
        
        return ValidarMagicNumbers(buffer, extension);
    }
}
```

### Protección de Datos

#### Encriptación de Datos Sensibles
```csharp
public class DataProtectionService
{
    private readonly IDataProtector _protector;
    
    public string EncriptarDatos(string datos)
    {
        return _protector.Protect(datos);
    }
    
    public string DesencriptarDatos(string datosEncriptados)
    {
        return _protector.Unprotect(datosEncriptados);
    }
}
```

#### Auditoría y Logging
```csharp
public class AuditService
{
    public async Task RegistrarAccionAsync(string usuarioId, string accion, 
        string entidad, int entidadId, object datosAnteriores = null, object datosNuevos = null)
    {
        var logEntry = new ActivityLog
        {
            Date = DateTime.UtcNow,
            User = usuarioId,
            Observaciones = $"{accion} en {entidad} ({entidadId})",
            DatosAnteriores = JsonConvert.SerializeObject(datosAnteriores),
            DatosNuevos = JsonConvert.SerializeObject(datosNuevos)
        };
        
        await _unitOfWork.ActivityLogs.AddAsync(logEntry);
        await _unitOfWork.CompleteAsync();
    }
}
```

### Configuración de Seguridad

#### Headers de Seguridad
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseHsts(); // HTTP Strict Transport Security
    app.UseHttpsRedirection();
    
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        await next();
    });
}
```

#### Rate Limiting
```csharp
services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 100
        },
        new RateLimitRule
        {
            Endpoint = "POST:/auth/login",
            Period = "1m",
            Limit = 5
        }
    };
});
```

### Mejores Prácticas de Seguridad

1. **Principio de Menor Privilegio**: Usuarios solo tienen permisos mínimos necesarios
2. **Validación en Servidor**: Nunca confiar en validación del cliente únicamente
3. **Sanitización de Entrada**: Limpiar todos los datos de entrada
4. **Logging Comprensivo**: Registrar todas las acciones sensibles
5. **Actualizaciones Regulares**: Mantener dependencias actualizadas
6. **Backup Seguro**: Backups encriptados y almacenados de forma segura
7. **Monitoreo Continuo**: Vigilancia de actividades sospechosas
8. **Pruebas de Seguridad**: Pruebas regulares de penetración y vulnerabilidades

---

Este manual técnico proporciona una guía completa para desarrolladores que trabajen con el backend de SITCA 2.0. Para consultas específicas o actualizaciones, consulte la documentación en línea o contacte al equipo de desarrollo.

**Versión del Manual**: 1.0  
**Fecha de Actualización**: Enero 2025  
**Versión del Sistema**: SITCA 2.0 - .NET 8.0