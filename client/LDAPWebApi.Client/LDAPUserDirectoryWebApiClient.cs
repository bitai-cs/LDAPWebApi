using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Formatting;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bitai.LDAPHelper.DTO;
using Bitai.LDAPWebApi.DTO;
using Bitai.WebApi.Client;
using IdentityModel.Client;
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




		#region GET /api/{serverProfile}/{catalogType}/Directory/Users/filterBy
		public Task<IHttpResponse> SearchFilteringByAsync(string samAccountName, bool setBearerToken = true, string? requestLabel = default)
		{
			return SearchFilteringByAsync(null, samAccountName, null, null, null, null, requestLabel, setBearerToken, default);
		}

		public Task<IHttpResponse> SearchFilteringByAsync(string samAccountName, RequiredEntryAttributes requiredAttributes, bool setBearerToken = true, string? requestLabel = default)
		{
			return SearchFilteringByAsync(null, samAccountName, null, null, null, requiredAttributes, requestLabel, setBearerToken, default);
		}

		public Task<IHttpResponse> SearchFilteringByAsync(EntryAttribute filterAttribute, string filterValue, bool setBearerToken = true, string? requestLabel = default)
		{
			return SearchFilteringByAsync(filterAttribute, filterValue, null, null, null, null, requestLabel, setBearerToken, default);
		}

		public Task<IHttpResponse> SearchFilteringByAsync(EntryAttribute filterAttribute, string filterValue, RequiredEntryAttributes requiredAttributes, bool setBearerToken = true, string? requestLabel = default)
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
		#endregion


		#region POST /api/{serverProfile}/{catalogType}/Directory/MsADUsers
		/// <summary>
		/// Create a Ms AS user account
		/// </summary>
		/// <param name="newUSerAccount">Data of the new user account. See <see cref="LDAPMsADUserAccount"/>.</param>
		/// <param name="requestLabel">Custom tag to identify the request and mark the data returned in the response.</param>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns>An <see cref="IHttpResponse{TContent}"/> that encapsulates an <see cref="IHttpResponse"/>.</returns>
		public async Task<IHttpResponse> CreateMsADUserAccountAsync(LDAPMsADUserAccount newUSerAccount, string? requestLabel = default, bool setBearerToken = true, CancellationToken cancellationToken = default)
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


		#region PATCH /api/{serverProfile}/{catalogType}/Directory/MsADUsers/{identifier}/Credential
		/// <summary>
		/// Set password to a Ms AD user account
		/// </summary>
		/// <param name="credential">The <see cref="LDAPCredential"/> that contains the password to be assigned to the user account.</param>
		/// <param name="identifierAttribute">The LDAP <see cref="EntryAttribute"/> type to which the <paramref name="credential"/> (route) parameter relates.</param>
		/// <param name="requestLabel">Custom tag to identify the request and mark the data returned in the response.</param>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns>An <see cref="IHttpResponse{TContent}"/> that encapsulates an <see cref="IHttpResponse"/>.</returns>
		public async Task<IHttpResponse> SetMsADUserAccountPasswordAsync(LDAPCredential credential, EntryAttribute? identifierAttribute = EntryAttribute.sAMAccountName, string? requestLabel = null, bool setBearerToken = true, CancellationToken cancellationToken = default)
		{
			string identifier = credential.UserAccount;
			if (identifierAttribute == EntryAttribute.sAMAccountName && credential.UserAccount.Contains('\\'))
			{
				var arr = credential.UserAccount.Split('\\');

				identifier = arr[arr.Length - 1];
			}

			var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.DirectoryController}/MsADUsers/{identifier}/Credential?identifierAttribute={GetOptionalEntryAttributeName(identifierAttribute)}&requestLabel={requestLabel}";

			using (var httpClient = await CreateHttpClient(setBearerToken))
			{
				using (var content = new ObjectContent<LDAPCredential>(credential, new JsonMediaTypeFormatter()))
				{
					var responseMessage = await httpClient.PatchAsync(uri, content, cancellationToken);
					if (!responseMessage.IsSuccessStatusCode)
						return await responseMessage.ToUnsuccessfulHttpResponseAsync();
					else
						return await responseMessage.ToSuccessfulHttpResponseAsync<LDAPPasswordUpdateResult>();
				}
			}
		}
		#endregion


		#region PATCH /api/{serverProfile}/{catalogType}/Directory/MsADUsers/{identifier}/disableBy
		/// <summary>
		/// Disable Ms AD user account.
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="identifierAttribute"></param>
		/// <param name="requestLabel"></param>
		/// <param name="setBearerToken"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<IHttpResponse> DisableMsADUserAccountAsync(string identifier, EntryAttribute? identifierAttribute = EntryAttribute.sAMAccountName, string? requestLabel = null, bool setBearerToken = true, CancellationToken cancellationToken = default)
		{
			var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.DirectoryController}/MsADUsers/{identifier}/disableBy?identifierAttribute={GetOptionalEntryAttributeName(identifierAttribute)}&requestLabel={requestLabel}";

			using (var httpClient = await CreateHttpClient(setBearerToken))
			{
				var responseMessage = await httpClient.PatchAsync(uri, null, cancellationToken);
				if (!responseMessage.IsSuccessStatusCode)
					return await responseMessage.ToUnsuccessfulHttpResponseAsync();
				else
					return await responseMessage.ToSuccessfulHttpResponseAsync<LDAPDisableUserAccountOperationResult>();
			}
		}
		#endregion PATCH /api/{serverProfile}/{catalogType}/Directory/MsADUsers/{identifier}/disableBy


		#region DELETE /api/{serverProfile}/{catalogType}/Directory/MsADUsers/{identifier}
		/// <summary>
		/// Remove Ms AD user account.
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="identifierAttribute"></param>
		/// <param name="requestLabel"></param>
		/// <param name="setBearerToken"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<IHttpResponse> RemoveMsADUserAccountAsync(string identifier, EntryAttribute? identifierAttribute = EntryAttribute.sAMAccountName, string? requestLabel = null, bool setBearerToken = true, CancellationToken cancellationToken = default)
		{
			var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.DirectoryController}/MsADUsers/{identifier}?identifierAttribute={GetOptionalEntryAttributeName(identifierAttribute)}&requestLabel={requestLabel}";

			using (var httpClient = await CreateHttpClient(setBearerToken))
			{
				var responseMessage = await httpClient.DeleteAsync(uri, cancellationToken);
				if (!responseMessage.IsSuccessStatusCode)
					return await responseMessage.ToUnsuccessfulHttpResponseAsync();
				else
					return await responseMessage.ToSuccessfulHttpResponseAsync<LDAPRemoveMsADUserAccountResult>();
			}
		}
		#endregion DELETE /api/{serverProfile}/{catalogType}/Directory/MsADUsers/{identifier}
	}
}