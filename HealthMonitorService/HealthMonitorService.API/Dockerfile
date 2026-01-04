FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all layer projects
COPY ["HealthMonitorService/HealthMonitorService.Domain/HealthMonitorService.Domain.csproj", "HealthMonitorService/HealthMonitorService.Domain/"]
COPY ["HealthMonitorService/HealthMonitorService.Application/HealthMonitorService.Application.csproj", "HealthMonitorService/HealthMonitorService.Application/"]
COPY ["HealthMonitorService/HealthMonitorService.Infrastructure/HealthMonitorService.Infrastructure.csproj", "HealthMonitorService/HealthMonitorService.Infrastructure/"]
COPY ["HealthMonitorService/HealthMonitorService.API/HealthMonitorService.API.csproj", "HealthMonitorService/HealthMonitorService.API/"]

# Copy shared services
COPY ["SharedService/SharedService.Caching/SharedService.Caching.csproj", "SharedService/SharedService.Caching/"]
COPY ["SharedService/SharedService.Contracts/SharedService.Contracts.csproj", "SharedService/SharedService.Contracts/"]

# Restore
RUN dotnet restore "HealthMonitorService/HealthMonitorService.API/HealthMonitorService.API.csproj"

# Copy all source files
COPY . .

# Build
WORKDIR "/src/HealthMonitorService/HealthMonitorService.API"
RUN dotnet build "HealthMonitorService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HealthMonitorService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HealthMonitorService.API.dll"]

