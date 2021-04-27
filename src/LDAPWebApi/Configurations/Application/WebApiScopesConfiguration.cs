using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Configurations.App
{
    public class WebApiScopesConfiguration
    {
        internal const string GlobalScopeAuthorizationPolicyName = "RequiredGlobalScopePolicy";

        public string GlobalScopeName { get; set; }
    }
}
