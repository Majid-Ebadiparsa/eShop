# InvoiceService API
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["InvoiceProcessingService/src/InvoiceService/InvoiceService.API/InvoiceService.API.csproj", "InvoiceProcessingService/src/InvoiceService/InvoiceService.API/"]
COPY ["InvoiceProcessingService/src/InvoiceService/InvoiceService.Application/InvoiceService.Application.csproj", "InvoiceProcessingService/src/InvoiceService/InvoiceService.Application/"]
COPY ["InvoiceProcessingService/src/InvoiceService/InvoiceService.Domain/InvoiceService.Domain.csproj", "InvoiceProcessingService/src/InvoiceService/InvoiceService.Domain/"]
COPY ["InvoiceProcessingService/src/InvoiceService/InvoiceService.Infrastructure/InvoiceService.Infrastructure.csproj", "InvoiceProcessingService/src/InvoiceService/InvoiceService.Infrastructure/"]
COPY ["InvoiceProcessingService/src/InvoiceService/InvoiceService.Infrastructure.Mongo/InvoiceService.Infrastructure.Mongo.csproj", "InvoiceProcessingService/src/InvoiceService/InvoiceService.Infrastructure.Mongo/"]
COPY ["SharedService/SharedService.Contracts/SharedService.Contracts.csproj", "SharedService/SharedService.Contracts/"]
COPY ["SharedService/SharedService.Consul/SharedService.Consul.csproj", "SharedService/SharedService.Consul/"]
RUN dotnet restore "InvoiceProcessingService/src/InvoiceService/InvoiceService.API/InvoiceService.API.csproj"
COPY . .
WORKDIR "/src/InvoiceProcessingService/src/InvoiceService/InvoiceService.API"
RUN dotnet publish "InvoiceService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InvoiceService.API.dll"]
