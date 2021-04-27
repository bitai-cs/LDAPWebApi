using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Configurations.Swagger
{
    /// <summary>
    /// Identity Server authority configuration model.
    /// </summary>
    public class SwaggerUIConfiguration
    {
        /// <summary>
        /// Api Scope registered with the identity server.
        /// </summary>
        public string SwaggerUITargetApiScope { get; set; }

        /// <summary>
        /// Api Same title. To display in the user interface.
        /// </summary>
        public string SwaggerUITargetApiScopeTitle { get; set; }

        /// <summary>
        /// Client ID registered with the identity server.
        /// </summary>
        public string SwaggerUIClientId { get; set; }
    }
}
