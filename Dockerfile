# Imagen base de .NET 8 para producción
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Imagen SDK para compilar el proyecto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar archivo .csproj y restaurar dependencias
COPY ["Gym_FitByte.csproj", "."]
RUN dotnet restore "./Gym_FitByte.csproj"

# Copiar el resto del código fuente
COPY . .
WORKDIR "/src/."
RUN dotnet build "./Gym_FitByte.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicar el proyecto
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Gym_FitByte.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Imagen final para ejecución
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gym_FitByte.dll"]
