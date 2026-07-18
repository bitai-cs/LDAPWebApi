using Bitai.WebApi.Client;

namespace Bitai.LDAPWebApi.Clients
{
	/// <summary>
	/// Client for the LDAP Web API ServerProfiles controller.
	/// </summary>
	public class LDAPServerProfilesWebApiClient : LDAPWebApiBaseClient
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPServerProfilesWebApiClient"/> class.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		public LDAPServerProfilesWebApiClient(string ldapWebApiBaseUrl) : base(ldapWebApiBaseUrl)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPServerProfilesWebApiClient"/> class with a custom HttpClientHandler.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="handler">The custom HttpClientHandler to handle HTTP requests.</param>
		/// <param name="disposeHandler">true if the inner handler should be disposed of by Dispose(), false if you intend to reuse the inner handler.</param>
		public LDAPServerProfilesWebApiClient(string ldapWebApiBaseUrl, HttpClientHandler handler, bool disposeHandler) : base(ldapWebApiBaseUrl, handler, disposeHandler)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPServerProfilesWebApiClient"/> class with Identity Server credentials.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="clientCredentials">Client credentials to request an access token  from the Identity Server. This access token will be sent in the HTTP authorization header as Bearer Token.</param>
		public LDAPServerProfilesWebApiClient(string ldapWebApiBaseUrl, WebApiClientCredential clientCredentials) : base(ldapWebApiBaseUrl, clientCredentials)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPServerProfilesWebApiClient"/> class with Identity Server credentials and a custom HttpClientHandler.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="clientCredentials">Client credentials to request an access token from the Identity Server. This access token will be sent in the HTTP authorization header as Bearer Token.</param>
		/// <param name="handler">The custom HttpClientHandler to handle HTTP requests.</param>
		/// <param name="disposeHandler">true if the inner handler should be disposed of by Dispose(), false if you intend to reuse the inner handler.</param>
		public LDAPServerProfilesWebApiClient(string ldapWebApiBaseUrl, WebApiClientCredential clientCredentials, HttpClientHandler handler, bool disposeHandler) : base(ldapWebApiBaseUrl, clientCredentials, handler, disposeHandler)
		{
		}




		/// <summary>
		/// Sends a GET request to retrieve all configured LDAP server profile identifiers.
		/// </summary>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns>An HTTP response containing the list of profile identifiers.</returns>
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
		/// Sends a GET request to retrieve one LDAP server profile by profile identifier.
		/// </summary>
		/// <param name="serverProfileId">LDAP Server profile Id.</param>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns>An HTTP response containing the requested LDAP server profile.</returns>
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
		/// Sends a GET request to retrieve all configured LDAP server profiles.
		/// </summary>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns>An HTTP response containing all LDAP server profiles.</returns>
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
