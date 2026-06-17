using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Bitai.LDAPHelper.DTO;
using Bitai.LDAPWebApi.DTO;
using Bitai.WebApi.Client;
using Serilog;

namespace Bitai.LDAPWebApi.Clients.Demo
{
    public class Program
    {
        static readonly string GeneratedCode = GenerateRamdonCode();
        static readonly string Selected_DemoUser_Cn = $"Demo User {GeneratedCode}";
        static readonly string Selected_DemoUser_samAccountName = $"demo.user.{GeneratedCode}";

        // Used only when there are problems with the SSL certificate of the Web API server. It allows to bypass the SSL certificate validation, but it should not be used in production environments.
        static HttpClientHandler clientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };

        static string WebApiBaseUrl = "https://localhost:5101";

        static WebApiClientCredential ClientCredentials = new WebApiClientCredential
        {
            AuthorityUrl = "https://localhost/IsSts9",
            ApiScope = "Bitai.LdapWebApi.Local.ApiScope.Reader",
            ClientId = "Bitai.LdapWebApi.Local.Machine.Client",
            ClientSecret = "Bitai.LdapWebApi.Local.Machine.Client"
        };

        static bool UseAccessTokenToCallLdapWebApi = false;

        static string RequestLabel { get; set; } = "DEMO";

        static string Selected_LDAPServerProfile { get; set; } = "BITAIVA";




