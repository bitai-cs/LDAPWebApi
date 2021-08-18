using Bitai.WebApi.Client;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Clients
{
    /// <summary>
    /// Client that makes requests to the authentication controller 
    /// of the LDAP Web Api.
    /// </summary>
    public class LDAPAuthenticationsWebApiClient : LDAPWebApiBaseClient
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
        /// <param name="ldapServerProfile">LDAP Server Profile Id.</param>
        /// <param name="useLdapServerGlobalCatalog">Whether or not the global catalog of the LDAP server will be used; otherwise the local catalog of the LDAP server will be used.</param>
        /// <param name="clientCredentials">Client credentials to request an access token  from the Identity Server. This access token will be sent in the HTTP authorization header as Bearer Token.</param>
        public LDAPAuthenticationsWebApiClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog, WebApiClientCredentials clientCredentials = null) : base(ldapWebApiBaseUrl, ldapServerProfile, useLdapServerGlobalCatalog, clientCredentials)
        {
        }



        /// <summary>
        /// Send a post request to LDAP Web Api Authentications controller.
        /// </summary>
        /// <param name="accountCredential">Account credentials or Network credentials</param>
        /// <param name="setAuthorizationHeaderWithBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
        /// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
        /// <returns></returns>
        public async Task<IHttpResponse> AccountAuthenticationAsync(DTO.LDAPAccountCredentials accountCredential, bool setAuthorizationHeaderWithBearerToken = true, CancellationToken cancellationToken = default)
        {
            var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.AuthenticationsController}";

            using (var httpClient = await CreateHttpClient(setAuthorizationHeaderWithBearerToken))
            {
                var content = new ObjectContent<DTO.LDAPAccountCredentials>(accountCredential, new JsonMediaTypeFormatter());

                var responseMessage = await httpClient.PostAsync(uri, content, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                    return await responseMessage.ToUnsuccessfulHttpResponseAsync();
                else
                    return await responseMessage.ToSuccessfulHttpResponseAsync<DTO.LDAPAccountAuthenticationStatus>();
            }
        }
    }
}