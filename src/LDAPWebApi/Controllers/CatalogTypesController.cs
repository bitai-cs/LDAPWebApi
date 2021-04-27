using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Bitai.LDAPWebApi.Controllers
{
    /// <summary>
    /// Web Api controller to handle LDAP Catalog names 
    /// </summary>
    [Authorize]
    [ApiController]
    public class CatalogTypesController : ApiControllerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">Injected <see cref="IConfiguration"/></param>
        /// <param name="ldapServerProfiles">Injected <see cref="Configurations.LDAP. LDAPServerProfiles"/></param>        
        public CatalogTypesController(IConfiguration configuration, Configurations.LDAP.LDAPServerProfiles ldapServerProfiles) : base(configuration, ldapServerProfiles)
        {
        }



        /// <summary>
        /// Get defined LDAP Catalog names
        /// </summary>
        /// <returns><see cref="DTO.LDAPCatalogTypes"/></returns>
        [Route("api/[controller]")]
        [HttpGet]
        public Task<DTO.LDAPCatalogTypes> GetAsync()
        {
            return Task.FromResult(CatalogTypeRoutes);
        }
    }
}
