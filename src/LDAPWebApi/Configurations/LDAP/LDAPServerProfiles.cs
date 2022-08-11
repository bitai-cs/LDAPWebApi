using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Configurations.LDAP
{
    /// <summary>
    /// List of <see cref="LDAPServerProfile"/>.
    /// </summary>
    public class LDAPServerProfiles : List<LDAPServerProfile>
    {
        public void CheckConfigurationIntegrity()
        {
            var repeatedProfileIds = this.GroupBy(id => id.ProfileId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (repeatedProfileIds.Count() > 0)
                throw new Exception($"Error in LDAPServerProfiles configuration. There are repeating ProfileIds ({string.Join(',', repeatedProfileIds)})");
        }
    }

    /// <summary>
    /// Profile that defines an LDAP service.
    /// </summary>
    public class LDAPServerProfile
    {
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
            PortForGlobalCatalog = "Default";
            BaseDN = "DC=local,DC=com";
            BaseDNforGlobalCatalog = "DC=com";
            DefaultDomainName = "LOCAL";
            ConnectionTimeout = 10;
            UseSSL = false;
            UseSSLforGlobalCatalog = false;
            DomainAccountName = "LOCAL\\user";
            DomainAccountPassword = "P@ssw0rd";
            HealthCheckPingTimeout = 2;
        }



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
        /// LDAP server port for global catalog service.
        /// </summary>
        public string PortForGlobalCatalog { get; set; }

        /// <summary>
        /// Base distinguished name to limit searches on the LDAP server when using the local catalog service.
        /// For example: DC=it,DC=us,DC=company,DC=com
        /// </summary>
        public string BaseDN { get; set; }

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
        /// Use secure socket layer to connect to the LDAP server when using local catalog service.
        /// </summary>
        public bool UseSSL { get; set; }

        /// <summary>
        /// Use secure socket layer to connect to the LDAP server when using global catalog service.
        /// </summary>
        public bool UseSSLforGlobalCatalog { get; set; }

        /// <summary>
        /// Name of the domain account to connect to the LDAP server.
        /// </summary>
        public string DomainAccountName { get; set; }

        /// <summary>
        /// Password of the domain account to connect to the LDAP server.
        /// </summary>
        public string DomainAccountPassword { get; set; }

        /// <summary>
        /// Ping timeout value to set the latency health check.
        /// </summary>
        public int HealthCheckPingTimeout { get; set; }



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
        public string? GetBaseDN(bool forGlobalCatalog)
        {
            return (forGlobalCatalog ? BaseDNforGlobalCatalog : BaseDN);
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
}