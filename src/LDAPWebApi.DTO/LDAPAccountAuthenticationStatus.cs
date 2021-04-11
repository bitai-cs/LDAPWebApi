using System;
using System.Collections.Generic;
using System.Text;

namespace Bitai.LDAPWebApi.DTO
{
	public class LDAPAccountAuthenticationStatus
	{
		public string RequestTag { get; set; }
		public string DomainName { get; set; }
		public string AccountName { get; set; }
		public bool IsAuthenticated { get; set; }
		public string Message { get; set; }
	}
}