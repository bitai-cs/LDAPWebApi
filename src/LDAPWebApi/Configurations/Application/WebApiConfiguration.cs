using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Configurations.App;

/// <summary>
/// Web Api configuration model 
/// </summary>
public class WebApiConfiguration
{
	/// <summary>
	/// Constructor
	/// </summary>
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




	/// <summary>
	/// API short name. It is displayed in Swagger's OpenAPI interface.
	/// </summary>
	public string WebApiName { get; set; }
	/// <summary>
	/// API long name. It is displayed in Swagger's OpenAPI interface.
	/// </summary>
	public string WebApiTitle { get; set; }
	/// <summary>
	/// API description.
	/// </summary>
	public string WebApiDescription { get; set; }
	/// <summary>
	/// Referential name of the API version.
	/// </summary>
	public string WebApiVersion { get; set; }
	/// <summary>
	/// API base URL.
	/// </summary>
	public string WebApiBaseUrl { get; set; }
	/// <summary>
	/// Name of the developer and/or technical support.
	/// </summary>
	public string WebApiContactName { get; set; }
	/// <summary>
	/// Mail of the developer and/or technical support.
	/// </summary>
	public string WebApiContactMail { get; set; }
	/// <summary>
	/// Web URL of the developer and/or technical support.
	/// </summary>
	public string WebApiContactUrl { get; set; }
	/// <summary>
	/// Web API health checks configuration
	/// </summary>
	public WebApiHealthChecksConfiguration HealthChecksConfiguration { get; set; }

	#region Inner class
	/// <summary>
	/// Web API health checks configuration model
	/// </summary>
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



		/// <summary>
		/// Indicates whether API health checks will be enabled or not.
		/// </summary>
		public bool EnableHealthChecks { get; set; }
		/// <summary>
		/// Page header text.
		/// </summary>
		public string HealthChecksHeaderText { get; set; }
		/// <summary>
		/// Group name text.
		/// </summary>
		public string HealthChecksGroupName { get; set; }
		/// <summary>
		/// Health checks API endpoint name.
		/// </summary>
		public string ApiEndPointName { get; set; }
		/// <summary>
		/// Health checks UI web path.
		/// </summary>
		public string UIPath { get; set; }
		/// <summary>
		/// Maximum history of checks
		/// </summary>
		public int MaximunHistoryEntries { get; set; }
		/// <summary>
		/// Health checks frecuency
		/// </summary>
		public int EvaluationTime { get; set; }
	}
	#endregion
}
