using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bitai.WebApi.Client;

namespace Bitai.LDAPWebApi.Clients
{
	/// <summary>
	/// Client that creates and submits requests to LDAP Web Api Server Profiles controller.
	/// </summary>
	public class LDAPServerProfilesWebApiClient : LDAPWebApiBaseClient
	{
		public LDAPServerProfilesWebApiClient(string ldapWebApiBaseUrl) : base(ldapWebApiBaseUrl)
		{
		}

		public LDAPServerProfilesWebApiClient(string ldapWebApiBaseUrl, HttpClientHandler handler, bool disposeHandler) : base(ldapWebApiBaseUrl, handler, disposeHandler)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="clientCredentials">Client credentials to request an access token  from the Identity Server. This access token will be sent in the HTTP authorization header as Bearer Token.</param>
		public LDAPServerProfilesWebApiClient(string ldapWebApiBaseUrl, WebApiClientCredential clientCredentials) : base(ldapWebApiBaseUrl, clientCredentials)
		{
		}

		public LDAPServerProfilesWebApiClient(string ldapWebApiBaseUrl, WebApiClientCredential clientCredentials, HttpClientHandler handler, bool disposeHandler) : base(ldapWebApiBaseUrl, clientCredentials, handler, disposeHandler)
		{
		}




		/// <summary>
		/// Send a GET request to [controller]/GetProfileIds
		/// </summary>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// 
		/// <returns><see cref="IHttpResponse"/></returns>
		public async Task<IHttpResponse> GetProfileIdsAsync(bool setBearerToken = true, CancellationToken cancellationToken = default)
		{
			var uri = $"{WebApiBaseUrl}/api/{ControllerNames.ServerProfilesController}/GetProfileIds";

			using (var httpClient = await CreateHttpClient(setBearerToken))
			{
				var responseMessage = await httpClient.GetAsync(uri, cancellationToken);
				if (!responseMessage.IsSuccessStatusCode)
					return await responseMessage.ToUnsuccessfulHttpResponseAsync();
				else
					return await responseMessage.ToSuccessfulHttpResponseAsync<IEnumerable<string>>();
			}
		}

		/// <summary>
		/// Send a GET request to [controller]/<paramref name="serverProfileId"/>
		/// </summary>
		/// <param name="serverProfileId">LDAP Server profile Id.</param>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns><see cref="IHttpResponse"/></returns>
		public async Task<IHttpResponse> GetByProfileIdAsync(string serverProfileId, bool setBearerToken = true, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrEmpty(serverProfileId))
				throw new ArgumentNullException(nameof(serverProfileId));

			var uri = $"{WebApiBaseUrl}/api/{ControllerNames.ServerProfilesController}/{serverProfileId}";

			using (var httpClient = await CreateHttpClient(setBearerToken))
			{
				var responseMessage = await httpClient.GetAsync(uri, cancellationToken);
				if (!responseMessage.IsSuccessStatusCode)
					return await responseMessage.ToUnsuccessfulHttpResponseAsync();
				else
					return await responseMessage.ToSuccessfulHttpResponseAsync<DTO.LDAPServerProfile>();
			}
		}

		/// <summary>
		/// Sen a GET request to [controller]
		/// </summary>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns><see cref="IHttpResponse"/></returns>
		public async Task<IHttpResponse> GetAllAsync(bool setBearerToken = true, CancellationToken cancellationToken = default)
		{
			var uri = $"{WebApiBaseUrl}/api/{ControllerNames.ServerProfilesController}";
			using (var httpClient = await CreateHttpClient(setBearerToken))
			{
				var responseMessage = await httpClient.GetAsync(uri, cancellationToken);
				if (!responseMessage.IsSuccessStatusCode)
					return await responseMessage.ToUnsuccessfulHttpResponseAsync();
				else
					return await responseMessage.ToSuccessfulHttpResponseAsync<IEnumerable<DTO.LDAPServerProfile>>();
			}
		}
	}
}