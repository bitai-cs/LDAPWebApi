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
        public WebApiSecurityDefinition WebApiSecurity { get; set; }



        public LDAPBaseClient(string webApiBaseUrl, WebApiSecurityDefinition webApiSecurity) : base(webApiBaseUrl)
        {
            WebApiSecurity = webApiSecurity;
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
                await CheckSecurityHeaderHealth(httpClient, WebApiSecurity);

            return httpClient;
        }



        #region Static methods
        internal static TokenResponse _cachedTokenResponse;
        internal static DateTime? _cachedTokenExpireDate;

        internal static async Task CheckSecurityHeaderHealth(HttpClient httpClient, WebApiSecurityDefinition webApiScurity)
        {
            var now = DateTime.Now;

            if (_cachedTokenResponse == null || now >= _cachedTokenExpireDate)
            {
                _cachedTokenResponse = await GetTokenForClientCredentials(httpClient, webApiScurity);

                if (_cachedTokenResponse == null)
                    throw new Exception($"Security token is null.");

                if (_cachedTokenResponse.IsError)
                    throw new Exception($"Can't get the token for client credentials. Error:{_cachedTokenResponse.Error} | Error description:{_cachedTokenResponse.ErrorDescription} | Error type: {_cachedTokenResponse.ErrorType} | Http Message: {_cachedTokenResponse.HttpErrorReason}");

                _cachedTokenExpireDate = now.AddSeconds(_cachedTokenResponse.ExpiresIn);
            }

            SetAuthenticationHeaderWithBearerToken(httpClient, _cachedTokenResponse);
        }

        internal static async Task<TokenResponse> GetTokenForClientCredentials(HttpClient httpClient, WebApiSecurityDefinition webApiSecurity)
        {
            var discoveryDoc = await httpClient.GetDiscoveryDocumentAsync(webApiSecurity.AuthorityUrl);
            if (discoveryDoc.IsError)
            {
                throw new Exception($"Can't get the discovery document from authority {webApiSecurity.AuthorityUrl}. Error:{discoveryDoc.Error} | Error type: {discoveryDoc.ErrorType} | Http Message: {discoveryDoc.HttpErrorReason}");
            }

            return await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = discoveryDoc.TokenEndpoint,
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
            public static readonly string CredentialsController = "Credentials";
            public static readonly string DirectoryController = "Directory";
        }
        #endregion
    }
}
