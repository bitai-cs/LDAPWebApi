using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Configurations.Security;

/// <summary>
/// Identity Server authority configuration model.
/// </summary>
public class AuthorityConfiguration
{
	private string? _authority;



	/// <summary>
	/// Identity Server Authority URL
	/// </summary>
	public string? Authority { get => _authority; set => _authority = (value != null ? value.ToLower() : null); }

	/// <summary>
	/// Api Resource name 
	/// </summary>
	public string? ApiResource { get; set; }

	/// <summary>
	/// Gets or sets if HTTPS is required for the metadata address or authority. 
	/// The default is true. 
	/// This should be disabled only in development environments.
	/// </summary>
	public bool? RequireHttpsMetadata { get; set; }
}
