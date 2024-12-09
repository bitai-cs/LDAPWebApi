using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Configurations.Swagger;

/// <summary>
/// Identity Server authority configuration model.
/// </summary>
public class SwaggerUIConfiguration
{
	/// <summary>
	/// Client ID registered with the identity server.
	/// </summary>
	public string SwaggerUIClientId { get; set; }

	/// <summary>
	/// The target API scopes that will be available for the Swagger UI.
	/// </summary>
	public SwaggerUITargetApiScopes SwaggerUITargetApiScopes { get; set; }




	/// <summary>
	/// Constructor
	/// </summary>
	public SwaggerUIConfiguration()
	{
		SwaggerUIClientId = "Bitai.LdapWebApi.Swagger.Client";

		SwaggerUITargetApiScopes = new SwaggerUITargetApiScopes();
	}
}

/// <summary>
/// Represents a list of Swagger UI target API scopes.
/// </summary>
public class SwaggerUITargetApiScopes: List<SwaggerUITargetApiScope> {

}

/// <summary>
/// Represents a single Swagger UI target API scope.
/// </summary>
public class SwaggerUITargetApiScope
{
	/// <summary>
	/// Api Scope registered with the identity server.
	/// </summary>
	public required string SwaggerUITargetApiScopeName { get; set; }

	/// <summary>
	/// Api Same title. To display in the user interface.
	/// </summary>
	public required string SwaggerUITargetApiScopeTitle { get; set; }
}
