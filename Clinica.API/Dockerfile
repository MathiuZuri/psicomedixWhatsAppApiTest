
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de solución y proyectos para restaurar de forma quirúrgica
COPY ["Clinica.sln", "./"]
COPY ["Clinica.API/Clinica.API.csproj", "Clinica.API/"]
COPY ["Clinica.Domain/Clinica.Domain.csproj", "Clinica.Domain/"]
COPY ["Clinica.Infrastructure/Clinica.Infrastructure.csproj", "Clinica.Infrastructure/"]

# Restaurar dependencias (se cachea si los .csproj no cambian)
RUN dotnet restore

COPY Clinica.API/ ./Clinica.API/
COPY Clinica.Domain/ ./Clinica.Domain/
COPY Clinica.Infrastructure/ ./Clinica.Infrastructure/

# Publicar la aplicación en configuración de producción
RUN dotnet publish "Clinica.API/Clinica.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Activamos el usuario seguro sin privilegios de root
USER app

# Copiamos con dueño seguro (chown) y permisos de solo lectura/ejecución (chmod)
COPY --from=build --chown=app:app --chmod=550 /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Clinica.API.dll"]