using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bitai.LDAPWebApi.DTO
{
    /// <summary>
    /// This class represents the credentials that the LDAP server will validate  
    /// </summary>
    public class LDAPAccountSecurityData
    {
        /// <summary>
        /// Domain of AccountName/>
        /// </summary>
        [Required]
        public string DomainName { get; set; }

        /// <summary>
        /// Network account password.
        /// </summary>
        [Required]
        public string AccountPassword { get; set; }
    }
}