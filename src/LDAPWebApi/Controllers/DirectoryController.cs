using System.Data;
using Bitai.LDAPHelper;
using Bitai.LDAPHelper.DTO;
using Bitai.LDAPHelper.LdapAdapters;
using Bitai.LDAPHelper.QueryFilters;
using Bitai.LDAPWebApi.Configurations.App;
using Bitai.LDAPWebApi.DTO;
using Bitai.WebApi.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novell.Directory.Ldap;

namespace Bitai.LDAPWebApi.Controllers;


/// <summary>
/// Controller that handles requests to the LDAP directory, it's the main entry point for directory operations.
/// </summary>
[Route("api")]
[ApiController]
public class DirectoryController : ApiControllerBase<DirectoryController>
{
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="configuration">Injected <see cref="IConfiguration"/></param>
	/// <param name="logger">Logger</param>
	/// <param name="serverProfiles">Injected <see cref="Configurations.LDAP.LDAPServerProfiles"/></param>
	/// <param name="connectionFactory">Injected <see cref="ILdapConnectionFactoryAdapter"/></param>
	public DirectoryController(IConfiguration configuration, ILogger<DirectoryController> logger, Configurations.LDAP.LDAPServerProfiles serverProfiles, ILdapConnectionFactoryAdapter connectionFactory) : base(configuration, logger, serverProfiles, connectionFactory)
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
	[Authorize(WebApiScopesConfiguration.AuthorizationPolicyForAnyApiScopeName)]
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
                if (searchResult.ErrorObject is LdapException)
                {
                    var unwrappedLdapException = (LdapException)searchResult.ErrorObject;

                    throw new LdapException(searchResult.OperationMessage, unwrappedLdapException.ResultCode, unwrappedLdapException.LdapErrorMessage, unwrappedLdapException);
                }
                else
                    throw new Exception(searchResult.OperationMessage, searchResult.ErrorObject);
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
    /// <exception cref="LdapException">When an LDAP error interrup a <see cref="DirectoryController"/> operation.</exception>
    /// <exception cref="Exception">When an <see cref="DirectoryController"/> operation does not complete successfully.</exception>
    [Authorize(WebApiScopesConfiguration.AuthorizationPolicyForAnyApiScopeName)]
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
                if (searchResult.ErrorObject is LdapException)
                {
                    var unwrappedLdapException = (LdapException)searchResult.ErrorObject;

                    throw new LdapException(searchResult.OperationMessage, unwrappedLdapException.ResultCode, unwrappedLdapException.LdapErrorMessage, unwrappedLdapException);
                }
                else
                    throw new Exception(searchResult.OperationMessage, searchResult.ErrorObject);
            }
            else
                throw new Exception(searchResult.OperationMessage);
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
	[Authorize(WebApiScopesConfiguration.AuthorizationPolicyForAdminApiScopeName)]
	[HttpPost]
	[Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/MsADUsers")]
	public async Task<ActionResult<LDAPCreateMsADUserAccountResult>> CreateMsADUserAccount(
		[FromRoute] string serverProfile,
		[FromRoute] string catalogType,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestLabel,
		[FromBody] LDAPMsADUserAccount newUserAccount)
	{
		Logger.LogInformation("Request path: {@spn}={@sp}, {@ctn}={@ct}, {@rtn}={@rt}, {@nuan}={@nua}", nameof(serverProfile), serverProfile, nameof(catalogType), catalogType, nameof(requestLabel), requestLabel, nameof(newUserAccount), newUserAccount);

		if (!catalogType.Equals(CatalogTypeRoutes.LocalCatalog, StringComparison.OrdinalIgnoreCase))
			throw new BadRequestException("Cannot create user accounts in the global catalog of the LDAP server.");

		var clientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

		var accountManager = GetAccountManager(clientConfig);
		accountManager.InitializeMissingMsADUserAccountDN(newUserAccount);

		var createUserAccountResult = await accountManager.CreateUserAccountForMsAD(newUserAccount, requestLabel);
		if (!createUserAccountResult.IsSuccessfulOperation)
		{
			if (createUserAccountResult.HasErrorObject)
			{
                if (createUserAccountResult.ErrorObject is DataValidationException)
                    throw new BadRequestException(createUserAccountResult.OperationMessage, createUserAccountResult.ErrorObject);
                if (createUserAccountResult.ErrorObject is DuplicateNameException)
                    throw new ConflictException(createUserAccountResult.OperationMessage, createUserAccountResult.ErrorObject);
                else
				    throw new Exception(createUserAccountResult.OperationMessage,  createUserAccountResult.ErrorObject);
			}
			else
				throw new Exception(createUserAccountResult.OperationMessage);
		}

		Logger.LogInformation("Response body: {@createUserAccountResult}", createUserAccountResult);

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
	[Authorize(WebApiScopesConfiguration.AuthorizationPolicyForAdminApiScopeName)]
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
				throw new BadRequestException($"The {strings[0]} domain of the usename {strings[1]} must be the same as the {serverProfile} domain specified in the API route.");

			if (!strings[1].Equals(identifier, StringComparison.OrdinalIgnoreCase))
				throw new BadRequestException($"The usernme {strings[1]} must be the same as the {identifier} identifier specified in the API route.");

			domainName = strings[0];
			userAccount = strings[1];
		}
		else
		{
            if (!credential.UserAccount.Equals(identifier, StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException($"The user account {credential.UserAccount} must be the same as the {identifier} identifier specified in the API route.");

            domainName = serverProfileObject.DefaultDomainName;
			userAccount = credential.UserAccount;
		}

		//var clientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));
		//var searcher = GetLdapSearcher(clientConfig);
		//var onlyUsersFilter = AttributeFilterCombiner.CreateOnlyUsersFilterCombiner();
		//var attributeFilter = new AttributeFilter(identifierAttribute.Value, new FilterValue(identifier));
		//var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyUsersFilter, attributeFilter });
		//var searchResult = await searcher.SearchEntriesAsync(searchFilter, RequiredEntryAttributes.Minimun, requestLabel);
		//if (!searchResult.IsSuccessfulOperation)
		//{
		//	if (searchResult.HasErrorObject)
		//	{
		//		Logger.LogError(searchResult.ErrorObject, "Failed to search for a domain user account based on search filter {@searchFilter}", searchFilter);

		//		throw searchResult.ErrorObject;
		//	}
		//	else
		//		throw new Exception(searchResult.OperationMessage);
		//}

		//if (searchResult.Entries.Count() == 0)
		//	throw new ResourceNotFoundException($"The user accoun with identifier {attributeFilter} was not found");

		//if (searchResult.Entries.Count() > 1)
		//	throw new BadRequestException($"More than one LDAP entry was obtained for the supplied identifier {attributeFilter}.");

		//var entry = searchResult.Entries.Single();

		//var dnCredential = new LDAPDistinguishedNameCredential(entry.distinguishedName, credential.Password);
		var accountManager = GetAccountManager(GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType)));
		var pwdUpdateResult = await accountManager.SetMsADUserAccountPassword(identifierAttribute.Value, userAccount, credential.Password, requestLabel);

		if (!pwdUpdateResult.IsSuccessfulOperation)
		{
            if (pwdUpdateResult.HasErrorObject)
            {
                if (pwdUpdateResult.ErrorObject is EntryNotFoundException)
                    throw new ResourceNotFoundException(pwdUpdateResult.OperationMessage, pwdUpdateResult.ErrorObject);
                else if (pwdUpdateResult.ErrorObject is DataValidationException)
                    throw new BadRequestException(pwdUpdateResult.OperationMessage, pwdUpdateResult.ErrorObject);
                else
                    throw new Exception(pwdUpdateResult.OperationMessage, pwdUpdateResult.ErrorObject);
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
	[Authorize(WebApiScopesConfiguration.AuthorizationPolicyForAdminApiScopeName)]
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
			throw new BadRequestException("User account deletion from the LDAP server’s global catalog is not permitted. This operation is only allowed in the LDAP server’s local catalog.");

		var clientConfig = GetLdapClientConfiguration(serverProfile, IsGlobalCatalog(catalogType));

		var accountManager = GetAccountManager(clientConfig);
		var disableResult = await accountManager.DisableMsADUserAccount(identifierAttribute.Value, identifier, requestLabel);
		if (!disableResult.IsSuccessfulOperation)
        {
            if (disableResult.HasErrorObject)
            {
                if (disableResult.ErrorObject is EntryNotFoundException)
                    throw new ResourceNotFoundException(disableResult.OperationMessage, disableResult.ErrorObject);
                else if (disableResult.ErrorObject is DataValidationException)
                    throw new BadRequestException(disableResult.OperationMessage, disableResult.ErrorObject);
                else
                    throw new Exception(disableResult.OperationMessage, disableResult.ErrorObject);
            }
            else
                throw new Exception(disableResult.OperationMessage);
        }
		//ThrowExceptionForUnsuccessfulOperation($"Failed to disable user account {identifier}.", disableResult);

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
	[Authorize(WebApiScopesConfiguration.AuthorizationPolicyForAdminApiScopeName)]
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

		var accountManager = GetAccountManager(clientConfig);
		var removeResult = await accountManager.RemoveMsADUserAccount(identifierAttribute.Value, identifier, requestLabel);
		if (!removeResult.IsSuccessfulOperation)
		{
			if (removeResult.HasErrorObject)
			{
                if (removeResult.ErrorObject is EntryNotFoundException)
                    throw new ResourceNotFoundException(removeResult.OperationMessage, removeResult.ErrorObject);
                else if (removeResult.ErrorObject is DataValidationException)
                    throw new BadRequestException(removeResult.OperationMessage, removeResult.ErrorObject);
                else
                    throw new Exception(removeResult.OperationMessage, removeResult.ErrorObject);
			}
			else
				throw new Exception(removeResult.OperationMessage);
		}

		Logger.LogInformation("Response body: {@removeResult}", removeResult);

		return Ok(removeResult);
	}

	/// <summary>
	/// Gets the list of parents for the specified user account in the LDAP server
	/// identified by the given <paramref name="serverProfile"/> and <paramref name="catalogType"/>.
	/// </summary>
	/// <param name="serverProfile">The LDAP profile identifier that defines the route for this endpoint.</param>
	/// <param name="catalogType">The LDAP catalog type name that defines the route for this endpoint. See <see cref="DTO.LDAPServerCatalogTypes"/>.</param>
	/// <param name="identifier">The identifier of the user account that will define the route of this endpoint. The value must be a valid for the LDAP attribute <see cref="EntryAttribute.sAMAccountName"/> or <see cref="EntryAttribute.distinguishedName"/>.</param>
	/// <param name="identifierAttribute">The attribute (<see cref="EntryAttribute.sAMAccountName"/> or <see cref="EntryAttribute.distinguishedName"/>) that will validate the <paramref name="identifier"/> parameter. This parameter is optional.</param>
	/// <param name="requiredAttributes">The list of LDAP attributes that will be included in the search result. This parameter is optional.</param>
	/// <param name="requestLabel">The custom tag that identifies the request and marks the data returned in the response. This parameter is optional.</param>
	/// <returns>A <see cref="LDAPSearchResult"/> object that represents the result of the operation.</returns>
	[Authorize(WebApiScopesConfiguration.AuthorizationPolicyForAnyApiScopeName)]
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

                if (searchResult.ErrorObject is EntryNotFoundException)
                    throw new ResourceNotFoundException($"{searchResult.OperationMessage} No user entry was found for the specified identifier {identifierAttribute}={identifier}.", searchResult.ErrorObject);
                else if (searchResult.ErrorObject is LdapException)
                {
                    var unwrappedLdapException = (LdapException)searchResult.ErrorObject;

                    throw new LdapException(searchResult.OperationMessage, unwrappedLdapException.ResultCode, unwrappedLdapException.LdapErrorMessage, unwrappedLdapException);
                }
                else
                    throw new Exception(searchResult.OperationMessage, searchResult.ErrorObject);
			}
			else
				throw new Exception(searchResult.OperationMessage);
		}

		Logger.LogInformation("Response body: {@searchResult}", searchResult);

		return Ok(searchResult);
	}

	/// <summary>
	/// Gets the list of LDAP users filtered by the specified search filters.
	/// </summary>
	/// <param name="serverProfile">The LDAP profile identifier that defines the route for this endpoint.</param>
	/// <param name="catalogType">The LDAP catalog type name that defines the route for this endpoint. See <see cref="DTO.LDAPServerCatalogTypes"/>.</param>
	/// <param name="searchFilters">The search filters that will be used to filter the LDAP users. This parameter is optional.</param>
	/// <param name="requiredAttributes">The list of LDAP attributes that will be included in the search result. This parameter is optional.</param>
	/// <param name="requestLabel">The custom tag that identifies the request and marks the data returned in the response. This parameter is optional.</param>
	/// <returns>A <see cref="LDAPSearchResult"/> object that represents the result of the operation.</returns>
	[Authorize(WebApiScopesConfiguration.AuthorizationPolicyForAnyApiScopeName)]
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

		LDAPSearchResult? searchResult = null;
		if (searchFilters.filterAttribute.HasValue && searchFilters.secondFilterAttribute.HasValue)
		{
			searchFilters.combineFilters = ValidateCombineFiltersParameter(searchFilters);

			var onlyUsersFilter = AttributeFilterCombiner.CreateOnlyUsersFilterCombiner();

			var firstAttributeFilter = new AttributeFilter(searchFilters.filterAttribute.Value, new FilterValue(searchFilters.filterValue));
			var secondAttributeFilter = new AttributeFilter(searchFilters.secondFilterAttribute.Value, new FilterValue(searchFilters.secondFilterValue));
			var combinedFilters = new AttributeFilterCombiner(false, searchFilters.combineFilters.Value, new ICombinableFilter[] { firstAttributeFilter, secondAttributeFilter });

			var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyUsersFilter, combinedFilters });

			searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestLabel);
		}
		else if (searchFilters.filterAttribute.HasValue)
		{
			var onlyUsersFilter = AttributeFilterCombiner.CreateOnlyUsersFilterCombiner();

			var attributeFilter = new AttributeFilter(searchFilters.filterAttribute.Value, new FilterValue(searchFilters.filterValue));

			var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { onlyUsersFilter, attributeFilter });

			searchResult = await searcher.SearchEntriesAsync(searchFilter, requiredAttributes.Value, requestLabel);
		}

		if (searchResult == null)
			throw new Exception("The search could not be performed or the result obtained.");

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

	/// <summary>
	/// Get a group by its identifier.
	/// </summary>
	/// <param name="serverProfile">LDAP Server profile Id.</param>
	/// <param name="catalogType">LDAP Server catalog type.</param>
	/// <param name="identifier">Group identifier value.</param>
	/// <param name="identifierAttribute">The attribute of the entry by which it will be identified. If no value is assigned, the <see cref="EntryAttribute.distinguishedName"/> attribute is assumed by default. This is an optional query string parameter.</param>
	/// <param name="requiredAttributes">Type of LDAP attribute set that the response should return. If no value is assigned, <see cref="RequiredEntryAttributes.Few"/> is assumed by default. This is an optional query string parameter.</param>
	/// <param name="requestLabel"></param>
	/// <returns><see cref="LDAPSearchResult"/> that encapsulates the group entry.</returns>
	[Authorize(WebApiScopesConfiguration.AuthorizationPolicyForAnyApiScopeName)]
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

	/// <summary>
	/// Get the parent groups of a group from the LDAP server for the <paramref name="identifier"/> value and the <paramref name="identifierAttribute"/> attribute.
	/// </summary>
	/// <param name="serverProfile">LDAP Profile Id that defines part of the route.</param>
	/// <param name="catalogType">LDAP Catalog Type name that defines part of the route. See <see cref="DTO.LDAPServerCatalogTypes"/>.</param>
	/// <param name="identifier">Identifier of the group account that will define the route of this Endpoint. There must be a valid value for the LDAP attributes <see cref="EntryAttribute.sAMAccountName"/> or <see cref="EntryAttribute.distinguishedName"/>.</param>
	/// <param name="identifierAttribute">Attribute (<see cref="EntryAttribute.sAMAccountName"/> or <see cref="EntryAttribute.distinguishedName"/>) that will validate the <paramref name="identifier"/> parameter.</param>
	/// <param name="requiredAttributes">Type of LDAP attribute set that the response should return. If no value is assigned, <see cref="RequiredEntryAttributes.Few"/> is assumed by default. This is an optional query string parameter.</param>
	/// <param name="requestLabel">Optional and custom label</param>
	/// <returns>A <see cref="LDAPSearchResult"/> object that represents the result of the operation.</returns>
	[Authorize(WebApiScopesConfiguration.AuthorizationPolicyForAnyApiScopeName)]
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

                if (searchResult.ErrorObject is EntryNotFoundException)
                    throw new ResourceNotFoundException($"{searchResult.OperationMessage} The group entry with identifier {identifierAttribute}: {identifier} was not found.", searchResult.ErrorObject);
                else if (searchResult.ErrorObject is LdapException)
                {
                    var unwrappedLdapException = (LdapException)searchResult.ErrorObject;

                    throw new LdapException(searchResult.OperationMessage, unwrappedLdapException.ResultCode, unwrappedLdapException.LdapErrorMessage, unwrappedLdapException);
                }
                else
                    throw new Exception(searchResult.OperationMessage, searchResult.ErrorObject);
            }
            else
            {
                throw new Exception(searchResult.OperationMessage);
            }
		}

		Logger.LogInformation("Response body: {@result}", searchResult);

		return Ok(searchResult);
	}

	/// <summary>
	/// Gets the list of LDAP groups filtered by the specified search filters.
	/// </summary>
	/// <param name="serverProfile">The LDAP profile identifier that defines the route for this endpoint.</param>
	/// <param name="catalogType">The LDAP catalog type name that defines the route for this endpoint. See <see cref="DTO.LDAPServerCatalogTypes"/>.</param>
	/// <param name="searchFilters">The search filters that will be used to filter the LDAP groups. This parameter is optional.</param>
	/// <param name="requiredAttributes">The list of LDAP attributes that will be included in the search result. This parameter is optional.</param>
	/// <param name="requestLabel">The custom tag that identifies the request and marks the data returned in the response. This parameter is optional.</param>
	/// <returns>A <see cref="LDAPSearchResult"/> object that represents the result of the operation.</returns>
	[Authorize(WebApiScopesConfiguration.AuthorizationPolicyForAnyApiScopeName)]
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
