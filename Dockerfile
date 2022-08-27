FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine as build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet build --no-restore -c Debug -v normal
RUN rm src/LDAPWebApi/Bitai.LDAPWebApi.xml
RUN dotnet publish --no-build -c Debug -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine as runtime
WORKDIR /app
COPY --from=build /app/publish .
RUN chmod a+x /app/Bitai.LDAPWebApi.dll
ENTRYPOINT [ "dotnet", "/app/Bitai.LDAPWebApi.dll" ]