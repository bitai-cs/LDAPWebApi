using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bitai.LDAPHelper.DTO;
using Bitai.WebApi.Client;

namespace Bitai.LDAPWebApi.Clients
{
    public class LDAPDirectoryClient<DTOType> : LDAPBaseClient<DTOType>
    {
        public LDAPDirectoryClient(string webApiBaseUrl, string serverProfile, bool useGlobalCatalog, WebApiSecurityDefinition webApiScurityDefinition) : base(webApiBaseUrl, serverProfile, useGlobalCatalog, webApiScurityDefinition)
        {
        }



        public async Task<IHttpResponse> SearchByIdentifierAsync(string identifier, EntryAttribute? identifierAttribute, RequiredEntryAttributes? requiredAttributes = null, string requestTag = null)
        {
            var uri = $"/api/{ServerProfile}/{CatalogTypes.GetCatalogTypeName(UseGlobalCatalog)}/{ControllerNames.DirectoryController}/{identifier}?identifierAttribute={GetOptionalEntryAttributeName(identifierAttribute)}&requiredAttributes={GetOptionalRequiredEntryAttributesName(requiredAttributes)}&requestTag={requestTag}";

            using (var httpClient = await CreateHttpClient(true))
            {
                var responseMessage = await httpClient.GetAsync(uri);
                if (!responseMessage.IsSuccessStatusCode)
                    return await ParseHttpResponseToNoSuccessResponseAsync(responseMessage);
                else
                    return await ParseHttpResponseToSuccessDTOResponseAsync(responseMessage);
            }
        }

        public async Task<IHttpResponse> SearchFilteringByAsync(EntryAttribute filterAttribute, string filterValue, EntryAttribute? secondFilterAttribute = null, string secondFilterValue = null, bool? combineFilters = null, RequiredEntryAttributes? requiredAttributes = null, string requestTag = null)
        {
            var uri = $"/api/{ServerProfile}/{CatalogTypes.GetCatalogTypeName(UseGlobalCatalog)}/{ControllerNames.DirectoryController}/filterBy?filterAttribute={filterAttribute}&filterValue={filterValue}&secondFilterAttribute={GetOptionalEntryAttributeName(secondFilterAttribute)}&secondFilterValue={secondFilterValue}&combineFilters={GetOptionalBooleanValue(combineFilters)}&requiredAttributes={GetOptionalRequiredEntryAttributesName(requiredAttributes)}&requestTag={requestTag}";

            using (var httpClient = await CreateHttpClient(true))
            {
                var responseMessage = await httpClient.GetAsync(uri);
                if (!responseMessage.IsSuccessStatusCode)
                    return await ParseHttpResponseToNoSuccessResponseAsync(responseMessage);
                else
                    return await ParseHttpResponseToSuccessDTOResponseAsync(responseMessage);
            }
        }
    }
}