using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bitai.LDAPHelper.DTO;
using Bitai.LDAPHelper.QueryFilters;
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
    [Authorize]
    [ApiController]
    public class CredentialsController : ApiControllerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">Injected <see cref="IConfiguration"/></param>
        /// <param name="serverProfiles">Injected <see cref="Configurations.LDAP. LDAPServerProfiles"/></param>        
        public CredentialsController(IConfiguration configuration, Configurations.LDAP.LDAPServerProfiles serverProfiles) : base(configuration, serverProfiles) { }



        /// <summary>
        /// Validate Domain Account Name credential.
        /// </summary>
        /// <param name="serverProfile">LDAP Server Profile Id that defines part of the path. See <see cref="Configurations.LDAP.LDAPServerProfile"/></param>
        /// <param name="catalogType">Name of the LDAP catalog that defines part of the path. See <see cref="DTO.LDAPCatalogTypes"/></param>
        /// <param name="accountName">Account Name that defines part of the path.</param>
        /// <param name="accountCredential">Account credentials to validate. See <see cref="DTO.LDAPAccountCredentials"/></param>
        /// <param name="requestTag">Valor personalizado para etiquetar la respuesta. Can e null</param>
        /// <returns><see cref="DTO.LDAPAccountAuthenticationStatus"/></returns>
        [HttpPost]
        [Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/{accountName}/[action]")]
        [ActionName("Authentication")]
        public async Task<ActionResult<DTO.LDAPAccountAuthenticationStatus>> PostAuthenticationAsync(
            [FromRoute] string serverProfile,
            [FromRoute] string catalogType,
            [FromRoute] string accountName,
            [FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestTag,
            [FromBody] DTO.LDAPAccountCredentials accountCredential)
        {
            if (accountName != accountCredential.AccountName)
                throw new Exception($"{nameof(accountName)}:{accountName} route does not match the {nameof(DTO.LDAPAccountCredentials.AccountName)}:{accountCredential.AccountName} provided in the credential. They must be the same.");

            var ldapClientConfig = GetLdapClientConfiguration(serverProfile.ToString(), IsGlobalCatalog(catalogType));

            var validationStatus = new DTO.LDAPAccountAuthenticationStatus
            {
                DomainName = accountCredential.DomainName,
                AccountName = accountCredential.AccountName,
                RequestTag = requestTag
            };

            var attributeFilter = new AttributeFilter(EntryAttribute.sAMAccountName, new FilterValue(accountName));
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
                    throw new ResourceNotFoundException($"The account name {accountName} could not be found, verify that the account name exists. The LDAP server profile that was used: {serverProfile}, Global Catalog: {IsGlobalCatalog(catalogType)}, Base DN: [{ldapClientConfig.SearchLimits.BaseDN}]");
                }
            }
            else
            {
                var authenticator = new LDAPHelper.Authenticator(ldapClientConfig.ServerSettings);
                var domainAccountName = $"{accountCredential.DomainName}\\{accountCredential.AccountName}";
                var credentialToAuthenticate = new LDAPHelper.Credentials(domainAccountName, accountCredential.AccountPassword);
                var isAuthenticated = await authenticator.AuthenticateAsync(credentialToAuthenticate);

                //Asignar respuesta de la autenticación
                validationStatus.IsAuthenticated = isAuthenticated;
                validationStatus.Message = isAuthenticated ? "The credentials are valid." : "Wrong Domain or password.";
            }

            return Ok(validationStatus);
        }
    }
}