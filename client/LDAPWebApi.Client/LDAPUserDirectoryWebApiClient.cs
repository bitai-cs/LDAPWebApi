using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Formatting;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bitai.LDAPHelper.DTO;
using Bitai.LDAPWebApi.DTO;
using Bitai.WebApi.Client;
using Microsoft.VisualBasic;

namespace Bitai.LDAPWebApi.Clients
{
	/// <summary>
	/// Client that makes requests to the Web Api Directory controller.
	/// </summary>
	public class LDAPUserDirectoryWebApiClient : LDAPWebApiBaseClient
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="webApiBaseUrl">Api address.</param>
		/// <param name="serverProfile">Server profile Id.</param>
		/// <param name="useGlobalCatalog">Whether or not to use the global catalog.</param>
		/// <param name="clientCredential">Client credential to connect Api.</param>
		public LDAPUserDirectoryWebApiClient(string webApiBaseUrl, string serverProfile, bool useGlobalCatalog, WebApiClientCredential? clientCredential = null) : base(webApiBaseUrl, serverProfile, useGlobalCatalog, clientCredential)
		{
		}




		public Task<IHttpResponse> SearchFilteringByAsync(string samAccountName, bool setBearerToken = true, string requestLabel = null)
		{
			return SearchFilteringByAsync(null, samAccountName, null, null, null, null, requestLabel, setBearerToken, default);
		}

		public Task<IHttpResponse> SearchFilteringByAsync(string samAccountName, RequiredEntryAttributes requiredAttributes, bool setBearerToken = true, string requestLabel = null)
		{
			return SearchFilteringByAsync(null, samAccountName, null, null, null, requiredAttributes, requestLabel, setBearerToken, default);
		}

		public Task<IHttpResponse> SearchFilteringByAsync(EntryAttribute filterAttribute, string filterValue, bool setBearerToken = true, string requestLabel = null)
		{
			return SearchFilteringByAsync(filterAttribute, filterValue, null, null, null, null, requestLabel, setBearerToken, default);
		}

		public Task<IHttpResponse> SearchFilteringByAsync(EntryAttribute filterAttribute, string filterValue, RequiredEntryAttributes requiredAttributes, bool setBearerToken = true, string requestLabel = null)
		{
			return SearchFilteringByAsync(filterAttribute, filterValue, null, null, null, requiredAttributes, requestLabel, setBearerToken, default);
		}

		/// <summary>
		/// Search for one or more user accounts using search filters.
		/// </summary>
		/// <param name="filterAttribute">LDAP attribute type that should contain (fully or partially) the value of the <paramref name="filterValue"/> parameter.</param>
		/// <param name="filterValue">Value that will be used to filter the LDAP attribute assigned in the <paramref name="filterAttribute"/> parameter.</param>
		/// <param name="secondFilterAttribute">Optional LDAP attribute type that should contain (fully or partially) the value of the <paramref name="secondFilterValue"/> parameter.</param>
		/// <param name="secondFilterValue">Optional value that will be used to filter the LDAP attribute assigned in the <paramref name="secondFilterAttribute"/> parameter.</param>
		/// <param name="combineFilters">Whether the first filter is combined inclusively or exclusively with the second filter.</param>
		/// <param name="requiredAttributes">Set of LDAP attributes that the search result should contain.</param>
		/// <param name="requestLabel">Custom tag to identify the request and mark the data returned in the response.</param>
		/// <param name="setBearerToken">Whether or not to add the security "Bearer Token" to the HTTP request header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns>An <see cref="IHttpResponse{TContent}"/> that encapsulates an <see cref="IHttpResponse"/>.</returns>
		public async Task<IHttpResponse> SearchFilteringByAsync(EntryAttribute? filterAttribute, string filterValue, EntryAttribute? secondFilterAttribute = null, string? secondFilterValue = null, bool? combineFilters = null, RequiredEntryAttributes? requiredAttributes = null, string? requestLabel = null, bool setBearerToken = true, CancellationToken cancellationToken = default)
		{
			var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.DirectoryController}/Users/filterBy?filterAttribute={filterAttribute}&filterValue={filterValue}&secondFilterAttribute={GetOptionalEntryAttributeName(secondFilterAttribute)}&secondFilterValue={secondFilterValue}&combineFilters={GetOptionalBooleanValue(combineFilters)}&requiredAttributes={GetOptionalRequiredEntryAttributesName(requiredAttributes)}&requestLabel={requestLabel}";

			using (var httpClient = await CreateHttpClient(setBearerToken))
			{
				var responseMessage = await httpClient.GetAsync(uri, cancellationToken);
				if (!responseMessage.IsSuccessStatusCode)
					return await responseMessage.ToUnsuccessfulHttpResponseAsync();
				else
					return await responseMessage.ToSuccessfulHttpResponseAsync<LDAPSearchResult>();
			}
		}

		public Task<IHttpResponse> SetPasswordAsync(LDAPCredential credential, bool setBearerToken = true, string requestLabel = null, CancellationToken cancellationToken = default)
		{
			return SetPasswordAsync(credential, null, setBearerToken, requestLabel, cancellationToken);
		}

		public async Task<IHttpResponse> SetPasswordAsync(LDAPCredential credential, EntryAttribute? identifierAttribute = null, bool setBearerToken = true, string requestLabel = null, CancellationToken cancellationToken = default)
		{
			var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.DirectoryController}/Users/{credential.UserAccount}/Credential?identifierAttribute={GetOptionalEntryAttributeName(identifierAttribute)}&requestLabel={requestLabel}";

			using (var httpClient = await CreateHttpClient(setBearerToken))
			{
				var content = new ObjectContent<LDAPCredential>(credential, new JsonMediaTypeFormatter());
				var responseMessage = await httpClient.PostAsync(uri, content, cancellationToken);
				if (!responseMessage.IsSuccessStatusCode)
					return await responseMessage.ToUnsuccessfulHttpResponseAsync();
				else
					return await responseMessage.ToSuccessfulHttpResponseAsync<LDAPPasswordUpdateResult>();
			}
		}
	}
}