# Imagen base para correr la app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Imagen para compilar el código
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
# CAMBIA 'PortalAcademico.csproj' por el nombre real de tu archivo de proyecto
COPY ["PortalAcademico.csproj", "."]
RUN dotnet restore "./PortalAcademico.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "PortalAcademico.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PortalAcademico.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PortalAcademico.dll"]