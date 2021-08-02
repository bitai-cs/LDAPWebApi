using System;
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
using Serilog;

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
        public AuthenticationsController(IConfiguration configuration, ILogger logger, Configurations.LDAP.LDAPServerProfiles serverProfiles) : base(configuration, logger, serverProfiles) { }



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
            Logger.Information("{0}", nameof(AuthenticationsController.PostAuthenticationAsync));

            var ldapClientConfig = GetLdapClientConfiguration(serverProfile.ToString(), IsGlobalCatalog(catalogType));

            var accountAuthenticationStatus = new DTO.LDAPAccountAuthenticationStatus
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
                    accountAuthenticationStatus.IsAuthenticated = false;
                    accountAuthenticationStatus.Message = $"The account name {accountCredentials.AccountName} could not be found, verify that the account name exists.";
                }
            }
            else
            {
                var authenticator = new LDAPHelper.Authenticator(ldapClientConfig.ServerSettings);
                var domainAccountName = $"{accountCredentials.DomainName}\\{accountCredentials.AccountName}";
                var credentialToAuthenticate = new LDAPHelper.Credentials(domainAccountName, accountCredentials.AccountPassword);
                var isAuthenticated = await authenticator.AuthenticateAsync(credentialToAuthenticate);

                accountAuthenticationStatus.IsAuthenticated = isAuthenticated;
                accountAuthenticationStatus.Message = isAuthenticated ? "The credentials are valid." : "Wrong Domain or password.";
            }

            return Ok(accountAuthenticationStatus);
        }
    }
}