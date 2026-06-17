using System;

namespace Bitai.LDAPWebApi.DTO;

/// <summary>
/// Represents the status of the authentication process for the
/// credentials of a user account.
/// </summary>
public class LWADomainAccountAuthenticationResult: LWAOperationResult
{
	public LDAPHelper.DTO.LDAPDomainAccountCredential Credential { get; set; }

	/// <summary>
	/// Value indicating whether the account was able to authenticate
	/// to the LDAP server.
	/// </summary>
	public bool IsAuthenticated { get; set; }




	/// <summary>
	/// Default constructor.
	/// </summary>
	public LWADomainAccountAuthenticationResult()
	{
		//Do not remove this constructor, it is required to deserialize data.

        Credential = new LDAPHelper.DTO.LDAPDomainAccountCredential();
	}

	public LWADomainAccountAuthenticationResult(LDAPHelper.DTO.LDAPDomainAccountAuthenticationResult model)
	{
		this.Credential = model.Credential;
		this.IsAuthenticated = model.IsAuthenticated;
        this.OperationMessage = model.OperationMessage;
        if (model.HasErrorObject)
        {
            this.ErrorObject = new LWAOperationExceptionInfo(model.ErrorObject);
            this.ErrorType = model.ErrorType;
        }
        this.IsSuccessfulOperation = model.IsSuccessfulOperation;
        this.RequestLabel = model.RequestLabel;
	}
}
