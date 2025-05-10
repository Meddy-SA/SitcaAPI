# SITCA con .NET Core 8.0

## Descripción
SITCA (Sistema de Integración Turística Centroamericana) es una aplicación desarrollada con .NET Core 8.0, diseñada SITCA (Sistema de Integración Turística Centroamericana) es una aplicación desarrollada con .NET Core 8.0, diseñada para gestionar y promover el Sistema Integrado Centroamericano de Calidad y Sostenibilidad Turística (SICCS). El propósito principal de SITCA es llevar un registro detallado de los certificados SICCS, gestionar las evaluaciones de certificados, y facilitar el proceso de certificación turística para las MIPYME (Micro, Pequeñas y Medianas Empresas) en Centroamérica.

## Estructura del Proyecto
La solución SitcaApi está organizada en los siguientes proyectos:

1. **Sitca**: 
   - Contiene los controladores y los controladores de vista.
   - Punto de entrada principal de la aplicación.

2. **Sitca.DataAccess**: 
   - Contiene el contexto de datos, migraciones y servicios.
   - Maneja toda la lógica de acceso a datos y operaciones de base de datos.

3. **Sitca.Models**: 
   - Contiene las clases de datos y DTOs (Data Transfer Objects).
   - Define la estructura de los datos utilizados en toda la aplicación.

4. **Utilities**: 
   - Contiene clases de utilidad.
   - Proporciona funcionalidades comunes y herramientas para otros proyectos.

## Requisitos Previos
- .NET Core SDK 8.0
- SQL Server (versión X.X o superior)
- [Otros requisitos específicos del proyecto]

## Configuración Inicial
1. Clone el repositorio:
   ```
   git clone https://github.com/Meddy-SA/SitcaAPI.git
   ```
2. Navegue al directorio del proyecto:
   ```
   cd SitcaApi
   ```
3. Restaure las dependencias:
   ```
   dotnet restore
   ```

## Configuración de la Base de Datos
Asegúrese de tener la cadena de conexión correcta en el archivo `appsettings.json` o `appsettings.Development.json` en el proyecto Sitca.

## Migraciones
Para aplicar todas las migraciones pendientes, ejecute el siguiente comando desde la raíz de la solución:

```shell
dotnet ef database update --project Sitca.DataAccess/ --startup-project Sitca/
```

Este comando actualizará la base de datos con todas las migraciones pendientes.

## Ejecución del Proyecto
Para ejecutar el proyecto, use el siguiente comando:

```shell
dotnet run --project Sitca/
```

La aplicación estará disponible en `https://localhost:5001` (o el puerto que haya configurado).


    "ApiKey": "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",


# SITCA: Sistema Informático Técnico de Certificación y Acreditación



## Visión General

SITCA es una aplicación web desarrollada en Angular diseñada para gestionar procesos de certificación y acreditación para empresas turísticas. El sistema facilita todo el flujo de trabajo desde la solicitud inicial, consultoría, auditoría, certificación y recertificación.



## Características Principales



### Sistema de Roles de Usuario

- **Administradores**: Gestión completa del sistema

- **Técnicos de País**: Administración a nivel nacional

- **Consultores/Asesores**: Apoyan a empresas en el proceso de certificación

- **Auditores**: Evalúan empresas según estándares establecidos

- **Empresas**: Entidades que buscan certificación

- **Empresas Auditoras**: Organizaciones que proporcionan servicios de auditoría



### Gestión de Procesos de Certificación

- Seguimiento de estados: Inicial, En Proceso, Finalizado

- Gestión de recertificaciones para certificados próximos a vencer

- Asignación de asesores y auditores a empresas



### Gestión de Empresas

- Registro y administración de datos de empresas turísticas

- Categorización por país, tipología (tipo de negocio) y estado

- Visualización de estado actual en el proceso de certificación



### Auditoría y Evaluación

- Flujo de trabajo para auditorías de empresas

- Clara separación entre fases de asesoría y auditoría

- Gestión de personal auditor



### Gestión Documental

- Carga y administración de archivos

- Organización de documentación por proceso y empresa

- Seguimiento de documentos requeridos vs. completados



### Cuestionarios y Evaluaciones

- Módulo de cuestionarios para evaluaciones

- Guías de evaluación específicas según tipo de negocio turístico

- Historial de respuestas y calificaciones



### Reportes

- Generación de informes sobre empresas, certificaciones y auditorías

- Exportación a formato Excel

- Estadísticas y métricas clave



### Internacionalización

- Soporte multilingüe (español e inglés)

- Claves de traducción organizadas por áreas funcionales



### Dashboard y Visualización

- Panel de control con información general

- Listados de empresas con filtros avanzados

- Interfaz basada en Material Design con tablas y componentes responsive



## Características Técnicas



1. **Framework Angular**: Desarrollado en Angular (versión 11)

2. **Autenticación**: Sistema basado en JWT con funcionalidad de login/registro

3. **Diseño Responsivo**: Adaptado para uso en escritorio y dispositivos móviles

4. **Integración API**: Comunicación con backend para operaciones de datos

5. **Arquitectura Modular**: Organizado en módulos funcionales

6. **Soporte Docker**: Configuración para despliegue en contenedores



## Objetivo del Software

SITCA es un sistema especializado para la gestión de certificaciones turísticas, utilizado por organizaciones de estándares o autoridades turísticas. El objetivo principal es facilitar y administrar el proceso de certificación de empresas turísticas según estándares de calidad, con diferentes módulos que apoyan todo el flujo de trabajo desde la solicitud inicial hasta la recertificación.



El sistema permite a los diferentes actores (administradores, auditores, consultores y las propias empresas) interactuar con el proceso de certificación según sus roles, mientras mantiene la documentación, seguimiento del estado y generación de informes sobre las entidades certificadas.
