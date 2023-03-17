using Bitai.WebApi.Client;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Clients
{
	/// <summary>
	/// Client that makes requests to the authentication controller 
	/// of the LDAP Web Api.
	/// </summary>
	public class LDAPAuthenticationsWebApiClient : LDAPWebApiBaseClient
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="ldapServerProfile">LDAP Server Profile Id.</param>
		/// <param name="useLdapServerGlobalCatalog">Whether or not the global catalog of the LDAP server will be used; otherwise the local catalog of the LDAP server will be used.</param>
		/// <param name="clientCredential">Client credentials to request an access token  from the Identity Server. This access token will be sent in the HTTP authorization header as Bearer Token.</param>
		public LDAPAuthenticationsWebApiClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog, WebApiClientCredential? clientCredential = null) : base(ldapWebApiBaseUrl, ldapServerProfile, useLdapServerGlobalCatalog, clientCredential)
		{
		}



		/// <summary>
		/// Send a post request to LDAP Web Api Authentications controller.
		/// </summary>
		/// <param name="ldapCredential">Account credentials or Network credentials</param>
		/// <param name="requestLabel">Custom tag to identify the request and mark the data returned in the response.</param>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns><see cref="IHttpResponse"/></returns>
		public async Task<IHttpResponse> AuthenticateAsync(Bitai.LDAPHelper.DTO.LDAPDomainAccountCredential ldapCredential, string? requestLabel = null, bool setBearerToken = true, CancellationToken cancellationToken = default)
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
						return await responseMessage.ToSuccessfulHttpResponseAsync<LDAPHelper.DTO.LDAPDomainAccountAuthenticationResult>();
				}
			}
		}
	}
}