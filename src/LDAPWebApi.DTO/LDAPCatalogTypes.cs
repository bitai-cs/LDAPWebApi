using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bitai.LDAPWebApi.DTO
{
    public class LDAPCatalogTypes
    {
        public string LocalCatalog => "LC";

        public string GlobalCatalog => "GC";


        public string GetCatalogTypeName(bool isGlobalCatalog)
        {
            return isGlobalCatalog ? GlobalCatalog : LocalCatalog;
        }
    }
}
