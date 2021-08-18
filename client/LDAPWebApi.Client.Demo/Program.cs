using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Bitai.WebApi.Client;
using Serilog;
using Serilog.Sinks.SystemConsole;

namespace Bitai.LDAPWebApi.Clients.Demo
{
    class Program
    {
        static string WebApiBaseUrl = "https://localhost:5101";
        static bool WebApiRequiresAccessToken = true;
        static WebApiClientCredentials ClientCredentials = new WebApiClientCredentials
        {
            AuthorityUrl = "https://localhost:44310",
            ApiScope = "Bitai.LdapWebApi.Scope.Global",
            ClientId = "Is4.Sts.LdapWebApi.Client",
            ClientSecret = "232459a4-747c-6e0e-2516-72aba52a7069"
        };
        static string Tag { get; set; } = "DEMO";
        static string Selected_LDAPServerProfile { get; set; } = "Profile1";


        static async Task Main(string[] args)
        {
            try
            {
                ConfigLogger();

                Console.WriteLine("Presione la tecla enter para iniciar el demo...");
                Console.ReadLine();

                await CatalogTypesClient_GetAllAsync();

                await AuthenticationsClient_AccountAuthenticationAsync();

                await ServerProfilesClient_GetProfileIdsAsync();

                await ServerProfilesClient_GetAllAsync();
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
                    var dto = await client.GetDTOFromResponseAsync<DTO.LDAPCatalogTypes>(httpResponse);

                    LogInfo($"{nameof(DTO.LDAPCatalogTypes.LocalCatalog)}: {dto.LocalCatalog}");
                    LogInfo($"{nameof(DTO.LDAPCatalogTypes.GlobalCatalog)}: {dto.GlobalCatalog}");

                    LogInfo("Set Parameters.CatalogTypes property values.");
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
                var client = new LDAPAuthenticationsWebApiClient(WebApiBaseUrl, Selected_LDAPServerProfile, true, ClientCredentials);

                LogInfoOfType(client.GetType());

                var accountCredentials = new DTO.LDAPAccountCredentials
                {
                    DomainName = "DOMAIN",
                    AccountName = "usr_ext012",
                    AccountPassword = "Pq$",
                };

                LogInfo($"{nameof(client.AccountAuthenticationAsync)}...");
                var httpResponse = await client.AccountAuthenticationAsync(accountCredentials, WebApiRequiresAccessToken);
                if (!httpResponse.IsSuccessResponse)
                {
                    client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
                }
                else
                {
                    var status = await client.GetDTOFromResponseAsync<DTO.LDAPAccountAuthenticationStatus>(httpResponse);

                    LogInfo($"CustomTag:{status.RequestTag} | DomainName:{status.DomainName} | AccountName:{status.AccountName} | Authenticated:{status.IsAuthenticated} | Message:{status.Message}");
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

                LogInfo($"{nameof(client.GetProfileIdsAsync)}...");
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
                        LogInfo($"DomainAccountName: {ldapServerProfile.DomainAccountName}");
                        LogInfo($"DomainAccountPassword: {ldapServerProfile.DomainAccountPassword}");
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

        static void LogInfoOfCaller([CallerMemberName] string memberName = null)
        {
            Log.Information("{0}...", memberName.ToUpper());
        }

        static void LogInfoOfType(Type type)
        {
            Log.Information("{0} **************************", type.FullName);
        }

        static void LogInfo(string message)
        {
            Log.Information(message);
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
            Log.Error(ex.StackTrace);
            Console.WriteLine();
            if (ex.InnerException == null) return;

            Log.Error($"Message: {ex.InnerException.Message}");
            Log.Error($"Source: {ex.InnerException.Source}");
            Log.Error(ex.InnerException.StackTrace);
            Console.WriteLine();
            if (ex.InnerException.InnerException == null) return;

            Log.Error($"Message: {ex.InnerException.InnerException.Message}");
            Log.Error($"Source: {ex.InnerException.InnerException.Message}");
            Log.Error(ex.InnerException.InnerException.StackTrace);
            Console.WriteLine();
        }

        static void LogWebApiRequestError(WebApiRequestException ex)
        {
            LogInfoOfCaller();

            Log.Error("{@error}", ex);

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

                var _exceptionJsonFormat = noSuccessResponse.Content;
                while (_exceptionJsonFormat != null)
                {
                    LogError(_exceptionJsonFormat.GetType().FullName);
                    LogError(_exceptionJsonFormat.Message);
                    LogError(_exceptionJsonFormat.Source);
                    LogError(_exceptionJsonFormat.StackTrace);

                    NewBlankLines(1);

                    _exceptionJsonFormat = _exceptionJsonFormat.InnerMiddlewareException;
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
    }
}