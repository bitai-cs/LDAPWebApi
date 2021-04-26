using System;
using System.Collections.Generic;
using System.Text;

namespace Bitai.LDAPWebApi.Clients
{
    public class WebApiSecurityDefinition
    {
        public string AuthorityUrl { get; set; }
        public string ApiScope { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}