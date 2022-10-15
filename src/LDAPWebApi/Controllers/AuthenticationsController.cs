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
using Microsoft.Extensions.Logging;

namespace Bitai.LDAPWebApi.Controllers;

/// <summary>
/// Web Api controller to process credentials.
/// </summary>
[Route("api")]
[Authorize(WebApiScopesConfiguration.GlobalScopeAuthorizationPolicyName)]
[ApiController]
public class AuthenticationsController : ApiControllerBase<AuthenticationsController>
{
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="configuration">Injected <see cref="IConfiguration"/></param>
	/// <param name="logger">See <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="serverProfiles">Injected <see cref="Configurations.LDAP. LDAPServerProfiles"/></param>        
	public AuthenticationsController(IConfiguration configuration, ILogger<AuthenticationsController> logger, Configurations.LDAP.LDAPServerProfiles serverProfiles) : base(configuration, logger, serverProfiles) { }



	/// <summary>
	/// Validate Domain Account Name credential.
	/// </summary>
	/// <param name="serverProfile">LDAP Server Profile Id that defines part of the path. See <see cref="Configurations.LDAP.LDAPServerProfile"/></param>
	/// <param name="catalogType">Name of the LDAP catalog that defines part of the path. See <see cref="DTO.LDAPCatalogTypes"/></param>
	/// <param name="credential">Account credentials to validate. See <see cref="DTO.LDAPAccountCredentials"/></param>
	/// <param name="requestTag">Valor personalizado para etiquetar la respuesta. Can e null</param>
	/// <returns><see cref="DTO.LDAPAccountAuthenticationStatus"/></returns>
	[HttpPost]
	[Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]")]
	public async Task<ActionResult<LDAPDomainAccountAuthenticationResult>> PostAuthenticationAsync(
		[FromRoute] string serverProfile,
		[FromRoute] string catalogType,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestTag,
		[FromBody] LDAPDomainAccountCredential credential)
	{
		Logger.LogInformation($"Request path: {nameof(serverProfile)}={serverProfile}, {nameof(catalogType)}={catalogType}, {nameof(requestTag)}={requestTag}");

		Logger.LogInformation("Request body: {@credentials}", credential.SecureClone());

		var ldapClientConfig = GetLdapClientConfiguration(serverProfile.ToString(), IsGlobalCatalog(catalogType));

		var attributeFilter = new AttributeFilter(EntryAttribute.sAMAccountName, new FilterValue(credential.AccountName));
		var searcher = GetLdapSearcher(ldapClientConfig);
		var searchResult = await searcher.SearchEntriesAsync(attributeFilter, RequiredEntryAttributes.OnlyObjectSid, null);
		if (!searchResult.IsSuccessfulOperation)
		{
			Logger.LogError($"Failed to authenticate {credential.DomainAccountName} account.");

			if (searchResult.HasErrorObject)
				throw new Exception(searchResult.OperationMessage, searchResult.ErrorObject);
			else
				throw new Exception(searchResult.OperationMessage);
		}

		LDAPDomainAccountAuthenticationResult authenticationResult;
		if (searchResult.Entries.Count() == 0)
		{
			authenticationResult = new LDAPDomainAccountAuthenticationResult(credential.SecureClone(), false, requestTag);
			authenticationResult.SetSuccessfullOperation($"The domain user account {credential.DomainName}\\{credential.AccountName} could not be found.");

			return authenticationResult;
		}
		else if (searchResult.Entries.Count() > 1)
		{
			authenticationResult = new LDAPDomainAccountAuthenticationResult(credential.SecureClone(), false, requestTag);
			authenticationResult.SetSuccessfullOperation($"Multiple {credential.DomainName}\\{credential.AccountName} accounts were found. Accounts must be unique. Verify the parameters with which the search for user accounts is carried out.");

			return authenticationResult;
		}
		else //Only one LDAP entry found
		{
			var authenticator = new LDAPHelper.Authenticator(ldapClientConfig.ServerSettings);
			
			authenticationResult = await authenticator.AuthenticateAsync(credential, requestTag);
			if (!authenticationResult.IsSuccessfulOperation)
			{
				Logger.LogError("Failed to authenticate user account {@credential}.", credential.DomainAccountName);

				if (authenticationResult.HasErrorObject)
					throw authenticationResult.ErrorObject;
				else
					throw new Exception(authenticationResult.OperationMessage);
			}

			Logger.LogInformation("Response body: {@result}", authenticationResult);

			return Ok(authenticationResult);
		}
	}
}