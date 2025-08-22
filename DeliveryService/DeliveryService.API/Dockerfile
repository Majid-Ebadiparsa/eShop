# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG CONFIGURATION=Release

WORKDIR /app/deliveryservice

# copy csproj for cache
COPY ["DeliveryService/DeliveryService.API/DeliveryService.API.csproj","DeliveryService/DeliveryService.API/"]
COPY ["DeliveryService/DeliveryService.Application/DeliveryService.Application.csproj","DeliveryService/DeliveryService.Application/"]
COPY ["DeliveryService/DeliveryService.Infrastructure/DeliveryService.Infrastructure.csproj","DeliveryService/DeliveryService.Infrastructure/"]
COPY ["DeliveryService/DeliveryService.Domain/DeliveryService.Domain.csproj","DeliveryService/DeliveryService.Domain/"]
COPY ["SharedService/SharedService.Contracts/SharedService.Contracts.csproj","SharedService/SharedService.Contracts/"]

RUN dotnet restore "DeliveryService/DeliveryService.API/DeliveryService.API.csproj"

# copy source
COPY DeliveryService/DeliveryService.API/ DeliveryService/DeliveryService.API/
COPY DeliveryService/DeliveryService.Application/ DeliveryService/DeliveryService.Application/
COPY DeliveryService/DeliveryService.Infrastructure/ DeliveryService/DeliveryService.Infrastructure/
COPY DeliveryService/DeliveryService.Domain/ DeliveryService/DeliveryService.Domain/
COPY SharedService/SharedService.Contracts/ SharedService/SharedService.Contracts/

RUN dotnet publish "DeliveryService/DeliveryService.API/DeliveryService.API.csproj" -c $CONFIGURATION -o /out

# run
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app/deliveryservice

# Healthcheck to monitor service availability
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s CMD curl --fail http://localhost/health || exit 1

# Copy published output from the build stage
COPY --from=build /out .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Staging
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet","DeliveryService.API.dll"]
