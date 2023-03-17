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
/// Web Api controller to process credential.
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
	/// Authenticate with domain user account credential.
	/// </summary>
	/// <param name="serverProfile">LDAP Server Profile Id that defines part of the path. See <see cref="Configurations.LDAP.LDAPServerProfile"/></param>
	/// <param name="catalogType">Name of the LDAP catalog that defines part of the path. See <see cref="DTO.LDAPServerCatalogTypes"/></param>
	/// <param name="credential">Account credential to validate. See <see cref="LDAPDomainAccountCredential"/></param>
	/// <param name="requestLabel">Valor personalizado para etiquetar la respuesta. Can e null</param>
	/// <returns><see cref="LDAPDomainAccountAuthenticationResult"/></returns>
	[HttpPost]
	[Route("{serverProfile:ldapSvrPf}/{catalogType:ldapCatType}/[controller]/[action]")]
	[ActionName("authenticate")]
	public async Task<ActionResult<LDAPDomainAccountAuthenticationResult>> AuthenticateAsync(
		[FromRoute] string serverProfile,
		[FromRoute] string catalogType,
		[FromQuery][ModelBinder(BinderType = typeof(Binders.OptionalQueryStringBinder))] string requestLabel,
		[FromBody] LDAPDomainAccountCredential credential)
	{
		Logger.LogInformation("Request path: {serverProfileRoute}={serverProfile}, {catalogTypeRoute}={catalogType}, {requestLabelQuery}={requestLabel}", nameof(serverProfile), serverProfile, nameof(catalogType), catalogType, nameof(requestLabel), requestLabel);

		Logger.LogInformation("Request body: {@credential}", credential.SecureClone());

		var ldapClientConfig = GetLdapClientConfiguration(serverProfile.ToString(), IsGlobalCatalog(catalogType));

		var authenticator = new LDAPHelper.Authenticator(ldapClientConfig.ServerSettings);

		var authenticationResult = await authenticator.AuthenticateAsync(credential, ldapClientConfig.SearchLimits, ldapClientConfig.DomainAccountCredential, requestLabel);
		if (!authenticationResult.IsSuccessfulOperation)
		{
			Logger.LogError("Failed to authenticate user account {domainAccountName}.", credential.DomainAccountName);

			if (authenticationResult.HasErrorObject)
				throw authenticationResult.ErrorObject;
			else
				throw new Exception(authenticationResult.OperationMessage);
		}

		Logger.LogInformation("Response body: {@authenticationResult}", authenticationResult);

		return Ok(authenticationResult);
	}
}