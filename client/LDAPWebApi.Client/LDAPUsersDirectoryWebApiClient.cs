using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bitai.LDAPHelper.DTO;
using Bitai.WebApi.Client;

namespace Bitai.LDAPWebApi.Clients
{
    public class LDAPUsersDirectoryWebApiClient : LDAPWebApiBaseClient
    {
        public LDAPUsersDirectoryWebApiClient(string webApiBaseUrl, string serverProfile, bool useGlobalCatalog, WebApiClientCredentials clientCredentials) : base(webApiBaseUrl, serverProfile, useGlobalCatalog, clientCredentials)
        {
        }



        public async Task<IHttpResponse> SearchByIdentifierAsync(string identifier, EntryAttribute? identifierAttribute, RequiredEntryAttributes? requiredAttributes = null, string requestTag = null, bool setAuthorizationHeaderWithBearerToken = true, CancellationToken cancellationToken = default)
        {
            var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.DirectoryController}/Users/{identifier}?identifierAttribute={GetOptionalEntryAttributeName(identifierAttribute)}&requiredAttributes={GetOptionalRequiredEntryAttributesName(requiredAttributes)}&requestTag={requestTag}";

            using (var httpClient = await CreateHttpClient(setAuthorizationHeaderWithBearerToken))
            {
                var responseMessage = await httpClient.GetAsync(uri, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                    return await responseMessage.ToUnsuccessfulHttpResponseAsync();
                else
                    return await responseMessage.ToSuccessfulHttpResponseAsync<LDAPSearchResult>();
            }
        }

        public async Task<IHttpResponse> SearchFilteringByAsync(EntryAttribute filterAttribute, string filterValue, EntryAttribute? secondFilterAttribute = null, string secondFilterValue = null, bool? combineFilters = null, RequiredEntryAttributes? requiredAttributes = null, string requestTag = null, bool setAuthorizationHeaderWithBearerToken = true, CancellationToken cancellationToken = default)
        {
            var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.DirectoryController}/Users/filterBy?filterAttribute={filterAttribute}&filterValue={filterValue}&secondFilterAttribute={GetOptionalEntryAttributeName(secondFilterAttribute)}&secondFilterValue={secondFilterValue}&combineFilters={GetOptionalBooleanValue(combineFilters)}&requiredAttributes={GetOptionalRequiredEntryAttributesName(requiredAttributes)}&requestTag={requestTag}";

            using (var httpClient = await CreateHttpClient(setAuthorizationHeaderWithBearerToken))
            {
                var responseMessage = await httpClient.GetAsync(uri, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                    return await responseMessage.ToUnsuccessfulHttpResponseAsync();
                else
                    return await responseMessage.ToSuccessfulHttpResponseAsync<LDAPSearchResult>();
            }
        }
    }
}