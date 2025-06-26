# Base image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG CONFIGURATION=Release

WORKDIR /app/orderservice


# Copy only .csproj files to leverage Docker cache
COPY ["OrderService/OrderService.API/OrderService.API.csproj", "OrderService/OrderService.API/"]
COPY ["OrderService/OrderService.Application/OrderService.Application.csproj", "OrderService/OrderService.Application/"]
COPY ["OrderService/OrderService.Infrastructure/OrderService.Infrastructure.csproj", "OrderService/OrderService.Infrastructure/"]
COPY ["OrderService/OrderService.Domain/OrderService.Domain.csproj", "OrderService/OrderService.Domain/"]
COPY ["OrderService/OrderService.Shared/OrderService.Shared.csproj", "OrderService/OrderService.Shared/"]
COPY ["SharedService/SharedService.Contracts/SharedService.Contracts.csproj", "../SharedService/SharedService.Contracts/"]

# Restore dependencies (cached if csproj files don't change)
RUN dotnet restore "OrderService/OrderService.API/OrderService.API.csproj"

# Copy the full source code
COPY OrderService/OrderService.API/ OrderService/OrderService.API/
COPY OrderService/OrderService.Application/ OrderService/OrderService.Application/
COPY OrderService/OrderService.Infrastructure/ OrderService/OrderService.Infrastructure/
COPY OrderService/OrderService.Domain/ OrderService/OrderService.Domain/
COPY OrderService/OrderService.Shared/ OrderService/OrderService.Shared/
COPY SharedService/SharedService.Contracts/ SharedService/SharedService.Contracts/

# Light restore because in the previouse step, NuGet packages were already restored
RUN dotnet restore "OrderService/OrderService.API/OrderService.API.csproj"

# Build and Publish the application directly
RUN dotnet publish "OrderService/OrderService.API/OrderService.API.csproj" -c $CONFIGURATION -o /out --no-restore

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app/orderservice

# Healthcheck to monitor service availability
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s CMD curl --fail http://localhost/health || exit 1

# Copy published output from the build stage
COPY --from=build /out .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Staging

# Start the application
ENTRYPOINT ["dotnet", "OrderService.API.dll"]