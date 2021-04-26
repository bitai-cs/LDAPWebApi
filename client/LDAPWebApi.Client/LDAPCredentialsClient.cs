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
    public class LDAPCredentialsClient<DTOType> : LDAPBaseClient<DTOType>
    {
        public LDAPCredentialsClient(string webApiBaseUrl, string serverProfile, bool useGlobalCatalog, WebApiSecurityDefinition webApiScurity) : base(webApiBaseUrl, serverProfile, useGlobalCatalog, webApiScurity)
        {
        }



        public async Task<IHttpResponse> AccountAuthenticationAsync(string accountName, DTO.LDAPAccountCredential accountCredential, CancellationToken cancellationToken = default)
        {
            var uri = $"/api/{ServerProfile}/{CatalogTypes.GetCatalogTypeName(UseGlobalCatalog)}/{ControllerNames.CredentialsController}/{accountName}/Authentication";

            using (var httpClient = await CreateHttpClient(true))
            {
                var content = new ObjectContent<DTO.LDAPAccountCredential>(accountCredential, new JsonMediaTypeFormatter());

                var responseMessage = await httpClient.PostAsync(uri, content, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                    return await ParseHttpResponseToNoSuccessResponseAsync(responseMessage);
                else
                    return await ParseHttpResponseToSuccessDTOResponseAsync(responseMessage);
            }
        }
    }
}