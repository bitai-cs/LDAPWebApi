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

namespace Bitai.LDAPWebApi.Controllers
{
    [Authorize(WebApiScopesConfiguration.GlobalScopeAuthorizationPolicyName)]
    [ApiController]
    public class ServerProfilesController : ApiControllerBase<ServerProfilesController>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">See <see cref="Microsoft.Extensions.Configuration.IConfiguration"/></param>
        /// <param name="logger">See <see cref="ILogger{TCategoryName}"/></param>
        /// <param name="ldapServerProfiles">Injected <see cref="Configurations.LDAP. LDAPServerProfiles"/></param>
        public ServerProfilesController(IConfiguration configuration, ILogger<ServerProfilesController> logger, Configurations.LDAP.LDAPServerProfiles ldapServerProfiles) : base(configuration, logger, ldapServerProfiles)
        {
        }



        [Route("api/[controller]/GetProfileIds")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetProfileIdsAsync()
        {
            var _return = await Task.Run(() =>
            {
                return ServerProfiles.Select(p => p.ProfileId);
            });

            return Ok(_return);
        }

        [Route("api/[controller]/{profileId}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DTO.LDAPServerProfile>>> GetByProfileIdAsync(string profileId)
        {
            var _return = await Task.Run(() =>
            {
                var _ldapServerProfile = ServerProfiles.Where(p => p.ProfileId.Equals(profileId, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();

                if (_ldapServerProfile == null)
                    return null;

                return new DTO.LDAPServerProfile
                {
                    ProfileId = _ldapServerProfile.ProfileId,
                    Server = _ldapServerProfile.Server,
                    Port = _ldapServerProfile.Port,
                    PortForGlobalCatalog = _ldapServerProfile.PortForGlobalCatalog,
                    BaseDN = _ldapServerProfile.BaseDN,
                    BaseDNforGlobalCatalog = _ldapServerProfile.BaseDNforGlobalCatalog,
                    ConnectionTimeout = _ldapServerProfile.ConnectionTimeout,
                    UseSSL = _ldapServerProfile.UseSSL,
                    UseSSLforGlobalCatalog = _ldapServerProfile.UseSSLforGlobalCatalog,
                    DomainAccountName = _ldapServerProfile.DomainAccountName,
                    DomainAccountPassword = string.Empty
                };
            });

            if (_return == null)
                return NotFound();
            else
                return Ok(_return);
        }

        [Route("api/[controller]")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DTO.LDAPServerProfile>>> GetAsync()
        {
            var _return = await Task.Run(() =>
            {
                return ServerProfiles.Select(p => new DTO.LDAPServerProfile
                {
                    ProfileId = p.ProfileId,
                    Server = p.Server,
                    Port = p.Port,
                    PortForGlobalCatalog = p.PortForGlobalCatalog,
                    BaseDN = p.BaseDN,
                    BaseDNforGlobalCatalog = p.BaseDNforGlobalCatalog,
                    ConnectionTimeout = p.ConnectionTimeout,
                    UseSSL = p.UseSSL,
                    UseSSLforGlobalCatalog = p.UseSSLforGlobalCatalog,
                    DomainAccountName = p.DomainAccountName,
                    DomainAccountPassword = string.Empty
                });
            });

            return Ok(_return);
        }
    }
}
