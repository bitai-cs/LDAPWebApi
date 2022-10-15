using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Controllers.AuthRequirements;

/// <summary>
/// Class that defines an ApiScope authorization requirement.
/// This class implements <see cref="IAuthorizationRequirement"/> interface. 
/// </summary>
public class ApiScopeRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// List of ApiScopes
    /// </summary>
    public IEnumerable<string> Scopes { get; }

    /// <summary>
    /// Authority (openId, oauth) Url
    /// </summary>
    public string Issuer { get; }



	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="scopes">See <see cref="Scopes"/></param>
	/// <param name="issuer">See <see cref="Issuer"/></param>
	/// <exception cref="ArgumentNullException">When at least one ApiScope or the Issuer is not defined.</exception>
	public ApiScopeRequirement(IEnumerable<string> scopes, string issuer)
    {
        if (scopes == null || scopes.Count().Equals(0))
            throw new ArgumentNullException(nameof(scopes));

        Scopes = scopes;

        Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
    }
}
