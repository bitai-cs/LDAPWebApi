using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Controllers.AuthRequirements;

public class ApiScopeRequirementHandler : AuthorizationHandler<ApiScopeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiScopeRequirement requirement)
    {
        if (!context.User.HasClaim(c => c.Type == "scope" && c.Issuer == requirement.Issuer))
            return Task.CompletedTask;

        var claim = context.User.FindFirst(c => c.Type == "scope" && c.Issuer == requirement.Issuer);
        if (claim == null)
            throw new Exception($"The issuer named {requirement.Issuer} was not found in the user's claims.");

        var scopes = claim.Value.Split(' ');

        if (scopes.Any(s => requirement.Scopes.Contains(s)))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
