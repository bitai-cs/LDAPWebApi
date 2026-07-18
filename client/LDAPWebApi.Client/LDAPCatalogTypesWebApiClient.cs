using Bitai.WebApi.Client;

namespace Bitai.LDAPWebApi.Clients
{
	/// <summary>
	/// Client for the LDAP Web API CatalogTypes controller.
	/// </summary>
	public class LDAPCatalogTypesWebApiClient : LDAPWebApiBaseClient
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPCatalogTypesWebApiClient"/> class.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		public LDAPCatalogTypesWebApiClient(string ldapWebApiBaseUrl) : base(ldapWebApiBaseUrl)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPCatalogTypesWebApiClient"/> class with a custom HttpClientHandler.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="handler">The custom HttpClientHandler to handle HTTP requests.</param>
		/// <param name="disposeHandler">true if the inner handler should be disposed of by Dispose(), false if you intend to reuse the inner handler.</param>
		public LDAPCatalogTypesWebApiClient(string ldapWebApiBaseUrl, HttpClientHandler handler, bool disposeHandler) : base(ldapWebApiBaseUrl, handler, disposeHandler)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPCatalogTypesWebApiClient"/> class with Identity Server credentials.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="clientCredentials">Client credentials to request an access token  from the Identity Server. This access token will be sent in the HTTP authorization header as Bearer Token.</param>
		public LDAPCatalogTypesWebApiClient(string ldapWebApiBaseUrl, WebApiClientCredential clientCredentials) : base(ldapWebApiBaseUrl, clientCredentials)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPCatalogTypesWebApiClient"/> class with Identity Server credentials and a custom HttpClientHandler.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="clientCredentials">Client credentials to request an access token from the Identity Server. This access token will be sent in the HTTP authorization header as Bearer Token.</param>
		/// <param name="handler">The custom HttpClientHandler to handle HTTP requests.</param>
		/// <param name="disposeHandler">true if the inner handler should be disposed of by Dispose(), false if you intend to reuse the inner handler.</param>
		public LDAPCatalogTypesWebApiClient(string ldapWebApiBaseUrl, WebApiClientCredential clientCredentials, HttpClientHandler handler, bool disposeHandler) : base(ldapWebApiBaseUrl, clientCredentials, handler, disposeHandler)
		{
		}




		/// <summary>
		/// Sends a GET request to retrieve available LDAP catalog types.
		/// </summary>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns>An HTTP response containing the configured LDAP catalog types.</returns>
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