        static async Task Main(string[] args)
        {
            try
            {
                //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                ConfigLogger();

                Console.WriteLine("Presione la tecla enter para iniciar el demo...");
                Console.ReadLine();

                Log.Warning($"{nameof(Selected_LDAPServerProfile)}: {Selected_LDAPServerProfile}");

                await ServerProfilesClient_GetProfileIdsAsync();

                await ServerProfilesClient_GetAllAsync();

                await CatalogTypesClient_GetAllAsync();

                await UserDirectoryClient_CreateMsADUserAccountAsync();

                await UserDirectoryClient_SetMsADUserAccountPassword();

                await AuthenticationsClient_AccountAuthenticationAsync();

                await UserDirectoryClient_DisableMsADUserAccount();

                await UserDirectoryClient_RemoveMsADUserAccount();

                await DirectoryClient_SearchByIdentifierAsync("victor.bastidas");

                await UserDirectoryClient_FilterByIdentifierAsync("victor.bastidas");

                await UserDirectoryClient_FilterByIdentifierAsync("victor.bastidas");
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
                var httpResponse = await client.GetAllAsync(UseAccessTokenToCallLdapWebApi);
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

        static async Task UserDirectoryClient_CreateMsADUserAccountAsync()
        {
            try
            {
                var client = new LDAPUserDirectoryWebApiClient(WebApiBaseUrl, Selected_LDAPServerProfile, false, ClientCredentials);

                LogInfoOfType(client.GetType());
                NewBlankLines(1);

                var newUserAccount = new LDAPMsADUserAccount
                {
                    DistinguishedNameOfContainer = "CN=Users,DC=va,DC=bitai,DC=com",
                    Cn = Selected_DemoUser_Cn,
                    DisplayName = $"{Selected_DemoUser_Cn} (Test)",
                    ObjectClass = new string[] { "user" },
                    SAMAccountName = Selected_DemoUser_samAccountName,
                    UserAccountControl = "NORMAL_ACCOUNT,DONT_EXPIRE_PASSWORD",
                    Password = "P@ssw0rd"
                };

                LogInfo(newUserAccount);
                NewBlankLines(1);

                LogInfo($"{nameof(client.CreateMsADUserAccountAsync)}...");
                var httpResponse = await client.CreateMsADUserAccountAsync(newUserAccount, RequestLabel, UseAccessTokenToCallLdapWebApi);
                if (!httpResponse.IsSuccessResponse)
                {
                    client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
                }
                else
                {
                    var result = await client.GetDTOFromResponseAsync<LWACreateMsADUserAccountResult>(httpResponse);

                    LogInfo(result);

                    Console.WriteLine("MS AD User created. You can verify it. Press Enter to continue...");
                    Console.ReadLine();
                }
            }
            catch (WebApiRequestException ex)
            {
                LogWebApiRequestError(ex);
            }
        }

        static async Task UserDirectoryClient_SetMsADUserAccountPassword()
        {
            try
            {
                Console.WriteLine($"This will CHANGE THE PASSWORD for the user account with {EntryAttribute.sAMAccountName} = {Selected_DemoUser_samAccountName}. Do you want to continue? n=No,other=Yes");
                var response = Console.ReadLine() ?? string.Empty;
                if (response.Equals("n", StringComparison.OrdinalIgnoreCase))
                    return;

                var client = new LDAPUserDirectoryWebApiClient(WebApiBaseUrl, Selected_LDAPServerProfile, false, ClientCredentials);

                LogInfoOfType(client.GetType());
                NewBlankLines(1);

                Console.WriteLine($"Enter the new password for {Selected_DemoUser_samAccountName}:");

                var credential = new LDAPCredential
                {
                    //UserAccount = $"{Selected_LDAPServerProfile}\\victor.bastidas",
                    UserAccount = Selected_DemoUser_samAccountName,
                    Password = readPassword('*')
                };
                LogInfo(credential);
                NewBlankLines(1);

                LogInfo($"{nameof(client.SetMsADUserAccountPasswordAsync)}...");
                var httpResponse = await client.SetMsADUserAccountPasswordAsync(credential, EntryAttribute.sAMAccountName, RequestLabel, UseAccessTokenToCallLdapWebApi);
                if (!httpResponse.IsSuccessResponse)
                {
                    client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
                }
                else
                {
                    var result = await client.GetDTOFromResponseAsync<LWAPasswordUpdateResult>(httpResponse);

                    LogInfo(result);

                    Console.WriteLine("The password for MS AD User was set. You can verify it. Press Enter to continue...");
                    Console.ReadLine();
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

                Console.WriteLine("Enter the domain user account to authenticate (e.g. HOLDING\\victor.bastidas):");
                var userAccount = Console.ReadLine();
                if (string.IsNullOrEmpty(userAccount) || string.IsNullOrWhiteSpace(userAccount))
                {
                    LogError("The user account cannot be empty or null.");
                    return;
                }
                if (userAccount.IndexOf('\\') < 0)
                {
                    LogError("The user account must be in the format DOMAIN\\username.");
                    return;
                }
                var userAccountParts = userAccount.Split('\\', StringSplitOptions.RemoveEmptyEntries);
                if (userAccountParts.Length != 2)
                {
                    LogError("The user account must be in the format DOMAIN\\username.");
                    return;
                }

                var enteredPassword = requestAccountPassword(userAccount);

                var accountCredentials = new LDAPDomainAccountCredential(userAccountParts[0], userAccountParts[1], enteredPassword);

                LogInfo($"{nameof(client.AuthenticateAsync)}...");
                var httpResponse = await client.AuthenticateAsync(accountCredentials, RequestLabel, UseAccessTokenToCallLdapWebApi);
                if (!httpResponse.IsSuccessResponse)
                {
                    client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
                }
                else
                {
                    var result = await client.GetDTOFromResponseAsync<LWADomainAccountAuthenticationResult>(httpResponse);

                    LogInfo(result);
                }
            }
            catch (WebApiRequestException ex)
            {
                LogWebApiRequestError(ex);
            }
        }

        static async Task UserDirectoryClient_DisableMsADUserAccount()
        {
            try
            {
                Console.WriteLine($"This will disable the user account with {EntryAttribute.sAMAccountName} = {Selected_DemoUser_samAccountName}. Do you want to continue? n=No,other=Yes");
                var response = Console.ReadLine() ?? string.Empty;
                if (response.Equals("n", StringComparison.OrdinalIgnoreCase))
                    return;

                var client = new LDAPUserDirectoryWebApiClient(WebApiBaseUrl, Selected_LDAPServerProfile, false, ClientCredentials);

                LogInfoOfType(client.GetType());
                NewBlankLines(1);

                LogInfo($"{nameof(client.DisableMsADUserAccountAsync)}...");
                var httpResponse = await client.DisableMsADUserAccountAsync(Selected_DemoUser_samAccountName, EntryAttribute.sAMAccountName, RequestLabel, UseAccessTokenToCallLdapWebApi);
                if (!httpResponse.IsSuccessResponse)
                {
                    client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
                }
                else
                {
                    var result = await client.GetDTOFromResponseAsync<LWADisableUserAccountOperationResult>(httpResponse);

                    LogInfo(result);

                    Console.WriteLine("MS AD User disabled. You can verify it. Press Enter to continue...");
                    Console.ReadLine();
                }
            }
            catch (WebApiRequestException ex)
            {
                LogWebApiRequestError(ex);
            }
        }

        static async Task UserDirectoryClient_RemoveMsADUserAccount()
        {
            try
            {
                Console.WriteLine($"This will REMOVE the user account with {EntryAttribute.sAMAccountName} = {Selected_DemoUser_samAccountName}. Do you want to continue? n=No, other=Yes");
                var response = Console.ReadLine() ?? string.Empty;
                if (response.Equals("n", StringComparison.OrdinalIgnoreCase))
                    return;

                var client = new LDAPUserDirectoryWebApiClient(WebApiBaseUrl, Selected_LDAPServerProfile, false, ClientCredentials);

                LogInfoOfType(client.GetType());
                NewBlankLines(1);

                LogInfo($"{nameof(client.RemoveMsADUserAccountAsync)}...");
                var httpResponse = await client.RemoveMsADUserAccountAsync(Selected_DemoUser_samAccountName, EntryAttribute.sAMAccountName, RequestLabel, UseAccessTokenToCallLdapWebApi);
                if (!httpResponse.IsSuccessResponse)
                {
                    client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
                }
                else
                {
                    var result = await client.GetDTOFromResponseAsync<LWARemoveMsADUserAccountResult>(httpResponse);

                    LogInfo(result);

                    Console.WriteLine("MS AD User REMOVED. You can verify it. Press Enter to continue...");
                    Console.ReadLine();
                }
            }
            catch (WebApiRequestException ex)
            {
                LogWebApiRequestError(ex);
            }
        }

        static async Task DirectoryClient_SearchByIdentifierAsync(string identifier)
        {
            try
            {
                var client = new LDAPDirectoryWebApiClient(WebApiBaseUrl, Selected_LDAPServerProfile, false, ClientCredentials);

                LogInfoOfType(client.GetType());

                LogInfo($"{nameof(client.SearchByIdentifierAsync)}...");
                var httpResponse = await client.SearchByIdentifierAsync(identifier, RequestLabel, UseAccessTokenToCallLdapWebApi);
                if (!httpResponse.IsSuccessResponse)
                {
                    client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
                }
                else
                {
                    var result = await client.GetDTOFromResponseAsync<LWASearchResult>(httpResponse);

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
                var httpResponse = await client.SearchFilteringByAsync(samAccountName, UseAccessTokenToCallLdapWebApi, RequestLabel);
                if (!httpResponse.IsSuccessResponse)
                {
                    client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
                }
                else
                {
                    var result = await client.GetDTOFromResponseAsync<LWASearchResult>(httpResponse);

                    LogInfo($"Se encontró {result.Entries.Count()} registro(s).");

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
                var httpResponse = await client.GetProfileIdsAsync(UseAccessTokenToCallLdapWebApi);
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
                var httpResponse = await client.GetAllAsync(UseAccessTokenToCallLdapWebApi);
                if (!httpResponse.IsSuccessResponse)
                {
                    client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
                }
                else
                {
                    var enumerableDTO = await client.GetEnumerableDTOFromResponseAsync<LDAPServerProfile>(httpResponse);
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

        private static string readPassword(char passwordChar)
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

        private static string GenerateRamdonCode()
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            var sb = new StringBuilder(6);

            for (int i = 0; i < 3; i++)
                sb.Append(letters[RandomNumberGenerator.GetInt32(letters.Length)]);

            for (int i = 0; i < 3; i++)
                sb.Append(digits[RandomNumberGenerator.GetInt32(digits.Length)]);

            return sb.ToString();
        }
    }
}
