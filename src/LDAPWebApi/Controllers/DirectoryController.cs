using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
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
	/// Get an directory entry by its identifier.
	/// </summary>
	/// <param name="serverProfile">LDAP Server profile Id.</param>
	/// <param name="catalogType">LDAP Server catalog type.</param>
	/// <param name="identifier">Directory entry identifier value.</param>
	/// <param name="identifierAttribute">Attribute of the entry by which it will be identified. If no value is assigned, the <see cref="EntryAttribute.sAMAccountName"/> attribute is assumed by default. This is an optional query string parameter.</param>
	/// <param name="requiredAttributes">Type of LDAP attribute set that the response should return. If no value is assigned, <see cref="RequiredEntryAttributes.Few"/> is assumed by default. This is an optional query string parameter.</param>
	/// <param name="requestLabel">Custom tag that identifies the request and marks the data returned in the response. This is an optional query string parameter.</param>
	/// <returns><see cref="LDAPSearchResult"/></returns>
	/// <exception cref="ResourceNotFoundException">When no directory entry found.</exception>
	/// <exception cref="BadRequestException">When more than one directory entry is found.</exception>
	[HttpGet]
	[Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/{identifier}")]
	public async Task<ActionResult<LDAPSearchResult>> GetByIdentifier(
		[FromRoute] string serverProfile,
		[FromRoute] string catalogType,
		[FromRoute] string identifier,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalIdentifierAttributeBinder))] EntryAttribute? identifierAttribute,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalRequiredAttributesBinder))] RequiredEntryAttributes? requiredAttributes,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestLabel)
	{
		Logger.LogInformation($"Request path: {nameof(serverProfile)}={serverProfile}, {nameof(catalogType)}={catalogType}, {nameof(identifier)}={identifier}, {nameof(identifierAttribute)}={identifierAttribute}, {nameof(requiredAttributes)}={requiredAttributes}, {nameof(requestLabel)}={requestLabel}");

		var clientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

		var searcher = GetLdapSearcher(clientConfig);

		var searchFilter = new AttributeFilter(identifierAttribute!.Value, new FilterValue(identifier));

		var searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes!.Value, requestLabel);

		if (!searchResult.IsSuccessfulOperation)
		{
			if (searchResult.HasErrorObject)
			{
				Logger.LogError(searchResult.ErrorObject, "Search failed by {@ida} identifier with value {@id}.", identifierAttribute, identifier);

				throw searchResult.ErrorObject;
			}
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

	/// <summary>
	/// Search directory entries based on the specified filters.
	/// </summary>
	/// <param name="serverProfile">LDAP Server profile Id.</param>
	/// <param name="catalogType">LDAP Server catalog type.</param>
	/// <param name="searchFilters"><see cref="Models.SearchFiltersModel"/> that encapsulates attribute filters in query string.</param>
	/// <param name="requiredAttributes">Type of LDAP attribute set that the response should return. If no value is assigned, <see cref="RequiredEntryAttributes.Few"/> is assumed by default. This is an optional query string parameter.</param>
	/// <param name="requestLabel">Custom tag that identifies the request and marks the data returned in the response. This is an optional query string parameter.</param>
	/// <returns><see cref="LDAPSearchResult"/></returns>
	/// <exception cref="ApplicationException">When an <see cref="LDAPHelper"/> operation does not complete successfully.</exception>
	[HttpGet]
	[Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/[action]")]
	[ActionName("filterBy")]
	public async Task<ActionResult<LDAPSearchResult>> FilterByAsync(
		[FromRoute] string serverProfile,
		[FromRoute] string catalogType,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.SearchFiltersBinder))] Models.SearchFiltersModel searchFilters,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalRequiredAttributesBinder))] RequiredEntryAttributes? requiredAttributes,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestLabel)
	{
		Logger.LogInformation($"Request path: {nameof(serverProfile)}={serverProfile}, {nameof(catalogType)}={catalogType}, {nameof(searchFilters.filterAttribute)}={searchFilters.filterAttribute}, {nameof(searchFilters.filterValue)}={searchFilters.filterValue}, {nameof(searchFilters.secondFilterAttribute)}={searchFilters.secondFilterAttribute}, {nameof(searchFilters.secondFilterValue)}={searchFilters.secondFilterValue},{nameof(searchFilters.combineFilters)}={searchFilters.combineFilters},{nameof(requiredAttributes)}={requiredAttributes}, {nameof(requestLabel)}={requestLabel}");

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

			searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestLabel);
		}
		else
		{
			var searchFilter = new AttributeFilter(searchFilters.filterAttribute, new FilterValue(searchFilters.filterValue));

			searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestLabel);
		}

		if (!searchResult.IsSuccessfulOperation)
		{
			if (searchResult.HasErrorObject)
			{
				Logger.LogError(searchResult.ErrorObject, "Error when performing the search for LDAP entries by the filter: {@searchFilters}", searchFilters);

				throw searchResult.ErrorObject;
			}
			else
				throw new ApplicationException(searchResult.OperationMessage);
		}

		Logger.LogInformation("Search result count: {0}", searchResult.Entries.Count());

		return Ok(searchResult);
	}

	/// <summary>
	/// Create MS AD user account.
	/// </summary>
	/// <param name="serverProfile"></param>
	/// <param name="catalogType"></param>
	/// <param name="requestLabel"></param>
	/// <param name="newUserAccount"></param>
	/// <returns></returns>
	/// <exception cref="BadRequestException"></exception>
	/// <exception cref="Exception"></exception>
	[HttpPost]
	[Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/MsADUsers")]
	public async Task<ActionResult<LDAPCreateMsADUserAccountResult>> CreateUserAccountForMsAD(
		[FromRoute] string serverProfile,
		[FromRoute] string catalogType,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestLabel,
		[FromBody] LDAPMsADUserAccount newUserAccount)
	{
		Logger.LogInformation("Request path: {@spn}={@sp}, {@ctn}={@ct}, {@rtn}={@rt}, {@nuan}={@nua}", nameof(serverProfile), serverProfile, nameof(catalogType), catalogType, nameof(requestLabel), requestLabel, nameof(newUserAccount), newUserAccount);

		if (!catalogType.Equals(CatalogTypeRoutes.LocalCatalog, StringComparison.OrdinalIgnoreCase))
			throw new BadRequestException("Cannot create user accounts in the global catalog of the LDAP server.");

		var clientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

		var accountManager = new LDAPHelper.AccountManager(clientConfig);
		accountManager.InitializeMissingMsADUserAccountDN(newUserAccount);

		#region Check if DN already exists
		var onlyUsersFilterCombiner = LDAPHelper.QueryFilters.AttributeFilterCombiner.CreateOnlyUsersFilterCombiner();
		var attributeFilter = new LDAPHelper.QueryFilters.AttributeFilter(EntryAttribute.distinguishedName, new LDAPHelper.QueryFilters.FilterValue(newUserAccount.DistinguishedName));
		var searchFilterCombiner = new LDAPHelper.QueryFilters.AttributeFilterCombiner(false, true, new List<LDAPHelper.QueryFilters.ICombinableFilter> { onlyUsersFilterCombiner, attributeFilter });

		var searcher = new LDAPHelper.Searcher(clientConfig);
		var searchResult = await searcher.SearchEntriesAsync(searchFilterCombiner, RequiredEntryAttributes.Minimun, requestLabel);
		if (!searchResult.IsSuccessfulOperation)
		{
			if (searchResult.HasErrorObject)
				throw searchResult.ErrorObject;
			else
				throw new Exception(searchResult.OperationMessage);
		}
		if (searchResult.Entries.Count() != 0)
		{
			throw new ConflictException($"There is already an entry in the AD with the {EntryAttribute.distinguishedName} equal to {newUserAccount.DistinguishedName}");
		}
		#endregion

		#region Check if samAccountName already exists
		attributeFilter = new LDAPHelper.QueryFilters.AttributeFilter(EntryAttribute.sAMAccountName, new LDAPHelper.QueryFilters.FilterValue(newUserAccount.SAMAccountName));
		searchFilterCombiner = new LDAPHelper.QueryFilters.AttributeFilterCombiner(false, true, new List<LDAPHelper.QueryFilters.ICombinableFilter> { onlyUsersFilterCombiner, attributeFilter });

		searchResult = await searcher.SearchEntriesAsync(searchFilterCombiner, RequiredEntryAttributes.Minimun, requestLabel);
		if (!searchResult.IsSuccessfulOperation)
		{
			if (searchResult.HasErrorObject)
				throw searchResult.ErrorObject;
			else
				throw new Exception(searchResult.OperationMessage);
		}
		if (searchResult.Entries.Count() != 0)
		{
			throw new ConflictException($"There is already an entry in the AD with the {EntryAttribute.sAMAccountName} equal to {newUserAccount.SAMAccountName}");
		}
		#endregion

		var createUserAccountResult = await accountManager.CreateUserAccountForMsAD(newUserAccount, requestLabel);
		if (!createUserAccountResult.IsSuccessfulOperation)
		{
			if (createUserAccountResult.HasErrorObject)
			{
				Logger.LogError(createUserAccountResult.ErrorObject, "Error creating user account {@ua}", newUserAccount.SecureClone());

				throw createUserAccountResult.ErrorObject;
			}
			else
				throw new Exception(createUserAccountResult.OperationMessage);
		}

		Logger.LogInformation("Response body: {@pwdUpdateResult}", createUserAccountResult);

		return Ok(createUserAccountResult);
	}

	/// <summary>
	/// Set MS AD user account password. 
	/// </summary>
	/// <param name="serverProfile">LDAP Profile Id that defines part of the route.</param>
	/// <param name="catalogType">LDAP Catalog Type name that defines part of the route. See <see cref="DTO.LDAPServerCatalogTypes"/>.</param>
	/// <param name="identifier">Identifier of the user account that will define the route of this Endpoint. There must be a valid value for the LDAP attributes <see cref="EntryAttribute.sAMAccountName"/> or <see cref="EntryAttribute.distinguishedName"/>.</param>
	/// <param name="identifierAttribute">Attribute (<see cref="EntryAttribute.sAMAccountName"/> or <see cref="EntryAttribute.distinguishedName"/>) that will validate the <paramref name="identifier"/> parameter.</param>
	/// <param name="requestLabel">Custom tag that identifies the request and marks the data returned in the response. This is an optional query string parameter.</param>
	/// <param name="credential"><see cref="LDAPCredential"/> with new password. The <see cref="LDAPCredential.UserAccount"/> property must correspond to the <paramref name="identifier"/> parameter</param>
	/// <returns><see cref="LDAPPasswordUpdateResult"/></returns>
	[HttpPatch]
	[Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/MsADUsers/{identifier}/Credential")]
	public async Task<ActionResult<LDAPPasswordUpdateResult>> SetUserAccountCredentialForMsAD(
		[FromRoute] string serverProfile,
		[FromRoute] string catalogType,
		[FromRoute] string identifier,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalUserAccountIdentifierAttributeBinder))] EntryAttribute? identifierAttribute,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestLabel,
		[FromBody] LDAPCredential credential)
	{
		Logger.LogInformation("Request path: {@spn}={@sp}, {@ctn}={@ct}, {@in}={@i}, {@ian}={@ia}, {@rtn}={@rt}, {@cn}={@c}", nameof(serverProfile), serverProfile, nameof(catalogType), catalogType, nameof(identifier), identifier, nameof(identifierAttribute), identifierAttribute, nameof(requestLabel), requestLabel, nameof(credential), credential.UserAccount);

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
		var searchResult = await searcher.SearchEntriesAsync(searchFilter, RequiredEntryAttributes.Minimun, requestLabel);
		if (!searchResult.IsSuccessfulOperation)
		{
			if (searchResult.HasErrorObject)
			{
				Logger.LogError(searchResult.ErrorObject, "Failed to search for a domain user account based on search filter {@searchFilter}", searchFilter);

				throw searchResult.ErrorObject;
			}
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
		var pwdUpdateResult = await accountManager.SetUserAccountPasswordForMsAD(dnCredential, requestLabel);

		if (!pwdUpdateResult.IsSuccessfulOperation)
		{
			if (pwdUpdateResult.HasErrorObject)
			{
				Logger.LogError(pwdUpdateResult.ErrorObject, "Failed password assignment for user account {identifier} with distinguishedName {distinguishedName}", identifier, EntryAttribute.distinguishedName, dnCredential.DistinguishedName);

				throw pwdUpdateResult.ErrorObject;
			}
			else
				throw new Exception(pwdUpdateResult.OperationMessage);
		}

		Logger.LogInformation("Response body: {@pwdUpdateResult}", pwdUpdateResult);

		return Ok(pwdUpdateResult);
	}

	/// <summary>
	/// Disable MS AD user account.
	/// </summary>
	/// <param name="serverProfile">LDAP Profile Id that defines part of the route.</param>
	/// <param name="catalogType">LDAP Catalog Type name that defines part of the route. See <see cref="DTO.LDAPServerCatalogTypes"/>.</param>
	/// <param name="identifier">Identifier of the user account that will define the route of this Endpoint. There must be a valid value for the LDAP attributes <see cref="EntryAttribute.sAMAccountName"/> or <see cref="EntryAttribute.distinguishedName"/>.</param>
	/// <param name="identifierAttribute">Attribute (<see cref="EntryAttribute.sAMAccountName"/> or <see cref="EntryAttribute.distinguishedName"/>) that will validate the <paramref name="identifier"/> parameter.</param>
	/// <param name="requestLabel">Custom tag that identifies the request and marks the data returned in the response. This is an optional query string parameter.</param>
	/// <returns><see cref="LDAPRemoveMsADUserAccountResult"/></returns>
	[HttpPatch]
	[Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/MsADUsers/{identifier}/[action]")]
	[ActionName("disableBy")]
	public async Task<ActionResult<LDAPDisableUserAccountOperationResult>> DisableMsADUserAccount(
		[FromRoute] string serverProfile,
		[FromRoute] string catalogType,
		[FromRoute] string identifier,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalUserAccountIdentifierAttributeBinder))] EntryAttribute? identifierAttribute,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestLabel)
	{
		Logger.LogInformation("Request path: {@spn}={@sp}, {@ctn}={@ct}, {@idn}={@id}, {@idan}={@ida}, {@rtn}={@rt}", nameof(serverProfile), serverProfile, nameof(catalogType), catalogType, nameof(identifier), identifier, nameof(identifierAttribute), identifierAttribute, nameof(requestLabel), requestLabel);

		if (!catalogType.Equals(CatalogTypeRoutes.LocalCatalog, StringComparison.OrdinalIgnoreCase))
			throw new BadRequestException("No se puede eliminar la cuenta de usuario en el catálogo global de un servidor LDAP. Esta operación solo está permitida en el catálogo local de un servidor LDAP.");

		var clientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

		string distinguishedName = identifier;
		if (identifierAttribute == EntryAttribute.sAMAccountName)
		{
			var searchResult = await SearchUserAccountAsync(clientConfig, identifier, identifierAttribute.Value, requestLabel);

			if (searchResult.IsSuccessfulOperation)
			{
				if (searchResult.Entries.Count() == 0)
					throw new ResourceNotFoundException($"There is no user account according to the criteria {identifierAttribute} = {identifier}");

				distinguishedName = searchResult.Entries.Single().distinguishedName;
			}
			else
				ThrowExceptionForUnsuccessfulOperation($"Failed to disable user account {identifier}.", searchResult);
		}

		var accountManager = new LDAPHelper.AccountManager(clientConfig);

		var disableResult = await accountManager.DisableUserAccountForMsAD(distinguishedName, requestLabel);
		if (!disableResult.IsSuccessfulOperation)
			ThrowExceptionForUnsuccessfulOperation($"Failed to disable user account {identifier}.", disableResult);

		Logger.LogInformation("Response body: {@removeResult}", disableResult);

		return Ok(disableResult);
	}

	/// <summary>
	/// Remove MS AD user account.
	/// </summary>
	/// <param name="serverProfile">LDAP Profile Id that defines part of the route.</param>
	/// <param name="catalogType">LDAP Catalog Type name that defines part of the route. See <see cref="DTO.LDAPServerCatalogTypes"/>.</param>
	/// <param name="identifier">Identifier of the user account that will define the route of this Endpoint. There must be a valid value for the LDAP attributes <see cref="EntryAttribute.sAMAccountName"/> or <see cref="EntryAttribute.distinguishedName"/>.</param>
	/// <param name="identifierAttribute">Attribute (<see cref="EntryAttribute.sAMAccountName"/> or <see cref="EntryAttribute.distinguishedName"/>) that will validate the <paramref name="identifier"/> parameter.</param>
	/// <param name="requestLabel">Custom tag that identifies the request and marks the data returned in the response. This is an optional query string parameter.</param>
	/// <returns><see cref="LDAPRemoveMsADUserAccountResult"/></returns>
	[HttpDelete]
	[Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/MsADUsers/{identifier}")]
	public async Task<ActionResult<LDAPRemoveMsADUserAccountResult>> RemoveMsADUserAccount(
		[FromRoute] string serverProfile,
		[FromRoute] string catalogType,
		[FromRoute] string identifier,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalUserAccountIdentifierAttributeBinder))] EntryAttribute? identifierAttribute,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestLabel)
	{
		Logger.LogInformation("Request path: {@spn}={@sp}, {@ctn}={@ct}, {@idn}={@id}, {@idan}={@ida}, {@rtn}={@rt}", nameof(serverProfile), serverProfile, nameof(catalogType), catalogType, nameof(identifier), identifier, nameof(identifierAttribute), identifierAttribute, nameof(requestLabel), requestLabel);

		if (!catalogType.Equals(CatalogTypeRoutes.LocalCatalog, StringComparison.OrdinalIgnoreCase))
			throw new BadRequestException("Cannot remove user account in the global catalog of the LDAP server. This operation is only allowed in local catalog of a LDAP server.");

		var clientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

		string distinguishedName = identifier;
		if (identifierAttribute == EntryAttribute.sAMAccountName)
		{
			var searchResult = await SearchUserAccountAsync(clientConfig, identifier, identifierAttribute.Value, requestLabel);

			if (searchResult.IsSuccessfulOperation)
			{
				if (searchResult.Entries.Count() == 0)
					throw new ResourceNotFoundException($"There is no user account according to the criteria {identifierAttribute} = {identifier}");
				else
					distinguishedName = searchResult.Entries.Single().distinguishedName;
			}
			else
				ThrowExceptionForUnsuccessfulOperation($"Failed to disable user account {identifier}.", searchResult);
		}

		var accountManager = new LDAPHelper.AccountManager(clientConfig);

		var removeResult = await accountManager.RemoveUserAccountForMsAD(distinguishedName, requestLabel);

		if (!removeResult.IsSuccessfulOperation)
		{
			if (removeResult.HasErrorObject)
			{
				throw removeResult.ErrorObject;
			}
			else
				throw new Exception(removeResult.OperationMessage);
		}

		Logger.LogInformation("Response body: {@removeResult}", removeResult);

		return Ok(removeResult);
	}

	[HttpGet]
	[Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/Users/{identifier}/Parents")]
	public async Task<ActionResult<LDAPSearchResult>> GetParentsForUserIdentifier(
		[FromRoute] string serverProfile,
		[FromRoute] string catalogType,
		[FromRoute] string identifier,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalIdentifierAttributeBinder))] EntryAttribute? identifierAttribute,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalRequiredAttributesBinder))] RequiredEntryAttributes? requiredAttributes,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestLabel)
	{
		Logger.LogInformation("Request path: {serverProfileRoute}={serverProfile}, {catalogTypeRoute}={catalogType}, {identifierRoute}={identifier}, {identifierAttributeQuery}={identifierAttribute}, {requiredAttributesQuery}={requiredAttributes}, {requestLabelQuery}={requestLabel}", nameof(serverProfile), serverProfile, nameof(catalogType), catalogType, nameof(identifier), identifier, nameof(identifierAttribute), identifierAttribute, nameof(requiredAttributes), requiredAttributes, nameof(requestLabel), requestLabel);

		ValidateIdentifierAttribute(ref identifierAttribute);

		ValidateRequiredAttributes(ref requiredAttributes);

		var clientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

		var searcher = GetLdapSearcher(clientConfig);

		var onlyUsersFilter = AttributeFilterCombiner.CreateOnlyUsersFilterCombiner();
		var attributeFilter = new AttributeFilter(identifierAttribute.Value, new FilterValue(identifier));

		var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyUsersFilter, attributeFilter });

		var searchResult = await searcher.SearchParentEntriesAsync(searchFilter, requiredAttributes.Value, requestLabel);

		if (!searchResult.IsSuccessfulOperation)
		{
			if (searchResult.HasErrorObject)
			{
				Logger.LogError(searchResult.ErrorObject, "Failed to get LDAP parent entries for user account with {@identifierAttribute}={identifier}", identifierAttribute, identifier);

				throw searchResult.ErrorObject;
			}
			else
				throw new Exception(searchResult.OperationMessage);
		}

		Logger.LogInformation("Response body: {@searchResult}", searchResult);

		return Ok(searchResult);
	}

	/// <summary>
	/// Search user accounts according to the filters.
	/// </summary>
	/// <param name="serverProfile"></param>
	/// <param name="catalogType"></param>
	/// <param name="searchFilters"></param>
	/// <param name="requiredAttributes"></param>
	/// <param name="requestLabel"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	[HttpGet]
	[Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/Users/[action]")]
	[ActionName("filterBy")]
	public async Task<ActionResult<LDAPSearchResult>> GetUsersFilteringByAsync(
		[FromRoute] string serverProfile,
		[FromRoute] string catalogType,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalSearchFiltersBinder))] Models.OptionalSearchFiltersModel searchFilters,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalRequiredAttributesBinder))] RequiredEntryAttributes? requiredAttributes,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestLabel)
	{
		Logger.LogInformation($"Request path: {nameof(serverProfile)}={serverProfile}, {nameof(catalogType)}={catalogType}, {nameof(searchFilters.filterAttribute)}={searchFilters.filterAttribute}, {nameof(searchFilters.filterValue)}={searchFilters.filterValue}, {nameof(searchFilters.secondFilterAttribute)}={searchFilters.secondFilterAttribute}, {nameof(searchFilters.secondFilterValue)}={searchFilters.secondFilterValue},{nameof(searchFilters.combineFilters)}={searchFilters.combineFilters},{nameof(requiredAttributes)}={requiredAttributes}, {nameof(requestLabel)}={requestLabel}");

		ValidateRequiredAttributes(ref requiredAttributes);

		var clientConfiguration = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

		var searcher = GetLdapSearcher(clientConfiguration);

		LDAPSearchResult searchResult;
		if (searchFilters.secondFilterAttribute.HasValue)
		{
			searchFilters.combineFilters = ValidateCombineFiltersParameter(searchFilters);

			var onlyUsersFilter = AttributeFilterCombiner.CreateOnlyUsersFilterCombiner();

			var firstAttributeFilter = new AttributeFilter(searchFilters.filterAttribute.Value, new FilterValue(searchFilters.filterValue));
			var secondAttributeFilter = new AttributeFilter(searchFilters.secondFilterAttribute.Value, new FilterValue(searchFilters.secondFilterValue));
			var combinedFilters = new AttributeFilterCombiner(false, searchFilters.combineFilters.Value, new ICombinableFilter[] { firstAttributeFilter, secondAttributeFilter });

			var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyUsersFilter, combinedFilters });

			searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestLabel);
		}
		else
		{
			var onlyUsersFilter = AttributeFilterCombiner.CreateOnlyUsersFilterCombiner();

			var attributeFilter = new AttributeFilter(searchFilters.filterAttribute.Value, new FilterValue(searchFilters.filterValue));

			var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyUsersFilter, attributeFilter });

			searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestLabel);
		}

		if (!searchResult.IsSuccessfulOperation)
		{
			if (searchResult.HasErrorObject)
			{
				Logger.LogError(searchResult.ErrorObject, "Error searching for users with the following filter: {@searchFilters}", searchFilters);

				throw searchResult.ErrorObject;
			}
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
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestLabel)
	{
		Logger.LogInformation($"Request path: {nameof(serverProfile)}={serverProfile}, {nameof(catalogType)}={catalogType}, {nameof(identifier)}={identifier}, {nameof(identifierAttribute)}={identifierAttribute}, {nameof(requiredAttributes)}={requiredAttributes}, {nameof(requestLabel)}={requestLabel}");

		ValidateIdentifierAttribute(ref identifierAttribute);

		ValidateRequiredAttributes(ref requiredAttributes);

		var clientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

		var searcher = GetLdapSearcher(clientConfig);

		var onlyGroupsFilter = AttributeFilterCombiner.CreateOnlyGroupsFilterCombiner();

		var attributeFilter = new AttributeFilter(identifierAttribute!.Value, new FilterValue(identifier));

		var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyGroupsFilter, attributeFilter });

		var searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes!.Value, requestLabel);

		if (!searchResult.IsSuccessfulOperation)
		{
			if (searchResult.HasErrorObject)
			{
				Logger.LogError(searchResult.ErrorObject, "Failed to look up the LDAP entry of a group by its identifier: {@identifierAttribute}={identifier}", identifierAttribute, identifier);

				throw searchResult.ErrorObject;
			}
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
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestLabel)
	{
		Logger.LogInformation($"Request path: {nameof(serverProfile)}={serverProfile}, {nameof(catalogType)}={catalogType}, {nameof(identifier)}={identifier}, {nameof(identifierAttribute)}={identifierAttribute}, {nameof(requiredAttributes)}={requiredAttributes}, {nameof(requestLabel)}={requestLabel}");

		ValidateIdentifierAttribute(ref identifierAttribute);

		ValidateRequiredAttributes(ref requiredAttributes);

		var clientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

		var searcher = GetLdapSearcher(clientConfig);

		var onlyGroupsFilter = AttributeFilterCombiner.CreateOnlyGroupsFilterCombiner();

		var attributeFilter = new AttributeFilter(identifierAttribute.Value, new FilterValue(identifier));

		var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyGroupsFilter, attributeFilter });

		var searchResult = await searcher.SearchParentEntriesAsync(searchFilter, requiredAttributes.Value, requestLabel);

		if (!searchResult.IsSuccessfulOperation)
		{
			if (searchResult.HasErrorObject)
			{
				Logger.LogError(searchResult.ErrorObject, "Failed to get LDAP parent entries for user account with {@identifierAttribute}={identifier}", identifierAttribute, identifier);

				throw searchResult.ErrorObject;
			}
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
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestLabel
		)
	{
		Logger.LogInformation($"Request path: {nameof(serverProfile)}={serverProfile}, {nameof(catalogType)}={catalogType}, {nameof(searchFilters.filterAttribute)}={searchFilters.filterAttribute}, {nameof(searchFilters.filterValue)}={searchFilters.filterValue}, {nameof(searchFilters.secondFilterAttribute)}={searchFilters.secondFilterAttribute}, {nameof(searchFilters.secondFilterValue)}={searchFilters.secondFilterValue},{nameof(searchFilters.combineFilters)}={searchFilters.combineFilters},{nameof(requiredAttributes)}={requiredAttributes}, {nameof(requestLabel)}={requestLabel}");

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

			searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestLabel);
		}
		else
		{
			var onlyGroupsFilter = AttributeFilterCombiner.CreateOnlyGroupsFilterCombiner();

			var attributeFilter = new AttributeFilter(searchFilters.filterAttribute, new FilterValue(searchFilters.filterValue));

			var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyGroupsFilter, attributeFilter });

			searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestLabel);
		}

		if (!searchResult.IsSuccessfulOperation)
		{
			if (searchResult.HasErrorObject)
			{
				Logger.LogError(searchResult.ErrorObject, "Failed to get LDAP group entries by  {@searchFilters}", searchFilters);

				throw searchResult.ErrorObject;
			}
			else
				throw new Exception(searchResult.OperationMessage);
		}

		Logger.LogInformation("Search result count: {0}", searchResult.Entries.Count());

		return Ok(searchResult);
	}
}