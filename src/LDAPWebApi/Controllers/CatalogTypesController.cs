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
    //[Authorize]
    [ApiController]
    public class CatalogTypesController : ApiControllerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">Injected <see cref="Microsoft.Extensions.Configuration.IConfiguration"/></param>
        /// <param name="ldapServerProfiles">Injected <see cref="Configurations.LDAP. LDAPServerProfiles"/></param>
        /// <param name="ldapCatalogTypeRoutes">Injected <see cref="Configurations.LDAP.LDAPCatalogTypeRoutes"/></param>
        public CatalogTypesController(IConfiguration configuration, Configurations.LDAP.LDAPServerProfiles ldapServerProfiles, Configurations.LDAP.LDAPCatalogTypeRoutes ldapCatalogTypeRoutes) : base(configuration, ldapServerProfiles, ldapCatalogTypeRoutes)
        {
        }

        [Route("api/[controller]")]
        [HttpGet]
        public Task<DTO.LDAPCatalogTypes> GetAsync()
        {
            return Task.Run(() => new DTO.LDAPCatalogTypes { LocalCatalog = CatalogTypeRoutes.LocalCatalogRoute, GlobalCatalog = CatalogTypeRoutes.GlobalCatalogRoute });
        }
    }
}
