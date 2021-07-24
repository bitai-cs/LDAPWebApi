using Bitai.LDAPHelper.DTO;
using Bitai.LDAPWebApi.DTO;
using Bitai.WebApi.Client;
using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Clients
{
    public class LDAPBaseClient<DTOType> : WebApiBaseClient<DTOType>
    {
        public string ServerProfile { get; set; }
        public bool UseGlobalCatalog { get; set; }
        public LDAPCatalogTypes CatalogTypes => new LDAPCatalogTypes();
        public WebApiSecurityDefinition WebApiSecurityDefinition { get; set; }



        public LDAPBaseClient(string webApiBaseUrl, WebApiSecurityDefinition webApiSecurity) : base(webApiBaseUrl)
        {
            WebApiSecurityDefinition = webApiSecurity;
        }

        public LDAPBaseClient(string webApiBaseUrl, string serverProfile, bool useGlobalCatalog, WebApiSecurityDefinition webApiSecurity) : this(webApiBaseUrl, webApiSecurity)
        {
            ServerProfile = serverProfile;
            UseGlobalCatalog = useGlobalCatalog;
        }



        public string GetOptionalEntryAttributeName(EntryAttribute? nullable)
        {
            return nullable.HasValue ? nullable.ToString() : string.Empty;
        }

        public string GetOptionalRequiredEntryAttributesName(RequiredEntryAttributes? nullable)
        {
            return nullable.HasValue ? nullable.ToString() : string.Empty;
        }

        public string GetOptionalBooleanValue(bool? nullable)
        {
            return nullable.HasValue ? nullable.ToString() : string.Empty;
        }

        public async Task<HttpClient> CreateHttpClient(bool setAuthenticationHeader)
        {
            var httpClient = CreateHttpClient();

            if (setAuthenticationHeader)
                await CheckSecurityHeaderHealth(httpClient, WebApiSecurityDefinition);

            return httpClient;
        }



        #region Static methods
        internal static TokenResponse _cachedTokenResponse;
        internal static DateTime? _cachedTokenExpireDate;

        internal static async Task CheckSecurityHeaderHealth(HttpClient httpClient, WebApiSecurityDefinition webApiScurityDefinition)
        {
            var now = DateTime.Now;

            if (_cachedTokenResponse == null || now >= _cachedTokenExpireDate)
            {
                _cachedTokenResponse = await GetTokenForClientCredentials(httpClient, webApiScurityDefinition);

                if (_cachedTokenResponse.IsError)
                    throw new Exception($"Can't get security token. Authority: {webApiScurityDefinition.AuthorityUrl} | ClientId: {webApiScurityDefinition.ClientId} | Error: {_cachedTokenResponse.Error}" + (string.IsNullOrEmpty(_cachedTokenResponse.ErrorDescription) ? string.Empty : $" | Error description: {_cachedTokenResponse.ErrorDescription}") + $" | Error type: {_cachedTokenResponse.ErrorType} | Http Message: {_cachedTokenResponse.HttpErrorReason}");

                _cachedTokenExpireDate = now.AddSeconds(_cachedTokenResponse.ExpiresIn);
            }

            SetAuthenticationHeaderWithBearerToken(httpClient, _cachedTokenResponse);
        }

        internal static async Task<TokenResponse> GetTokenForClientCredentials(HttpClient httpClient, WebApiSecurityDefinition webApiSecurity)
        {
            var discoveryDocResponse = await httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = webApiSecurity.AuthorityUrl,
                Policy = new DiscoveryPolicy
                {
                    RequireHttps = false
                }
            });

            if (discoveryDocResponse.IsError)
            {
                var exceptionMessage = $"Failed to get security token. An error occurred while getting the discovery document from authority {webApiSecurity.AuthorityUrl}. Error: {discoveryDocResponse.Error} | Error type: {discoveryDocResponse.ErrorType}";

                if (discoveryDocResponse.HttpResponse != null)
                    exceptionMessage += $" Http Status Code: {Convert.ToInt32(discoveryDocResponse.HttpStatusCode)} | Http Message: {discoveryDocResponse.HttpErrorReason}";

                throw new Exception(exceptionMessage);
            }

            return await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = discoveryDocResponse.TokenEndpoint,
                Scope = webApiSecurity.ApiScope,
                ClientId = webApiSecurity.ClientId,
                ClientSecret = webApiSecurity.ClientSecret
            });
        }

        internal static void SetAuthenticationHeaderWithBearerToken(HttpClient httpClient, TokenResponse tokenResponse)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
        }
        #endregion



        #region Static inner class
        public static class ControllerNames
        {
            public static readonly string ServerProfilesController = "ServerProfiles";
            public static readonly string CatalogTypesController = "CatalogTypes";
            public static readonly string AuthenticationsController = "Authentications";
            public static readonly string DirectoryController = "Directory";
        }
        #endregion
    }
}
