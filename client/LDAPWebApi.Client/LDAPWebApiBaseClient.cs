using Bitai.LDAPHelper.DTO;
using Bitai.LDAPWebApi.DTO;
using Bitai.WebApi.Client;

namespace Bitai.LDAPWebApi.Clients
{
	/// <summary>
	/// Provides shared configuration and helper utilities for LDAP Web API clients.
	/// </summary>
	public abstract class LDAPWebApiBaseClient : WebApiBaseClient
	{
		/// <summary>
		/// Gets or sets the LDAP server profile identifier defined in the LDAP Web API configuration.
		/// </summary>
		public string? LDAPServerProfile { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether requests should target the server global catalog instead of the local catalog.
		/// </summary>
		public bool UseLDAPServerGlobalCatalog { get; set; }




		/// <summary>
		/// Gets helper methods for resolving LDAP catalog type route values.
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
		/// Initializes a new instance of the <see cref="LDAPWebApiBaseClient"/> class with LDAP server profile routing settings.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="ldapServerProfile">LDAP Server Profile Id.</param>
		/// <param name="useLdapServerGlobalCatalog">Whether the global catalog or the local catalog will be used.</param>
		protected LDAPWebApiBaseClient(string ldapWebApiBaseUrl, string ldapServerProfile, bool useLdapServerGlobalCatalog) : base(ldapWebApiBaseUrl)
		{
			LDAPServerProfile = ldapServerProfile;
			UseLDAPServerGlobalCatalog = useLdapServerGlobalCatalog;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LDAPWebApiBaseClient"/> class with LDAP server profile routing settings and Identity Server credentials.
		/// </summary>
		/// <param name="ldapWebApiBaseUrl">LDAP Web Api base URL.</param>
		/// <param name="ldapServerProfile">LDAP Server Profile Id.</param>
		/// <param name="useLdapServerGlobalCatalog">Whether the global catalog or the local catalog will be used.</param>
		/// <param name="clientCredentials">Security parameters to obtain an access token from the Identity Server.</param>
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
		/// Gets the query-string value for an optional <see cref="EntryAttribute"/> parameter.
		/// </summary>
		/// <param name="optionalEntryAttribute">Nullable <see cref="EntryAttribute"/>.</param>
		/// <returns>The attribute name when specified; otherwise an empty string.</returns>
		public string GetOptionalEntryAttributeName(EntryAttribute? optionalEntryAttribute)
		{
			return optionalEntryAttribute.HasValue ? optionalEntryAttribute.Value.ToString() : string.Empty;
		}

		/// <summary>
		/// Gets the query-string value for an optional <see cref="RequiredEntryAttributes"/> parameter.
		/// </summary>
		/// <param name="nullable">Nullable <see cref="RequiredEntryAttributes"/>.</param>
		/// <returns>The attribute set value when specified; otherwise an empty string.</returns>
		public string GetOptionalRequiredEntryAttributesName(RequiredEntryAttributes? nullable)
		{
			return nullable.HasValue ? nullable.Value.ToString() : string.Empty;
		}

		/// <summary>
		/// Gets the query-string value for an optional Boolean parameter.
		/// </summary>
		/// <param name="nullable">Nullable Boolean value.</param>
		/// <returns>The Boolean value as text when specified; otherwise an empty string.</returns>
		public string GetOptionalBooleanValue(bool? nullable)
		{
			return nullable.HasValue ? nullable.Value.ToString() : string.Empty;
		}




		#region Static inner class
		/// <summary>
		/// Exposes constant route segment names for LDAP Web API controllers.
		/// </summary>
		public static class ControllerNames
		{
			/// <summary>
			/// Server Profiles controller name.
			/// </summary>
			public static readonly string ServerProfilesController = "ServerProfiles";
			/// <summary>
			/// Catalog Types controller name.
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
