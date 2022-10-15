﻿using System;
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

[Authorize(WebApiScopesConfiguration.GlobalScopeAuthorizationPolicyName)]
[ApiController]
public class ServerProfilesController : ApiControllerBase<ServerProfilesController>
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configuration">See <see cref="Microsoft.Extensions.Configuration.IConfiguration"/></param>
    /// <param name="logger">See <see cref="ILogger{TCategoryName}"/></param>
    /// <param name="serverProfiles">Injected <see cref="Configurations.LDAP. LDAPServerProfiles"/></param>
    public ServerProfilesController(IConfiguration configuration, ILogger<ServerProfilesController> logger, Configurations.LDAP.LDAPServerProfiles serverProfiles) : base(configuration, logger, serverProfiles)
    {
    }



    [Route("api/[controller]/GetProfileIds")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> GetProfileIdsAsync()
    {
        var serverProfileIds = await Task.Run(() =>
        {
            return ServerProfiles.Select(p => p.ProfileId);
        });

        return Ok(serverProfileIds);
    }

    [Route("api/[controller]/{profileId}")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DTO.LDAPServerProfile>>> GetByProfileIdAsync(string profileId)
    {
        var dto = await Task.Run(() =>
        {
            var serverProfile = ServerProfiles.Where(p => p.ProfileId.Equals(profileId, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();

            if (serverProfile == null)
                return null;

            return new DTO.LDAPServerProfile
            {
                ProfileId = serverProfile.ProfileId,
                Server = serverProfile.Server,
                Port = serverProfile.Port,
                PortForGlobalCatalog = serverProfile.PortForGlobalCatalog,
                BaseDN = serverProfile.BaseDN,
                BaseDNforGlobalCatalog = serverProfile.BaseDNforGlobalCatalog,
                ConnectionTimeout = serverProfile.ConnectionTimeout,
                UseSSL = serverProfile.UseSSL,
                UseSSLforGlobalCatalog = serverProfile.UseSSLforGlobalCatalog,
                DomainUserAccount = serverProfile.DomainUserAccount
            };
        });

        if (dto == null)
            return NotFound();
        else
            return Ok(dto);
    }

    [Route("api/[controller]")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DTO.LDAPServerProfile>>> GetAsync()
    {
        var dtos = await Task.Run(() =>
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
                DomainUserAccount = p.DomainUserAccount
            });
        });

        return Ok(dtos);
    }
}
