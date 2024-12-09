using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Configurations.App;

/// <summary>
/// Web Api scopes configuration model
/// </summary>
public class WebApiScopesConfiguration
{
	internal const string AuthorizationPolicyForAnyApiScopeName = "AuthorizationPolicyForAnyApiScope";
	internal const string AuthorizationPolicyForAdminApiScopeName = "AuthorizationPolicyForAdminApiScope";




	/// <summary>
	/// Constructor. 
	/// Set default values.
	/// </summary>
	public WebApiScopesConfiguration()
	{
		BypassApiScopesAuthorization = true;
		AdminApiScopeName = "Bitai.LdapWebApi.Scope.Admin";
		ReaderApiScopeName = "Bitai.LdapWebApi.Scope.Reader";
	}



	/// <summary>
	/// Whether or not to bypass authorization validation for Api Scopes.
	/// </summary>
	public bool BypassApiScopesAuthorization { get; set; }

	/// <summary>
	/// Name of the Admin ApiScope
	/// </summary>
	public string AdminApiScopeName { get; set; }

	/// <summary>
	/// Name of the Reader ApiScope
	/// </summary>
	public string ReaderApiScopeName { get; set; }
}
