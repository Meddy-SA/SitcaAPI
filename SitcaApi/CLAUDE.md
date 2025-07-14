# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SITCA (Sistema Integrado de Certificación) is an ASP.NET Core 8.0 Web API for managing certifications, audits, and compliance for companies across Central American countries. The system handles multi-tenant operations with support for different countries, languages (Spanish/English), and user roles.

## Architecture

The solution follows Clean Architecture with 4 main projects:

- **Sitca**: Main Web API project with controllers, middleware, and authentication
- **Sitca.DataAccess**: Entity Framework Core data layer with repositories and migrations
- **Sitca.Models**: Domain entities, DTOs, view models, and business logic
- **Utilities**: Shared utilities and helper functions

Key patterns:
- Repository pattern for data access
- JWT Bearer authentication with ASP.NET Core Identity
- Hangfire for background jobs (notifications, reminders)
- Multi-tenancy by country (Pais entity)
- Audit trail on entities (AuditableEntity base class)

## Essential Commands

### Build and Run
```bash
# Build entire solution
dotnet build

# Run the API (default: https://localhost:44367)
dotnet run --project Sitca/Sitca.csproj

# Run with specific environment
ASPNETCORE_ENVIRONMENT=Development dotnet run --project Sitca/Sitca.csproj
```

### Database Management
```bash
# Add a new migration
dotnet ef migrations add MigrationName --project Sitca.DataAccess --startup-project Sitca

# Update database with migrations
dotnet ef database update --project Sitca.DataAccess --startup-project Sitca

# Remove last migration (if not applied)
dotnet ef migrations remove --project Sitca.DataAccess --startup-project Sitca
```

### Publishing
```bash
# Build for production
dotnet publish -c Release
```

## Key Technologies

- **Framework**: .NET 8.0, ASP.NET Core
- **Database**: SQL Server with Entity Framework Core 8.0.8
- **Authentication**: JWT tokens with ASP.NET Core Identity
- **Background Jobs**: Hangfire (in-memory storage)
- **Email**: Brevo (Sendinblue) SMTP API
- **PDF Generation**: iText, GemBox.Document, wkhtmltopdf
- **Validation**: FluentValidation
- **Additional ORM**: Dapper for complex queries

## Database Context

The main DbContext is `DataContext` in `Sitca.DataAccess/Data/DataContext.cs`. Key entities include:
- **Empresa**: Companies being certified
- **ProcesoCertificacion**: Certification processes
- **Cuestionario**: Questionnaires for audits
- **Auditor**: Auditors managing certifications
- **Pais**: Countries (multi-tenancy)

## User Roles

- **Admin**: System administrators
- **Auditor**: Certification auditors
- **Empresa**: Company users
- **ATP**: Technical assistance providers
- **Administrador**: Country administrators

## Important Notes

1. **No Test Project**: Currently no automated tests exist. Consider test requirements before implementing new features.

2. **File Storage**: Files are stored in `/Sitca/Resources/Files/` with validation for allowed extensions configured in appsettings.json.

3. **Pending Database Indexes**: The following indexes need to be created (see Sitca.DataAccess/Readme.md):
   ```sql
   CREATE NONCLUSTERED INDEX IX_AspNetUsers_PaisId_Active_Notificaciones 
   ON AspNetUsers(PaisId, Active, Notificaciones) 
   INCLUDE (Email, FirstName, LastName, Lenguage);
   
   CREATE INDEX IX_Cuestionario_ProcesoCertificacionId_Prueba 
   ON Cuestionario(ProcesoCertificacionId, Prueba)
   INCLUDE (FechaRevisionAuditor);
   ```

4. **Configuration**: Main settings in `appsettings.json` include JWT configuration, CORS settings, email configuration, and file storage settings.

5. **Current Branch**: Development is on branch `9-company-centered-reports` with main branch as `main`.