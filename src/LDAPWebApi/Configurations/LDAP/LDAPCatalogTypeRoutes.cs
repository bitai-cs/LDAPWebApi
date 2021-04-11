using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Configurations.LDAP
{
    /// <summary>
    /// Defines the name of the Web API routes used for both local catalog and global catalog.
    /// </summary>
    public class LDAPCatalogTypeRoutes
    {
        /// <summary>
        /// Global catalog route name.
        /// </summary>
        public string GlobalCatalogRoute { get; set; }

        /// <summary>
        /// Local catalogroute name.
        /// </summary>
        public string LocalCatalogRoute { get; set; }
    }
}