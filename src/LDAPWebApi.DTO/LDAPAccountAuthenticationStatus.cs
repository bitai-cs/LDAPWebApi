using System;
using System.Collections.Generic;
using System.Text;

namespace Bitai.LDAPWebApi.DTO
{
	/// <summary>
	/// Represents the status of the authentication process for the 
	/// credentials of a user account.
	/// </summary>
	public class LDAPAccountAuthenticationStatus
	{
		/// <summary>
		/// Request label.
		/// </summary>
		public string RequestTag { get; set; }
		
		/// <summary>
		/// Directory service domain.
		/// </summary>
		public string DomainName { get; set; }

		/// <summary>
		/// Account name
		/// </summary>
		public string AccountName { get; set; }

		/// <summary>
		/// Value indicating whether the account was able to authenticate 
		/// to the LDAP server.
		/// </summary>
		public bool IsAuthenticated { get; set; }

		/// <summary>
		/// Mensaje infirmativo del proceso de autenticación.
		/// </summary>
		public string Message { get; set; }
	}
}