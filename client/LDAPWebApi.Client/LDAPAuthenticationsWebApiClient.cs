using Bitai.LDAPWebApi.DTO;
using Bitai.WebApi.Client;
using System.Net.Http.Formatting;

namespace Bitai.LDAPWebApi.Clients
{
	/// <summary>
	/// Client for the LDAP Web API Authentications controller.
	/// </summary>
	public class LDAPAuthenticationsWebApiClient : LDAPWebApiBaseClient
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPAuthenticationsWebApiClient"/> class.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="ldapServerProfile">LDAP Server Profile Id.</param>
		/// <param name="useLdapServerGlobalCatalog">Whether or not the global catalog of the LDAP server will be used; otherwise the local catalog of the LDAP server will be used.</param>
		public LDAPAuthenticationsWebApiClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog) : base(ldapWebApiBaseUrl, ldapServerProfile, useLdapServerGlobalCatalog)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPAuthenticationsWebApiClient"/> class with a custom HttpClientHandler.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="ldapServerProfile">LDAP Server Profile Id.</param>
		/// <param name="useLdapServerGlobalCatalog">Whether or not the global catalog of the LDAP server will be used; otherwise the local catalog of the LDAP server will be used.</param>
		/// <param name="handler">The custom HttpClientHandler to handle HTTP requests.</param>
		/// <param name="disposeHandler">true if the inner handler should be disposed of by Dispose(), false if you intend to reuse the inner handler.</param>
		public LDAPAuthenticationsWebApiClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog, HttpClientHandler handler, bool disposeHandler) : base(ldapWebApiBaseUrl, ldapServerProfile, useLdapServerGlobalCatalog, handler, disposeHandler)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPAuthenticationsWebApiClient"/> class with Identity Server credentials.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="ldapServerProfile">LDAP Server Profile Id.</param>
		/// <param name="useLdapServerGlobalCatalog">Whether or not the global catalog of the LDAP server will be used; otherwise the local catalog of the LDAP server will be used.</param>
		/// <param name="clientCredential">Client credentials to request an access token  from the Identity Server. This access token will be sent in the HTTP authorization header as Bearer Token.</param>
		public LDAPAuthenticationsWebApiClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog, WebApiClientCredential clientCredential) : base(ldapWebApiBaseUrl, ldapServerProfile, useLdapServerGlobalCatalog, clientCredential)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPAuthenticationsWebApiClient"/> class with Identity Server credentials and a custom HttpClientHandler.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="ldapServerProfile">LDAP Server Profile Id.</param>
		/// <param name="useLdapServerGlobalCatalog">Whether or not the global catalog of the LDAP server will be used; otherwise the local catalog of the LDAP server will be used.</param>
		/// <param name="clientCredential">Client credentials to request an access token from the Identity Server. This access token will be sent in the HTTP authorization header as Bearer Token.</param>
		/// <param name="handler">The custom HttpClientHandler to handle HTTP requests.</param>
		/// <param name="disposeHandler">true if the inner handler should be disposed of by Dispose(), false if you intend to reuse the inner handler.</param>
		public LDAPAuthenticationsWebApiClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog, WebApiClientCredential clientCredential, HttpClientHandler handler, bool disposeHandler) : base(ldapWebApiBaseUrl, ldapServerProfile, useLdapServerGlobalCatalog, clientCredential, handler, disposeHandler)
		{
		}




		/// <summary>
		/// Sends a POST request to authenticate a domain account credential.
		/// </summary>
		/// <param name="ldapCredential">Account credentials or network credentials.</param>
		/// <param name="requestLabel">Custom tag to identify the request and mark the data returned in the response.</param>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns>An HTTP response containing the domain account authentication result.</returns>
		public async Task<IHttpResponse> AuthenticateAsync(LDAPHelper.DTO.LDAPDomainAccountCredential ldapCredential, string? requestLabel = null, bool setBearerToken = true, CancellationToken cancellationToken = default)
		{
			var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.AuthenticationsController}/authenticate?requestLabel={requestLabel}";

			using (var httpClient = await CreateHttpClient(setBearerToken))
			{
				using (var content = new ObjectContent<LDAPHelper.DTO.LDAPDomainAccountCredential>(ldapCredential, new JsonMediaTypeFormatter()))
				{
					var responseMessage = await httpClient.PostAsync(uri, content, cancellationToken);
					if (!responseMessage.IsSuccessStatusCode)
						return await responseMessage.ToUnsuccessfulHttpResponseAsync();
					else
						return await responseMessage.ToSuccessfulHttpResponseAsync<LWADomainAccountAuthenticationResult>();
				}
			}
		}

		/// <summary>
		/// Sends a POST request to authenticate a domain account credential without loading user details.
		/// </summary>
		/// <param name="ldapCredential">Account credentials or network credentials.</param>
		/// <param name="requestLabel">Custom tag to identify the request and mark the data returned in the response.</param>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns>An HTTP response containing the domain account authentication result.</returns>
		public async Task<IHttpResponse> AuthenticateWithoutUserLookupAsync(LDAPHelper.DTO.LDAPDomainAccountCredential ldapCredential, string? requestLabel = null, bool setBearerToken = true, CancellationToken cancellationToken = default)
		{
			var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.AuthenticationsController}/authenticateWithoutUserLookup?requestLabel={requestLabel}";

			using (var httpClient = await CreateHttpClient(setBearerToken))
			{
				using (var content = new ObjectContent<LDAPHelper.DTO.LDAPDomainAccountCredential>(ldapCredential, new JsonMediaTypeFormatter()))
				{
					var responseMessage = await httpClient.PostAsync(uri, content, cancellationToken);
					if (!responseMessage.IsSuccessStatusCode)
						return await responseMessage.ToUnsuccessfulHttpResponseAsync();
					else
						return await responseMessage.ToSuccessfulHttpResponseAsync<LWADomainAccountAuthenticationResult>();
				}
			}
		}
	}
}
