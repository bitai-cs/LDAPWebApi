using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Controllers.AuthRequirements;

/// <summary>
/// Requirement that allows access to the action when the user has at least one of the allowed scopes.
/// </summary>
public class AnyApiScopeRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the list of allowed scopes. The requirement is fulfilled if the user has at least one of these scopes.
    /// </summary>
    public IEnumerable<string> AllowedScopes { get; }

    /// <summary>
    /// Gets the issuer of the allowed scopes. This identifies the party that issued the token.
    /// </summary>
    public string Issuer { get; }




	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="allowedScopes">See <see cref="AllowedScopes"/></param>
	/// <param name="issuer">See <see cref="Issuer"/></param>
	/// <exception cref="ArgumentNullException">When at least one ApiScope or the Issuer is not defined.</exception>
	public AnyApiScopeRequirement(IEnumerable<string> allowedScopes, string issuer)
    {
        if (allowedScopes == null || allowedScopes.Count().Equals(0))
            throw new ArgumentNullException(nameof(allowedScopes));

        AllowedScopes = allowedScopes;

        Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
    }
}
