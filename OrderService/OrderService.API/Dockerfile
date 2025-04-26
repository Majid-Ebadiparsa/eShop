# Use the official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG CONFIGURATION=Release

WORKDIR /app

# Copy csproj files
COPY ["OrderService.API/OrderService.API.csproj", "OrderService.API/"]
COPY ["OrderService.Application/OrderService.Application.csproj", "OrderService.Application/"]
COPY ["OrderService.Infrastructure/OrderService.Infrastructure.csproj", "OrderService.Infrastructure/"]
COPY ["OrderService.Domain/OrderService.Domain.csproj", "OrderService.Domain/"]

# Restore dependencies
WORKDIR /app/OrderService.API
RUN dotnet restore "OrderService.API.csproj"

# Copy source files only
COPY OrderService.API/ OrderService.API/
COPY OrderService.Application/ OrderService.Application/
COPY OrderService.Infrastructure/ OrderService.Infrastructure/
COPY OrderService.Domain/ OrderService.Domain/

# Clean bin/obj folders to avoid duplicate attributes
RUN find . -type d -name bin -exec rm -rf {} + && find . -type d -name obj -exec rm -rf {} +

# Build
RUN dotnet build "OrderService.API.csproj" -c $CONFIGURATION

# Publish
FROM build AS publish
RUN dotnet publish "OrderService.API.csproj" -c $CONFIGURATION -o /app/publish --no-restore

# Final runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s CMD curl --fail http://localhost/health || exit 1

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "OrderService.API.dll"]
