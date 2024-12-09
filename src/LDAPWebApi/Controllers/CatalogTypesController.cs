using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bitai.LDAPWebApi.Configurations.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
	public CatalogTypesController(IConfiguration configuration, ILogger<CatalogTypesController> logger, Configurations.LDAP.LDAPServerProfiles serverProfiles) : base(configuration, logger, serverProfiles)
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
