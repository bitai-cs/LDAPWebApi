# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# --- Runtime Stage ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# --- By default .net uses port 8080
EXPOSE 8080
EXPOSE 8081

# --- Build Stage ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files first to leverage Docker cache restore layer
COPY ["src/LDAPWebApi/Bitai.LDAPWebApi.csproj", "src/LDAPWebApi/"]
COPY ["src/LDAPWebApi.DTO/Bitai.LDAPWebApi.DTO.csproj", "src/LDAPWebApi.DTO/"]

# Restore NuGet packages
RUN dotnet restore "src/LDAPWebApi/Bitai.LDAPWebApi.csproj"

# Copy the rest of the source code
COPY . .

# --- Publish Stage ---
WORKDIR "/src/src/LDAPWebApi"
RUN dotnet publish "Bitai.LDAPWebApi.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false /p:GeneratePackageOnBuild=false

# --- Final Production Stage ---
FROM base AS final
WORKDIR /app

# Install ping utility (needed by HealthChecks.Network / System.Net.NetworkInformation.Ping fallback)
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends iputils-ping curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# CHANGE OWNERSHIP: Give the $APP_UID user full control over /app
RUN chown -R $APP_UID:$APP_UID /app

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD curl -fsS http://localhost:8080/hc || exit 1

USER $APP_UID

ENTRYPOINT ["dotnet", "Bitai.LDAPWebApi.dll"]
