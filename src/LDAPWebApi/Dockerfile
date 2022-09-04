#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/LDAPWebApi/Bitai.LDAPWebApi.csproj", "src/LDAPWebApi/"]
RUN dotnet restore "src/LDAPWebApi/Bitai.LDAPWebApi.csproj"
COPY . .
WORKDIR "/src/src/LDAPWebApi"
RUN dotnet build "Bitai.LDAPWebApi.csproj" -c Debug --no-restore -o /app/build

FROM build AS publish
RUN dotnet publish "Bitai.LDAPWebApi.csproj" -c Debug --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN dotnet dev-certs https --trust
ENTRYPOINT ["dotnet", "Bitai.LDAPWebApi.dll"]