using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bitai.WebApi.Client;

namespace Bitai.LDAPWebApi.Clients
{
    public class LDAPServerProfilesClient<DTOType> : LDAPBaseClient<DTOType>
    {
        public LDAPServerProfilesClient(string webApiBaseUrl) : base(webApiBaseUrl)
        {
        }



        public async Task<IHttpResponse> GetProfileIdsAsync()
        {
            var uri = $"/api/{ControllerNames.ServerProfilesController}/GetProfileIds";

            using (var httpClient = CreateHttpClient())
            {
                var responseMessage = await httpClient.GetAsync(uri);
                if (!responseMessage.IsSuccessStatusCode)
                    return await ParseHttpResponseToNoSuccessResponseAsync(responseMessage);
                else
                    return await ParseHttpResponseToSuccessEnumerableDTOResponseAsync(responseMessage);
            }
        }

        public async Task<IHttpResponse> GetByProfileIdAsync(string profileId)
        {
            if (string.IsNullOrEmpty(profileId))
                throw new ArgumentNullException(nameof(profileId));

            var uri = $"/api/{ControllerNames.ServerProfilesController}/{profileId}";

            using (var httpClient = CreateHttpClient())
            {
                var responseMessage = await httpClient.GetAsync(uri);
                if (!responseMessage.IsSuccessStatusCode)
                    return await ParseHttpResponseToNoSuccessResponseAsync(responseMessage);
                else
                    return await ParseHttpResponseToSuccessDTOResponseAsync(responseMessage);
            }
        }

        public async Task<IHttpResponse> GetAllAsync()
        {
            var uri = $"/api/{ControllerNames.ServerProfilesController}";
            using (var httpClient = CreateHttpClient())
            {
                var responseMessage = await httpClient.GetAsync(uri);
                if (!responseMessage.IsSuccessStatusCode)
                    return await ParseHttpResponseToNoSuccessResponseAsync(responseMessage);
                else
                    return await ParseHttpResponseToSuccessEnumerableDTOResponseAsync(responseMessage);
            }
        }
    }
}