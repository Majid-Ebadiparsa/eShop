# ===========================================================================
#  DeliveryService Dockerfile
#  Multi-stage, non-root, healthcheck, optimized caching
# ===========================================================================

# -------- Build stage --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG CONFIGURATION=Release
WORKDIR /src

# If Solution-wide files like Directory.Build.props/targets exist, copy them first to leverage caching in dotnet restore
COPY Directory.Build.props* Directory.Build.targets* ./ 2>/dev/null || true

# Only copy csproj files to leverage Docker cache
COPY DeliveryService/DeliveryService.API/DeliveryService.API.csproj DeliveryService/DeliveryService.API/
COPY DeliveryService/DeliveryService.Application/DeliveryService.Application.csproj DeliveryService/DeliveryService.Application/
COPY DeliveryService/DeliveryService.Infrastructure/DeliveryService.Infrastructure.csproj DeliveryService/DeliveryService.Infrastructure/
COPY DeliveryService/DeliveryService.Domain/DeliveryService.Domain.csproj DeliveryService/DeliveryService.Domain/
COPY SharedService/SharedService.Contracts/SharedService.Contracts.csproj SharedService.Contracts/

# Using the API csproj as the entry point for restore (Without copying full source)
RUN dotnet restore DeliveryService/DeliveryService.API/DeliveryService.API.csproj

# Copy the rest of the source code (Delivery + Shared)
COPY DeliveryService/ DeliveryService/
COPY SharedService/SharedService.Contracts/ SharedService.Contracts/

# Publish (Without apphost for smaller image)
RUN dotnet publish DeliveryService/DeliveryService.API/DeliveryService.API.csproj \
    -c $CONFIGURATION -o /app/publish /p:UseAppHost=false

# -------- Runtime stage --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# For healthcheck, we use curl  
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Service will listen on port 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8080

# If there is a health endpoint in the API (recommended), set it here
# Temporarily we can point to /swagger/index.html, but a health endpoint is more standard
HEALTHCHECK --interval=20s --timeout=3s --retries=3 \
    CMD curl -fsS http://localhost:8080/health || exit 1

# Executing the service as a non-root user (more secure)
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copy published output
COPY --from=build /app/publish ./

EXPOSE 8080

# For tracking OCI version labels (optional)
ARG BUILD_VERSION=0.0.0
ARG BUILD_SHA=dev
LABEL org.opencontainers.image.title="DeliveryService" \
      org.opencontainers.image.version=$BUILD_VERSION \
      org.opencontainers.image.revision=$BUILD_SHA \
      org.opencontainers.image.source="https://github.com/Majid-Ebadiparsa/eShop"

ENTRYPOINT ["dotnet", "DeliveryService.API.dll"]
