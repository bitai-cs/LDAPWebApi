using System;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Bitai.LDAPHelper.DTO;
using Bitai.LDAPWebApi.DTO;
using Bitai.WebApi.Client;
using Serilog;
using Serilog.Sinks.SystemConsole;

namespace Bitai.LDAPWebApi.Clients.Demo
{
	public class Program
	{
		static string WebApiBaseUrl = "http://10.100.54.40:8077/Visiva.LDAPWebApi";
		//static string WebApiBaseUrl = "https://localhost:5101";

		static bool WebApiRequiresAccessToken = false;

		static WebApiClientCredential ClientCredentials = new WebApiClientCredential
		{
			AuthorityUrl = "https://localhost:44310",
			ApiScope = "Bitai.LdapWebApi.Scope.Global",
			ClientId = "Is4.Sts.LdapWebApi.Client",
			ClientSecret = "232459a4-747c-6e0e-2516-72aba52a7069"
		};

		static string RequestLabel { get; set; } = "DEMO";

		static string Selected_LDAPServerProfile { get; set; } = "CERTUS";




		static async Task Main(string[] args)
		{
			try
			{
				ConfigLogger();

				Console.WriteLine("Presione la tecla enter para iniciar el demo...");
				Console.ReadLine();

				await ServerProfilesClient_GetProfileIdsAsync();

				await ServerProfilesClient_GetAllAsync();

				await CatalogTypesClient_GetAllAsync();

				await DirectoryClient_CreateMsADUserAccountAsync();

				await DirectoryClient_SetMsADUserAccountPassword();

				await AuthenticationsClient_AccountAuthenticationAsync();

				await DirectoryClient_SearchByIdentifierAsync();

				await UserDirectoryClient_FilterByIdentifierAsync("vbastidas");

				await UserDirectoryClient_FilterByIdentifierAsync("??????");

				await UserDirectoryClient_SetPasswordAsync();
			}
			catch (Exception ex)
			{
				LogError(ex);
			}
			finally
			{
				Console.ReadLine();
			}
		}




		static async Task CatalogTypesClient_GetAllAsync()
		{
			try
			{
				var client = new LDAPCatalogTypesWebApiClient(WebApiBaseUrl, ClientCredentials);

				LogInfoOfType(client.GetType());

				LogInfo($"{nameof(client.GetAllAsync)}...");
				var httpResponse = await client.GetAllAsync(WebApiRequiresAccessToken);
				if (!httpResponse.IsSuccessResponse)
				{
					client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
				}
				else
				{
					var dto = await client.GetDTOFromResponseAsync<DTO.LDAPServerCatalogTypes>(httpResponse);

					LogInfo($"{nameof(DTO.LDAPServerCatalogTypes.LocalCatalog)}: {dto.LocalCatalog}");
					LogInfo($"{nameof(DTO.LDAPServerCatalogTypes.GlobalCatalog)}: {dto.GlobalCatalog}");

					LogInfo("Set Parameters.CatalogTypes property values.");
				}
			}
			catch (WebApiRequestException ex)
			{
				LogWebApiRequestError(ex);
			}
		}

		static async Task DirectoryClient_CreateMsADUserAccountAsync()
		{
			try
			{
				var client = new LDAPDirectoryWebApiClient(WebApiBaseUrl, Selected_LDAPServerProfile, false, ClientCredentials);

				LogInfoOfType(client.GetType());
				NewBlankLines(1);

				var newUserAccount = new LDAPMsADUserAccount
				{
					DistinguishedNameOfContainer = "OU=TEST_RPA_MDA,OU=ADM,OU=CERTUS,DC=certus,DC=edu,DC=pe",
					Cn = "Victor German Bastidas Gonzales",
					DisplayName = "Victor German Bastidas Gonzales (SDN)",
					ObjectClass = new string[] { "user" },
					SAMAccountName = "vbastidas01",
					UserAccountControl = "NORMAL_ACCOUNT,DONT_EXPIRE_PASSWORD",
					Password = "rpa2023@@"
				};
				LogInfo(newUserAccount);
				NewBlankLines(1);

				LogInfo($"{nameof(client.CreateMsADUserAccountAsync)}...");
				var httpResponse = await client.CreateMsADUserAccountAsync(newUserAccount, RequestLabel, WebApiRequiresAccessToken);
				if (!httpResponse.IsSuccessResponse)
				{
					client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
				}
				else
				{
					var result = await client.GetDTOFromResponseAsync<LDAPHelper.DTO.LDAPCreateMsADUserAccountResult>(httpResponse);

					LogInfo(result);
				}
			}
			catch (WebApiRequestException ex)
			{
				LogWebApiRequestError(ex);
			}
		}

