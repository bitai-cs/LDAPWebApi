using Bitai.LDAPHelper.DTO;
using Bitai.LDAPWebApi.DTO;
using Bitai.WebApi.Client;

namespace Bitai.LDAPWebApi.Clients
{
    /// <summary>
    /// Encapsulates basic LDAP Web Api client behavior. 
    /// </summary>
    public abstract class LDAPWebApiBaseClient : WebApiBaseClient
    {
        /// <summary>
        /// LDAP Server profile Id, which is defined in appsettings.json 
        /// of LDAPWebApi. 
        /// </summary>
        public string? LDAPServerProfile { get; set; }
        /// <summary>
        /// True, if the client will connect to the server's global catalog,
        /// otherwise, it will connect to the server's local catalog.
        /// </summary>
        public bool UseLDAPServerGlobalCatalog { get; set; }




        /// <summary>
        /// See <see cref="LDAPServerCatalogTypes"/>.
        /// </summary>
        protected LDAPServerCatalogTypes LDAPServerCatalogTypes => new LDAPServerCatalogTypes();




        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL</param>
        /// <param name="clientCredentials">Security parameters to get an access token from Identiti Server.</param>
        protected LDAPWebApiBaseClient(string ldapWebApiBaseUrl, WebApiClientCredential? clientCredentials) : base(ldapWebApiBaseUrl, clientCredentials)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL</param>
        /// <param name="ldapServerProfile">LDAP Server Profile Id</param>
        /// <param name="useLdapServerGlobalCatalog">Wheter the global catalog or the local catalog will be used.</param>
        /// <param name="clientCredentials">Security parameters to get an access token from Identiti Server.</param>
        protected LDAPWebApiBaseClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog, WebApiClientCredential? clientCredentials) : this(ldapWebApiBaseUrl, clientCredentials)
        {
            LDAPServerProfile = ldapServerProfile;
            UseLDAPServerGlobalCatalog = useLdapServerGlobalCatalog;
        }




        /// <summary>
        /// Get value for an optional <see cref="EntryAttribute"/> query string parameter.
        /// </summary>
        /// <param name="optionalEntryAttribute">Nullable <see cref="EntryAttribute"/></param>
        /// <returns></returns>
        public string GetOptionalEntryAttributeName(EntryAttribute? optionalEntryAttribute)
        {
            return optionalEntryAttribute.HasValue ? optionalEntryAttribute.Value.ToString() : string.Empty;
        }

        /// <summary>
        /// Get value for an optional <see cref="RequiredEntryAttributes"/> query string parameter.
        /// </summary>
        /// <param name="nullable">Nullable <see cref="RequiredEntryAttributes"/></param>
        /// <returns></returns>
        public string GetOptionalRequiredEntryAttributesName(RequiredEntryAttributes? nullable)
        {
            return nullable.HasValue ? nullable.Value.ToString() : string.Empty;
        }

        /// <summary>
        /// Get value for an optional <see cref="bool"/> query string parameter.
        /// </summary>
        /// <param name="nullable"></param>
        /// <returns></returns>
        public string GetOptionalBooleanValue(bool? nullable)
        {
            return nullable.HasValue ? nullable.Value.ToString() : string.Empty;
        }




        #region Static inner class
        /// <summary>
        /// Helper class which allow to identify LDAP Web Api controllers.
        /// </summary>
        public static class ControllerNames
        {
            /// <summary>
            /// Server Profiles controller name.
            /// </summary>
            public static readonly string ServerProfilesController = "ServerProfiles";
            /// <summary>
            /// Catalog Typrs controller name.
            /// </summary>
            public static readonly string CatalogTypesController = "CatalogTypes";
            /// <summary>
            /// Authentications controller name
            /// </summary>
            public static readonly string AuthenticationsController = "Authentications";
            /// <summary>
            /// Directory controller name.
            /// </summary>
            public static readonly string DirectoryController = "Directory";
        }
        #endregion
    }
}
