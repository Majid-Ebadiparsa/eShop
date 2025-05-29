# Base image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG CONFIGURATION=Release

WORKDIR /app/inventoryservice


# Copy only .csproj files to leverage Docker cache
COPY ["InventoryService/InventoryService.API/InventoryService.API.csproj", "InventoryService/InventoryService.API/"]
COPY ["InventoryService/InventoryService.Application/InventoryService.Application.csproj", "InventoryService/InventoryService.Application/"]
COPY ["InventoryService/InventoryService.Infrastructure/InventoryService.Infrastructure.csproj", "InventoryService/InventoryService.Infrastructure/"]
COPY ["InventoryService/InventoryService.Domain/InventoryService.Domain.csproj", "InventoryService/InventoryService.Domain/"]
COPY ["InventoryService/InventoryService.Shared/InventoryService.Shared.csproj", "InventoryService/InventoryService.Shared/"]
COPY ["SharedService/SharedService.Contracts/SharedService.Contracts.csproj", "SharedService/SharedService.Contracts/"]

# Restore dependencies (cached if csproj files don't change)
RUN dotnet restore "InventoryService/InventoryService.API/InventoryService.API.csproj"

# Copy the full source code
COPY InventoryService/InventoryService.API/ InventoryService/InventoryService.API/
COPY InventoryService/InventoryService.Application/ InventoryService/InventoryService.Application/
COPY InventoryService/InventoryService.Infrastructure/ InventoryService/InventoryService.Infrastructure/
COPY InventoryService/InventoryService.Domain/ InventoryService/InventoryService.Domain/
COPY InventoryService/InventoryService.Shared/ InventoryService/InventoryService.Shared/
COPY SharedService/SharedService.Contracts/ SharedService/SharedService.Contracts/

# Light restore because in the previouse step, NuGet packages were already restored
RUN dotnet restore "InventoryService/InventoryService.API/InventoryService.API.csproj"

# Build and Publish the application directly
RUN dotnet publish "InventoryService/InventoryService.API/InventoryService.API.csproj" -c $CONFIGURATION -o /out --no-restore

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app/inventoryservice

# Healthcheck to monitor service availability
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s CMD curl --fail http://localhost/health || exit 1

# Copy published output from the build stage
COPY --from=build /out .

# Start the application
ENTRYPOINT ["dotnet", "InventoryService.API.dll"]