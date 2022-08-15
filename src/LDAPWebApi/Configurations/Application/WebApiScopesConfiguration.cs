using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Configurations.App;

public class WebApiScopesConfiguration
{
    internal const string GlobalScopeAuthorizationPolicyName = "RequiredGlobalScopePolicy";



    /// <summary>
    /// Constructor. 
    /// Set default values.
    /// </summary>
    public WebApiScopesConfiguration()
		{
        BypassApiScopesAuthorization = true;
        GlobalScopeName = "Bitai.LdapWebApi.Scope.Global";
		}



    /// <summary>
    /// Whether or not to bypass authorization validation for Api Scopes.
    /// </summary>
    public bool BypassApiScopesAuthorization { get; set; }

    /// <summary>
    /// Name of the Global Scope
    /// </summary>
    public string GlobalScopeName { get; set; }
}
