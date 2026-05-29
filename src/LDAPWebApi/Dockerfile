# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# --- Runtime Stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# --- Build Stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first to leverage Docker cache restore layer
COPY ["src/LDAPWebApi/Bitai.LDAPWebApi.csproj", "src/LDAPWebApi/"]
COPY ["src/LDAPWebApi.DTO/Bitai.LDAPWebApi.DTO.csproj", "src/LDAPWebApi.DTO/"]

# Restore NuGet packages
RUN dotnet restore "src/LDAPWebApi/Bitai.LDAPWebApi.csproj"

# Copy the rest of the source code
COPY . .

# Build the project under Release configuration
WORKDIR "/src/src/LDAPWebApi"
RUN dotnet build "Bitai.LDAPWebApi.csproj" -c Release --no-restore -o /app/build

# --- Publish Stage ---
FROM build AS publish
RUN dotnet publish "Bitai.LDAPWebApi.csproj" -c Release --no-restore -o /app/publish /p:UseAppHost=false

# --- Final Production Stage ---
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Run under the secure built-in non-root user (Standard in .NET 8)
USER app

ENTRYPOINT ["dotnet", "Bitai.LDAPWebApi.dll"]