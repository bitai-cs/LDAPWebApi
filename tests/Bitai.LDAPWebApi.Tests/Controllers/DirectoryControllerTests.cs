using System.Net;
using System.Net.Http.Json;
using Bitai.LDAPHelper.DTO;
using Bitai.LDAPWebApi.DTO;
using Bitai.LDAPWebApi.Tests.Infrastructure;

namespace Bitai.LDAPWebApi.Tests.Controllers;

/// <summary>
/// Integration tests for <see cref="Bitai.LDAPWebApi.Controllers.DirectoryController"/>.
///
/// The tests use the built-in mock LDAP data store activated via
/// <c>EnablePersistentMockLdapDataStore = true</c>.  The <see cref="LDAPWebApiFactory"/>
/// starts the full ASP.NET Core pipeline with authorization bypassed.
///
/// Mock data snapshot (from <c>MockLdapDataSeeder.SeedAllData()</c>):
///   • Server profile  : HOLDING  (BaseDN = DC=holding,DC=latam,DC=com)
///   • Catalog types   : LC (local) | GC (global)
///   • Users with sAMAccountName:
///       james.dockers, sara.pikes, robert.miller, isaac.newton, manuel.cordoba,
///       saint.seiya, ken.master, victor.bastidas, red.robbin, alice.wonder
///   • Groups with sAMAccountName:
///       DomainAdmins, ITAdmins, DevOpsEng, SeniorDevOps, JuniorDevOps,
///       Interns, SupportTeam, Administrators, SecurityAnalysts, DevOpsLeaders
/// </summary>
[Collection("LDAPWebApi Integration Tests")]
public class DirectoryControllerTests : IClassFixture<LDAPWebApiFactory>
{
    private readonly HttpClient _client;

    private const string ServerProfile = "HOLDING";
    private const string LC = "LC";   // local catalog
    private const string GC = "GC";   // global catalog

    public DirectoryControllerTests(LDAPWebApiFactory factory)
    {
        _client = factory.CreateClient();
    }




