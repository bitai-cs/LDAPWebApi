using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bitai.WebApi.Client;

namespace Bitai.LDAPWebApi.Clients
{
    public class LDAPCatalogTypesClient<DTOType> : LDAPBaseClient<DTOType>
    {
        public LDAPCatalogTypesClient(string webApiBaseUrl, WebApiSecurityDefinition webApiScurity) : base(webApiBaseUrl, webApiScurity)
        {
        }



        public async Task<IHttpResponse> GetAllAsync()
        {
            var uri = $"/api/{ControllerNames.CatalogTypesController}";

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