# API (net9)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/InvoiceService/InvoiceService.API/InvoiceService.API.csproj", "src/InvoiceService/InvoiceService.API/"]
COPY ["src/InvoiceService/InvoiceService.Application/InvoiceService.Application.csproj", "src/InvoiceService/InvoiceService.Application/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "src/Shared/Shared.Contracts/"]
COPY ["src/InvoiceService/InvoiceService.Domain/InvoiceService.Domain.csproj", "src/InvoiceService/InvoiceService.Domain/"]
COPY ["src/InvoiceService/InvoiceService.Infrastructure/InvoiceService.Infrastructure.csproj", "src/InvoiceService/InvoiceService.Infrastructure/"]
COPY ["src/InvoiceService/InvoiceService.Infrastructure.Mongo/InvoiceService.Infrastructure.Mongo.csproj", "src/InvoiceService/InvoiceService.Infrastructure.Mongo/"]
RUN dotnet restore "src/InvoiceService/InvoiceService.API/InvoiceService.API.csproj"
COPY . .
WORKDIR "/src/src/InvoiceService/InvoiceService.API"
RUN dotnet publish "InvoiceService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN mkdir -p /app/datado
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InvoiceService.API.dll"]
