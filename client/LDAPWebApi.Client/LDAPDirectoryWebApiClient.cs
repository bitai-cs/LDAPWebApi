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
    /// Client that creates and submits requests to LDAP Web Api Directory controller.
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
        public LDAPDirectoryWebApiClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog, WebApiClientCredentials clientCredentials) : base(ldapWebApiBaseUrl, ldapServerProfile, useLdapServerGlobalCatalog, clientCredentials)
        {
        }



        /// <summary>
        /// Send a GET request to the route {serverProfile}/{catalogType}/[controller]/{identifier} of LDAP Web Api Directory controller.
        /// </summary>
        /// <param name="identifier">The value that must be compared with the <paramref name="identifierAttribute"/> in order to satisfy the search condition.</param>
        /// <param name="identifierAttribute"><see cref="EntryAttribute"/> that identifies an LDAP entry.</param>
        /// <param name="requiredAttributes">The attributes that we want to obtain from the LDAP entry as a result of the search.</param>
        /// <param name="requestTag">Label that will identify the results of the operation.</param>
        /// <param name="setAuthorizationHeaderWithBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
        /// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="IHttpResponse"/></returns>
        public async Task<IHttpResponse> SearchByIdentifierAsync(string identifier, EntryAttribute? identifierAttribute, RequiredEntryAttributes? requiredAttributes = null, string requestTag = null, bool setAuthorizationHeaderWithBearerToken = true, CancellationToken cancellationToken = default)
        {
            var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.DirectoryController}/{identifier}?identifierAttribute={GetOptionalEntryAttributeName(identifierAttribute)}&requiredAttributes={GetOptionalRequiredEntryAttributesName(requiredAttributes)}&requestTag={requestTag}";

            using (var httpClient = await CreateHttpClient(setAuthorizationHeaderWithBearerToken))
            {
                var responseMessage = await httpClient.GetAsync(uri, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                    return await responseMessage.ToUnsuccessfulHttpResponseAsync();
                else
                    return await responseMessage.ToSuccessfulHttpResponseAsync<LDAPSearchResult>();
            }
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
        /// <param name="requestTag">Label that will identify the results of the operation.</param>
        /// <param name="setAuthorizationHeaderWithBearerToken">Whether or not to request and / or assign the access token in the authorization HTTP header.</param>
        /// <param name="cancellationToken">See <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="IHttpResponse"/></returns>
        public async Task<IHttpResponse> SearchByFiltersAsync(EntryAttribute filterAttribute, string filterValue, EntryAttribute? secondFilterAttribute = null, string secondFilterValue = null, bool? combineFilters = null, RequiredEntryAttributes? requiredAttributes = null, string requestTag = null, bool setAuthorizationHeaderWithBearerToken = true, CancellationToken cancellationToken = default)
        {
            var uri = $"{WebApiBaseUrl}/api/{LDAPServerProfile}/{LDAPServerCatalogTypes.GetCatalogTypeName(UseLDAPServerGlobalCatalog)}/{ControllerNames.DirectoryController}/filterBy?filterAttribute={filterAttribute}&filterValue={filterValue}&secondFilterAttribute={GetOptionalEntryAttributeName(secondFilterAttribute)}&secondFilterValue={secondFilterValue}&combineFilters={GetOptionalBooleanValue(combineFilters)}&requiredAttributes={GetOptionalRequiredEntryAttributesName(requiredAttributes)}&requestTag={requestTag}";

            using (var httpClient = await CreateHttpClient(setAuthorizationHeaderWithBearerToken))
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