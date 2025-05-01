# Base image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG CONFIGURATION=Release

WORKDIR /app


# Copy only .csproj files to leverage Docker cache
COPY ["OrderService.API/OrderService.API.csproj", "OrderService.API/"]
COPY ["OrderService.Application/OrderService.Application.csproj", "OrderService.Application/"]
COPY ["OrderService.Infrastructure/OrderService.Infrastructure.csproj", "OrderService.Infrastructure/"]
COPY ["OrderService.Domain/OrderService.Domain.csproj", "OrderService.Domain/"]
COPY ["OrderService.Domain/OrderService.Shared.csproj", "OrderService.Shared/"]

# Restore dependencies (cached if csproj files don't change)
RUN dotnet restore "OrderService.API/OrderService.API.csproj"

# Copy the full source code
COPY OrderService.API/ OrderService.API/
COPY OrderService.Application/ OrderService.Application/
COPY OrderService.Infrastructure/ OrderService.Infrastructure/
COPY OrderService.Domain/ OrderService.Domain/
COPY OrderService.Shared/ OrderService.Shared/

# Light restore because in the previouse step, NuGet packages were already restored
RUN dotnet restore "OrderService.API/OrderService.API.csproj"

# Build and Publish the application directly
RUN dotnet publish "OrderService.API/OrderService.API.csproj" -c $CONFIGURATION -o /out --no-restore

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

# Healthcheck to monitor service availability
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s CMD curl --fail http://localhost/health || exit 1

# Copy published output from the build stage
COPY --from=build /out .

# Start the application
ENTRYPOINT ["dotnet", "OrderService.API.dll"]