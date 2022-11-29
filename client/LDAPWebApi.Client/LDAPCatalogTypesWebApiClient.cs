using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bitai.WebApi.Client;

namespace Bitai.LDAPWebApi.Clients
{
    /// <summary>
    /// Client that creates and submits requests to LDAP Web Api Catalog Types controller.
    /// </summary>
    public class LDAPCatalogTypesWebApiClient : LDAPWebApiBaseClient
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
        /// <param name="clientCredentials">Client credentials to request an access token  from the Identity Server. This access token will be sent in the HTTP authorization header as Bearer Token.</param>
        public LDAPCatalogTypesWebApiClient(string ldapWebApiBaseUrl, WebApiClientCredential clientCredentials) : base(ldapWebApiBaseUrl, clientCredentials)
        {
        }



        /// <summary>
        /// Send a GET request to LDAP Web Api CatalogTypes controller.
        /// </summary>
        /// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
        /// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="IHttpResponse"/></returns>
        public async Task<IHttpResponse> GetAllAsync(bool setBearerToken = true, CancellationToken cancellationToken = default)
        {
            var uri = $"{WebApiBaseUrl}/api/{ControllerNames.CatalogTypesController}";

            using (var httpClient = await CreateHttpClient(setBearerToken))
            {
                var responseMessage = await httpClient.GetAsync(uri, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                    return await responseMessage.ToUnsuccessfulHttpResponseAsync();
                else
                    return await responseMessage.ToSuccessfulHttpResponseAsync<DTO.LDAPServerCatalogTypes>();
            }
        }
    }
}