# Bitai.LDAPWebApi.Clients

[![NuGet Version](https://img.shields.io/nuget/v/Bitai.LDAPWebApi.Clients.svg?style=flat-sign)](https://www.nuget.org/packages/Bitai.LDAPWebApi.Clients/)
[![.NET](https://img.shields.io/badge/.NET-10-blue.svg)](https://dotnet.microsoft.com)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)]()

Bitai.LDAPWebApi.Clients is a lightweight, strongly-typed .NET client library for consuming the Bitai.LDAPWebApi service. It removes HTTP boilerplate and exposes concise, testable client types for authentication, directory searches, group operations, and Active Directory management.

Key benefits:
- Fluent, controller-aligned client classes
- Token-based authentication support (IdentityServer / OAuth2)
- Minimal payload requests via configurable attribute selection
- Async-first API suitable for server and desktop applications

Table of contents
- Installation
- Quick start
- Configuration
- Clients and capabilities
- Contributing
- License

## Installation

Install from NuGet:

```bash
dotnet add package Bitai.LDAPWebApi.Clients
```

## Quick start

Create a client and perform a simple user search (IdentityServer example):

```csharp
using Bitai.LDAPWebApi.Clients;
using Bitai.WebApi.Client; // WebApiClientCredential

var apiBaseUrl = "https://api.company.local/ldap";
var profileId = "DefaultActiveDirectory";
var useGlobalCatalog = false;

var credentials = new WebApiClientCredential
{
    ClientId = "ldap-client-app",
    ClientSecret = "<secret>",
    Authority = "https://identity.company.local",
    Scope = "ldap.api.read"
};

var userClient = new LDAPUserDirectoryWebApiClient(
    apiBaseUrl,
    profileId,
    useGlobalCatalog,
    credentials // optional; setBearerToken can be used per-call
);

var response = await userClient.SearchFilteringByAsync(
    EntryAttribute.mail,
    "alice@example.com",
    RequiredEntryAttributes.Few,
    setBearerToken: true
);

if (response.IsSuccessStatusCode)
{
    var result = response.GetContent<LWASearchResult>();
    Console.WriteLine($"Found {result.Entries.Count} entries");
}
```

## Configuration

- apiBaseUrl: Base address of the LDAP Web API (e.g. https://api.example.com/ldap)
- profileId: Identifier for the LDAP server profile to target
- useGlobalCatalog: When true, requests use the global catalog endpoint if supported
- WebApiClientCredential: Optional credential object for automatic bearer token retrieval

## Clients and capabilities

- LDAPWebApiBaseClient — common base with routing and URL helpers
- LDAPAuthenticationsWebApiClient — authenticate domain accounts
- LDAPDirectoryWebApiClient — generic directory reads and searches
- LDAPGroupsDirectoryWebApiClient — group queries and filters
- LDAPUserDirectoryWebApiClient — user queries and AD account management
- LDAPCatalogTypesWebApiClient — catalog type enumeration
- LDAPServerProfilesWebApiClient — server profile management

## API reference

See the package reference or repository documentation for full method signatures and examples.

## Contributing

Contributions are welcome. Please submit issues and pull requests in the upstream repository. Suggested workflow:
1. Fork the repository
2. Create a branch (feature/bugfix)
3. Open a pull request with a clear description and tests where applicable

## License

This project is licensed under the MIT License — see the LICENSE file for details.

Maintainers

Bitai contributors