		static async Task DirectoryClient_SetMsADUserAccountPassword()
		{
			try
			{
				var client = new LDAPDirectoryWebApiClient(WebApiBaseUrl, Selected_LDAPServerProfile, false, ClientCredentials);

				LogInfoOfType(client.GetType());
				NewBlankLines(1);

				var credential = new LDAPCredential
				{
					UserAccount = "CERTUS\\vbastidas01",
					Password = "17viko@@"
				};
				LogInfo(credential);
				NewBlankLines(1);

				LogInfo($"{nameof(client.SetMsADUserAccountPassword)}...");
				var httpResponse = await client.SetMsADUserAccountPassword("vbastidas01", credential, EntryAttribute.sAMAccountName, RequestLabel, WebApiRequiresAccessToken);
				if (!httpResponse.IsSuccessResponse)
				{
					client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
				}
				else
				{
					var result = await client.GetDTOFromResponseAsync<LDAPHelper.DTO.LDAPPasswordUpdateResult>(httpResponse);

					LogInfo(result);
				}
			}
			catch (WebApiRequestException ex)
			{
				LogWebApiRequestError(ex);
			}
		}

		static async Task AuthenticationsClient_AccountAuthenticationAsync()
		{
			try
			{
				var client = new LDAPAuthenticationsWebApiClient(WebApiBaseUrl, Selected_LDAPServerProfile, false, ClientCredentials);

				LogInfoOfType(client.GetType());

				Console.WriteLine($"Enter the password of the CERTUS\\vbastidas account.");
				Console.WriteLine("(It is not nescessary to enter the real password)");

				var accountCredentials = new LDAPHelper.DTO.LDAPDomainAccountCredential("CERTUS", "vbastidas", requestAccountPassword("CERTUS\\vbastidas"));

				LogInfo($"{nameof(client.AccountAuthenticationAsync)}...");
				var httpResponse = await client.AccountAuthenticationAsync(accountCredentials, RequestLabel, WebApiRequiresAccessToken);
				if (!httpResponse.IsSuccessResponse)
				{
					client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
				}
				else
				{
					var result = await client.GetDTOFromResponseAsync<LDAPHelper.DTO.LDAPDomainAccountAuthenticationResult>(httpResponse);

					LogInfo(result);
				}
			}
			catch (WebApiRequestException ex)
			{
				LogWebApiRequestError(ex);
			}
		}

		static async Task DirectoryClient_SearchByIdentifierAsync()
		{
			try
			{
				var client = new LDAPDirectoryWebApiClient(WebApiBaseUrl, Selected_LDAPServerProfile, false, ClientCredentials);

				LogInfoOfType(client.GetType());

				LogInfo($"{nameof(client.SearchByIdentifierAsync)}...");
				var httpResponse = await client.SearchByIdentifierAsync("vbastidas", RequestLabel, false);
				if (!httpResponse.IsSuccessResponse)
				{
					client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
				}
				else
				{
					var result = await client.GetDTOFromResponseAsync<LDAPHelper.DTO.LDAPSearchResult>(httpResponse);

					LogInfo(result);
				}
			}
			catch (WebApiRequestException ex)
			{
				LogWebApiRequestError(ex);
			}
		}

		static async Task UserDirectoryClient_FilterByIdentifierAsync(string samAccountName)
		{
			try
			{
				var client = new LDAPUserDirectoryWebApiClient(WebApiBaseUrl, Selected_LDAPServerProfile, false, ClientCredentials);

				LogInfoOfType(client.GetType());

				LogInfo($"{nameof(client.SearchFilteringByAsync)}...");
				var httpResponse = await client.SearchFilteringByAsync(samAccountName, false, RequestLabel);
				if (!httpResponse.IsSuccessResponse)
				{
					client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
				}
				else
				{
					var result = await client.GetDTOFromResponseAsync<LDAPHelper.DTO.LDAPSearchResult>(httpResponse);

					LogInfo($"Se encontró {result.Entries.Count()} registro(s).");

					LogInfo(result);
				}
			}
			catch (WebApiRequestException ex)
			{
				LogWebApiRequestError(ex);
			}
		}

