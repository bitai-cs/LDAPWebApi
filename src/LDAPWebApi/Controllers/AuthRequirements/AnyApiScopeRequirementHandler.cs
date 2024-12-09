using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Controllers.AuthRequirements;

/// <summary>
/// Handles the authentication requirement for API scopes.
/// </summary>
public class AnyApiScopeRequirementHandler : AuthorizationHandler<AnyApiScopeRequirement>
{
	/// <summary>
	/// Handles the authentication requirement for API scopes.
	/// </summary>
	/// <param name="context">The authorization context. See <see cref="AuthorizationHandlerContext"/>.</param>
	/// <param name="requirement">The authentication requirement. See <see cref="AnyApiScopeRequirement"/> </param>
	/// <returns></returns>
	protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AnyApiScopeRequirement requirement)
	{
		if (!context.User.HasClaim(c => c.Type == "scope" && c.Issuer == requirement.Issuer))
			return Task.CompletedTask;

		var claim = context.User.FindFirst(c => c.Type == "scope" && c.Issuer == requirement.Issuer);
		if (claim == null)
			throw new Exception($"The issuer named {requirement.Issuer} was not found in the user's claims.");

		var scopes = claim.Value.Split(' ');

		if (scopes.Any(s => requirement.AllowedScopes.Contains(s)))
			context.Succeed(requirement);

		return Task.CompletedTask;
	}
}