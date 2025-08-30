FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Subscriber/InvoiceSubscriber.Console/InvoiceSubscriber.Console.csproj", "src/Subscriber/InvoiceSubscriber.Console/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "src/Shared/Shared.Contracts/"]
RUN dotnet restore "src/Subscriber/InvoiceSubscriber.Console/InvoiceSubscriber.Console.csproj"

COPY . .
WORKDIR "/src/src/Subscriber/InvoiceSubscriber.Console"
RUN dotnet build "InvoiceSubscriber.Console.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "InvoiceSubscriber.Console.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

RUN mkdir -p /app/data

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "InvoiceSubscriber.Console.dll"]
