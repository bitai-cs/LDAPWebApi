﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bitai.LDAPHelper.DTO;
using Bitai.LDAPHelper.QueryFilters;
using Bitai.LDAPWebApi.Configurations.App;
using Bitai.WebApi.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;

namespace Bitai.LDAPWebApi.Controllers
{
    /// <summary>
    /// Web Api controller to process credentials.
    /// </summary>
    [Route("api")]
    [Authorize(WebApiScopesConfiguration.GlobalScopeAuthorizationPolicyName)]
    [ApiController]
    public class AuthenticationsController : ApiControllerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">Injected <see cref="IConfiguration"/></param>
        /// <param name="serverProfiles">Injected <see cref="Configurations.LDAP. LDAPServerProfiles"/></param>        
        public AuthenticationsController(IConfiguration configuration, Configurations.LDAP.LDAPServerProfiles serverProfiles) : base(configuration, serverProfiles) { }



        /// <summary>
        /// Validate Domain Account Name credential.
        /// </summary>
        /// <param name="serverProfile">LDAP Server Profile Id that defines part of the path. See <see cref="Configurations.LDAP.LDAPServerProfile"/></param>
        /// <param name="catalogType">Name of the LDAP catalog that defines part of the path. See <see cref="DTO.LDAPCatalogTypes"/></param>
        /// <param name="accountCredentials">Account credentials to validate. See <see cref="DTO.LDAPAccountCredentials"/></param>
        /// <param name="requestTag">Valor personalizado para etiquetar la respuesta. Can e null</param>
        /// <returns><see cref="DTO.LDAPAccountAuthenticationStatus"/></returns>
        [HttpPost]
        [Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]")]
        public async Task<ActionResult<DTO.LDAPAccountAuthenticationStatus>> PostAuthenticationAsync(
            [FromRoute] string serverProfile,
            [FromRoute] string catalogType,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestTag,
            [FromBody] DTO.LDAPAccountCredentials accountCredentials)
        {
            var ldapClientConfig = GetLdapClientConfiguration(serverProfile.ToString(), IsGlobalCatalog(catalogType));

            var validationStatus = new DTO.LDAPAccountAuthenticationStatus
            {
                DomainName = accountCredentials.DomainName,
                AccountName = accountCredentials.AccountName,
                RequestTag = requestTag
            };

            var attributeFilter = new AttributeFilter(EntryAttribute.sAMAccountName, new FilterValue(accountCredentials.AccountName));
            var ldapSearcher = await GetLdapSearcher(ldapClientConfig);
            var ldapSearchResult = await ldapSearcher.SearchEntriesAsync(attributeFilter, RequiredEntryAttributes.OnlyObjectSid, null);

            if (ldapSearchResult.Entries.Count() == 0)
            {
                if (ldapSearchResult.HasErrorInfo)
                {
                    throw ldapSearchResult.ErrorObject;
                }
                else
                {
                    throw new ResourceNotFoundException($"The account name {accountCredentials.AccountName} could not be found, verify that the account name exists. The LDAP server profile that was used: {serverProfile}, Global Catalog: {IsGlobalCatalog(catalogType)}, Base DN: [{ldapClientConfig.SearchLimits.BaseDN}]");
                }
            }
            else
            {
                var authenticator = new LDAPHelper.Authenticator(ldapClientConfig.ServerSettings);
                var domainAccountName = $"{accountCredentials.DomainName}\\{accountCredentials.AccountName}";
                var credentialToAuthenticate = new LDAPHelper.Credentials(domainAccountName, accountCredentials.AccountPassword);
                var isAuthenticated = await authenticator.AuthenticateAsync(credentialToAuthenticate);

                //Asignar respuesta de la autenticación
                validationStatus.IsAuthenticated = isAuthenticated;
                validationStatus.Message = isAuthenticated ? "The credentials are valid." : "Wrong Domain or password.";
            }

            return Ok(validationStatus);
        }
    }
}