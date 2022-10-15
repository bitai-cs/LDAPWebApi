using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
	protected DTO.LDAPCatalogTypes CatalogTypeRoutes => new DTO.LDAPCatalogTypes();




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
	/// <returns></returns>
	protected LDAPHelper.ClientConfiguration GetLdapClientConfiguration(string serverProfile, bool useGlobalCatalog)
	{
		if (string.IsNullOrEmpty(serverProfile))
			throw new ArgumentNullException(nameof(serverProfile));

		var ldapServerProfile = this.ServerProfiles.Where(p => p.ProfileId.Equals(serverProfile, StringComparison.OrdinalIgnoreCase)).Single();

		var connectionInfo = new LDAPHelper.ConnectionInfo(ldapServerProfile.Server, ldapServerProfile.GetPort(useGlobalCatalog), ldapServerProfile.GetUseSsl(useGlobalCatalog), ldapServerProfile.ConnectionTimeout);

		var accountParts = ldapServerProfile.DomainUserAccount.Split("\\");
		var credentials = new LDAPHelper.DTO.LDAPDomainAccountCredential(accountParts[0], accountParts[1], ldapServerProfile.DomainAccountPassword);

		var searchLimits = new LDAPHelper.SearchLimits(ldapServerProfile.GetBaseDN(useGlobalCatalog));

		return new LDAPHelper.ClientConfiguration(connectionInfo, credentials, searchLimits);
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

	protected void ValidateIdentifierAttribute([NotNull] ref LDAPHelper.DTO.EntryAttribute? identifierAttribute)
	{
		if (!identifierAttribute.HasValue)
			throw new InvalidOperationException($"{typeof(Binders.OptionalIdentifierAttributeBinder).FullName} could not identify / set {nameof(identifierAttribute)} parameter.");
	}

	protected void ValidateRequiredAttributes([NotNull] ref LDAPHelper.DTO.RequiredEntryAttributes? requiredAttributes)
	{
		if (!requiredAttributes.HasValue)
			throw new InvalidOperationException($"{typeof(Binders.OptionalRequiredAttributesBinder).FullName} could not identify / set {nameof(requiredAttributes)} parameter.");
	}

	protected bool ValidateCombineFiltersParameter(Models.SearchFiltersModel searchFilters)
	{
		if (!searchFilters.combineFilters.HasValue)
			throw new InvalidOperationException($"Failed to identify search filter parameters that are in the URL query string. {typeof(Binders.SearchFiltersBinder).FullName} could not initialize correctly {typeof(Models.SearchFiltersModel)}.");

		return searchFilters.combineFilters.Value;
	}
}
