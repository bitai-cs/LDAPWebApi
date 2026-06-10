using System.Net;
using System.Net.Http.Json;
using Bitai.LDAPHelper.DTO;
using Bitai.LDAPWebApi.Tests.Infrastructure;

namespace Bitai.LDAPWebApi.Tests.Controllers;

/// <summary>
/// Integration tests for <see cref="Bitai.LDAPWebApi.Controllers.AuthenticationsController"/>.
///
/// These tests rely on the application's built-in mock LDAP data store 
/// (<c>EnablePersistentMockLdapDataStore = true</c>).  The <see cref="LDAPWebApiFactory"/>
/// starts the full ASP.NET Core pipeline with <see cref="Bitai.LDAPWebApi.Startup"/> and
/// authorization bypass enabled, so every request is treated as authenticated.
///
/// Route template:
///   POST  api/{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/Authentications/authenticate
///
/// Mock data used (from MockLdapDataSeeder):
///   • Server profile  : HOLDING  (BaseDN = DC=holding,DC=latam,DC=com)
///   • Catalog type    : LC  (local catalog)
///   • Valid user      : james.dockers  /  Domain: HOLDING
///   • Disabled user   : sara.pikes@US (userAccountControl = 514)
/// </summary>
[Collection("LDAPWebApi Integration Tests")]
public class AuthenticationsControllerTests : IClassFixture<LDAPWebApiFactory>
{
    private readonly HttpClient _client;

    private const string ServerProfile = "HOLDING";
    private const string LocalCatalog = "LC";




    public AuthenticationsControllerTests(LDAPWebApiFactory factory)
    {
        _client = factory.CreateClient();
    }




    #region Private methods
    private static string AuthenticateUrl(string serverProfile, string catalogType, string? requestLabel = null)
    {        
        return AuthenticateBaseUrl(serverProfile, catalogType, "authenticate", requestLabel);
    }

    private static string AuthenticateWithoutUserLookupUrl(string serverProfile, string catalogType, string? requestLabel = null)
    {
        return AuthenticateBaseUrl(serverProfile, catalogType, "authenticateWithoutUserLookup", requestLabel);
    }

    private static string AuthenticateBaseUrl(string serverProfile, string catalogType, string authenticateAction, string? requestLabel = null)
    {
        var url = $"api/{serverProfile}/{catalogType}/Authentications/{authenticateAction}";

        if (!string.IsNullOrEmpty(requestLabel))
            url += $"?requestLabel={Uri.EscapeDataString(requestLabel)}";

        return url;
    }
    #endregion

