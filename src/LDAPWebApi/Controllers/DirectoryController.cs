using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bitai.LDAPHelper.DTO;
using Bitai.LDAPHelper.QueryFilters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;

namespace Bitai.LDAPWebApi.Controllers
{
    /// <summary>
    /// Web API controller for operations with LDAP entries
    /// </summary>
    [Route("api")]
    [Authorize]
    [ApiController]
    public class DirectoryController : ApiControllerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">Injected <see cref="IConfiguration"/></param>
        /// <param name="serverProfiles">Injected <see cref="Configurations.LDAP.LDAPServerProfiles"/></param>        
        public DirectoryController(IConfiguration configuration, Configurations.LDAP.LDAPServerProfiles serverProfiles) : base(configuration, serverProfiles)
        {
        }



        [HttpGet]
        [Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/{identifier}")]
        public async Task<ActionResult<LDAPSearchResult>> GetByIdentifier(
            [FromRoute] string serverProfile,
            [FromRoute] string catalogType,
            [FromRoute] string identifier,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalIdentifierAttributeBinder))] EntryAttribute? identifierAttribute,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalRequiredAttributesBinder))] RequiredEntryAttributes? requiredAttributes,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestTag)
        {
            var ldapClientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

            var ldapSearcher = await GetLdapSearcher(ldapClientConfig);

            var searchFilter = new AttributeFilter(identifierAttribute.Value, new FilterValue(identifier));

            var searchResult = await ldapSearcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);

            if (searchResult.Entries.Count() == 0 && searchResult.HasErrorInfo)
                throw searchResult.ErrorObject;

            if (searchResult.Entries.Count() > 1)
                throw new InvalidOperationException($"More than one LDAP entry was obtained for the supplied identifier '{identifier}'. Verify the identifier and the attribute '{identifierAttribute}' to which it applies.");

            return Ok(searchResult);
        }

        [HttpGet]
        [Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/[action]")]
        [ActionName("filterBy")]
        public async Task<ActionResult<LDAPSearchResult>> FilterByAsync(
            [FromRoute] string serverProfile,
            [FromRoute] string catalogType,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.SearchFiltersBinder))] Models.SearchFiltersModel searchFilters,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalRequiredAttributesBinder))] RequiredEntryAttributes? requiredAttributes,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestTag)
        {
            var ldapClientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

            var ldapSearcher = await GetLdapSearcher(ldapClientConfig);

            LDAPSearchResult searchResult;
            if (searchFilters.secondFilterAttribute.HasValue)
            {
                var firstAttributeFilter = new AttributeFilter(searchFilters.filterAttribute.Value, new FilterValue(searchFilters.filterValue));
                var secondAttributeFilter = new AttributeFilter(searchFilters.secondFilterAttribute.Value, new FilterValue(searchFilters.secondFilterValue));
                var searchFilter = new AttributeFilterCombiner(false, searchFilters.combineFilters.Value, new ICombinableFilter[] { firstAttributeFilter, secondAttributeFilter });

                searchResult = await ldapSearcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);
            }
            else
            {
                var searchFilter = new AttributeFilter(searchFilters.filterAttribute.Value, new FilterValue(searchFilters.filterValue));

                searchResult = await ldapSearcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);
            }

            if (searchResult.Entries.Count() == 0 && searchResult.HasErrorInfo)
                throw searchResult.ErrorObject;

            return Ok(searchResult);
        }

        /// <summary>
        /// Gets an LDAP entry with the data of a user. 
        /// </summary>
        /// <param name="serverProfile">LDAP Profile Id that defines part of the route.</param>
        /// <param name="catalogType">LDAP Catalog Type name that defines part of the route. See <see cref="DTO.LDAPCatalogTypes"/></param>
        /// <param name="identifier">Value for <paramref name="identifierAttribute"/> attribute that defines a user. It also defines part of the route./></param>
        /// <param name="identifierAttribute">Optional, default value is <see cref="LDAPHelper.DTO.EntryAttribute.sAMAccountName"/>. It is the attribute by which a user will be identified.</param>
        /// <param name="requiredAttributes">The type of attribute set to return in the result. See <see cref="LDAPHelper.DTO.RequiredEntryAttributes"/></param>
        /// <param name="requestTag">Custom value to tag response values.</param>
        /// <returns><see cref="LDAPSearchResult"/></returns>
        [HttpGet]
        [Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/Users/{identifier}")]
        public async Task<ActionResult<LDAPSearchResult>> GetUserByIdentifier(
            [FromRoute] string serverProfile,
            [FromRoute] string catalogType,
            [FromRoute] string identifier,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalIdentifierAttributeBinder))] EntryAttribute? identifierAttribute,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalRequiredAttributesBinder))] RequiredEntryAttributes? requiredAttributes,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestTag)
        {
            var ldapClientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

            var ldapSearcher = await GetLdapSearcher(ldapClientConfig);

            var onlyUsersFilter = AttributeFilterCombiner.CreateOnlyUsersFilterCombiner();
            var attributeFilter = new AttributeFilter(identifierAttribute.Value, new FilterValue(identifier));

            var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyUsersFilter, attributeFilter });

            var searchResult = await ldapSearcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);

            if (searchResult.Entries.Count() == 0 && searchResult.HasErrorInfo)
                throw searchResult.ErrorObject;

            if (searchResult.Entries.Count() > 1)
                throw new InvalidOperationException($"More than one LDAP entry was obtained for the supplied identifier '{identifier}'. Verify the identifier and the attribute '{identifierAttribute}' to which it applies.");

            return Ok(searchResult);
        }

        [HttpGet]
        [Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/Users/{identifier}/Parents")]
        public async Task<ActionResult<LDAPSearchResult>> GetParentsForUserIdentifier(
            [FromRoute] string serverProfile,
            [FromRoute] string catalogType,
            [FromRoute] string identifier,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalIdentifierAttributeBinder))] EntryAttribute? identifierAttribute,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalRequiredAttributesBinder))] RequiredEntryAttributes? requiredAttributes,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestTag)
        {
            var ldapClientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

            var ldapSearcher = await GetLdapSearcher(ldapClientConfig);

            var onlyUsersFilter = AttributeFilterCombiner.CreateOnlyUsersFilterCombiner();
            var attributeFilter = new AttributeFilter(identifierAttribute.Value, new FilterValue(identifier));

            var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyUsersFilter, attributeFilter });

            var searchResult = await ldapSearcher.SearchParentEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);

            if (searchResult.Entries.Count() == 0 && searchResult.HasErrorInfo)
                throw searchResult.ErrorObject;

            if (searchResult.Entries.Count() > 1)
                throw new InvalidOperationException($"More than one LDAP entry was obtained for the supplied identifier '{identifier}'. Verify the identifier and the attribute '{identifierAttribute}' to which it applies.");

            return Ok(searchResult);
        }

        [HttpGet]
        [Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/Users/[action]")]
        [ActionName("filterBy")]
        public async Task<ActionResult<LDAPSearchResult>> GetUsersFilteringByAsync(
            [FromRoute] string serverProfile,
            [FromRoute] string catalogType,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.SearchFiltersBinder))] Models.SearchFiltersModel searchFilters,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalRequiredAttributesBinder))] RequiredEntryAttributes? requiredAttributes,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestTag)
        {
            var clientConfiguration = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

            var ldapSearcher = await GetLdapSearcher(clientConfiguration);

            LDAPSearchResult searchResult;
            if (searchFilters.secondFilterAttribute.HasValue)
            {
                var onlyUsersFilter = AttributeFilterCombiner.CreateOnlyUsersFilterCombiner();

                var firstAttributeFilter = new AttributeFilter(searchFilters.filterAttribute.Value, new FilterValue(searchFilters.filterValue));
                var secondAttributeFilter = new AttributeFilter(searchFilters.secondFilterAttribute.Value, new FilterValue(searchFilters.secondFilterValue));
                var combinedFilters = new AttributeFilterCombiner(false, searchFilters.combineFilters.Value, new ICombinableFilter[] { firstAttributeFilter, secondAttributeFilter });

                var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyUsersFilter, combinedFilters });

                searchResult = await ldapSearcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);
            }
            else
            {
                var onlyUsersFilter = AttributeFilterCombiner.CreateOnlyUsersFilterCombiner();

                var attributeFilter = new AttributeFilter(searchFilters.filterAttribute.Value, new FilterValue(searchFilters.filterValue));

                var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyUsersFilter, attributeFilter });

                searchResult = await ldapSearcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);
            }

            if (searchResult.Entries.Count() == 0 && searchResult.HasErrorInfo)
                throw searchResult.ErrorObject;

            return Ok(searchResult);
        }

        [HttpGet]
        [Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/Groups/{identifier}")]
        public async Task<ActionResult<LDAPSearchResult>> GetGroupByIdentifier(
            [FromRoute] string serverProfile,
            [FromRoute] string ldapCatalogType,
            [FromRoute] string identifier,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalIdentifierAttributeBinder))] EntryAttribute? identifierAttribute,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalRequiredAttributesBinder))] RequiredEntryAttributes? requiredAttributes,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestTag)
        {
            var ldapClientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(ldapCatalogType));

            var ldapSearcher = await GetLdapSearcher(ldapClientConfig);

            var onlyGroupsFilter = AttributeFilterCombiner.CreateOnlyGroupsFilterCombiner();

            var attributeFilter = new AttributeFilter(identifierAttribute.Value, new FilterValue(identifier));

            var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyGroupsFilter, attributeFilter });

            var searchResult = await ldapSearcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);

            if (searchResult.Entries.Count() == 0 && searchResult.HasErrorInfo)
                throw searchResult.ErrorObject;

            if (searchResult.Entries.Count() > 1)
                throw new InvalidOperationException($"More than one LDAP entry was obtained for the supplied identifier '{identifier}'. Verify the identifier and the attribute '{identifierAttribute}' to which it applies.");

            return Ok(searchResult);
        }

        [HttpGet]
        [Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/Groups/{identifier}/Parents")]
        public async Task<ActionResult<LDAPSearchResult>> GetParentsForGroupIdentifier([FromRoute] string serverProfile,
            [FromRoute] string ldapCatalogType,
            [FromRoute] string identifier,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalIdentifierAttributeBinder))] EntryAttribute? identifierAttribute,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalRequiredAttributesBinder))] RequiredEntryAttributes? requiredAttributes,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestTag)
        {
            var ldapClientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(ldapCatalogType));

            var ldapSearcher = await GetLdapSearcher(ldapClientConfig);

            var onlyGroupsFilter = AttributeFilterCombiner.CreateOnlyGroupsFilterCombiner();

            var attributeFilter = new AttributeFilter(identifierAttribute.Value, new FilterValue(identifier));

            var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyGroupsFilter, attributeFilter });

            var searchResult = await ldapSearcher.SearchParentEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);

            if (searchResult.Entries.Count() == 0 && searchResult.HasErrorInfo)
                throw searchResult.ErrorObject;

            if (searchResult.Entries.Count() > 1)
                throw new InvalidOperationException($"More than one LDAP entry was obtained for the supplied identifier '{identifier}'. Verify the identifier and the attribute '{identifierAttribute}' to which it applies.");

            return Ok(searchResult);
        }

        [HttpGet]
        [Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/Groups/[action]")]
        [ActionName("filterBy")]
        public async Task<ActionResult<LDAPSearchResult>> GetGroupsFilteringByAsync(
            [FromRoute] string serverProfile,
            [FromRoute] string ldapCatalogType,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.SearchFiltersBinder))] Models.SearchFiltersModel searchFilters,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalRequiredAttributesBinder))] RequiredEntryAttributes? requiredAttributes,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestTag
            )
        {
            var clientConfiguration = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(ldapCatalogType));

            var ldapSearcher = await GetLdapSearcher(clientConfiguration);

            LDAPSearchResult searchResult;
            if (searchFilters.secondFilterAttribute.HasValue)
            {
                var onlyGroupsFilter = AttributeFilterCombiner.CreateOnlyGroupsFilterCombiner();

                var firstAttributeFilter = new AttributeFilter(searchFilters.filterAttribute.Value, new FilterValue(searchFilters.filterValue));
                var secondAttributeFilter = new AttributeFilter(searchFilters.secondFilterAttribute.Value, new FilterValue(searchFilters.secondFilterValue));
                var combinedFilters = new AttributeFilterCombiner(false, searchFilters.combineFilters.Value, new ICombinableFilter[] { firstAttributeFilter, secondAttributeFilter });

                var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyGroupsFilter, combinedFilters });

                searchResult = await ldapSearcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);
            }
            else
            {
                var onlyGroupsFilter = AttributeFilterCombiner.CreateOnlyGroupsFilterCombiner();

                var attributeFilter = new AttributeFilter(searchFilters.filterAttribute.Value, new FilterValue(searchFilters.filterValue));

                var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyGroupsFilter, attributeFilter });

                searchResult = await ldapSearcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);
            }

            if (searchResult.Entries.Count() == 0 && searchResult.HasErrorInfo)
                throw searchResult.ErrorObject;

            return Ok(searchResult);
        }
    }
}