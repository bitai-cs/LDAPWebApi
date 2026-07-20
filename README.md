# Bitai LDAP Web API

[![CI](https://github.com/bitai-cs/LDAPWebApi/actions/workflows/ci.yml/badge.svg)](https://github.com/bitai-cs/LDAPWebApi/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux-lightgrey.svg)]()

An enterprise-grade ASP.NET Core Web API and companion client library that provides a centralized HTTP REST proxy layer for LDAP operations such as authenticate users, search directories and also user provisioning for Microsoft Active Directory and compatibles such as Samba AD — all through a uniform, OAuth2-secured REST interface.

---

## Quick Start

```bash
dotnet restore
dotnet build --configuration Release
dotnet run --project src/LDAPWebApi/Bitai.LDAPWebApi.csproj
```

The API starts at `https://localhost:5001` (or the URL configured in `appsettings.json`). Swagger UI is available at `/swagger`.

---

## Table of Contents

- [Overview](#overview)
- [Solution Layout](#solution-layout)
- [Key Features](#key-features)
- [API Endpoints](#api-endpoints)
- [Configuration](#configuration)
- [Client Library](#client-library)
- [Build & Test](#build--test)
- [Docker Support](#docker-support)
- [Observability](#observability)
- [Project Structure](#project-structure)
- [License](#license)

---

## Overview

Direct LDAP/AD integrations scatter connection logic, credential handling, and protocol-specific code across multiple applications. **Bitai LDAP Web API** centralizes this into a single, secure, cross-platform service:

```
┌──────────────┐   HTTP REST / OAuth2   ┌─────────────────────┐   LDAP / LDAPS   ┌─────────────────┐
│  Client App  │ ──────────────────────► │  Bitai.LDAPWebApi   │ ───────────────► │  AD / LDAP Server│
│  (any stack) │ ◄────────────────────── │  (ASP.NET Core 10)  │ ◄─────────────── │  (multi-tenant)  │
└──────────────┘                        └─────────────────────┘                  └─────────────────┘
       ▲                                         ▲
       │           ┌─────────────────────┐        │
       └───────────│  Bitai.LDAPWebApi   │────────┘
                   │  Client Library     │  (optional typed C# client)
                   └─────────────────────┘
```

---

## Solution Layout

| Project | Path | Description |
|---|---|---|
| **Bitai.LDAPWebApi** | `src/LDAPWebApi/` | ASP.NET Core Web API host with controllers, authentication, and health checks |
| **Bitai.LDAPWebApi.DTO** | `src/LDAPWebApi.DTO/` | Shared data-transfer objects and contract models |
| **Bitai.LDAPWebApi.Clients** | `client/LDAPWebApi.Client/` | Strongly-typed C# HTTP client wrappers |
| **Bitai.LDAPWebApi.Clients.Demo** | `client/LDAPWebApi.Client.Demo/` | Interactive demo / reference implementation for the client library |
| **Bitai.LDAPWebApi.Tests** | `tests/Bitai.LDAPWebApi.Tests/` | Unit and integration tests (xUnit) |

---

## Key Features

### Authentication

| Capability | Details |
|---|---|
| Domain account authentication | Validate credentials against AD domain accounts |
| Distinguished Name authentication | Authenticate using raw DN strings |
| Membership resolution | Returns full group membership roles in the authentication response |

### Directory Search

| Capability | Details |
|---|---|
| General entry lookup | Fetch any LDAP entry by identifier |
| Attribute-filtered search | Multi-attribute filter queries on any LDAP object class |
| User-specific search | Targeted search scoped to user entries |
| Group search | Query and filter LDAP group entries |
| Payload optimization | `Minimum`, `Few`, `All` attribute sets to control response size |

### Active Directory User Provisioning

| Operation | HTTP Verb | Endpoint |
|---|---|---|
| Create user account | `POST` | `/Directory/MsADUsers` |
| Update password | `PATCH` | `/Directory/MsADUsers/{id}/Credential` |
| Disable account | `PATCH` | `/Directory/MsADUsers/{id}/disableBy` |
| Delete account | `DELETE` | `/Directory/MsADUsers/{id}` |

All provisioning operations use native AD `UserAccountControl` bitwise flags and `unicodePwd` attribute handling.

### Multi-Tenant Server Profiles

Configure multiple independent LDAP/AD connections and route queries by profile and catalog type:

```
/api/{serverProfile}/{catalogType}/[controller]/[action]
```

Each profile maintains its own host, port, base DN, credentials, and SSL settings.

---

## API Endpoints

### Authentications

| Method | Path | Description |
|---|---|---|
| `POST` | `/api/{profile}/{catalog}/Authentications/authenticate` | Validate domain user credentials |

### Directory

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/{profile}/{catalog}/Directory/{identifier}` | Retrieve LDAP entry by ID |
| `GET` | `/api/{profile}/{catalog}/Directory/filterBy` | Search entries with attribute filters |
| `GET` | `/api/{profile}/{catalog}/Directory/Users/filterBy` | Search user entries with filters |
| `POST` | `/api/{profile}/{catalog}/Directory/MsADUsers` | Create new AD user account |
| `PATCH` | `/api/{profile}/{catalog}/Directory/MsADUsers/{id}/Credential` | Reset user password |
| `PATCH` | `/api/{profile}/{catalog}/Directory/MsADUsers/{id}/disableBy` | Disable AD user account |
| `DELETE` | `/api/{profile}/{catalog}/Directory/MsADUsers/{id}` | Remove AD user account |
| `GET` | `/api/{profile}/{catalog}/Directory/Groups/{identifier}` | Retrieve group entry by ID |
| `GET` | `/api/{profile}/{catalog}/Directory/Groups/filterBy` | Search groups with filters |

### Metadata

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/CatalogTypes` | List supported catalog categories |
| `GET` | `/api/ServerProfiles` | List configured LDAP server profiles |

---

## Configuration

All settings live in `appsettings.json` (or environment-specific variants like `appsettings.Development.json`). A full reference is available at `src/LDAPWebApi/appsettings.full.example.json`.

### `AllowedHosts`

Standard ASP.NET Core host filtering. Set to `"*"` to accept requests from any host, or restrict to specific hostnames.

```json
{
  "AllowedHosts": "*"
}
```

### `WebApiConfiguration`

Controls the API's identity, Swagger metadata, health-check behavior, and test-mode toggles.

```json
{
  "WebApiConfiguration": {
    "WebApiName": "Bitai.LDAPWebApi",
    "WebApiTitle": "LDAP Web API (BITAI)",
    "WebApiDescription": "Web API to search and authenticate on LDAP Server.",
    "WebApiVersion": "Global",
    "WebApiContactName": "Viko Bastidas",
    "WebApiContactMail": "vico.bastidas@gmail.com",
    "WebApiContactUrl": "https://www.linkedin.com/in/victorbastidasg/",
    "HealthChecksConfiguration": {
      "EnableHealthChecks": true,
      "HealthChecksHeaderText": "LDAP WEB API Health Checks",
      "HealthChecksGroupName": "Health Checks results",
      "WebApiBaseUrl": "https://localhost:5101",
      "ApiEndPointName": "hc",
      "UIPath": "hc-ui",
      "MaximunHistoryEntries": 200,
      "EvaluationTime": 180
    },
    "TestConfiguration": {
      "EnablePersistentMockLdapDataStore": false
    }
  }
}
```

| Key | Description |
|---|---|
| `WebApiName` / `WebApiTitle` / `WebApiDescription` | Shown in Swagger UI and API responses |
| `WebApiVersion` | API version string exposed in OpenAPI docs |
| `WebApiContact*` | Contact information displayed in Swagger UI |
| `HealthChecksConfiguration.EnableHealthChecks` | Toggle health-check middleware on/off |
| `HealthChecksConfiguration.WebApiBaseUrl` | Base URL used by the health-checks UI to probe endpoints |
| `HealthChecksConfiguration.ApiEndPointName` | Raw health-check endpoint name (e.g. `/hc`) |
| `HealthChecksConfiguration.UIPath` | Health-checks dashboard path (e.g. `/hc-ui`) |
| `HealthChecksConfiguration.MaximunHistoryEntries` | Number of historical results retained in the UI |
| `HealthChecksConfiguration.EvaluationTime` | Seconds between health-check evaluations |
| `TestConfiguration.EnablePersistentMockLdapDataStore` | When `true`, uses mock LDAP data instead of live directory connections (for testing) |

### `WebApiCorsConfiguration`

Configures Cross-Origin Resource Sharing for browser-based consumers.

```json
{
  "WebApiCorsConfiguration": {
    "AllowAnyOrigin": false,
    "AllowedOrigins": [
      "http://localhost:5100",
      "https://localhost:5101",
      "https://localhost:44310"
    ]
  }
}
```

| Key | Description |
|---|---|
| `AllowAnyOrigin` | When `true`, accepts requests from any origin (not recommended for production) |
| `AllowedOrigins` | Whitelist of permitted origin URLs |

### `WebApiLogConfiguration`

Logging via Serilog using console and file sinks.

```json
{
  "WebApiLogConfiguration": {
    "ConsoleLog": {
      "Enabled": true,
      "MinimunLogEventLevel": "Information"
    },
    "FileLog": {
      "Enabled": true,
      "LogFilePath": "./logs/Bitai.LDAPWebApi-.log",
      "MinimunLogEventLevel": "Warning",
      "RollingInterval": "Day",
      "RetainedFileCountLimit": 30,
      "FlushToDiskIntervalInMinutes": 3
    }
  }
}
```

| Sink | Key | Description |
|---|---|---|
| **Console** | `Enabled` | Write logs to stdout |
| | `MinimunLogEventLevel` | Minimum severity (`Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`) |
| **File** | `LogFilePath` | Path template; `-` is replaced by the date for rolling files |
| | `RollingInterval` | Rotation frequency (`Day`, `Hour`, `Infinite`, etc.) |
| | `RetainedFileCountLimit` | Maximum old files retained on disk |
| | `FlushToDiskIntervalInMinutes` | How often buffered writes are flushed |

### `WebApiScopesConfiguration`

Controls OAuth2 API scope-based authorization.

```json
{
  "WebApiScopesConfiguration": {
    "BypassApiScopesAuthorization": false,
    "AdminApiScopeName": "Bitai.LdapWebApi.Scope.Admin",
    "ReaderApiScopeName": "Bitai.LdapWebApi.Scope.Reader"
  }
}
```

| Key | Description |
|---|---|
| `BypassApiScopesAuthorization` | When `true`, skips scope validation (useful for local development only) |
| `AdminApiScopeName` | Required scope for mutating operations (create, update, delete) |
| `ReaderApiScopeName` | Required scope for read-only operations (search, get) |

### `AuthorityConfiguration`

Points the API to the OAuth2 / OpenID Connect identity server that issues access tokens.

```json
{
  "AuthorityConfiguration": {
    "Authority": "https://localhost:44310",
    "ApiResource": "Bitai.LdapWebApi.Resource",
    "RequireHttpsMetadata": true
  }
}
```

| Key | Description |
|---|---|
| `Authority` | Base URL of the IdentityServer / OIDC provider |
| `ApiResource` | The API resource name registered in the identity server |
| `RequireHttpsMetadata` | Enforces HTTPS for the OIDC discovery metadata endpoint |

### `LDAPServerProfiles`

An array of LDAP / Active Directory server connections. Each profile is addressable via the route segment `{profile}` in API paths.

```json
{
  "LDAPServerProfiles": [
    {
      "ProfileId": "CompanyDomainProfile",
      "Server": "192.168.7.253",
      "Port": "Default",
      "UseSSL": "false",
      "BaseDN": "DC=ie,DC=edu,DC=pe",
      "PortForGlobalCatalog": "Default",
      "UseSSLforGlobalCatalog": "false",
      "BaseDNforGlobalCatalog": "DC=edu,DC=pe",
      "DefaultDomainName": "CompanyDomain",
      "ConnectionTimeout": 10,
      "DomainAccountName": "HoldingDomain\\LDAPWebApiUser",
      "DomainAccountPassword": "Pa5$w0rd123",
      "HealthCheckPingTimeout": 100
    }
  ]
}
```

| Key | Description |
|---|---|
| `ProfileId` | Unique identifier used in the API route (e.g. `/api/EDU/local/...`) |
| `Server` | Hostname or IP of the LDAP / AD server |
| `Port` | LDAP port; `"Default"` resolves to 389 (plain) or 636 (SSL) |
| `UseSSL` | `"true"` / `"false"` — enables LDAPS on the standard port |
| `BaseDN` | Root distinguished name for directory searches |
| `PortForGlobalCatalog` | Global Catalog port; `"Default"` resolves to 3268 (plain) or 3269 (SSL) |
| `UseSSLforGlobalCatalog` | `"true"` / `"false"` — enables SSL for Global Catalog queries |
| `BaseDNforGlobalCatalog` | Root DN used when querying the Global Catalog |
| `DefaultDomainName` | NetBIOS domain name used during domain-account authentication |
| `ConnectionTimeout` | Seconds before a connection attempt times out |
| `DomainAccountName` | Bind account in `DOMAIN\username` format |
| `DomainAccountPassword` | Bind account password |
| `HealthCheckPingTimeout` | Milliseconds for the health-check ping probe |

> **Tip:** Sensitive values (`DomainAccountPassword`, etc.) should be managed via [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment variables in non-development environments.

### `SwaggerUIConfiguration`

Configures the Swagger UI OAuth2 client for interactive API testing through the browser.

```json
{
  "SwaggerUIConfiguration": {
    "SwaggerUIClientId": "Bitai.LdapWebApi.Swagger.Client",
    "SwaggerUITargetApiScopes": [
      {
        "SwaggerUITargetApiScopeName": "Bitai.LdapWebApi.Scope.Global",
        "SwaggerUITargetApiScopeTitle": "BITAI LDAP Web Api Global Scope"
      },
      {
        "SwaggerUITargetApiScopeName": "Bitai.LdapWebApi.Scope.Dummy",
        "SwaggerUITargetApiScopeTitle": "BITAI LDAP Web Api Dummy Scope"
      }
    ]
  }
}
```

| Key | Description |
|---|---|
| `SwaggerUIClientId` | OAuth2 client ID registered in the identity server for the Swagger UI implicit/code flow |
| `SwaggerUITargetApiScopes` | Array of scopes presented as selectable checkboxes in the Swagger UI authorize dialog |

---

## Client Library

The `Bitai.LDAPWebApi.Clients` NuGet package provides strongly-typed C# wrappers so consuming applications don't need to write raw HTTP handlers.

### Available Clients

| Client Class | Purpose |
|---|---|
| `LDAPUserDirectoryWebApiClient` | Directory search and AD user provisioning |
| `LDAPAuthenticationsWebApiClient` | Credential validation |
| `LDAPServerProfilesWebApiClient` | Server profile and catalog type metadata |
| `LDAPGroupsDirectoryWebApiClient` | Group directory operations |

### Usage

```csharp
using Bitai.LDAPHelper.DTO;
using Bitai.LDAPWebApi.DTO;
using Bitai.WebApi.Client;
using Bitai.LDAPWebApi.Clients;

// 1. Configure OAuth2 client credentials for the Web API
var clientCredentials = new WebApiClientCredential
{
    AuthorityUrl   = "https://auth.company.local/identity",
    ApiScope       = "Bitai.LdapWebApi.ApiScope.Reader",
    ClientId       = "my-service-client",
    ClientSecret   = "my-service-secret"
};

// 2. Create a typed client, targeting a specific LDAP server profile
var userClient = new LDAPUserDirectoryWebApiClient(
    baseUrl:           "https://ldap-api.company.local",
    serverProfileId:   "DefaultActiveDirectory",
    useGlobalCatalog:  false,
    credentials:       clientCredentials);

// 3. Create a new AD user account
var newUser = new LDAPMsADUserAccount
{
    DistinguishedNameOfContainer = "CN=Users,DC=company,DC=local",
    Cn                           = "John Doe",
    DisplayName                  = "John Doe (IT)",
    ObjectClass                  = new[] { "user" },
    SAMAccountName               = "john.doe",
    UserAccountControl           = "NORMAL_ACCOUNT,DONT_EXPIRE_PASSWORD",
    Password                     = "P@ssw0rd!"
};

var response = await userClient.CreateMsADUserAccountAsync(newUser, "CREATE-USER");
if (response.IsSuccessResponse)
{
    var result = await userClient.GetDTOFromResponseAsync<LWACreateMsADUserAccountResult>(response);
    // result.DistinguishedName, result.SAMAccountName, etc.
}

// 4. Search for a user by sAMAccountName
var searchResponse = await userClient.SearchFilteringByAsync("john.doe");
var searchResult   = await userClient.GetDTOFromResponseAsync<LWASearchResult>(searchResponse);

// 5. Disable an AD user account
var disableResponse = await userClient.DisableMsADUserAccountAsync(
    identifier:       "john.doe",
    identifierType:   EntryAttribute.sAMAccountName,
    requestLabel:     "DISABLE-USER");

// 6. Remove an AD user account
var removeResponse = await userClient.RemoveMsADUserAccountAsync(
    identifier:       "john.doe",
    identifierType:   EntryAttribute.sAMAccountName,
    requestLabel:     "REMOVE-USER");
```

All client classes (`LDAPAuthenticationsWebApiClient`, `LDAPDirectoryWebApiClient`, `LDAPGroupsDirectoryWebApiClient`, `LDAPServerProfilesWebApiClient`) follow the same pattern: construct with `WebApiClientCredential`, call the async method, check `IsSuccessResponse`, and deserialize with `GetDTOFromResponseAsync<T>`.

See `client/LDAPWebApi.Client.Demo/Program.cs` for a full interactive reference.

---

## Build & Test

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022 / VS Code with C# Dev Kit (optional)

### Commands

```bash
# Restore all dependencies
dotnet restore Bitai.LDAPWebApi.sln

# Build all projects
dotnet build Bitai.LDAPWebApi.sln --configuration Release

# Run tests
dotnet test Bitai.LDAPWebApi.sln --configuration Release --verbosity normal
```

### CI

The repository includes a GitHub Actions workflow (`.github/workflows/ci.yml`) that runs restore → build → test on every push, pull request, merge group, and workflow dispatch.

---

## Docker Support

The main API project includes a Dockerfile and `.dockerignore` for containerized deployments:

```bash
# Build Debug image (uses Dockerfile ARG BUILD_CONFIGURATION=Debug)
docker build \
  --build-arg BUILD_CONFIGURATION=Debug \
  -t bitai-ldapwebapi:debug \
  -f src/LDAPWebApi/Dockerfile .

# Build Release image (uses Dockerfile ARG BUILD_CONFIGURATION=Release)
docker build \
  --build-arg BUILD_CONFIGURATION=Release \
  -t bitai-ldapwebapi:release \
  -f src/LDAPWebApi/Dockerfile .

# Run with production-oriented parameters
docker run -d \
  --name bitai-ldapwebapi \
  -p 5100:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e WebApiConfiguration__HealthChecksConfiguration__EnableHealthChecks=true \
  -e LDAPServerProfiles__0__ProfileId=DOMAIN_PROFILEID \
  -e LDAPServerProfiles__0__Server=192.168.1.200 \
  -e LDAPServerProfiles__0__Port=Default \
  -e LDAPServerProfiles__0__UseSSL=true \
  -e LDAPServerProfiles__0__BaseDN="DC=va,DC=bitai,DC=com" \
  -e LDAPServerProfiles__0__PortForGlobalCatalog=Default \
  -e LDAPServerProfiles__0__UseSSLforGlobalCatalog=false \
  -e LDAPServerProfiles__0__BaseDNforGlobalCatalog="DC=bitai,DC=com" \
  -e LDAPServerProfiles__0__DefaultDomainName=BITAIVA \
  -e LDAPServerProfiles__0__ConnectionTimeout=10 \
  -e LDAPServerProfiles__0__DomainUserAccount="BITAIVA\\Administrator" \
  -e LDAPServerProfiles__0__DomainAccountPassword='ThePassword' \
  -e LDAPServerProfiles__0__HealthCheckPingTimeout=50 \
  bitai-ldapwebapi:release
```

> Security: never hardcode LDAP bind passwords in source control or shell history. Prefer injecting `LDAPServerProfiles__0__DomainAccountPassword` from a secret store or CI/CD secret variable.

The project is configured with `DockerDefaultTargetOS=Linux` in the `.csproj`.

---

## Observability

The API integrates multiple logging and monitoring backends via Serilog and ASP.NET Core Health Checks:

| Backend | Package |
|---|---|
| File logging | `Serilog.Sinks.File` |
| Elasticsearch | `Serilog.Sinks.Elasticsearch` |
| Grafana Loki | `Serilog.Sinks.Grafana.Loki` |
| Health Checks UI | `AspNetCore.HealthChecks.UI` + InMemory storage |
| Network health checks | `AspNetCore.HealthChecks.Network` |
| URI endpoint checks | `AspNetCore.HealthChecks.Uris` |

Health check dashboards are available at the configured `/healthchecks-ui` endpoint.

---

## Project Structure

```
Bitai.LDAPWebApi/
├── src/
│   ├── LDAPWebApi/                  # Web API host
│   │   ├── Controllers/             # REST controllers
│   │   │   ├── AuthRequirements/    # Authorization requirements
│   │   │   ├── Binders/             # Model binders
│   │   │   ├── Constraints/         # Route constraints
│   │   │   ├── Models/              # Request/response models
│   │   │   └── PolicyEvaluators/    # Authorization policy evaluators
│   │   ├── Program.cs               # Application entry point
│   │   └── Startup.cs               # Service configuration
│   └── LDAPWebApi.DTO/              # Shared DTOs and contracts
├── client/
│   ├── LDAPWebApi.Client/           # Typed C# HTTP client library
│   └── LDAPWebApi.Client.Demo/      # Reference demo application
├── tests/
│   └── Bitai.LDAPWebApi.Tests/      # xUnit test suite
│       └── Controllers/             # Controller tests
├── misc/                            # Development settings templates
├── resources/                       # Package resources (icons, etc.)
└── .github/workflows/ci.yml         # CI pipeline
```

---

## License

This project is licensed under the [MIT License](LICENSE.md).

Copyright &copy; 2026 BITAI LDAP Web API.
