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
    public class LDAPAuthenticationsClient<DTOType> : LDAPBaseClient<DTOType>
    {
        public LDAPAuthenticationsClient(string webApiBaseUrl, string serverProfile, bool useGlobalCatalog, WebApiSecurityDefinition webApiScurityDefinition) : base(webApiBaseUrl, serverProfile, useGlobalCatalog, webApiScurityDefinition)
        {
        }



        public async Task<IHttpResponse> AccountAuthenticationAsync(DTO.LDAPAccountCredentials accountCredential, CancellationToken cancellationToken = default)
        {
            var uri = $"{WebApiBaseUrl}/api/{ServerProfile}/{CatalogTypes.GetCatalogTypeName(UseGlobalCatalog)}/{ControllerNames.AuthenticationsController}";

            using (var httpClient = await CreateHttpClient(true))
            {
                var content = new ObjectContent<DTO.LDAPAccountCredentials>(accountCredential, new JsonMediaTypeFormatter());

                var responseMessage = await httpClient.PostAsync(uri, content, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                    return await ParseHttpResponseToNoSuccessResponseAsync(responseMessage);
                else
                    return await ParseHttpResponseToSuccessDTOResponseAsync(responseMessage);
            }
        }
    }
}