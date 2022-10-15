using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Bitai.LDAPHelper.DTO;
using Bitai.LDAPHelper.QueryFilters;
using Bitai.LDAPWebApi.Configurations.App;
using Bitai.LDAPWebApi.DTO;
using Bitai.WebApi.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bitai.LDAPWebApi.Controllers;

/// <summary>
/// Web API controller for operations with LDAP entries
/// </summary>
[Route("api")]
[Authorize(WebApiScopesConfiguration.GlobalScopeAuthorizationPolicyName)]
[ApiController]
public class DirectoryController : ApiControllerBase<DirectoryController>
{
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="configuration">Injected <see cref="IConfiguration"/></param>
	/// <param name="logger">Logger</param>
	/// <param name="serverProfiles">Injected <see cref="Configurations.LDAP.LDAPServerProfiles"/></param>        
	public DirectoryController(IConfiguration configuration, ILogger<DirectoryController> logger, Configurations.LDAP.LDAPServerProfiles serverProfiles) : base(configuration, logger, serverProfiles)
	{
	}


	/// <summary>
	/// Search for entries by an identifier field.
	/// </summary>
	/// <param name="serverProfile">LDAP Server profile Id.</param>
	/// <param name="catalogType">LDAP Server catalog type.</param>
	/// <param name="identifier">Entry identifier value.</param>
	/// <param name="identifierAttribute">Optional. Attribute of the entry by which it will be identified. If no value is assigned, the <see cref="EntryAttribute.sAMAccountName"/> attribute is assumed by default.</param>
	/// <param name="requiredAttributes">Optional. . If no value is assigned, <see cref="RequiredEntryAttributes.Few"/> is assumed by default.</param>
	/// <param name="requestTag">Optopnal. Tag to identify the request.</param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
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
		Logger.LogInformation($"Request path: {nameof(serverProfile)}={serverProfile}, {nameof(catalogType)}={catalogType}, {nameof(identifier)}={identifier}, {nameof(identifierAttribute)}={identifierAttribute}, {nameof(requiredAttributes)}={requiredAttributes}, {nameof(requestTag)}={requestTag}");

		var clientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

		var searcher = GetLdapSearcher(clientConfig);

		var searchFilter = new AttributeFilter(identifierAttribute!.Value, new FilterValue(identifier));

