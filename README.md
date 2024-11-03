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
