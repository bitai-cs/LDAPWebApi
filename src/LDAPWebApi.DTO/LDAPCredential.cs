using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Bitai.LDAPWebApi.DTO;

/// <summary>
/// Class that represents the credentials of a user.
/// </summary>
public class LDAPCredential
{
	public string UserAccount { get; set; }

	public string Password { get; set; }



	/// <summary>
	/// Constructor
	/// </summary>
	public LDAPCredential()
	{
		UserAccount = string.Empty;
		Password = string.Empty;
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="userAccount"></param>
	/// <param name="password"></param>
	public LDAPCredential(string userAccount, string password) : this()
	{
		UserAccount = userAccount;
		Password = password;
	}



	public static bool Validate([NotNull] ref LDAPCredential credential, bool passwordRequired, out string? validations)
	{
		validations = null;

		var isValid = true;

		if (string.IsNullOrEmpty(credential.UserAccount))
		{
			validations += $"{nameof(credential.UserAccount)} can not be empty or null. ";
			isValid = false;
		}

		if (passwordRequired && string.IsNullOrEmpty(credential.Password))
		{
			validations += $"{nameof(credential.Password)} can not be emtpty or null. ";
			isValid = false;
		}

		if (!string.IsNullOrEmpty(credential.UserAccount))
		{
			var strings = credential.UserAccount.Split('\\', StringSplitOptions.None);
			if (strings.Length > 2)
			{
				validations += $"{nameof(credential.UserAccount)}: {credential.UserAccount} is not valid.";
				isValid = false;
			}
		}

		return isValid;
	}
}
