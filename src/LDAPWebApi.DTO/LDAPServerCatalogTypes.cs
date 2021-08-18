using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bitai.LDAPWebApi.DTO
{
    /// <summary>
    /// Directory service catalog types.
    /// </summary>
    public class LDAPServerCatalogTypes
    {
        public string LocalCatalog => "LC";

        public string GlobalCatalog => "GC";


        /// <summary>
        /// Helper method to get catalog type name
        /// </summary>
        /// <param name="isGlobalCatalog">Global catalog name is required.</param>
        /// <returns></returns>
        public string GetCatalogTypeName(bool isGlobalCatalog)
        {
            return isGlobalCatalog ? GlobalCatalog : LocalCatalog;
        }
    }
}
