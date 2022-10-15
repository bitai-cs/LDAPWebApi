using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Configurations.App;

/// <summary>
/// Web Api CORS configuration model 
/// </summary>
public class WebApiCorsConfiguration
{
	/// <summary>
	/// Constructor
	/// </summary>
	public WebApiCorsConfiguration()
	{
		AllowAnyOrigin = false;
		AllowedOrigins = new string[] {
			"http://localhost:5100",
			"https://localhost:5101"
		};
	}


	/// <summary>
	/// Indicates whether CORS is enabled or not.
	/// </summary>
	public bool AllowAnyOrigin { get; set; }

	/// <summary>
	/// List of allowed CORS origins.
	/// </summary>
	public string[] AllowedOrigins { get; set; }
}
