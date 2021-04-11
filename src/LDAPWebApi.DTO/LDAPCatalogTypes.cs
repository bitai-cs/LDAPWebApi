using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bitai.LDAPWebApi.DTO
{
    public class LDAPCatalogTypes
    {
        [Required]
        [MinLength(2)]
        public string LocalCatalog { get; set; }

        [Required]
        [MinLength(2)]
        public string GlobalCatalog { get; set; }


        public string GetCatalogTypeName(bool isGlobalCatalog)
        {
            return isGlobalCatalog ? GlobalCatalog : LocalCatalog;
        }
    }
}
