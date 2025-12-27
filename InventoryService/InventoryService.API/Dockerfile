# =================================================
# InventoryService Dockerfile
# .NET 8 multi-stage, secure, healthcheck, non-root
# ==================================================

# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG CONFIGURATION=Release
WORKDIR /src

# (1) Copy only csproj first for better layer caching
COPY InventoryService/InventoryService.API/InventoryService.API.csproj InventoryService/InventoryService.API/
COPY InventoryService/InventoryService.Application/InventoryService.Application.csproj InventoryService/InventoryService.Application/
COPY InventoryService/InventoryService.Infrastructure/InventoryService.Infrastructure.csproj InventoryService/InventoryService.Infrastructure/
COPY InventoryService/InventoryService.Domain/InventoryService.Domain.csproj InventoryService/InventoryService.Domain/
COPY InventoryService/InventoryService.Shared/InventoryService.Shared.csproj InventoryService/InventoryService.Shared/
COPY SharedService/SharedService.Contracts/SharedService.Contracts.csproj SharedService/SharedService.Contracts/
COPY SharedService/SharedService.Consul/SharedService.Consul.csproj SharedService/SharedService.Consul/

# Restore dependencies (cached if csproj not changed)
RUN dotnet restore InventoryService/InventoryService.API/InventoryService.API.csproj

# (2) Copy the rest of the source
COPY InventoryService/ InventoryService/
COPY SharedService/SharedService.Contracts/ SharedService/SharedService.Contracts/
COPY SharedService/SharedService.Consul/ SharedService/SharedService.Consul/

# (3) Publish trimmed app (no native host)
RUN dotnet publish InventoryService/InventoryService.API/InventoryService.API.csproj \
    -c $CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for healthcheck (if your base image doesn't have it)
USER root
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# App will listen on 8080 in container
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Optional: Harden GC memory (tune as needed for your node)
# ENV DOTNET_GCHeapHardLimit=0

# Simple metadata (optional)
LABEL org.opencontainers.image.title="InventoryService" \
      org.opencontainers.image.source="https://github.com/Majid-Ebadiparsa/eShop" \
      org.opencontainers.image.vendor="eShop" \
      org.opencontainers.image.licenses="MIT"

# Copy published output
COPY --from=build /app/publish ./

# For healthcheck, we use curl + adduser
RUN apt-get update && apt-get install -y --no-install-recommends curl adduser \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user and run as it
RUN adduser --disabled-password --gecos "" appuser \
    && chown -R appuser:appuser /app
USER appuser

# Health check (change /health if your endpoint differs)
HEALTHCHECK --interval=20s --timeout=3s --retries=3 \
  CMD curl -fsS http://localhost:8080/health || exit 1

EXPOSE 8080
ENTRYPOINT ["dotnet", "InventoryService.API.dll"]