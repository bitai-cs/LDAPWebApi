# Bitai.LDAPWebApi.Clients

[![NuGet Version](https://img.shields.io/nuget/v/Bitai.LDAPWebApi.Clients.svg?style=flat-sign)](https://www.nuget.org/packages/Bitai.LDAPWebApi.Clients/)
[![.NET Core](https://img.shields.io/badge/.NET-8.0%20%7C%20Standard%202.0-blue.svg)](https://dotnet.microsoft.com)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)]()

`Bitai.LDAPWebApi.Client` is a strongly-typed .NET client library designed to streamline integration with the **Bitai.LDAPWebApi** service. It eliminates boilerplate HTTP code by providing highly specialized, fluent client handlers for authentication, directory lookups, Active Directory administration, and profile management.

---

## 🌟 Key Architecture & Public Clients

The library is organized into specialized client classes, each mapping directly to an LDAP Web API controller. All clients leverage a common base logic for request generation, authorization headers, and custom network configurations.

### 1. `LDAPWebApiBaseClient`
The abstract base class for all Web API clients in the ecosystem. It inherits from `WebApiBaseClient` and manages the core connection details.
*   **Key Responsibilities**:
    *   Exposes global routing configurations like `LDAPServerProfile` and `UseLDAPServerGlobalCatalog`.
    *   Provides inner URL formatting helpers (`GetOptionalEntryAttributeName`, `GetOptionalRequiredEntryAttributesName`, `GetOptionalBooleanValue`).
    *   Defines static controller routing tokens inside the `ControllerNames` nested class.

### 2. `LDAPAuthenticationsWebApiClient`
Dedicated to performing authentication operations against the directory services.
*   **Core Method**:
    *   `AuthenticateAsync(LDAPDomainAccountCredential credential, ...)`: Submits domain credentials to the LDAP API to validate user authenticity, returning a detailed `LDAPDomainAccountAuthenticationResult` containing error context on failures.

### 3. `LDAPDirectoryWebApiClient`
Provides generic read-only entry search functionalities across the LDAP directory.
*   **Core Methods**:
    *   `SearchByIdentifierAsync(...)`: Resolves any LDAP entry matching a specific unique identifier (such as distinguishedName, mail, etc.), filtering properties to optimize performance.
    *   `SearchByFiltersAsync(...)`: Performs logical conditional queries mapping up to two query attributes (e.g., mail, cn, company) to filter entries.

### 4. `LDAPGroupsDirectoryWebApiClient`
Specialized client focused exclusively on group queries.
*   **Core Methods**:
    *   `SearchByIdentifierAsync(...)`: Fetches information about a specific group matching an identifier.
    *   `SearchFilteringByAsync(...)`: Queries multiple groups using configurable directory attribute filters.

### 5. `LDAPUserDirectoryWebApiClient`
The most comprehensive client in the library, handling advanced user searches and full Microsoft Active Directory user provisioning.
*   **Core Methods**:
    *   `SearchFilteringByAsync(...)`: Provides multiple overloads to query user accounts (by sAMAccountName or other attributes).
    *   `CreateMsADUserAccountAsync(LDAPMsADUserAccount account, ...)`: Provisions a new MS Active Directory user account.
    *   `SetMsADUserAccountPasswordAsync(LDAPCredential credential, ...)`: Firmly updates or resets user passwords inside AD securely.
    *   `DisableMsADUserAccountAsync(string identifier, ...)`: Toggles active directory flags to disable a user account.
    *   `RemoveMsADUserAccountAsync(string identifier, ...)`: Safely deletes a user account from AD.

### 6. `LDAPCatalogTypesWebApiClient`
Retrieves the types of catalogs supported by the LDAP service.
*   **Core Method**:
    *   `GetAllAsync(...)`: Queries all catalog types (Global vs Local) registered on the LDAP Web API.

### 7. `LDAPServerProfilesWebApiClient`
Exposes management configurations for configured LDAP servers.
*   **Core Methods**:
    *   `GetProfileIdsAsync(...)`: Retrieves the unique IDs of all configured LDAP server profiles.
    *   `GetByProfileIdAsync(string profileId, ...)`: Fetches full structural details of a specific server profile.
    *   `GetAllAsync(...)`: Retrieves all profiles registered in the system.

---

## 🚀 Installation

Install the library via the NuGet Package Manager Console:

```bash
dotnet add package Bitai.LDAPWebApi.Client
```

---

## 💻 Integrated Code Example

The following code illustrates how to configure the `LDAPUserDirectoryWebApiClient` with Identity Server credentials and execute a secure user query:

```csharp
using System;
using System.Threading.Tasks;
using Bitai.LDAPHelper.DTO;
using Bitai.LDAPWebApi.Clients;
using Bitai.WebApi.Client;

public class LDAPService
{
    private readonly LDAPUserDirectoryWebApiClient _userClient;

    public LDAPService()
    {
        // Define API base address and target LDAP Profile ID
        string apiBaseUrl = "https://api.company.local/ldap";
        string serverProfileId = "DefaultActiveDirectory";
        bool useGlobalCatalog = false;

        // Configure client credentials to automatically get Bearer Tokens
        var credentials = new WebApiClientCredential
        {
            ClientId = "ldap-client-app",
            ClientSecret = "SuperSecureClientSecret123!",
            Authority = "https://identity.company.local",
            Scope = "ldap.api.read ldap.api.write"
        };

        // Initialize the client
        _userClient = new LDAPUserDirectoryWebApiClient(
            apiBaseUrl, 
            serverProfileId, 
            useGlobalCatalog, 
            credentials
        );
    }

    public async Task FindUserByEmailAsync(string email)
    {
        // Execute search requesting specific attributes (minimizing payload size)
        var response = await _userClient.SearchFilteringByAsync(
            EntryAttribute.mail, 
            email, 
            RequiredEntryAttributes.Few, 
            setBearerToken: true
        );

        if (response.IsSuccessStatusCode)
        {
            var searchResult = response.GetContent<LDAPSearchResult>();
            Console.WriteLine($"Search completed. Matched {searchResult.Entries.Count} user(s).");
            
            foreach (var entry in searchResult.Entries)
            {
                Console.WriteLine($"Name: {entry.displayName} | DN: {entry.distinguishedName}");
            }
        }
        else
        {
            Console.WriteLine($"Error querying API: {response.ReasonPhrase}");
        }
    }
}
```

---
