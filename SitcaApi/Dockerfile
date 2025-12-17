# Dockerfile para SITCA API (.NET 8.0) - Producción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copiar archivos de proyecto para restaurar dependencias
COPY ["Sitca/Sitca.csproj", "Sitca/"]
COPY ["Sitca.DataAccess/Sitca.DataAccess.csproj", "Sitca.DataAccess/"]
COPY ["Sitca.Models/Sitca.Models.csproj", "Sitca.Models/"]
COPY ["Utilities/Utilities.csproj", "Utilities/"]

# Restaurar dependencias
RUN dotnet restore "Sitca/Sitca.csproj"

# Copiar todo el código fuente
COPY . .

# Compilar en modo Release
WORKDIR /src/Sitca
RUN dotnet build -c Release -o /app/build

# Publicar
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Imagen final de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Instalar dependencias para wkhtmltopdf (PDF generation)
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Sitca.dll"]

# =============================================================================
# USO (Producción):
# =============================================================================
#
# docker build -t sitca-api .
# docker run -d -p 5000:5000 \
#   -e ConnectionStrings__DefaultConnection="Server=...;Database=...;" \
#   sitca-api
#
# =============================================================================
