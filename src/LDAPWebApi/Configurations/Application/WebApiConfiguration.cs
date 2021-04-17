using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Configurations.App
{
    /// <summary>
    /// Web Api configuration model 
    /// </summary>
    public class WebApiConfiguration
    {
        public string WebApiName { get; set; }
        public string WebApiTitle { get; set; }
        public string WebApiDescription { get; set; }
        public string WebApiVersion { get; set; }
        public string WebApiBaseUrl { get; set; }
        public string WebApiContactName { get; set; }
        public string WebApiContactMail { get; set; }
        public string WebApiContactUrl { get; set; }
    }
}
