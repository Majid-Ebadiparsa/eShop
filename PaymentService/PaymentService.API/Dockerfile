# PaymentService.API/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only .csproj files first for better layer caching
COPY ["PaymentService/PaymentService.API/PaymentService.API.csproj", "PaymentService/PaymentService.API/"]
COPY ["PaymentService/PaymentService.Application/PaymentService.Application.csproj", "PaymentService/PaymentService.Application/"]
COPY ["PaymentService/PaymentService.Infrastructure/PaymentService.Infrastructure.csproj", "PaymentService/PaymentService.Infrastructure/"]
COPY ["PaymentService/PaymentService.Domain/PaymentService.Domain.csproj", "PaymentService/PaymentService.Domain/"]
COPY ["SharedService/SharedService.Contracts/SharedService.Contracts.csproj", "SharedService/SharedService.Contracts/"]
COPY ["SharedService/SharedService.Consul/SharedService.Consul.csproj", "SharedService/SharedService.Consul/"]

# Restore dependencies
RUN dotnet restore "PaymentService/PaymentService.API/PaymentService.API.csproj"

# Copy the rest of the source
COPY PaymentService/ PaymentService/
COPY SharedService/SharedService.Contracts/ SharedService/SharedService.Contracts/
COPY SharedService/SharedService.Consul/ SharedService/SharedService.Consul/

# Build and publish
RUN dotnet publish "PaymentService/PaymentService.API/PaymentService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.API.dll"]