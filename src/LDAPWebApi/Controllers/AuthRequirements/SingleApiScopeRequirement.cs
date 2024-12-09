using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Controllers.AuthRequirements;

/// <summary>
/// Requirement that allows access to the action when the user has a specific API scope.
/// </summary>
public class SingleApiScopeRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// API scope that is allowed.
    /// </summary>
    public string AllowedScope { get; }

    /// <summary>
    /// Issuer of the API scope.
    /// </summary>
    public string Issuer { get; }




	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="allowedScope">API scope that is allowed. See <see cref="AllowedScope"/></param>
	/// <param name="issuer">Issuer of the API scope. See <see cref="Issuer"/></param>
	/// <exception cref="ArgumentNullException">When the AllowedScope or Issuer is null or empty.</exception>
	public SingleApiScopeRequirement(string allowedScope, string issuer)
    {
        if (string.IsNullOrEmpty(allowedScope))
            throw new ArgumentNullException(nameof(allowedScope));

        AllowedScope = allowedScope;

        Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
    }
}