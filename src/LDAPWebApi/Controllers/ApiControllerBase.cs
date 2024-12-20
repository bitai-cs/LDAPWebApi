﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bitai.LDAPHelper.DTO;
using Bitai.LDAPHelper.QueryFilters;
using Bitai.WebApi.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bitai.LDAPWebApi.Controllers;

/// <summary>
/// Base Web Controller Api that defines the basic behavior of any controller.
/// </summary>
public abstract class ApiControllerBase<T> : ControllerBase
{
	/// <summary>
	/// <see cref="IConfiguration"/>    
	/// </summary>
	protected IConfiguration Configuration { get; }

	/// <summary>
	/// Logger
	/// </summary>
	protected ILogger<T> Logger { get; }

	/// <summary>
	/// List of <see cref="Configurations.LDAP.LDAPServerProfile"/>
	/// </summary>
	protected Configurations.LDAP.LDAPServerProfiles ServerProfiles { get; }

	/// <summary>
	/// Route name for each LDAP Catalog.
	/// </summary>
	protected DTO.LDAPServerCatalogTypes CatalogTypeRoutes => new DTO.LDAPServerCatalogTypes();




	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="configuration">Injected <see cref="IConfiguration"/></param>
	/// <param name="logger">Logger</param>
	/// <param name="serverProfiles">Injected <see cref="Configurations.LDAP.LDAPServerProfiles"/></param>
	protected ApiControllerBase(IConfiguration configuration, ILogger<T> logger, Configurations.LDAP.LDAPServerProfiles serverProfiles)
	{
		Configuration = configuration;
		Logger = logger;
		ServerProfiles = serverProfiles;
	}




	/// <summary>
	/// Get an instance of <see cref="LDAPHelper.ClientConfiguration"/>
	/// </summary>
	/// <param name="serverProfile">LDAP server profile ID.</param>
	/// <param name="useGlobalCatalog">Use or not the Global LDAP Catalog.</param>
	/// <returns>See <see cref="LDAPHelper.ClientConfiguration"/></returns>
	protected LDAPHelper.ClientConfiguration GetLdapClientConfiguration(string serverProfile, bool useGlobalCatalog)
	{
		return GetLdapClientConfiguration(serverProfile, useGlobalCatalog, out var ldapServerProfile);
	}

	/// <summary>
	/// Get an instance of <see cref="LDAPHelper.ClientConfiguration"/>
	/// </summary>
	/// <param name="serverProfile">LDAP server profile ID.</param>
	/// <param name="useGlobalCatalog">Use or not the Global LDAP Catalog.</param>
	/// <param name="ldapServerProfile">See <see cref="Configurations.LDAP.LDAPServerProfile"/></param>
	/// <returns>See <see cref="LDAPHelper.ClientConfiguration"/></returns>
	protected LDAPHelper.ClientConfiguration GetLdapClientConfiguration(string serverProfile, bool useGlobalCatalog, out Configurations.LDAP.LDAPServerProfile ldapServerProfile)
	{
		if (string.IsNullOrEmpty(serverProfile))
			throw new ArgumentNullException(nameof(serverProfile));

		ldapServerProfile = this.ServerProfiles.Where(p => p.ProfileId.Equals(serverProfile, StringComparison.OrdinalIgnoreCase)).Single();

		var connectionInfo = new LDAPHelper.ConnectionInfo(ldapServerProfile.Server, ldapServerProfile.GetPort(useGlobalCatalog), ldapServerProfile.GetUseSsl(useGlobalCatalog), ldapServerProfile.ConnectionTimeout);

		var domainUserAccount = ldapServerProfile.DomainUserAccount;
		if (!domainUserAccount.Contains('\\'))
			domainUserAccount = $"{ldapServerProfile.DefaultDomainName}\\{domainUserAccount}";

		var accountParts = domainUserAccount.Split("\\");
		var credential = new LDAPHelper.DTO.LDAPDomainAccountCredential(accountParts[0], accountParts[1], ldapServerProfile.DomainAccountPassword);

		var searchLimits = new LDAPHelper.SearchLimits(ldapServerProfile.GetBaseDN(useGlobalCatalog));

		return new LDAPHelper.ClientConfiguration(connectionInfo, credential, searchLimits);
	}

	/// <summary>
	/// Get an instance of <see cref="LDAPHelper.Searcher"/>
	/// </summary>
	/// <param name="clientConfiguration"><see cref="LDAPHelper.ClientConfiguration"/> to be used by <see cref="LDAPHelper.Searcher"/></param>
	/// <returns></returns>
	protected LDAPHelper.Searcher GetLdapSearcher(LDAPHelper.ClientConfiguration clientConfiguration)
	{
		return new LDAPHelper.Searcher(clientConfiguration);
	}

	/// <summary>
	/// Check if the name of the catalog type is the 
	/// name of the global catalog.
	/// </summary>
	/// <param name="ldapCatalogType">Name of Catalog type.</param>
	/// <returns></returns>
	protected bool IsGlobalCatalog(string ldapCatalogType)
	{
		if (CatalogTypeRoutes.GlobalCatalog.Equals(ldapCatalogType, StringComparison.OrdinalIgnoreCase))
			return true;

		if (CatalogTypeRoutes.LocalCatalog.Equals(ldapCatalogType, StringComparison.OrdinalIgnoreCase))
			return false;

		throw new Exception($"LDAP Catalog type '{ldapCatalogType}' not found.");
	}

