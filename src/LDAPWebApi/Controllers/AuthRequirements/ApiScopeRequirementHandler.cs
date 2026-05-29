using Microsoft.AspNetCore.Authorization;

namespace Bitai.LDAPWebApi.Controllers.AuthRequirements;

/// <summary>
/// Handles the authentication requirement for API scopes.
/// </summary>
public class ApiScopeRequirementHandler : AuthorizationHandler<ApiScopeRequirement>
{
    /// <summary>
    /// Handles the authentication requirement for API scopes.
    /// </summary>
    /// <param name="context">The authorization context. See <see cref="AuthorizationHandlerContext"/>.</param>
    /// <param name="requirement">The authentication requirement. See <see cref="ApiScopeRequirement"/>.</param>
    /// <returns></returns>
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
