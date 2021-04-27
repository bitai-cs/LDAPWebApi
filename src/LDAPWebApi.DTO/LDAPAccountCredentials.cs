using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bitai.LDAPWebApi.DTO
{
    /// <summary>
    /// This class represents the credentials that the LDAP server will validate  
    /// </summary>
    public class LDAPAccountCredentials
    {
        /// <summary>
        /// Domain name of the <see cref="AccountName"/>/>
        /// </summary>
        [Required]
        public string DomainName { get; set; }

        /// <summary>
        /// Account name
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// <see cref="AccountName"/> password
        /// </summary>
        [Required]
        public string AccountPassword { get; set; }
    }
}