using System.Diagnostics.CodeAnalysis;

namespace Bitai.LDAPWebApi.DTO;

/// <summary>
/// Represents the LDAP Server properties
/// </summary>
public class LDAPServerProfile
{
	/// <summary>
	/// Profile identifier.
	/// </summary>
	public string ProfileId { get; set; }

	/// <summary>
	/// LDAP server address.
	/// </summary>
	public string Server { get; set; }

	/// <summary>
	/// LDAP server port for local catalog service.
	/// </summary>
	public string Port { get; set; }

	/// <summary>
	/// Use secure socket layer to connect to the LDAP server when using local catalog service.
	/// </summary>
	public bool UseSSL { get; set; }

	/// <summary>
	/// Base distinguished name to limit searches on the LDAP server when using the local catalog service.
	/// For example: DC=it,DC=us,DC=company,DC=com
	/// </summary>
	public string BaseDN { get; set; }

	/// <summary>
	/// LDAP server port for global catalog service.
	/// </summary>
	public string PortForGlobalCatalog { get; set; }

	/// <summary>
	/// Use secure socket layer to connect to the LDAP server when using global catalog service.
	/// </summary>
	public bool UseSSLforGlobalCatalog { get; set; }

	/// <summary>
	/// Base distinguished name to limit searches on the LDAP server when using the global catalog service.
	/// For example: DC=company,DC=com
	/// </summary>
	public string BaseDNforGlobalCatalog { get; set; }

	/// <summary>
	/// Default domain name.
	/// </summary>
	public string DefaultDomainName { get; set; }

	/// <summary>
	/// Max time to get connection to the LDAP Server.
	/// </summary>
	public short ConnectionTimeout { get; set; }

	/// <summary>
	/// Name of the domain account to connect to the LDAP server.
	/// </summary>
	public string DomainUserAccount { get; set; }



	/// <summary>
	/// Constructor
	/// </summary>
	public LDAPServerProfile()
	{
		ProfileId = "LDAPServer";
		Server = "localhost";
		Port = "Default";
		UseSSL = false;
		BaseDN = "ou=company";
		PortForGlobalCatalog = "Default";
		UseSSLforGlobalCatalog = false;
		BaseDNforGlobalCatalog = "ou=company";
		DefaultDomainName = "COMPANY";
		ConnectionTimeout = 10;
		DomainUserAccount = "administrator";
	}
}