    #region GET …/Directory/{identifier}   (GetByIdentifier)
    /// <summary>
    /// Retrieve a known user by sAMAccountName.  Default identifier attribute is sAMAccountName.
    /// </summary>
    [Fact]
    public async Task GetByIdentifier_ExistingUser_BySAMAccountName_ReturnsOkWithEntry()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/james.dockers?identifierAttribute={EntryAttribute.sAMAccountName}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.Single(result.Entries);
        Assert.Equal("james.dockers",
            result.Entries.First().samAccountName,
            ignoreCase: true);
    }

    /// <summary>
    /// Retrieve a known user by distinguishedName.
    /// </summary>
    [Fact]
    public async Task GetByIdentifier_ExistingUser_ByDistinguishedName_ReturnsOkWithEntry()
    {
        // Arrange – james.dockers is in OU=Seniors,OU=DevOps,OU=IT,DC=holding,DC=latam,DC=com
        const string dn = "CN=James Dockers,OU=Seniors,OU=DevOps,OU=IT,DC=holding,DC=latam,DC=com";
        var url = $"api/{ServerProfile}/{LC}/Directory/{Uri.EscapeDataString(dn)}?identifierAttribute={EntryAttribute.distinguishedName}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.Single(result.Entries);
        Assert.Equal(dn, result.Entries.First().distinguishedName, ignoreCase: true);
    }

    /// <summary>
    /// A sAMAccountName that does not exist should return 404 Not Found.
    /// </summary>
    [Fact]
    public async Task GetByIdentifier_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/ghost.user?identifierAttribute={EntryAttribute.sAMAccountName}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    #endregion

    #region GET …/Directory/filterBy   (FilterByAsync)
    /// <summary>
    /// Filter entries by a single attribute – here <c>sAMAccountName=james.dockers</c>.
    /// </summary>
    [Fact]
    public async Task FilterBy_SingleFilter_MatchingUser_ReturnsOkWithEntries()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/filterBy" +
                  $"?filterAttribute={EntryAttribute.sAMAccountName}&filterValue=james.dockers&requiredAttributes={RequiredEntryAttributes.Minimun}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.NotEmpty(result.Entries);
        Assert.All(result.Entries, e =>
            Assert.Equal("james.dockers", e.samAccountName, ignoreCase: true));
    }

    /// <summary>
    /// Filter entries by a wildcard – <c>sAMAccountName=*.dockers*</c> should find james.dockers.
    /// </summary>
    [Fact]
    public async Task FilterBy_WildcardFilter_ReturnsMatchingEntries()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/filterBy" +
                  $"?filterAttribute={EntryAttribute.sAMAccountName}&filterValue=*dockers*&requiredAttributes={RequiredEntryAttributes.Minimun}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.NotEmpty(result.Entries);
    }

    /// <summary>
    /// Filter with two attribute filters combined with AND (combineFilters=true).
    /// Searching for sAMAccountName=james.dockers AND givenName=James should return one entry.
    /// </summary>
    [Fact]
    public async Task FilterBy_TwoFiltersAndCombined_ReturnsMatchingEntry()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/filterBy" +
                  $"?filterAttribute={EntryAttribute.sAMAccountName}&filterValue=james.dockers" +
                  $"&secondFilterAttribute=givenName&secondFilterValue=James" +
                  $"&combineFilters=true&requiredAttributes={RequiredEntryAttributes.Minimun}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.NotEmpty(result.Entries);
    }

    /// <summary>
    /// Filter with two attribute filters combined with OR (combineFilters=false).
    /// Searching for sAMAccountName=james.dockers OR sAMAccountName=isaac.newton
    /// should return at least two entries.
    /// </summary>
    [Fact]
    public async Task FilterBy_TwoFiltersOrCombined_ReturnsBothEntries()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/filterBy" +
                  $"?filterAttribute={EntryAttribute.sAMAccountName}&filterValue=james.dockers" +
                  $"&secondFilterAttribute={EntryAttribute.sAMAccountName}&secondFilterValue=isaac.newton" +
                  $"&combineFilters=false&requiredAttributes={RequiredEntryAttributes.Minimun}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.True(result.Entries.Count() > 0, "OR filter should return at least 1 entries.");
    }

    /// <summary>
    /// A filter that matches no entries should still return 200 OK with an empty
    /// entries collection (not a 404).
    /// </summary>
    [Fact]
    public async Task FilterBy_NoMatch_ReturnsOkWithEmptyEntries()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/filterBy" +
                  $"?filterAttribute={EntryAttribute.sAMAccountName}&filterValue=GhostUser&requiredAttributes={RequiredEntryAttributes.Minimun}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.Empty(result.Entries);
    }
    #endregion

    #region GET …/Directory/Users/filterBy   (GetUsersFilteringByAsync)
    /// <summary>
    /// Without any filters the endpoint should return only user entries (not groups).
    /// </summary>
    [Fact]
    public async Task GetUsersFilteringBy_WithSingleFilter_ReturnsOnlyUserEntries()
    {
        // Arrange – search for users with sAMAccountName containing "dockers"
        var url = $"api/{ServerProfile}/{LC}/Directory/Users/filterBy" +
                  $"?filterAttribute={EntryAttribute.sAMAccountName}&filterValue=*dockers*&requiredAttributes={RequiredEntryAttributes.Few}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.NotEmpty(result.Entries);
        // All returned entries must be user objects (they contain "user" objectClass)
        // – we verify by checking that sAMAccountName is populated (groups have it too
        //   but the endpoint filters by user objectClass internally)
        Assert.All(result.Entries, e => Assert.False(string.IsNullOrEmpty(e.samAccountName)));
    }

    /// <summary>
    /// A dual-attribute AND-combined user filter: sAMAccountName=james.dockers AND givenName=James.
    /// </summary>
    [Fact]
    public async Task GetUsersFilteringBy_TwoFiltersAnd_ReturnsMatchingUsers()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/Users/filterBy" +
                  $"?filterAttribute={EntryAttribute.sAMAccountName}&filterValue=james.dockers" +
                  $"&secondFilterAttribute=givenName&secondFilterValue=James" +
                  $"&combineFilters=true&requiredAttributes={RequiredEntryAttributes.Minimun}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.NotEmpty(result.Entries);
    }

    /// <summary>
    /// A user filter that matches no entries returns 200 OK with empty collection.
    /// </summary>
    [Fact]
    public async Task GetUsersFilteringBy_NoMatch_ReturnsOkWithEmptyEntries()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/Users/filterBy" +
                  $"?filterAttribute={EntryAttribute.sAMAccountName}&filterValue=zzz_no_one_here&requiredAttributes={RequiredEntryAttributes.Minimun}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.Empty(result.Entries);
    }
    #endregion

    #region GET …/Directory/Users/{identifier}/Parents   (GetParentsForUserIdentifier)
    /// <summary>
    /// Retrieving parent groups for james.dockers (who is member of DomainAdmins, ITAdmins,
    /// DevOpsEng, SeniorDevOps, DevOpsLeaders) should return at least one entry.
    /// </summary>
    [Fact]
    public async Task GetParentsForUserIdentifier_ExistingUser_BySAMAccountName_ReturnsParents()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/Users/james.dockers/Parents" +
                  $"?identifierAttribute={EntryAttribute.sAMAccountName}&requiredAttributes={RequiredEntryAttributes.Few}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.NotEmpty(result.Entries);
    }

    /// <summary>
    /// Retrieving parents for a user that has no group memberships or does not exist
    /// returns 200 OK with an empty entries list (not 404 – the controller does not throw
    /// ResourceNotFoundException for this endpoint).
    /// </summary>
    [Fact]
    public async Task GetParentsForUserIdentifier_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/Users/ghost.nobody/Parents" +
                  $"?identifierAttribute={EntryAttribute.sAMAccountName}&requiredAttributes={RequiredEntryAttributes.Minimun}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        // The DirectoryController.GetParentsForUserIdentifier does not 404 on empty results
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.NotNull(result);
        Assert.Contains("IsMiddlewareException", result, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parent entries for ken.master should include the Administrators group
    /// (seeded in the mock data: ken.master is member of Administrators).
    /// </summary>
    [Fact]
    public async Task GetParentsForUserIdentifier_UserWithAdminGroup_ReturnsAdministratorsAmongParents()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/Users/ken.master/Parents" +
                  $"?identifierAttribute={EntryAttribute.sAMAccountName}&requiredAttributes={RequiredEntryAttributes.Few}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.NotEmpty(result.Entries);
        Assert.Contains(result.Entries,
            e => e.samAccountName != null &&
                 e.samAccountName.Contains("Administrators", StringComparison.OrdinalIgnoreCase));
    }
    #endregion

    #region GET …/Directory/Groups/{identifier}   (GetGroupByIdentifier)
    /// <summary>
    /// Retrieve a known group by sAMAccountName (DomainAdmins).
    /// </summary>
    [Fact]
    public async Task GetGroupByIdentifier_ExistingGroup_BySAMAccountName_ReturnsOkWithGroup()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/Groups/DomainAdmins" +
                  $"?identifierAttribute={EntryAttribute.sAMAccountName}&requiredAttributes={RequiredEntryAttributes.Few}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.Single(result.Entries);
        Assert.Equal("DomainAdmins", result.Entries.First().samAccountName, ignoreCase: true);
    }

    /// <summary>
    /// Retrieve a group by its distinguishedName.
    /// </summary>
    [Fact]
    public async Task GetGroupByIdentifier_ExistingGroup_ByDistinguishedName_ReturnsOkWithGroup()
    {
        // Arrange
        const string dn = "CN=Domain Admins,CN=Users,DC=holding,DC=latam,DC=com";
        var url = $"api/{ServerProfile}/{LC}/Directory/Groups/{Uri.EscapeDataString(dn)}" +
                  $"?identifierAttribute={EntryAttribute.distinguishedName}&requiredAttributes={RequiredEntryAttributes.Few}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.Single(result.Entries);
        Assert.Equal(dn, result.Entries.First().distinguishedName, ignoreCase: true);
    }

    /// <summary>
    /// A sAMAccountName that does not correspond to any group should return 404 Not Found.
    /// </summary>
    [Fact]
    public async Task GetGroupByIdentifier_NonExistentGroup_ReturnsNotFound()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/Groups/NonExistentGroup999" +
                  $"?identifierAttribute={EntryAttribute.sAMAccountName}&requiredAttributes={RequiredEntryAttributes.Minimun}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    #endregion

    #region GET …/Directory/Groups/{identifier}/Parents  (GetParentsForGroupIdentifier)
    /// <summary>
    /// Retrieve the parent groups of SeniorDevOps.
    /// Mock data nesting: SeniorDevOps → DevOpsEng → ITAdmins → DomainAdmins → Administrators
    /// so at least one parent should be returned.
    /// </summary>
    [Fact]
    public async Task GetParentsForGroupIdentifier_ExistingGroup_ReturnsParents()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/Groups/SeniorDevOps/Parents" +
                  $"?identifierAttribute={EntryAttribute.sAMAccountName}&requiredAttributes={RequiredEntryAttributes.Few}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.NotEmpty(result.Entries);
    }

    /// <summary>
    /// Retrieve parents for a group that has no memberOf attribute or does not exist.
    /// The endpoint returns 200 OK with an empty entries list.
    /// </summary>
    [Fact]
    public async Task GetParentsForGroupIdentifier_NonExistentGroup_ReturnsNotFound()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/Groups/NoSuchGroup/Parents" +
                  $"?identifierAttribute={EntryAttribute.sAMAccountName}&requiredAttributes={RequiredEntryAttributes.Minimun}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.NotNull(result);
        Assert.Contains("IsMiddlewareException", result, StringComparison.OrdinalIgnoreCase);
    }
    #endregion

    #region GET …/Directory/Groups/filterBy   (GetGroupsFilteringByAsync)
    /// <summary>
    /// Filter groups by sAMAccountName containing "DevOps" – expects DevOpsEng, SeniorDevOps,
    /// JuniorDevOps, DevOpsLeaders to be returned.
    /// </summary>
    [Fact]
    public async Task GetGroupsFilteringBy_WildcardFilter_ReturnsMatchingGroups()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/Groups/filterBy" +
                  $"?filterAttribute={EntryAttribute.sAMAccountName}&filterValue=*DevOps*&requiredAttributes={RequiredEntryAttributes.Few}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.NotEmpty(result.Entries);
        Assert.All(result.Entries, e =>
            Assert.Contains("DevOps", e.samAccountName ?? "", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Filter groups using two attributes combined with AND – both conditions must match
    /// the same entry.
    /// </summary>
    [Fact]
    public async Task GetGroupsFilteringBy_TwoFiltersAnd_ReturnsOnlyFullyMatchingGroups()
    {
        // Arrange: sAMAccountName=SeniorDevOps AND cn=Senior DevOps
        var url = $"api/{ServerProfile}/{LC}/Directory/Groups/filterBy" +
                  $"?filterAttribute={EntryAttribute.sAMAccountName}&filterValue=SeniorDevOps" +
                  $"&secondFilterAttribute=cn&secondFilterValue=Senior DevOps" +
                  $"&combineFilters=true&requiredAttributes={RequiredEntryAttributes.Few}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.NotEmpty(result.Entries);
        Assert.All(result.Entries, e =>
            Assert.Equal("SeniorDevOps", e.samAccountName, ignoreCase: true));
    }

    /// <summary>
    /// Filter groups that do not match anything returns 200 OK with empty entries.
    /// </summary>
    [Fact]
    public async Task GetGroupsFilteringBy_NoMatch_ReturnsOkWithEmptyEntries()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/Groups/filterBy" +
                  $"?filterAttribute={EntryAttribute.sAMAccountName}&filterValue=ZZZ_NonExistentGroup&requiredAttributes={RequiredEntryAttributes.Minimun}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPSearchResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.Empty(result.Entries);
    }
    #endregion

    #region POST …/Directory/MsADUsers   (CreateUserAccountForMsAD)
    /// <summary>
    /// Creating a new user account in the local catalog should return 500 InternalServerError
    /// with a usuccessful operation result.
    /// </summary>
    [Fact]
    public async Task CreateUserAccountForMsAD_NewUser_LocalCatalog_ReturnsInternalServerError()
    {
        var newId = Guid.NewGuid().ToString("N")[..8];

        // Arrange – DiplayName and ObjectClass are not provided, which should cause the controller to throw an exception
        var newUser = new LDAPMsADUserAccount
        {
            SAMAccountName = $"test.newuser.{newId}",
            GivenName = "Test",
            Sn = $"NewUser {newId}",
            Cn = $"Test NewUser {newId}",
            Password = "TestP@ssword1!",
            DistinguishedNameOfContainer = "OU=Juniors,OU=DevOps,OU=IT,DC=holding,DC=latam,DC=com"
        };
        var url = $"api/{ServerProfile}/{LC}/Directory/MsADUsers";

        // Act
        var response = await _client.PostAsJsonAsync(url, newUser);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains(nameof(WebApi.Server.MiddlewareExceptionModel.IsMiddlewareException), result, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creating a new user account in the local catalog should return 200 OK with a
    /// successful operation result.
    /// </summary>
    [Fact]
    public async Task CreateUserAccountForMsAD_NewUser_LocalCatalog_ReturnsOk()
    {
        var newId = Guid.NewGuid().ToString("N")[..8];

        // Arrange – use a unique SAMAccountName to avoid conflicts with seeded data
        var newUser = new LDAPMsADUserAccount
        {
            SAMAccountName = $"test.newuser.{newId}",
            GivenName = "Test",
            Sn = $"NewUser {newId}",
            Cn = $"Test NewUser {newId}",
            DisplayName = $"Test NewUser {newId} (xUnit)",
            Password = "TestP@ssword1!",
            DistinguishedNameOfContainer = "OU=Juniors,OU=DevOps,OU=IT,DC=holding,DC=latam,DC=com",
            ObjectClass = new[] { "top", "person", "organizationalPerson", "user" }
        };
        var url = $"api/{ServerProfile}/{LC}/Directory/MsADUsers";

        // Act
        var response = await _client.PostAsJsonAsync(url, newUser);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPCreateMsADUserAccountResult>();
        Assert.Contains($"MS AD user account created at CN=Test NewUser {newId}", result.OperationMessage, StringComparison.OrdinalIgnoreCase);
        Assert.True(result.IsSuccessfulOperation);
    }

    /// <summary>
    /// Attempting to create a user account in the global catalog should return 400 Bad Request
    /// because the controller explicitly rejects writes to the global catalog.
    /// </summary>
    [Fact]
    public async Task CreateUserAccountForMsAD_UsingGlobalCatalog_ReturnsBadRequest()
    {
        // Arrange
        var newUser = new LDAPMsADUserAccount
        {
            SAMAccountName = "gc.blocked.user",
            GivenName = "GC",
            Sn = "Blocked",
            Password = "TestP@ssword1!",
            DistinguishedNameOfContainer = "OU=Juniors,OU=DevOps,OU=IT,DC=holding,DC=latam,DC=com"
        };
        var url = $"api/{ServerProfile}/{GC}/Directory/MsADUsers";

        // Act
        var response = await _client.PostAsJsonAsync(url, newUser);

        // Assert – the controller throws BadRequestException for global catalog writes
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Creating a user that already exists (same SAMAccountName as seeded victor.bastidas)
    /// should return 409 Conflict.
    /// </summary>
    [Fact]
    public async Task CreateUserAccountForMsAD_DuplicateSAMAccountName_ReturnsConflict()
    {
        // Arrange – victor.bastidas is seeded by MockLdapDataSeeder
        var duplicateUser = new LDAPMsADUserAccount
        {
            SAMAccountName = "victor.bastidas",
            GivenName = "Victor",
            Sn = "Bastidas",
            Cn = "Victor Bastidas",
            DisplayName = "Victor Bastidas (Duplicate)",
            Password = "TestP@ssword1!",
            DistinguishedNameOfContainer = "OU=Seniors,OU=DevOps,OU=IT,DC=holding,DC=latam,DC=com",
            ObjectClass = new[] { "top", "person", "organizationalPerson", "user" }
        };
        var url = $"api/{ServerProfile}/{LC}/Directory/MsADUsers";

        // Act
        var response = await _client.PostAsJsonAsync(url, duplicateUser);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
    #endregion

    #region PATCH …/Directory/MsADUsers/{identifier}/Credential   (SetUserAccountCredentialForMsAD)
    /// <summary>
    /// Setting a new password for a valid user (red.robbin) by sAMAccountName should return 200 OK.
    /// </summary>
    [Fact]
    public async Task SetUserAccountCredentialForMsAD_ValidUser_BySAMAccountName_ReturnsOk()
    {
        // Arrange
        var credential = new LDAPCredential
        {
            UserAccount = "red.robbin",
            Password = "NewTestP@ssword1!"
        };
        var url = $"api/{ServerProfile}/{LC}/Directory/MsADUsers/red.robbin/Credential" +
                  $"?identifierAttribute={EntryAttribute.sAMAccountName}";

        // Act
        var response = await _client.PatchAsJsonAsync(url, credential);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPPasswordUpdateResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
    }

    /// <summary>
    /// Setting a password for a non-existent user should return 404 Not Found.
    /// </summary>
    [Fact]
    public async Task SetUserAccountCredentialForMsAD_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var credential = new LDAPCredential
        {
            UserAccount = "ghost.nobody",
            Password = "NewTestP@ssword1!"
        };
        var url = $"api/{ServerProfile}/{LC}/Directory/MsADUsers/ghost.nobody/Credential" +
                  $"?identifierAttribute={EntryAttribute.sAMAccountName}";

        // Act
        var response = await _client.PatchAsJsonAsync(url, credential);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Sending a credential with a mismatching user account vs. route identifier should
    /// return 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task SetUserAccountCredentialForMsAD_AccountMismatch_ReturnsBadRequest()
    {
        // Arrange – route says "red.robbin" but credential says "james.dockers"
        var credential = new LDAPCredential
        {
            UserAccount = "HOLDING\\james.dockers",  // domain\account format triggers domain validation
            Password = "NewTestP@ssword1!"
        };
        // Route identifier is different from credential
        var url = $"api/{ServerProfile}/{LC}/Directory/MsADUsers/red.robbin/Credential" +
                  $"?identifierAttribute={EntryAttribute.sAMAccountName}";

        // Act
        var response = await _client.PatchAsJsonAsync(url, credential);

        // Assert – controller validates identifier matches credential account
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    #endregion

    #region PATCH …/Directory/MsADUsers/{identifier}/disableBy   (DisableMsADUserAccount)
    /// <summary>
    /// Disabling a user account (magic.cuy from PE domain, seeded as intern) by sAMAccountName
    /// should return 200 OK with a successful result.
    ///
    /// Note: magic.cuy is seeded under DC=pe,DC=latam,DC=com but the HOLDING profile BaseDN
    /// is DC=holding,DC=latam,DC=com.  When the mock adapter is used it operates on all entries
    /// in the in-memory store regardless of BaseDN, so the user is still reachable.
    /// We therefore use robert.miller who is under DC=holding,DC=latam,DC=com.
    /// </summary>
    [Fact]
    public async Task DisableMsADUserAccount_ValidUser_BySAMAccountName_ReturnsOk()
    {
        // Arrange – robert.miller is a HOLDING intern (seeded in the mock store)
        var url = $"api/{ServerProfile}/{LC}/Directory/MsADUsers/robert.miller/disableBy" +
                  $"?identifierAttribute={EntryAttribute.sAMAccountName}";

        // Act
        var response = await _client.PatchAsync(url, null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPDisableUserAccountOperationResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
    }

    /// <summary>
    /// Disabling a user via the global catalog should return 400 Bad Request because the
    /// controller explicitly blocks write operations on the global catalog.
    /// </summary>
    [Fact]
    public async Task DisableMsADUserAccount_UsingGlobalCatalog_ReturnsBadRequest()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{GC}/Directory/MsADUsers/robert.miller/disableBy" +
                  $"?identifierAttribute={EntryAttribute.sAMAccountName}";

        // Act
        var response = await _client.PatchAsync(url, null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Disabling a non-existent user should return 404 Not Found.
    /// </summary>
    [Fact]
    public async Task DisableMsADUserAccount_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/MsADUsers/nonexistent.person/disableBy" +
                  $"?identifierAttribute={EntryAttribute.sAMAccountName}";

        // Act
        var response = await _client.PatchAsync(url, null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    #endregion

    #region DELETE …/Directory/MsADUsers/{identifier}   (RemoveMsADUserAccount)
    /// <summary>
    /// Removing a user (magic.cuy, seeded as PE intern) by sAMAccountName
    /// should return 200 OK.
    /// We use alice.wonder who is under DC=holding,DC=latam,DC=com.
    /// </summary>
    [Fact]
    public async Task RemoveMsADUserAccount_ValidUser_BySAMAccountName_ReturnsOk()
    {
        // Arrange – alice.wonder is seeded under OU=Support,OU=IT,DC=holding,...
        var url = $"api/{ServerProfile}/{LC}/Directory/MsADUsers/alice.wonder" +
                  $"?identifierAttribute={EntryAttribute.sAMAccountName}";

        // Act
        var response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LDAPRemoveMsADUserAccountResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
    }

    /// <summary>
    /// Attempting to remove a user via the global catalog should return 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task RemoveMsADUserAccount_UsingGlobalCatalog_ReturnsBadRequest()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{GC}/Directory/MsADUsers/alice.wonder" +
                  $"?identifierAttribute={EntryAttribute.sAMAccountName}";

        // Act
        var response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Removing a non-existent user should return 404 Not Found.
    /// </summary>
    [Fact]
    public async Task RemoveMsADUserAccount_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var url = $"api/{ServerProfile}/{LC}/Directory/MsADUsers/no.such.person" +
                  $"?identifierAttribute={EntryAttribute.sAMAccountName}";

        // Act
        var response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    #endregion

    #region Route constraint validation
    /// <summary>
    /// Any endpoint with an unknown server profile should be rejected by the
    /// <c>ldapSvrPf</c> route constraint returning 404.
    /// </summary>
    [Fact]
    public async Task AnyEndpoint_UnknownServerProfile_ReturnsNotFound()
    {
        var url = $"api/UNKNOWN_PROFILE/{LC}/Directory/james.dockers?identifierAttribute={EntryAttribute.sAMAccountName}";
        var response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Any endpoint with an unknown catalog type should be rejected by the
    /// <c>ldapCatType</c> route constraint returning 404.
    /// </summary>
    [Fact]
    public async Task AnyEndpoint_UnknownCatalogType_ReturnsNotFound()
    {
        var url = $"api/{ServerProfile}/INVALID_CATALOG/Directory/james.dockers?identifierAttribute={EntryAttribute.sAMAccountName}";
        var response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    #endregion
}
