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
		/// Initializes a new instance of the <see cref="LDAPWebApiBaseClient"/> class.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		protected LDAPWebApiBaseClient(string ldapWebApiBaseUrl) : base(ldapWebApiBaseUrl)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPWebApiBaseClient"/> class with a custom HttpClientHandler.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="handler">The custom HttpClientHandler to handle HTTP requests.</param>
		/// <param name="disposeHandler">true if the inner handler should be disposed of by Dispose(), false if you intend to reuse the inner handler.</param>
		protected LDAPWebApiBaseClient(string ldapWebApiBaseUrl, HttpClientHandler handler, bool disposeHandler) : base(ldapWebApiBaseUrl, handler, disposeHandler)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPWebApiBaseClient"/> class with Identity Server credentials.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="clientCredentials">Client credentials to request an access token from the Identity Server.</param>
		protected LDAPWebApiBaseClient(string ldapWebApiBaseUrl, WebApiClientCredential clientCredentials) : base(ldapWebApiBaseUrl, clientCredentials)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPWebApiBaseClient"/> class with Identity Server credentials and a custom HttpClientHandler.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="clientCredentials">Client credentials to request an access token from the Identity Server.</param>
		/// <param name="handler">The custom HttpClientHandler to handle HTTP requests.</param>
		/// <param name="disposeHandler">true if the inner handler should be disposed of by Dispose(), false if you intend to reuse the inner handler.</param>
		protected LDAPWebApiBaseClient(string ldapWebApiBaseUrl, WebApiClientCredential clientCredentials, HttpClientHandler handler, bool disposeHandler) : base(ldapWebApiBaseUrl, clientCredentials, handler, disposeHandler)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL</param>
		/// <param name="ldapServerProfile">LDAP Server Profile Id</param>
		/// <param name="useLdapServerGlobalCatalog">Wheter the global catalog or the local catalog will be used.</param>
		protected LDAPWebApiBaseClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog) : base(ldapWebApiBaseUrl)
		{
			LDAPServerProfile = ldapServerProfile;
			UseLDAPServerGlobalCatalog = useLdapServerGlobalCatalog;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL</param>
		/// <param name="ldapServerProfile">LDAP Server Profile Id</param>
		/// <param name="useLdapServerGlobalCatalog">Wheter the global catalog or the local catalog will be used.</param>
		/// <param name="clientCredentials">Security parameters to get an access token from Identity Server.</param>
		protected LDAPWebApiBaseClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog, WebApiClientCredential clientCredentials) : base(ldapWebApiBaseUrl, clientCredentials)
		{
			LDAPServerProfile = ldapServerProfile;
			UseLDAPServerGlobalCatalog = useLdapServerGlobalCatalog;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPWebApiBaseClient"/> class with server configuration details and a custom HttpClientHandler.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="ldapServerProfile">LDAP Server Profile Id.</param>
		/// <param name="useLdapServerGlobalCatalog">Whether or not the global catalog of the LDAP server will be used; otherwise the local catalog of the LDAP server will be used.</param>
		/// <param name="handler">The custom HttpClientHandler to handle HTTP requests.</param>
		/// <param name="disposeHandler">true if the inner handler should be disposed of by Dispose(), false if you intend to reuse the inner handler.</param>
		protected LDAPWebApiBaseClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog, HttpClientHandler handler, bool disposeHandler) : base(ldapWebApiBaseUrl, handler, disposeHandler)
		{
			LDAPServerProfile = ldapServerProfile;
			UseLDAPServerGlobalCatalog = useLdapServerGlobalCatalog;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPWebApiBaseClient"/> class with server configuration details, Identity Server credentials, and a custom HttpClientHandler.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="ldapServerProfile">LDAP Server Profile Id.</param>
		/// <param name="useLdapServerGlobalCatalog">Whether or not the global catalog of the LDAP server will be used; otherwise the local catalog of the LDAP server will be used.</param>
		/// <param name="clientCredentials">Client credentials to request an access token from the Identity Server.</param>
		/// <param name="handler">The custom HttpClientHandler to handle HTTP requests.</param>
		/// <param name="disposeHandler">true if the inner handler should be disposed of by Dispose(), false if you intend to reuse the inner handler.</param>
		protected LDAPWebApiBaseClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog, WebApiClientCredential clientCredentials, HttpClientHandler handler, bool disposeHandler) : base(ldapWebApiBaseUrl, clientCredentials, handler, disposeHandler)
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
