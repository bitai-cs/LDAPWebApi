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
        public WebApiConfiguration()
		{
            WebApiName = "LDAP Web Api";
            WebApiTitle = "BITAI LDAP Web Api";
            WebApiDescription = "Web Api for queryng LDAP servers";
            WebApiVersion = "v1";
            WebApiBaseUrl = "https://localhost:5101";
            WebApiContactName = "Viko Bastidas";
            WebApiContactMail = "vico.bastidasgmail.com";
            WebApiContactUrl = "https://www.linkedin.com/in/victorbastidasg";

            HealthChecksConfiguration = new WebApiHealthChecksConfiguration();
        }




        public string WebApiName { get; set; }
        public string WebApiTitle { get; set; }
        public string WebApiDescription { get; set; }
        public string WebApiVersion { get; set; }
        public string WebApiBaseUrl { get; set; }
        public string WebApiContactName { get; set; }
        public string WebApiContactMail { get; set; }
        public string WebApiContactUrl { get; set; }
        public WebApiHealthChecksConfiguration HealthChecksConfiguration { get; set; }

        #region Inner class
        public class WebApiHealthChecksConfiguration
        {
            /// <summary>
            /// Constructor
            /// </summary>
			public WebApiHealthChecksConfiguration()
			{
                EnableHealthChecks = false;
                HealthChecksHeaderText = "Health Checks UI";
                HealthChecksGroupName = "Health Checks";
                ApiEndPointName = "hc";
                UIPath = "hc-ui";
                MaximunHistoryEntries = 100;
                EvaluationTime = 30;
            }



			public bool EnableHealthChecks { get; set; }
            public string HealthChecksHeaderText { get; set; }
            public string HealthChecksGroupName { get; set; }
            public string ApiEndPointName { get; set; }
            public string UIPath { get; set; }
            public int MaximunHistoryEntries { get; set; }
            public int EvaluationTime { get; set; }
        }
        #endregion
    }
}
