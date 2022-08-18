using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Controllers.AuthRequirements;

public class ApiScopeRequirement : IAuthorizationRequirement
{
    public IEnumerable<string> Scopes { get; }
    public string Issuer { get; }

    public ApiScopeRequirement(IEnumerable<string> scopes, string issuer)
    {
        if (scopes == null || scopes.Count().Equals(0))
            throw new ArgumentNullException(nameof(scopes));

        Scopes = scopes;

        Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
    }
}
