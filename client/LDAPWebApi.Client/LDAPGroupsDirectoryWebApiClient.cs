﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bitai.LDAPHelper.DTO;
using Bitai.WebApi.Client;

namespace Bitai.LDAPWebApi.Clients
{
	/// <summary>
	/// Client that creates and submits requests to LDAP Web Api Directory controller and Groups path.
	/// </summary>
	public class LDAPGroupsDirectoryWebApiClient : LDAPWebApiBaseClient
	{
		public LDAPGroupsDirectoryWebApiClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog) : base(ldapWebApiBaseUrl, ldapServerProfile, useLdapServerGlobalCatalog)
		{
		}

		public LDAPGroupsDirectoryWebApiClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog, HttpClientHandler handler, bool disposeHandler) : base(ldapWebApiBaseUrl, ldapServerProfile, useLdapServerGlobalCatalog, handler, disposeHandler)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="ldapServerProfile">LDAP Server Profile Id.</param>
		/// <param name="useLdapServerGlobalCatalog">Whether or not the global catalog of the LDAP server will be used; otherwise the local catalog of the LDAP server will be used.</param>
		/// <param name="clientCredentials">Client credentials to request an access token  from the Identity Server. This access token will be sent in the HTTP authorization header as Bearer Token.</param>
		public LDAPGroupsDirectoryWebApiClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog, WebApiClientCredential clientCredentials) : base(ldapWebApiBaseUrl, ldapServerProfile, useLdapServerGlobalCatalog, clientCredentials)
		{
		}

		public LDAPGroupsDirectoryWebApiClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog, WebApiClientCredential clientCredentials, HttpClientHandler handler, bool disposeHandler) : base(ldapWebApiBaseUrl, ldapServerProfile, useLdapServerGlobalCatalog, clientCredentials, handler, disposeHandler)
		{
		}




		/// <summary>
		/// Send a GET request to the route {serverProfile}/{catalogType}/[controller]/Groups/{identifier} of LDAP Web Api Directory controller.
		/// </summary>
		/// <param name="identifier">The value that must be compared with the <paramref name="identifierAttribute"/> in order to satisfy the search condition.</param>
		/// <param name="identifierAttribute"><see cref="EntryAttribute"/> that identifies an LDAP entry.</param>
		/// <param name="requiredAttributes">The attributes that we want to obtain from the LDAP entry as a result of the search.</param>
		/// <param name="requestLabel">Label that will identify the results of the operation.</param>
		/// <param name="setBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
		/// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
		/// <returns><see cref="IHttpResponse"/></returns>
		public async Task<IHttpResponse> SearchByIdentifierAsync(string identifier, EntryAttribute? identifierAttribute, RequiredEntryAttributes? requiredAttributes = null, string? requestLabel = null, bool setBearerToken = true, CancellationToken cancellationToken = default)
		{
			var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.DirectoryController}/Groups/{identifier}?identifierAttribute={GetOptionalEntryAttributeName(identifierAttribute)}&requiredAttributes={GetOptionalRequiredEntryAttributesName(requiredAttributes)}&requestLabel={requestLabel}";

			using (var httpClient = await CreateHttpClient(setBearerToken))
			{
				var responseMessage = await httpClient.GetAsync(uri, cancellationToken);
				if (!responseMessage.IsSuccessStatusCode)
					return await responseMessage.ToUnsuccessfulHttpResponseAsync();
				else
					return await responseMessage.ToSuccessfulHttpResponseAsync<LDAPSearchResult>();
			}
		}

		/// <summary>
		/// Send a GET request to the route {serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/Groups/[action] of LDAP Web Api Directory controller.
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
		/// <returns><see cref="IHttpResponse"/></returns>
		public async Task<IHttpResponse> SearchFilteringByAsync(EntryAttribute filterAttribute, string filterValue, EntryAttribute? secondFilterAttribute = null, string? secondFilterValue = null, bool? combineFilters = null, RequiredEntryAttributes? requiredAttributes = null, string? requestLabel = null, bool setBearerToken = true, CancellationToken cancellationToken = default)
		{
			var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.DirectoryController}/Groups/filterBy?filterAttribute={filterAttribute}&filterValue={filterValue}&secondFilterAttribute={GetOptionalEntryAttributeName(secondFilterAttribute)}&secondFilterValue={secondFilterValue}&combineFilters={GetOptionalBooleanValue(combineFilters)}&requiredAttributes={GetOptionalRequiredEntryAttributesName(requiredAttributes)}&requestLabel={requestLabel}";

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