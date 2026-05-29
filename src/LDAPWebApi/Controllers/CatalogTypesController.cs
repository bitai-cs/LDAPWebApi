using Bitai.LDAPHelper.LdapAdapters;
using Bitai.LDAPWebApi.Configurations.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bitai.LDAPWebApi.Controllers;

/// <summary>
/// Web Api controller to handle LDAP Catalog names 
/// </summary>
[ApiController]
public class CatalogTypesController : ApiControllerBase<CatalogTypesController>
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configuration">Injected <see cref="IConfiguration"/></param>
    /// <param name="logger">See <see cref="ILogger{TCategoryName}"/></param>
    /// <param name="serverProfiles">Injected <see cref="Configurations.LDAP. LDAPServerProfiles"/></param>
    /// <param name="connectionFactory">Injected <see cref="ILdapConnectionFactoryAdapter"/></param>
    public CatalogTypesController(IConfiguration configuration, ILogger<CatalogTypesController> logger, Configurations.LDAP.LDAPServerProfiles serverProfiles, ILdapConnectionFactoryAdapter connectionFactory) : base(configuration, logger, serverProfiles, connectionFactory)
	{
	}



	/// <summary>
	/// Get defined LDAP Catalog names
	/// </summary>
	/// <returns><see cref="DTO.LDAPServerCatalogTypes"/></returns>
	[Authorize(WebApiScopesConfiguration.AuthorizationPolicyForAnyApiScopeName)]
	[Route("api/[controller]")]
	[HttpGet]
	public Task<DTO.LDAPServerCatalogTypes> GetAsync()
	{
		return Task.FromResult(CatalogTypeRoutes);
	}
}