		var searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes!.Value, requestTag);

		if (!searchResult.IsSuccessfulOperation)
		{
			Logger.LogError(searchResult.ErrorObject, "Search failed by {@attribute} identifier with value {@identfier}", identifierAttribute, identifier);

			if (searchResult.HasErrorObject)
				throw searchResult.ErrorObject;
			else
				throw new Exception(searchResult.OperationMessage);
		}
		
		if (searchResult.Entries.Count() == 0)
			throw new ResourceNotFoundException($"The user with identifier {identifierAttribute}={identifier} was not found");

		if (searchResult.Entries.Count() > 1)
			throw new BadRequestException($"More than one LDAP entry was obtained for the supplied identifier '{identifier}'. Verify the identifier and the attribute '{identifierAttribute}' to which it applies.");
		
		Logger.LogInformation("Response body: {@result}", searchResult);

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
		Logger.LogInformation($"Request path: {nameof(serverProfile)}={serverProfile}, {nameof(catalogType)}={catalogType}, {nameof(searchFilters.filterAttribute)}={searchFilters.filterAttribute}, {nameof(searchFilters.filterValue)}={searchFilters.filterValue}, {nameof(searchFilters.secondFilterAttribute)}={searchFilters.secondFilterAttribute}, {nameof(searchFilters.secondFilterValue)}={searchFilters.secondFilterValue},{nameof(searchFilters.combineFilters)}={searchFilters.combineFilters},{nameof(requiredAttributes)}={requiredAttributes}, {nameof(requestTag)}={requestTag}");

		ValidateRequiredAttributes(ref requiredAttributes);

		var clientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

		var searcher = GetLdapSearcher(clientConfig);

		LDAPSearchResult searchResult;
		if (searchFilters.secondFilterAttribute.HasValue)
		{
			searchFilters.combineFilters = ValidateCombineFiltersParameter(searchFilters);

			var firstAttributeFilter = new AttributeFilter(searchFilters.filterAttribute, new FilterValue(searchFilters.filterValue));
			var secondAttributeFilter = new AttributeFilter(searchFilters.secondFilterAttribute.Value, new FilterValue(searchFilters.secondFilterValue));
			var searchFilter = new AttributeFilterCombiner(false, searchFilters.combineFilters.Value, new ICombinableFilter[] { firstAttributeFilter, secondAttributeFilter });

			searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);
		}
		else
		{
			var searchFilter = new AttributeFilter(searchFilters.filterAttribute, new FilterValue(searchFilters.filterValue));

			searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);
		}

		if (!searchResult.IsSuccessfulOperation) {
			Logger.LogError(searchResult.ErrorObject, "Error when performing the search for LDAP entries by the filter: {@filterModel}", searchFilters);

			if (searchResult.HasErrorObject)
				throw searchResult.ErrorObject;
			else
				throw new Exception(searchResult.OperationMessage);
		}
		
		Logger.LogInformation("Search result count: {0}", searchResult.Entries.Count());

		return Ok(searchResult);
	}

	/// <summary>
	/// Gets an LDAP entry with the data of a user. 
	/// </summary>
	/// <param name="serverProfile">LDAP Profile Id that defines part of the route.</param>
	/// <param name="catalogType">LDAP Catalog Type name that defines part of the route. See <see cref="DTO.LDAPCatalogTypes"/>.</param>
	/// <param name="identifier">Identifier of the user account that will define the route of this Endpoint. There must be a valid value for the LDAP attributes <see cref="EntryAttribute.sAMAccountName"/> or <see cref="EntryAttribute.distinguishedName"/>.</param>
	/// <param name="identifierAttribute">Attribute (<see cref="EntryAttribute.sAMAccountName"/> or <see cref="EntryAttribute.distinguishedName"/>) that will validate the <paramref name="identifier"/> parameter.</param>
	/// <param name="requestTag">Custom value to tag response values.</param>
	/// <param name="credential"><see cref="LDAPCredential"/> with new password. The <see cref="LDAPCredential.UserAccount"/> property must correspond to the <paramref name="identifier"/> parameter</param>
	/// <returns><see cref="LDAPPasswordUpdateResult"/></returns>
	[HttpPost]
	[Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/Users/{identifier}/Credential")]
	public async Task<ActionResult<LDAPPasswordUpdateResult>> SetUserCredential(
		[FromRoute] string serverProfile,
		[FromRoute] string catalogType,
		[FromRoute] string identifier,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalUserAccountIdentifierAttributeBinder))] EntryAttribute? identifierAttribute,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestTag,
		[FromBody] LDAPCredential credential)
	{
		Logger.LogInformation("Request path: {@spn}={@sp}, {@ctn}={@ct}, {@in}={@i}, {@ian}={@ia}, {@rtn}={@rt}, {@cn}={@c}", nameof(serverProfile), serverProfile, nameof(catalogType), catalogType, nameof(identifier),identifier, nameof(identifierAttribute), identifierAttribute, nameof(requestTag),requestTag, nameof(credential), credential.UserAccount);

		ValidateIdentifierAttribute(ref identifierAttribute);

		if (!LDAPCredential.Validate(ref credential, true, out var validations))
			throw new BadRequestException(string.Format("{0} {1}", "The credential is not valid.", validations));

		var serverProfileObject = ServerProfiles.Where(sp => sp.ProfileId.Equals(serverProfile, StringComparison.OrdinalIgnoreCase)).Single();

		string domainName;
		string userAccount;
		if (identifierAttribute == EntryAttribute.sAMAccountName && credential.UserAccount.Contains('\\'))
		{	
			var strings = credential.UserAccount.Split('\\', StringSplitOptions.None);
			if (!strings[0].Equals(serverProfileObject.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
				throw new BadRequestException($"The {strings[0]} domain of the user account {strings[1]} must be the same as the {serverProfile} domain specified in the API route.");

			if (!strings[1].Equals(identifier, StringComparison.OrdinalIgnoreCase))
				throw new BadHttpRequestException($"The user account {strings[1]} must be the same as the {identifier} identifier specified in the API route.");

			domainName = strings[0];
			userAccount = strings[1];
		}
		else
		{
			domainName = serverProfileObject.DefaultDomainName;
			userAccount = credential.UserAccount;
		}

		var clientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));
		var searcher = GetLdapSearcher(clientConfig);
		var onlyUsersFilter = AttributeFilterCombiner.CreateOnlyUsersFilterCombiner();
		var attributeFilter = new AttributeFilter(identifierAttribute.Value, new FilterValue(identifier));
		var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyUsersFilter, attributeFilter });
		var searchResult = await searcher.SearchEntriesAsync(searchFilter, RequiredEntryAttributes.Minimun, requestTag);
		if (!searchResult.IsSuccessfulOperation) {
			Logger.LogError(searchResult.ErrorObject, "Failed to search for a domain user account based on search filter {@filter}.", searchFilter);

			if (searchResult.HasErrorObject)
				throw searchResult.ErrorObject;
			else
				throw new Exception(searchResult.OperationMessage);
		}
		
		if (searchResult.Entries.Count() == 0)
			throw new ResourceNotFoundException($"The user accoun with identifier {attributeFilter} was not found");

		if (searchResult.Entries.Count() > 1)
			throw new BadRequestException($"More than one LDAP entry was obtained for the supplied identifier {attributeFilter}.");

		var entry = searchResult.Entries.Single();

		var dnCredential = new LDAPDistinguishedNameCredential(entry.distinguishedName, credential.Password);
		var accountManager = new LDAPHelper.AccountManager(GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType)));
		var pwdUpdateResult = await accountManager.SetAccountPassword(dnCredential, requestTag);

		if (!pwdUpdateResult.IsSuccessfulOperation)
		{
			if (pwdUpdateResult.HasErrorObject)
			{
				Logger.LogError(pwdUpdateResult.ErrorObject, "Failed password assignment for user account {@identifier} with {@dnAttr}: {@dn}", identifier, EntryAttribute.distinguishedName, dnCredential.DistinguishedName);

				throw pwdUpdateResult.ErrorObject;
			}
			else
				throw new Exception(pwdUpdateResult.OperationMessage);
		}

		Logger.LogInformation("Response body: {@result}", pwdUpdateResult);
		
		return Ok(pwdUpdateResult);
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
		Logger.LogInformation($"Request path: {nameof(serverProfile)}={serverProfile}, {nameof(catalogType)}={catalogType}, {nameof(identifier)}={identifier}, {nameof(identifierAttribute)}={identifierAttribute}, {nameof(requiredAttributes)}={requiredAttributes}, {nameof(requestTag)}={requestTag}");

		ValidateIdentifierAttribute(ref identifierAttribute);

		ValidateRequiredAttributes(ref requiredAttributes);

		var clientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

		var searcher = GetLdapSearcher(clientConfig);

		var onlyUsersFilter = AttributeFilterCombiner.CreateOnlyUsersFilterCombiner();
		var attributeFilter = new AttributeFilter(identifierAttribute.Value, new FilterValue(identifier));

		var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyUsersFilter, attributeFilter });

		var searchResult = await searcher.SearchParentEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);

		if (!searchResult.IsSuccessfulOperation)
		{
			Logger.LogError(searchResult.ErrorObject, "Failed to get LDAP parent entries for user account with {@attribute} = {@value}.", identifier, identifierAttribute);

			if (searchResult.HasErrorObject)
				throw searchResult.ErrorObject;
			else
				throw new Exception(searchResult.OperationMessage);		
		}
		
		Logger.LogInformation("Response body: {@result}", searchResult);

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
		Logger.LogInformation($"Request path: {nameof(serverProfile)}={serverProfile}, {nameof(catalogType)}={catalogType}, {nameof(searchFilters.filterAttribute)}={searchFilters.filterAttribute}, {nameof(searchFilters.filterValue)}={searchFilters.filterValue}, {nameof(searchFilters.secondFilterAttribute)}={searchFilters.secondFilterAttribute}, {nameof(searchFilters.secondFilterValue)}={searchFilters.secondFilterValue},{nameof(searchFilters.combineFilters)}={searchFilters.combineFilters},{nameof(requiredAttributes)}={requiredAttributes}, {nameof(requestTag)}={requestTag}");

		ValidateRequiredAttributes(ref requiredAttributes);

		var clientConfiguration = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

		var searcher = GetLdapSearcher(clientConfiguration);

		LDAPSearchResult searchResult;
		if (searchFilters.secondFilterAttribute.HasValue)
		{
			searchFilters.combineFilters = ValidateCombineFiltersParameter(searchFilters);

			var onlyUsersFilter = AttributeFilterCombiner.CreateOnlyUsersFilterCombiner();

			var firstAttributeFilter = new AttributeFilter(searchFilters.filterAttribute, new FilterValue(searchFilters.filterValue));
			var secondAttributeFilter = new AttributeFilter(searchFilters.secondFilterAttribute.Value, new FilterValue(searchFilters.secondFilterValue));
			var combinedFilters = new AttributeFilterCombiner(false, searchFilters.combineFilters.Value, new ICombinableFilter[] { firstAttributeFilter, secondAttributeFilter });

			var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyUsersFilter, combinedFilters });

			searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);
		}
		else
		{
			var onlyUsersFilter = AttributeFilterCombiner.CreateOnlyUsersFilterCombiner();

			var attributeFilter = new AttributeFilter(searchFilters.filterAttribute, new FilterValue(searchFilters.filterValue));

			var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyUsersFilter, attributeFilter });

			searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);
		}

		if (!searchResult.IsSuccessfulOperation) {
			Logger.LogError(searchResult.ErrorObject, "Error searching for users with the following filter: {@searchFilter}.", searchFilters);

			if (searchResult.HasErrorObject)
				throw searchResult.ErrorObject;
			else
				throw new Exception(searchResult.OperationMessage);
		}

		Logger.LogInformation("Search result count: {0}", searchResult.Entries.Count());

		return Ok(searchResult);
	}

	[HttpGet]
	[Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/Groups/{identifier}")]
	public async Task<ActionResult<LDAPSearchResult>> GetGroupByIdentifier(
		[FromRoute] string serverProfile,
		[FromRoute] string catalogType,
		[FromRoute] string identifier,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalIdentifierAttributeBinder))] EntryAttribute? identifierAttribute,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalRequiredAttributesBinder))] RequiredEntryAttributes? requiredAttributes,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestTag)
	{
		Logger.LogInformation($"Request path: {nameof(serverProfile)}={serverProfile}, {nameof(catalogType)}={catalogType}, {nameof(identifier)}={identifier}, {nameof(identifierAttribute)}={identifierAttribute}, {nameof(requiredAttributes)}={requiredAttributes}, {nameof(requestTag)}={requestTag}");

		ValidateIdentifierAttribute(ref identifierAttribute);

		ValidateRequiredAttributes(ref requiredAttributes);

		var clientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

		var searcher = GetLdapSearcher(clientConfig);

		var onlyGroupsFilter = AttributeFilterCombiner.CreateOnlyGroupsFilterCombiner();

		var attributeFilter = new AttributeFilter(identifierAttribute!.Value, new FilterValue(identifier));

		var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyGroupsFilter, attributeFilter });

		var searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes!.Value, requestTag);

		if (!searchResult.IsSuccessfulOperation)
		{
			Logger.LogError(searchResult.ErrorObject, "Failed to look up the LDAP entry of a group by its identifier: {@attr}={@id}", identifierAttribute, identifier);

			if (searchResult.HasErrorObject)
				throw searchResult.ErrorObject;
			else
				throw new Exception(searchResult.OperationMessage);
		}

		if (searchResult.Entries.Count() == 0)
			throw new ResourceNotFoundException($"The LDAP group entry with identifier {identifierAttribute}={identifier} was not found");

		if (searchResult.Entries.Count() > 1)
			throw new BadRequestException($"More than one LDAP entry was obtained for the supplied identifier '{identifier}'. Verify the identifier and the attribute '{identifierAttribute}' to which it applies.");

		Logger.LogInformation("Response body: {@result}", searchResult);

		return Ok(searchResult);
	}

	[HttpGet]
	[Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/Groups/{identifier}/Parents")]
	public async Task<ActionResult<LDAPSearchResult>> GetParentsForGroupIdentifier([FromRoute] string serverProfile,
		[FromRoute] string catalogType,
		[FromRoute] string identifier,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalIdentifierAttributeBinder))] EntryAttribute? identifierAttribute,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalRequiredAttributesBinder))] RequiredEntryAttributes? requiredAttributes,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestTag)
	{
		Logger.LogInformation($"Request path: {nameof(serverProfile)}={serverProfile}, {nameof(catalogType)}={catalogType}, {nameof(identifier)}={identifier}, {nameof(identifierAttribute)}={identifierAttribute}, {nameof(requiredAttributes)}={requiredAttributes}, {nameof(requestTag)}={requestTag}");

		ValidateIdentifierAttribute(ref identifierAttribute);

		ValidateRequiredAttributes(ref requiredAttributes);

		var clientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

		var searcher = GetLdapSearcher(clientConfig);

		var onlyGroupsFilter = AttributeFilterCombiner.CreateOnlyGroupsFilterCombiner();

		var attributeFilter = new AttributeFilter(identifierAttribute.Value, new FilterValue(identifier));

		var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyGroupsFilter, attributeFilter });

		var searchResult = await searcher.SearchParentEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);

		if (!searchResult.IsSuccessfulOperation)
		{
			Logger.LogError(searchResult.ErrorObject, "Failed to get LDAP parent entries for user account with {@attribute} = {@value}.", identifier, identifierAttribute);

			if (searchResult.HasErrorObject)
				throw searchResult.ErrorObject;
			else
				throw new Exception(searchResult.OperationMessage);
		}

		Logger.LogInformation("Response body: {@result}", searchResult);

		return Ok(searchResult);
	}

	[HttpGet]
	[Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/Groups/[action]")]
	[ActionName("filterBy")]
	public async Task<ActionResult<LDAPSearchResult>> GetGroupsFilteringByAsync(
		[FromRoute] string serverProfile,
		[FromRoute] string catalogType,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.SearchFiltersBinder))] Models.SearchFiltersModel searchFilters,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalRequiredAttributesBinder))] RequiredEntryAttributes? requiredAttributes,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestTag
		)
	{
		Logger.LogInformation($"Request path: {nameof(serverProfile)}={serverProfile}, {nameof(catalogType)}={catalogType}, {nameof(searchFilters.filterAttribute)}={searchFilters.filterAttribute}, {nameof(searchFilters.filterValue)}={searchFilters.filterValue}, {nameof(searchFilters.secondFilterAttribute)}={searchFilters.secondFilterAttribute}, {nameof(searchFilters.secondFilterValue)}={searchFilters.secondFilterValue},{nameof(searchFilters.combineFilters)}={searchFilters.combineFilters},{nameof(requiredAttributes)}={requiredAttributes}, {nameof(requestTag)}={requestTag}");

		ValidateRequiredAttributes(ref requiredAttributes);

		var clientConfiguration = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

		var searcher = GetLdapSearcher(clientConfiguration);

		LDAPSearchResult searchResult;
		if (searchFilters.secondFilterAttribute.HasValue)
		{
			searchFilters.combineFilters = ValidateCombineFiltersParameter(searchFilters);

			var onlyGroupsFilter = AttributeFilterCombiner.CreateOnlyGroupsFilterCombiner();

			var firstAttributeFilter = new AttributeFilter(searchFilters.filterAttribute, new FilterValue(searchFilters.filterValue));
			var secondAttributeFilter = new AttributeFilter(searchFilters.secondFilterAttribute.Value, new FilterValue(searchFilters.secondFilterValue));
			var combinedFilters = new AttributeFilterCombiner(false, searchFilters.combineFilters.Value, new ICombinableFilter[] { firstAttributeFilter, secondAttributeFilter });

			var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyGroupsFilter, combinedFilters });

			searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);
		}
		else
		{
			var onlyGroupsFilter = AttributeFilterCombiner.CreateOnlyGroupsFilterCombiner();

			var attributeFilter = new AttributeFilter(searchFilters.filterAttribute, new FilterValue(searchFilters.filterValue));

			var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyGroupsFilter, attributeFilter });

			searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestTag);
		}

		if (!searchResult.IsSuccessfulOperation)
		{
			Logger.LogError(searchResult.ErrorObject, "Failed to get LDAP group entries by  {@filter}", searchFilters);

			if (searchResult.HasErrorObject)
				throw searchResult.ErrorObject;
			else
				throw new Exception(searchResult.OperationMessage);
		}

		Logger.LogInformation("Search result count: {0}", searchResult.Entries.Count());

		return Ok(searchResult);
	}
}