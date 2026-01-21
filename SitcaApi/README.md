# SITCA API - Backend .NET Core 8.0

API REST para el Sistema Informático Técnico de Certificación y Acreditación.

## Stack Tecnológico

| Componente | Tecnología |
|------------|------------|
| Framework | .NET Core 8.0 |
| ORM | Entity Framework Core 8.0 |
| Query Builder | Dapper |
| Database | SQL Server 2019 |
| Auth | JWT Bearer |
| Validation | FluentValidation |
| Jobs | Hangfire |
| Docs | Swagger/OpenAPI |

## Inicio Rápido

### Con Docker (Recomendado)

El API se ejecuta como parte del stack completo desde el proyecto Angular:

```bash
cd /Users/enzo/Documents/sitca/angular
docker-compose up --build
```

**URLs:**
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

### Desarrollo Local

```bash
# Restaurar dependencias
dotnet restore

# Ejecutar con hot-reload
dotnet watch run --project Sitca/Sitca.csproj

# O ejecutar sin hot-reload
dotnet run --project Sitca/Sitca.csproj
```

**Requisitos:**
- .NET SDK 8.0
- SQL Server (local o Docker)

## Estructura del Proyecto

```
/SitcaApi/
├── Sitca/                      # Proyecto principal (API)
│   ├── Controllers/            # Endpoints REST
│   ├── Extensions/             # Configuración de servicios
│   ├── Middlewares/            # Middleware personalizado
│   ├── Validators/             # FluentValidation
│   ├── Views/                  # Vistas Razor (emails, reportes)
│   ├── wwwroot/                # Archivos estáticos
│   ├── Resources/              # Almacenamiento de archivos
│   ├── appsettings.json        # Configuración producción
│   ├── appsettings.Development.json
│   ├── Program.cs              # Entry point
│   └── Startup.cs              # Configuración de servicios
│
├── Sitca.DataAccess/           # Capa de acceso a datos
│   ├── Data/                   # DbContext, configuraciones
│   └── Services/               # Repositorios y servicios
│
├── Sitca.Models/               # Modelos de dominio
│   ├── DTOs/                   # Data Transfer Objects
│   ├── Entities/               # Entidades de BD
│   └── ViewModels/             # View Models
│
├── Utilities/                  # Utilidades compartidas
│
├── Dockerfile                  # Build de producción
├── Dockerfile.dev              # Desarrollo con hot-reload
└── .dockerignore
```

## Configuración Docker

### Dockerfile.dev (Desarrollo)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0
# Hot-reload con dotnet watch
CMD ["dotnet", "watch", "run", "--no-launch-profile"]
```

### Dockerfile (Producción)

```dockerfile
# Multi-stage build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
```

## Variables de Entorno

| Variable | Descripción | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Entorno | Development |
| `ASPNETCORE_URLS` | URLs de escucha | http://+:5000 |
| `ConnectionStrings__DefaultConnection` | Conexión BD | (ver docker-compose) |
| `DOTNET_USE_POLLING_FILE_WATCHER` | Polling para hot-reload | true |

## Base de Datos

### Connection Strings

**Docker:**
```
Server=sqlserver;Database=Sitca;User Id=sa;Password=Passw0rd.4859;TrustServerCertificate=true;
```

**Local:**
```
Server=localhost;Database=Sitca;User Id=sa;Password=Passw0rd.4859;TrustServerCertificate=true;
```

### Migraciones

```bash
# Crear migración
dotnet ef migrations add NombreMigracion --project Sitca.DataAccess --startup-project Sitca

# Aplicar migraciones
dotnet ef database update --project Sitca.DataAccess --startup-project Sitca
```

## Autenticación

- JWT Bearer tokens
- Configuración en `appsettings.json` → `Jwt` section
- Identity con `ApplicationUser` y `ApplicationRole`

## Endpoints Principales

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/auth/login` | Autenticación |
| GET | `/api/empresas` | Listar empresas |
| GET | `/api/certificaciones` | Listar certificaciones |
| ... | `/swagger` | Documentación completa |

## Background Jobs (Hangfire)

- Dashboard: http://localhost:5000/hangfire (en desarrollo)
- Jobs recurrentes para notificaciones
- Configurado con almacenamiento en memoria

## Dependencias Principales

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.8" />
<PackageReference Include="Dapper" Version="2.1.35" />
<PackageReference Include="FluentValidation" Version="11.10.0" />
<PackageReference Include="Hangfire" Version="1.8.14" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.8" />
```

## Build y Publicación

```bash
# Build
dotnet build -c Release

# Publicar
dotnet publish -c Release -o ./publish

# Docker
docker build -t sitca-api .
```

## Testing

```bash
# Ejecutar tests
dotnet test

# Con coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Troubleshooting

### API no conecta a SQL Server
```bash
# Verificar que SQL Server esté corriendo
docker-compose logs sqlserver

# Verificar healthcheck
docker-compose ps
```

### Hot-reload no funciona
```bash
# Reconstruir imagen
docker-compose up --build api
```

### Error de certificado SSL
Agregar `TrustServerCertificate=true` al connection string.

## Notas Importantes

- El API espera a que SQL Server pase el healthcheck antes de iniciar
- En desarrollo usa `dotnet watch` para hot-reload automático
- Los archivos se almacenan en `/Resources/Files`
- CORS configurado para `localhost:4200`
