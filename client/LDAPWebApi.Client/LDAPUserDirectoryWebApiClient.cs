using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bitai.LDAPHelper.DTO;
using Bitai.WebApi.Client;

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
		public async Task<IHttpResponse> SearchFilteringByAsync(EntryAttribute filterAttribute, string filterValue, EntryAttribute? secondFilterAttribute = null, string? secondFilterValue = null, bool? combineFilters = null, RequiredEntryAttributes? requiredAttributes = null, string? requestLabel = null, bool setBearerToken = true, CancellationToken cancellationToken = default)
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
	}
}