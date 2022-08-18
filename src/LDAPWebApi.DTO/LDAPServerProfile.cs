namespace Bitai.LDAPWebApi.DTO;

/// <summary>
/// Represents the LDAP Server properties
/// </summary>
public class LDAPServerProfile
{
	public string? ProfileId { get; set; }

	public string? Server { get; set; }

	public string? Port { get; set; }

	public string? PortForGlobalCatalog { get; set; }

	public string? BaseDN { get; set; }

	public string? BaseDNforGlobalCatalog { get; set; }

	public short ConnectionTimeout { get; set; }

	public bool UseSSL { get; set; }

	public bool UseSSLforGlobalCatalog { get; set; }

	public string? DomainAccountName { get; set; }

	/// <summary>
	/// The password will never be sent from the Web API. This property will only serve to send a password change to the Web API
	/// </summary>
	public string? DomainAccountPassword { get; set; }
}