    #region Success scenarios
    /// <summary>
    /// A valid credential for a known user in the mock store should return 200 OK
    /// with <see cref="LDAPDomainAccountAuthenticationResult.IsSuccessfulOperation"/> = true
    /// and <see cref="LDAPDomainAccountAuthenticationResult.IsAuthenticated"/> = true.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_ValidCredential_ReturnsOkAndAuthenticated()
    {
        // Arrange
        var credential = new LDAPDomainAccountCredential(
            domainName: "HOLDING",
            username: "james.dockers",
            domainAccountPassword: "AnyPassword#1"  // mock adapter accepts any password for valid users
        );
        var url = AuthenticateUrl(ServerProfile, LocalCatalog, "test-auth-valid");

        // Act
        var response = await _client.PostAsJsonAsync(url, credential);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LDAPDomainAccountAuthenticationResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation, "Operation should be marked successful.");
        Assert.True(result.IsAuthenticated, "Account should be authenticated.");
    }

    /// <summary>
    /// When the <c>DomainName</c> property is empty the controller should fall back to
    /// the server profile's <c>DefaultDomainName</c> (HOLDING).  The result is still OK.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_CredentialWithoutDomainName_UsesProfileDefaultDomainAndReturnsOk()
    {
        // Arrange: omit domain name; expect the API to fill it from the server profile
        var credential = new LDAPDomainAccountCredential() {
            AccountName = "isaac.newton",
            DomainAccountPassword = "AnyPassword#1"
        };

        var url = AuthenticateUrl(ServerProfile, LocalCatalog);

        // Act
        var response = await _client.PostAsJsonAsync(url, credential);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LDAPDomainAccountAuthenticationResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.True(result.IsAuthenticated);
    }

    /// <summary>
    /// Authentication request with an optional <c>requestLabel</c> query parameter should
    /// propagate the label into the returned result.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_WithRequestLabel_ResultContainsRequestLabel()
    {
        // Arrange
        const string label = "unit-test-label-001";
        var credential = new LDAPDomainAccountCredential("HOLDING", "manuel.cordoba", "AnyPassword#1");
        var url = AuthenticateUrl(ServerProfile, LocalCatalog, label);

        // Act
        var response = await _client.PostAsJsonAsync(url, credential);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LDAPDomainAccountAuthenticationResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.Equal(label, result.RequestLabel);
    }

    /// <summary>
    /// A valid credential for a known user in the mock store should return 200 OK
    /// with <see cref="LDAPDomainAccountAuthenticationResult.IsSuccessfulOperation"/> = true
    /// and <see cref="LDAPDomainAccountAuthenticationResult.IsAuthenticated"/> = true.
    /// </summary>
    [Fact]
    public async Task AuthenticateWithoutUserLookupAsync_ValidCredential_ReturnsOkAndAuthenticated()
    {
        // Arrange
        var credential = new LDAPDomainAccountCredential(
            domainName: "HOLDING",
            username: "james.dockers",
            domainAccountPassword: "AnyPassword#1"  // mock adapter accepts any password for valid users
        );
        var url = AuthenticateWithoutUserLookupUrl(ServerProfile, LocalCatalog, "test-auth-valid");

        // Act
        var response = await _client.PostAsJsonAsync(url, credential);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LDAPDomainAccountAuthenticationResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation, "Operation should be marked successful.");
        Assert.True(result.IsAuthenticated, "Account should be authenticated.");
    }
    #endregion

    #region Failure scenarios
    /// <summary>
    /// A credential for a user that does not exist in the mock store should return a
    /// response where <see cref="LDAPDomainAccountAuthenticationResult.IsAuthenticated"/>
    /// is <c>false</c> (or throw, depending on the mock adapter behavior).
    /// We assert a non-2xx response OR a successful response with authentication = false.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_NonExistentUser_ReturnsNotAuthenticated()
    {
        // Arrange
        var credential = new LDAPDomainAccountCredential("HOLDING", "nobody.here", "AnyPassword#1");
        var url = AuthenticateUrl(ServerProfile, LocalCatalog);

        // Act
        var response = await _client.PostAsJsonAsync(url, credential);

        // Assert: the mock adapter should return authentication = false (or 500/4xx on error)
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LDAPDomainAccountAuthenticationResult>();
            Assert.NotNull(result);
            Assert.False(result.IsAuthenticated, "Unknown user should not be authenticated.");
        }
        else
        {
            // Any non-success HTTP status is also acceptable for a non-existent user
            Assert.True((int)response.StatusCode >= 400,
                $"Expected 4xx/5xx for non-existent user, got {(int)response.StatusCode}.");
        }
    }

    /// <summary>
    /// Sending an empty body (no credential) should return 400 Bad Request because the
    /// <c>[FromBody]</c> model binding will fail.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_NullBody_ReturnsBadRequest()
    {
        // Arrange
        var url = AuthenticateUrl(ServerProfile, LocalCatalog);
        var content = new StringContent("null", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(url, content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// An unknown server profile in the route should be rejected by the
    /// <c>ldapSvrPf</c> route constraint (returns 404).
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_UnknownServerProfile_ReturnsNotFound()
    {
        // Arrange
        var credential = new LDAPDomainAccountCredential("HOLDING", "james.dockers", "AnyPassword#1");
        var url = AuthenticateUrl("UNKNOWN_PROFILE", LocalCatalog);

        // Act
        var response = await _client.PostAsJsonAsync(url, credential);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// An invalid catalog type in the route should be rejected by the
    /// <c>ldapCatType</c> route constraint (returns 404).
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_InvalidCatalogType_ReturnsNotFound()
    {
        // Arrange
        var credential = new LDAPDomainAccountCredential("HOLDING", "james.dockers", "AnyPassword#1");
        var url = AuthenticateUrl(ServerProfile, "INVALID_CATALOG");

        // Act
        var response = await _client.PostAsJsonAsync(url, credential);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Authentication against the Global Catalog catalog type should work the same way
    /// as against the local catalog for a valid user.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_UsingGlobalCatalog_ValidCredential_ReturnsOk()
    {
        // Arrange
        var credential = new LDAPDomainAccountCredential("HOLDING", "james.dockers", "AnyPassword#1");
        var url = AuthenticateUrl(ServerProfile, "GC");

        // Act
        var response = await _client.PostAsJsonAsync(url, credential);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LDAPDomainAccountAuthenticationResult>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessfulOperation);
        Assert.True(result.IsAuthenticated);
    }
    #endregion
}
