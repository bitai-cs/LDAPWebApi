using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Configurations.LDAP;

/// <summary>
/// List of <see cref="LDAPServerProfile"/>.
/// </summary>
public class LDAPServerProfiles : List<LDAPServerProfile>
{
	/// Validate list of loaded <see cref="LDAPServerProfile"/>
	public void CheckConfigurationIntegrity()
	{
		#region Validate duplicated ProfileIds
		var repeatedProfileIds = this.GroupBy(id => id.ProfileId)
			.Where(g => g.Count() > 1)
			.Select(g => g.Key);

		if (repeatedProfileIds.Count() > 0)
			throw new Exception($"Error in {nameof(LDAPServerProfiles)} configuration. There are repeating {nameof(LDAPServerProfile.ProfileId)}s: {string.Join(',', repeatedProfileIds)}");
		#endregion

		#region Validate profiles without DefaultDomainName
		var profilesWithoutDefaultDomainName = this.Where(p => string.IsNullOrEmpty(p.DefaultDomainName));

		if (profilesWithoutDefaultDomainName.Count() > 0)
		{
			var invalidProfiles = string.Join(",", profilesWithoutDefaultDomainName.Select(p => p.ProfileId));

			throw new Exception($"Error in {nameof(LDAPServerProfiles)} configuration. There are {nameof(LDAPServerProfile)}s without {nameof(LDAPServerProfile.DefaultDomainName)}: {invalidProfiles}");
		}
		#endregion
	}
}

/// <summary>
/// LDAP Server Profile configurstion model.
/// Profile that defines an LDAP Server.
/// </summary>
public class LDAPServerProfile : DTO.LDAPServerProfile
{
	/// <summary>
	/// Password of the domain account to connect to the LDAP server.
	/// </summary>
	public string DomainAccountPassword { get; set; }



	/// <summary>
	/// Constructor
	/// </summary>
	/// <remarks>
	/// Set default property values.
	/// </remarks>
	public LDAPServerProfile()
	{
		ProfileId = "LOCAL";
		Server = "localhost";
		Port = "Default";
		UseSSL = true;
		BaseDN = "DC=local,DC=com";
		PortForGlobalCatalog = "Default";
		UseSSLforGlobalCatalog = false;
		BaseDNforGlobalCatalog = "DC=com";
		DefaultDomainName = "LOCAL";
		ConnectionTimeout = 10;
		DomainUserAccount = "LOCAL\\user";
		DomainAccountPassword = "P@ssw0rd";
		HealthCheckPingTimeout = 4;
	}



	/// <summary>
	/// Helper method to get Port from properties.
	/// </summary>
	/// <param name="forGlobalCatalog">If using the global catalog service.</param>
	/// <returns></returns>
	public int GetPort(bool forGlobalCatalog)
	{
		if (forGlobalCatalog)
		{
			if (PortForGlobalCatalog.ToLower().Equals("default"))
			{
				if (UseSSLforGlobalCatalog)
					return (int)LDAPHelper.LdapServerDefaultPorts.DefaultGlobalCatalogSslPort;
				else
					return (int)LDAPHelper.LdapServerDefaultPorts.DefaultGlobalCatalogPort;
			}

			return Convert.ToInt32(PortForGlobalCatalog);
		}
		else
		{
			if (Port.ToLower().Equals("default"))
			{
				if (UseSSL)
					return (int)LDAPHelper.LdapServerDefaultPorts.DefaultSslPort;
				else
					return (int)LDAPHelper.LdapServerDefaultPorts.DefaultPort;
			}

			return Convert.ToInt32(Port);
		}
	}

	/// <summary>
	/// Helper method to get BaseDN from properties.
	/// </summary>
	/// <param name="forGlobalCatalog">If using the global catalog service.</param>
	/// <returns></returns>
	public string GetBaseDN(bool forGlobalCatalog)
	{
		return forGlobalCatalog ? BaseDNforGlobalCatalog : BaseDN;
	}

	/// <summary>
	/// Helper method to get UseSsl from properties.
	/// </summary>
	/// <param name="forGlobalCatalog">If using the global catalog service.</param>
	/// <returns></returns>
	public bool GetUseSsl(bool forGlobalCatalog)
	{
		return (forGlobalCatalog ? UseSSLforGlobalCatalog : UseSSL);
	}
}