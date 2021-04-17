using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Configurations.App
{
    /// <summary>
    /// Web Api CORS configuration model 
    /// </summary>
    public class WebApiCorsConfiguration
    {
        public bool AllowAnyOrigin { get; set; }

        public string[] AllowedOrigins { get; set; }
    }
}