		static async Task UserDirectoryClient_SetPasswordAsync()
		{
			try
			{
				var client = new LDAPUserDirectoryWebApiClient(WebApiBaseUrl, Selected_LDAPServerProfile, false, ClientCredentials);

				LogInfoOfType(client.GetType());

				var credential = new LDAPCredential("000127868", "1A2B3C");

				LogInfo($"{nameof(client.SetPasswordAsync)}...");
				var httpResponse = await client.SetPasswordAsync(credential, false, RequestLabel);
				if (!httpResponse.IsSuccessResponse)
				{
					client.ThrowClientRequestException("Error al resetear la contraseña", httpResponse);
				}
				else
				{
					var result = await client.GetDTOFromResponseAsync<LDAPHelper.DTO.LDAPPasswordUpdateResult>(httpResponse);

					LogInfo(result);
				}
			}
			catch (WebApiRequestException ex)
			{
				LogWebApiRequestError(ex);
			}
		}

		static async Task ServerProfilesClient_GetProfileIdsAsync()
		{
			try
			{
				var client = new LDAPServerProfilesWebApiClient(WebApiBaseUrl, ClientCredentials);

				LogInfoOfType(client.GetType());

				LogInfo($"{nameof(LDAPServerProfilesWebApiClient.GetProfileIdsAsync)}...");
				var httpResponse = await client.GetProfileIdsAsync(WebApiRequiresAccessToken);
				if (!httpResponse.IsSuccessResponse)
				{
					client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
				}
				else
				{
					var enumerableDTO = await client.GetEnumerableDTOFromResponseAsync<string>(httpResponse);
					foreach (var profileId in enumerableDTO)
					{
						LogInfo($"ProfileId: {profileId}");
					}
				}
			}
			catch (WebApiRequestException ex)
			{
				LogWebApiRequestError(ex);
			}
		}

		static async Task ServerProfilesClient_GetAllAsync()
		{
			try
			{
				var client = new LDAPServerProfilesWebApiClient(WebApiBaseUrl, ClientCredentials);

				LogInfoOfType(client.GetType());

				LogInfo($"{nameof(client.GetAllAsync)}...");
				var httpResponse = await client.GetAllAsync(WebApiRequiresAccessToken);
				if (!httpResponse.IsSuccessResponse)
				{
					client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
				}
				else
				{
					var enumerableDTO = await client.GetEnumerableDTOFromResponseAsync<DTO.LDAPServerProfile>(httpResponse);
					foreach (var ldapServerProfile in enumerableDTO)
					{
						LogInfo($"ProfileId: {ldapServerProfile.ProfileId}");
						LogInfo($"Server: {ldapServerProfile.Server}");
						LogInfo($"Port: {ldapServerProfile.Port}");
						LogInfo($"PortForGlobalCatalog: {ldapServerProfile.PortForGlobalCatalog}");
						LogInfo($"BaseDN: {ldapServerProfile.BaseDN}");
						LogInfo($"BaseDNforGlobalCatalog: {ldapServerProfile.BaseDNforGlobalCatalog}");
						LogInfo($"UseSSL: {ldapServerProfile.UseSSL}");
						LogInfo($"UseSSLforGlobalCatalog: {ldapServerProfile.UseSSLforGlobalCatalog}");
						LogInfo($"ConnectionTimeout: {ldapServerProfile.ConnectionTimeout}");
						LogInfo($"DomainUserAccount: {ldapServerProfile.DomainUserAccount}");
						Console.WriteLine();
					}
				}
			}
			catch (WebApiRequestException ex)
			{
				LogWebApiRequestError(ex);
			}
		}

		static void ConfigLogger()
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console()
				.CreateLogger();
		}

		static void NewBlankLines(int lines)
		{
			for (var _l = 0; _l < lines; _l++)
				Console.WriteLine();
		}

		static void LogInfoOfCaller([CallerMemberName] string? memberName = null)
		{
			if (memberName == null)
				throw new Exception("The name of the calling member cannot be identified!");

			Log.Information("{0}...", memberName.ToUpper());
		}

		static void LogInfoOfType(Type type)
		{
			Console.WriteLine();
			Log.Information("{0} **************************", type.FullName);
		}

		static void LogInfo(string message)
		{
			Log.Information(message);
		}

		static void LogInfo<T>(T model)
		{
			Log.Information<T>("{@result}", model);
		}

		static void LogWarning(string message)
		{
			Log.Warning(message);
		}

		static void LogError(string message)
		{
			Log.Error(message);
		}

		static void LogError(Exception ex)
		{
			Log.Error($"Message: {ex.Message}");
			Log.Error($"Source: {ex.Source}");
			Log.Error(ex.StackTrace ?? string.Empty);
			Console.WriteLine();
			if (ex.InnerException == null) return;

			Log.Error($"Message: {ex.InnerException.Message}");
			Log.Error($"Source: {ex.InnerException.Source}");
			Log.Error(ex.InnerException.StackTrace ?? string.Empty);
			Console.WriteLine();
			if (ex.InnerException.InnerException == null) return;

			Log.Error($"Message: {ex.InnerException.InnerException.Message}");
			Log.Error($"Source: {ex.InnerException.InnerException.Message}");
			Log.Error(ex.InnerException.InnerException.StackTrace ?? string.Empty);
			Console.WriteLine();
		}

