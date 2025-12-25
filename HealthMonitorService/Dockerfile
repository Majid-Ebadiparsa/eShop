FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["HealthMonitorService/HealthMonitorService.csproj", "HealthMonitorService/"]
RUN dotnet restore "HealthMonitorService/HealthMonitorService.csproj"
COPY . .
WORKDIR "/src/HealthMonitorService"
RUN dotnet build "HealthMonitorService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HealthMonitorService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HealthMonitorService.dll"]

