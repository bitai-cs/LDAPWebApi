using Bitai.LDAPHelper.DTO;
using Bitai.LDAPWebApi.DTO;
using Bitai.WebApi.Client;
using System.Net.Http.Formatting;

namespace Bitai.LDAPWebApi.Clients
{
	/// <summary>
	/// Client that makes requests to the Web Api Directory controller.
	/// </summary>
	public class LDAPDirectoryWebApiClient : LDAPWebApiBaseClient
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="ldapServerProfile">LDAP Server Profile Id.</param>
		/// <param name="useLdapServerGlobalCatalog">Whether or not the global catalog of the LDAP server will be used; otherwise the local catalog of the LDAP server will be used.</param>
		/// <param name="clientCredentials">Client credentials to request an access token  from the Identity Server. This access token will be sent in the HTTP authorization header as Bearer Token.</param>
		public LDAPDirectoryWebApiClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog, WebApiClientCredential? clientCredentials = null) : base(ldapWebApiBaseUrl, ldapServerProfile, useLdapServerGlobalCatalog, clientCredentials)
		{
		}




		#region GET /API/{SERVERPROFILE}/{CATALOGTYPE}/DIRECTORY/{identifier}
		/// <summary>
		/// Send a GET request to the route {serverProfile}/{catalogType}/[controller]/{identifier} of LDAP Web Api Directory controller.
		/// </summary>
		/// <param name="identifier">User account identifier.</param>
		/// <param name="requestLabel">Custom tag to identify the request and mark the data returned in the response.</param>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <returns>An <see cref="IHttpResponse{TContent}"/> that encapsulates an <see cref="IHttpResponse"/>.</returns>
		public Task<IHttpResponse> SearchByIdentifierAsync(string identifier, string? requestLabel = null, bool setBearerToken = true)
		{
			return SearchByIdentifierAsync(identifier, null, null, requestLabel, setBearerToken, default);
		}

		/// <summary>
		/// Send a GET request to the route {serverProfile}/{catalogType}/[controller]/{identifier} of LDAP Web Api Directory controller.
		/// </summary>
		/// <param name="identifier">User account identifier.</param>
		/// <param name="requiredAttributes">Set of LDAP attributes that the search result should contain. See <see cref="RequiredEntryAttributes"/>.</param>
		/// <param name="requestLabel">Custom tag to identify the request and mark the data returned in the response.</param>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <returns>An <see cref="IHttpResponse{TContent}"/> that encapsulates an <see cref="IHttpResponse"/>.</returns>
		public Task<IHttpResponse> SearchByIdentifierAsync(string identifier, RequiredEntryAttributes? requiredAttributes = null, string? requestLabel = null, bool setBearerToken = true)
		{
			return SearchByIdentifierAsync(identifier, null, requiredAttributes, requestLabel, setBearerToken, default);
		}

		/// <summary>
		/// Send a GET request to the route {serverProfile}/{catalogType}/[controller]/{identifier} of LDAP Web Api Directory controller.
		/// </summary>
		/// <param name="identifier">User account identifier.</param>
		/// <param name="identifierAttribute">Type of attribute that serves as an identifier for the user account. See <see cref="EntryAttribute"/>.</param>
		/// <param name="requiredAttributes">Set of LDAP attributes that the search result should contain. See <see cref="RequiredEntryAttributes"/>.</param>
		/// <param name="requestLabel">Custom tag to identify the request and mark the data returned in the response.</param>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns>An <see cref="IHttpResponse{TContent}"/> that encapsulates an <see cref="IHttpResponse"/>.</returns>
		public async Task<IHttpResponse> SearchByIdentifierAsync(string identifier, EntryAttribute? identifierAttribute, RequiredEntryAttributes? requiredAttributes = null, string? requestLabel = null, bool setBearerToken = true, CancellationToken cancellationToken = default)
		{
			var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.DirectoryController}/{identifier}?identifierAttribute={GetOptionalEntryAttributeName(identifierAttribute)}&requiredAttributes={GetOptionalRequiredEntryAttributesName(requiredAttributes)}&requestLabel={requestLabel}";

			using (var httpClient = await CreateHttpClient(setBearerToken))
			{
				var responseMessage = await httpClient.GetAsync(uri, cancellationToken);
				if (!responseMessage.IsSuccessStatusCode)
					return await responseMessage.ToUnsuccessfulHttpResponseAsync();
				else
					return await responseMessage.ToSuccessfulHttpResponseAsync<LDAPSearchResult>();
			}
		}
		#endregion //GET /API/{SERVERPROFILE}/{CATALOGTYPE}/DIRECTORY/{identifier}


		#region GET /API/{SERVERPROFILE}/{CATALOGTYPE}/DIRECTORY/filterBy
		/// <summary>
		/// Send a GET request to the route {serverProfile}/{catalogType}/[controller]/[action] of LDAP Web Api Directory controller.
		/// </summary>
		/// <param name="filterAttribute"><see cref="EntryAttribute"/> that will condition the search according to the <paramref name="filterValue"/>.</param>
		/// <param name="filterValue">The value that must be compared with the <paramref name="filterAttribute"/> in order to satisfy the search condition.</param>
		/// <param name="requestLabel">Custom tag to identify the request and mark the data returned in the response.</param>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <returns>An <see cref="IHttpResponse{TContent}"/> that encapsulates an <see cref="IHttpResponse"/>.</returns>
		public Task<IHttpResponse> SearchByFiltersAsync(EntryAttribute filterAttribute, string filterValue, string? requestLabel = null, bool setBearerToken = true)
		{
			return SearchByFiltersAsync(filterAttribute, filterValue, null, null, null, null, requestLabel, setBearerToken, default);
		}

		/// <summary>
		/// Send a GET request to the route {serverProfile}/{catalogType}/[controller]/[action] of LDAP Web Api Directory controller.
		/// </summary>
		/// <param name="filterAttribute"><see cref="EntryAttribute"/> that will condition the search according to the <paramref name="filterValue"/>.</param>
		/// <param name="filterValue">The value that must be compared with the <paramref name="filterAttribute"/> in order to satisfy the search condition.</param>
		/// <param name="requiredAttributes">The attributes that we want to obtain for each LDAP entry that was obtained as result of the search.</param>
		/// <param name="requestLabel">Custom tag to identify the request and mark the data returned in the response.</param>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns>An <see cref="IHttpResponse{TContent}"/> that encapsulates an <see cref="IHttpResponse"/>.</returns>
		public Task<IHttpResponse> SearchByFiltersAsync(EntryAttribute filterAttribute, string filterValue, RequiredEntryAttributes? requiredAttributes = null, string? requestLabel = null, bool setBearerToken = true, CancellationToken cancellationToken = default)
		{
			return SearchByFiltersAsync(filterAttribute, filterValue, null, null, null, requiredAttributes, requestLabel, setBearerToken, default);
		}

		/// <summary>
		/// Send a GET request to the route {serverProfile}/{catalogType}/[controller]/[action] of LDAP Web Api Directory controller.
		/// </summary>
		/// <param name="filterAttribute"><see cref="EntryAttribute"/> that will condition the search according to the <paramref name="filterValue"/>.</param>
		/// <param name="filterValue">The value that must be compared with the <paramref name="filterAttribute"/> in order to satisfy the search condition.</param>
		/// <param name="secondFilterAttribute">Second <see cref="EntryAttribute"/> that will condition the search according to the <paramref name="secondFilterValue"/>.</param>
		/// <param name="secondFilterValue">The value that must be compared with the <paramref name="secondFilterAttribute"/> in order to satisfy the search condition.</param>
		/// <param name="combineFilters">Whether it is combined in a conjunction or not the search filters.</param>
		/// <param name="requestLabel">Custom tag to identify the request and mark the data returned in the response.</param>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns>An <see cref="IHttpResponse{TContent}"/> that encapsulates an <see cref="IHttpResponse"/>.</returns>
		public Task<IHttpResponse> SearchByFiltersAsync(EntryAttribute filterAttribute, string filterValue, EntryAttribute? secondFilterAttribute = null, string? secondFilterValue = null, bool? combineFilters = null, string? requestLabel = null, bool setBearerToken = true, CancellationToken cancellationToken = default)
		{
			return SearchByFiltersAsync(filterAttribute, filterValue, secondFilterAttribute, secondFilterValue, combineFilters, null, requestLabel, setBearerToken, default);
		}

		/// <summary>
		/// Send a GET request to the route {serverProfile}/{catalogType}/[controller]/[action] of LDAP Web Api Directory controller.
		/// </summary>
		/// <param name="filterAttribute"><see cref="EntryAttribute"/> that will condition the search according to the <paramref name="filterValue"/>.</param>
		/// <param name="filterValue">The value that must be compared with the <paramref name="filterAttribute"/> in order to satisfy the search condition.</param>
		/// <param name="secondFilterAttribute">Second <see cref="EntryAttribute"/> that will condition the search according to the <paramref name="secondFilterValue"/>.</param>
		/// <param name="secondFilterValue">The value that must be compared with the <paramref name="secondFilterAttribute"/> in order to satisfy the search condition.</param>
		/// <param name="combineFilters">Whether it is combined in a conjunction or not the search filters.</param>
		/// <param name="requiredAttributes">The attributes that we want to obtain for each LDAP entry that was obtained as result of the search.</param>
		/// <param name="requestLabel">Custom tag to identify the request and mark the data returned in the response.</param>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns>An <see cref="IHttpResponse{TContent}"/> that encapsulates an <see cref="IHttpResponse"/>.</returns>
		public async Task<IHttpResponse> SearchByFiltersAsync(EntryAttribute filterAttribute, string filterValue, EntryAttribute? secondFilterAttribute = null, string? secondFilterValue = null, bool? combineFilters = null, RequiredEntryAttributes? requiredAttributes = null, string? requestLabel = null, bool setBearerToken = true, CancellationToken cancellationToken = default)
		{
			var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.DirectoryController}/filterBy?filterAttribute={filterAttribute}&filterValue={filterValue}&secondFilterAttribute={GetOptionalEntryAttributeName(secondFilterAttribute)}&secondFilterValue={secondFilterValue}&combineFilters={GetOptionalBooleanValue(combineFilters)}&requiredAttributes={GetOptionalRequiredEntryAttributesName(requiredAttributes)}&requestLabel={requestLabel}";

			using (var httpClient = await CreateHttpClient(setBearerToken))
			{
				var responseMessage = await httpClient.GetAsync(uri, cancellationToken);
				if (!responseMessage.IsSuccessStatusCode)
					return await responseMessage.ToUnsuccessfulHttpResponseAsync();
				else
					return await responseMessage.ToSuccessfulHttpResponseAsync<LDAPSearchResult>();
			}
		}
		#endregion GET /API/{SERVERPROFILE}/{CATALOGTYPE}/DIRECTORY/filterBy


		#region POST /API/{SERVERPROFILE}/{CATALOGTYPE}/DIRECTORY/MsADUsers
		/// <summary>
		/// Create a Ms AS user account
		/// </summary>
		/// <param name="newUSerAccount">Data of the new user account. See <see cref="LDAPMsADUserAccount"/>.</param>
		/// <param name="requestLabel">Custom tag to identify the request and mark the data returned in the response.</param>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns>An <see cref="IHttpResponse{TContent}"/> that encapsulates an <see cref="IHttpResponse"/>.</returns>
		public async Task<IHttpResponse> CreateMsADUserAccountAsync(LDAPMsADUserAccount newUSerAccount, string? requestLabel = null, bool setBearerToken = true, CancellationToken cancellationToken = default)
		{
			var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.DirectoryController}/MsADUsers?requestLabel={requestLabel}";

			using (var httpClient = await CreateHttpClient(setBearerToken))
			{
				using (var content = new ObjectContent<LDAPMsADUserAccount>(newUSerAccount, new JsonMediaTypeFormatter()))
				{
					var responseMessage = await httpClient.PostAsync(uri, content, cancellationToken);
					if (!responseMessage.IsSuccessStatusCode)
						return await responseMessage.ToUnsuccessfulHttpResponseAsync();
					else
						return await responseMessage.ToSuccessfulHttpResponseAsync<LDAPCreateMsADUserAccountResult>();
				}
			}
		}
		#endregion


		#region POST /API/{SERVERPROFILE}/{CATALOGTYPE}/DIRECTORY/USERS/{IDENTIFIER}/CREDENTIAL
		/// <summary>
		/// Set password to a Ms AD user account
		/// </summary>
		/// <param name="identifier">User account identifier. Can be the value of a SAMAccountName or DistinguishedName attribute</param>
		/// <param name="credential">The <see cref="LDAPCredential"/> that contains the password to be assigned to the user account. It must be coherent with the value of the <paramref name="identifier"/> (route) parameter.</param>
		/// <param name="identifierAttribute">The LDAP <see cref="EntryAttribute"/> type to which the <paramref name="identifier"/> (route) parameter relates.</param>
		/// <param name="requestLabel">Custom tag to identify the request and mark the data returned in the response.</param>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns>An <see cref="IHttpResponse{TContent}"/> that encapsulates an <see cref="IHttpResponse"/>.</returns>
		public async Task<IHttpResponse> SetMsADUserAccountPassword(string identifier, LDAPCredential credential, EntryAttribute? identifierAttribute = EntryAttribute.sAMAccountName, string? requestLabel = null, bool setBearerToken = true, CancellationToken cancellationToken = default)
		{
			var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.DirectoryController}/Users/{identifier}/Credential?identifierAttribute={GetOptionalEntryAttributeName(identifierAttribute)}&requestLabel={requestLabel}";

			using (var httpClient = await CreateHttpClient(setBearerToken))
			{
				using (var content = new ObjectContent<LDAPCredential>(credential, new JsonMediaTypeFormatter()))
				{
					var responseMessage = await httpClient.PostAsync(uri, content, cancellationToken);
					if (!responseMessage.IsSuccessStatusCode)
						return await responseMessage.ToUnsuccessfulHttpResponseAsync();
					else
						return await responseMessage.ToSuccessfulHttpResponseAsync<LDAPPasswordUpdateResult>();
				}
			}
		}
		#endregion
	}
}