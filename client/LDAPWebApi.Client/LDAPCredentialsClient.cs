using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bitai.WebApi.Client;
using IdentityModel.Client;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;

namespace Bitai.LDAPWebApi.Clients
{
    //internal class LdapCredentials2Client
    //{
    //	HttpClient _httpClient;
    //	internal LdapCredentialsClient(HttpClient httpClient)
    //	{
    //		_httpClient = httpClient;
    //	}

    //	public async Task<string> GetTokenRefreshForClientCredentials()
    //	{
    //		var _discoveryClient = await _httpClient.GetDiscoveryDocumentAsync("http://npe-pres2081331/is4sts2");
    //		if (_discoveryClient.IsError) throw new Exception(_discoveryClient.Error);

    //		var _tokenResponse = await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
    //		{
    //			Address = _discoveryClient.TokenEndpoint,
    //			ClientId = "LdapProxyWebApiClient",
    //			ClientSecret = "9q80hoMHdWCU7s3PMW1Hn9EnnpJ6UYrZuP1WTsjHPgM=",
    //			Scope = "LdapProxyWebApi"
    //		});

    //		if (_tokenResponse.IsError) return null;

    //		return _tokenResponse.AccessToken;
    //	}
    //}

    public class LDAPCredentialsClient<DTOType> : LDAPBaseClient<DTOType>
    {
        public LDAPCredentialsClient(string webApiBaseUrl) : base(webApiBaseUrl)
        {
        }

        public LDAPCredentialsClient(string webApiBaseUrl, string serverProfile, bool useGlobalCatalog) : base(webApiBaseUrl, serverProfile, useGlobalCatalog)
        {
        }



        public async Task<IHttpResponse> AccountAuthenticationAsync(string accountName, DTO.LDAPAccountSecurityData authenticationRequest, CancellationToken cancellationToken = default)
        {
            var uri = $"/api/{ServerProfile}/{Parameters.CatalogTypes.GetCatalogTypeName(UseGlobalCatalog)}/{ControllerNames.CredentialsController}/{accountName}/Authentication";

            using (var httpClient = CreateHttpClient())
            {
                //TODO: To complete, assign token
                //var _token = await new LdapCredentialsClient(httpClient).GetTokenRefreshForClientCredentials();

                var content = new ObjectContent<DTO.LDAPAccountSecurityData>(authenticationRequest, new JsonMediaTypeFormatter());

                var responseMessage = await httpClient.PostAsync(uri, content, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                    return await ParseHttpResponseToNoSuccessResponseAsync(responseMessage);
                else
                    return await ParseHttpResponseToSuccessDTOResponseAsync(responseMessage);
            }
        }
    }
}