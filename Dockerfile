# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["InclusiON.Api/InclusiON.Api.csproj", "InclusiON.Api/"]
COPY ["InclusiON.Application/InclusiON.Application.csproj", "InclusiON.Application/"]
COPY ["InclusiON.Domain/InclusiON.Domain.csproj", "InclusiON.Domain/"]
COPY ["InclusiON.Infrastructure/InclusiON.Infrastructure.csproj", "InclusiON.Infrastructure/"]
COPY ["InclusiON.Infrastructure.Telemetry/InclusiON.Infrastructure.Telemetry.csproj", "InclusiON.Infrastructure.Telemetry/"]
COPY ["InclusiON.Data/InclusiON.Data.csproj", "InclusiON.Data/"]
COPY ["InclusiON.DTOs/InclusiON.DTOs.csproj", "InclusiON.DTOs/"]
COPY ["InclusiON.Shared/InclusiON.Shared.csproj", "InclusiON.Shared/"]
COPY ["InclusiON.SemanticSearch/InclusiON.SemanticSearch.csproj", "InclusiON.SemanticSearch/"]
RUN dotnet restore "InclusiON.Api/InclusiON.Api.csproj"

COPY . .
WORKDIR "/src/InclusiON.Api"
RUN dotnet build "InclusiON.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "InclusiON.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "InclusiON.Api.dll"]
