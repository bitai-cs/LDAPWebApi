using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Controllers.PolicyEvaluators;

/// <summary>
/// Evaluator that always allows access.
/// </summary>
/// <remarks>
/// This evaluator is used to bypass the authorization process.
/// </remarks>
public class AuthorizationBypassPolicyEvaluator : IPolicyEvaluator
{
    /// <summary>
    /// Authenticates the user based on the provided policy and context.
    /// </summary>
    /// <param name="policy">The authorization policy to be evaluated.</param>
    /// <param name="context">The HTTP context containing the user's information.</param>
    /// <returns>The authentication result.</returns>
    public virtual async Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        var bypassScheme = "BypassScheme";

        var principal = new ClaimsPrincipal();
        principal.AddIdentity(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, "Anonymous")
        }, bypassScheme));

        return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal,
            new AuthenticationProperties(), bypassScheme)));
    }

    /// <summary>
    /// Authorizes the user based on the provided policy, authentication result, HTTP context, and resource.
    /// </summary>
    /// <param name="policy">The authorization policy to be evaluated.</param>
    /// <param name="authenticationResult">The authentication result.</param>
    /// <param name="context">The HTTP context containing the user's information.</param>
    /// <param name="resource">The resource to be authorized.</param>
    /// <returns>The authorization result.</returns>
    public virtual async Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
    {
        return await Task.FromResult(PolicyAuthorizationResult.Success());
    }
}