		static void LogError<T>(T model)
		{
			Log.Error<T>("{@result}", model);
		}

		static void LogWebApiRequestError(WebApiRequestException ex)
		{
			LogInfoOfCaller();

			LogError(ex);

			//ex.NoSuccessResponse.ContentMediaType

			NewBlankLines(1);

			LogError("Below a resume of the error:");

			NewBlankLines(1);

			LogError($"Exception type: {ex.GetType().FullName}");
			LogError($"Error message: {ex.Message}");

			var typeOfContent = ex.NoSuccessResponse.GetType();
			LogError($"Type of response content: {typeOfContent.FullName}");

			if (typeOfContent == typeof(NoSuccessResponseWithJsonStringContent))
			{
				var noSuccessResponse = (NoSuccessResponseWithJsonStringContent)ex.NoSuccessResponse;

				LogError(noSuccessResponse.ReasonPhrase);
				LogError(((int)noSuccessResponse.HttpStatusCode).ToString());
				LogError(noSuccessResponse.ContentMediaType.ToString());
				LogError(noSuccessResponse.Content);
			}
			else if (typeOfContent == typeof(NoSuccessResponseWithJsonExceptionContent))
			{
				var noSuccessResponse = (NoSuccessResponseWithJsonExceptionContent)ex.NoSuccessResponse;

				LogError(noSuccessResponse.ReasonPhrase);
				LogError(((int)noSuccessResponse.HttpStatusCode).ToString());

				var exceptionJsonFormat = noSuccessResponse.Content;
				while (exceptionJsonFormat != null)
				{
					LogError(exceptionJsonFormat.GetType().FullName ?? throw new Exception("The type of error contained in the response cannot be identified!"));
					LogError(exceptionJsonFormat.Message);
					LogError(exceptionJsonFormat.Source);
					LogError(exceptionJsonFormat.StackTrace);

					NewBlankLines(1);

					exceptionJsonFormat = exceptionJsonFormat.InnerMiddlewareException;
				}
			}
			else if (typeOfContent == typeof(NoSuccessResponseWithHtmlContent))
			{
				var noSuccessResponse = (NoSuccessResponseWithHtmlContent)ex.NoSuccessResponse;

				LogError(noSuccessResponse.ReasonPhrase);
				LogError(((int)noSuccessResponse.HttpStatusCode).ToString());
				LogError(noSuccessResponse.Content);
			}
			else if (typeOfContent == typeof(NoSuccessResponseWithEmptyContent))
			{
				var noSuccessResponse = (NoSuccessResponseWithEmptyContent)ex.NoSuccessResponse;

				LogError(noSuccessResponse.ReasonPhrase);
				LogError(((int)noSuccessResponse.HttpStatusCode).ToString());
				LogError(noSuccessResponse.WebServer);
				LogError(noSuccessResponse.Date);
			}
		}

		private static string requestAccountPassword(string account)
		{
		REQUEST_PASSWORD:

			Console.WriteLine($"ENTER PASSWORD FOR {account}:");

			var password = readPassword('*');

			if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password)) goto REQUEST_PASSWORD;

			Log.Information($"Password entered.");

			return password;
		}

		public static string readPassword(char passwordChar)
		{
			const int _ENTER = 13, _BACKSP = 8, _CTRLBACKSP = 127;
			int[] _FILTERED = { 0, 27, 9, 10 /*, 32 space, if you care */ }; // const

			var _pass = new Stack<char>();
			char _chr = (char)0;

			while ((_chr = Console.ReadKey(true).KeyChar) != _ENTER)
			{
				if (_chr == _BACKSP)
				{
					if (_pass.Count > 0)
					{
						System.Console.Write("\b \b");
						_pass.Pop();
					}
				}
				else if (_chr == _CTRLBACKSP)
				{
					while (_pass.Count > 0)
					{
						System.Console.Write("\b \b");
						_pass.Pop();
					}
				}
				else if (_FILTERED.Count(x => _chr == x) > 0)
				{
					//Nothing to do
				}
				else
				{
					_pass.Push((char)_chr);
					System.Console.Write(passwordChar);
				}
			}

			System.Console.WriteLine();

			return new string(_pass.Reverse().ToArray());
		}
	}
}