	/// <summary>
	/// Validates if the <see cref="Binders.OptionalIdentifierAttributeBinder"/> was able to identify and/or assign a value to <paramref name="identifierAttribute"/>.
	/// </summary>
	/// <param name="identifierAttribute">Variable to be evaluated.</param>
	/// <exception cref="InvalidOperationException">When <paramref name="identifierAttribute"/> has no value.</exception>
	protected void ValidateIdentifierAttribute([NotNull] ref LDAPHelper.DTO.EntryAttribute? identifierAttribute)
	{
		if (!identifierAttribute.HasValue)
			throw new InvalidOperationException($"{typeof(Binders.OptionalIdentifierAttributeBinder).FullName} could not identify / set {nameof(identifierAttribute)} parameter.");
	}

	/// <summary>
	/// Validates if the <see cref="Binders.OptionalRequiredAttributesBinder"/> was able to identify and/or assign a value to <paramref name="requiredAttributes"/>.
	/// </summary>
	/// <param name="requiredAttributes">Variable to be evaluated.</param>
	/// <exception cref="InvalidOperationException">When <paramref name="requiredAttributes"/> has no value.</exception>
	protected void ValidateRequiredAttributes([NotNull] ref LDAPHelper.DTO.RequiredEntryAttributes? requiredAttributes)
	{
		if (!requiredAttributes.HasValue)
			throw new InvalidOperationException($"{typeof(Binders.OptionalRequiredAttributesBinder).FullName} could not identify / set {nameof(requiredAttributes)} parameter.");
	}

	/// <summary>
	/// Validates if the <see cref="Binders.SearchFiltersBinder"/> was able to initialize  <paramref name="searchFilters"/> variable and then return the value of <see cref="Models.SearchFiltersModel.combineFilters"/> property.
	/// </summary>
	/// <param name="searchFilters">Variable to be evaluated.</param>
	/// <returns>Value of <see cref="Models.SearchFiltersModel.combineFilters"/> property of the <paramref name="searchFilters"/> parameter.</returns>
	/// <exception cref="InvalidOperationException">When <paramref name="searchFilters"/> was not correctly initialized.</exception>
	protected bool ValidateCombineFiltersParameter(Models.SearchFiltersModel searchFilters)
	{
		if (!searchFilters.combineFilters.HasValue)
			throw new InvalidOperationException($"{typeof(Binders.SearchFiltersBinder).FullName} failed to identify search filter parameters that are in the URL query string. {typeof(Binders.SearchFiltersBinder).FullName} could not initialize correctly {typeof(Models.SearchFiltersModel)}.");

		return searchFilters.combineFilters.Value;
	}

	/// <summary>
	/// Validates if the <see cref="Binders.OptionalSearchFiltersBinder"/> was able to initialize  <paramref name="searchFilters"/> variable and then return the value of <see cref="Models.OptionalSearchFiltersModel.combineFilters"/> property.
	/// </summary>
	/// <param name="searchFilters">Variable to be evaluated.</param>
	/// <returns>Value of <see cref="Models.OptionalSearchFiltersModel.combineFilters"/> property of the <paramref name="searchFilters"/> parameter.</returns>
	/// <exception cref="InvalidOperationException">When <paramref name="searchFilters"/> was not correctly initialized.</exception>
	protected bool ValidateCombineFiltersParameter(Models.OptionalSearchFiltersModel searchFilters)
	{
		if (!searchFilters.combineFilters.HasValue)
			throw new InvalidOperationException($"{typeof(Binders.OptionalSearchFiltersBinder).FullName} failed to identify search filter parameters that are in the URL query string. {typeof(Binders.OptionalSearchFiltersBinder).FullName} could not initialize correctly {typeof(Models.OptionalSearchFiltersModel)}.");

		return searchFilters.combineFilters.Value;
	}

	/// <summary>
	/// Search for a user account.
	/// </summary>
	/// <param name="clientConfiguration"></param>
	/// <param name="userAccountIdentifier"></param>
	/// <param name="userAccountIdentifierAttribute"></param>
	/// <param name="requestLabel"></param>
	/// <returns></returns>
	protected Task<LDAPSearchResult> SearchUserAccountAsync(LDAPHelper.ClientConfiguration clientConfiguration, string userAccountIdentifier, EntryAttribute userAccountIdentifierAttribute, string requestLabel)
	{
		var searcher = GetLdapSearcher(clientConfiguration);
		var searchFilter = new AttributeFilterCombiner(false, true, new ICombinableFilter[] { AttributeFilterCombiner.CreateOnlyUsersFilterCombiner(), new AttributeFilter(userAccountIdentifierAttribute, new FilterValue(userAccountIdentifier)) });

		return searcher.SearchEntriesAsync(searchFilter, RequiredEntryAttributes.Minimun, requestLabel);
	}

	/// <summary>
	/// Throw an error according to the response of an unsuccessful operation.
	/// </summary>
	/// <param name="exceptionMessage"></param>
	/// <param name="unsuccessfulOperation"></param>
	/// <exception cref="Exception"></exception>
	protected void ThrowExceptionForUnsuccessfulOperation(string exceptionMessage, LDAPOperationResult unsuccessfulOperation)
	{
		if (unsuccessfulOperation.HasErrorObject)
			throw new Exception(exceptionMessage, unsuccessfulOperation.ErrorObject);
		else
			throw new Exception($"{exceptionMessage}. {unsuccessfulOperation.OperationMessage}");
	}
}