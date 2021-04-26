﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Bitai.WebApi.Client;
using Serilog;
using Serilog.Sinks.SystemConsole;

namespace Bitai.LDAPWebApi.Clients.Demo
{
    class Program
    {
        static string WebApiBaseUrl = "https://localhost:44331";
        static WebApiSecurityDefinition WebApiSecurity = new WebApiSecurityDefinition
        {
            AuthorityUrl = "https://localhost:44310",
            ApiScope = "Bitai.LdapWebApi.Scope.Global",
            ClientId = "Bitai.LdapWebApi.Demo.Client",
            ClientSecret = "2ef61cb6-9ca6-7418-c116-80784583d88f"
        };
        static string Tag { get; set; } = "DEMO";
        static string Selected_LDAPServerProfile { get; set; } = "PE";


        static async Task Main(string[] args)
        {
            Thread.Sleep(10000);

            try
            {
                ConfigLogger();

                await LDAPCatalogTypesClient_GetAll();

                await Authentication_Test();

                //await LDAPServerProfiles_GetProfileIds();

                //await LDAPServerProfiles_GetAll();
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

        static async Task LDAPCatalogTypesClient_GetAll()
        {
            try
            {
                LogInfo("LDAPCatalogTypesClient_GetAll --------------------------");

                var client = new LDAPCatalogTypesClient<DTO.LDAPCatalogTypes>(WebApiBaseUrl, WebApiSecurity);

                LogInfo("GetAllAsync...");
                var httpResponse = await client.GetAllAsync();
                if (client.IsNoSuccessResponse(httpResponse))
                {
                    client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
                }
                else
                {
                    var dto = await client.GetDTOFromResponseAsync(httpResponse);

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

        static async Task Authentication_Test()
        {
            try
            {
                LogInfo("Authentication_Test --------------------------");

                var client = new LDAPCredentialsClient<DTO.LDAPAccountAuthenticationStatus>(WebApiBaseUrl, Selected_LDAPServerProfile, true, WebApiSecurity);

                var accountSecurityData = new DTO.LDAPAccountCredential
                {
                    DomainName = "DOMAIN",
                    AccountName = "usr_ext01",
                    AccountPassword = "Pq$",
                };

                LogInfo("AccountAuthenticationAsync...");
                var httpResponse = await client.AccountAuthenticationAsync("usr_ext01", accountSecurityData);
                if (client.IsNoSuccessResponse(httpResponse))
                {
                    client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
                }
                else
                {
                    var status = await client.GetDTOFromResponseAsync(httpResponse);

                    LogInfo($"CustomTag:{status.RequestTag} | DomainName:{status.DomainName} | AccountName:{status.AccountName} | Authenticated:{status.IsAuthenticated}");
                }
            }
            catch (WebApiRequestException ex)
            {
                LogWebApiRequestError(ex);
            }
        }

        static async Task LDAPServerProfiles_GetProfileIds()
        {
            try
            {
                LogInfo("LDAPServerProfiles_GetProfileIds --------------------------");

                var client = new LDAPServerProfilesClient<string>(WebApiBaseUrl, WebApiSecurity);

                LogInfo("GetProfilesIdsAsync...");
                var httpResponse = await client.GetProfileIdsAsync();
                if (client.IsNoSuccessResponse(httpResponse))
                {
                    client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
                }
                else
                {
                    var enumerableDTO = await client.GetEnumerableDTOFromResponseAsync(httpResponse);
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

        static async Task LDAPServerProfiles_GetAll()
        {
            try
            {
                LogInfo("LDAPServerProfiles_GetAll --------------------------");

                var client = new LDAPServerProfilesClient<DTO.LDAPServerProfile>(WebApiBaseUrl, WebApiSecurity);

                LogInfo("GetProfilesIdsAsync...");
                var httpResponse = await client.GetAllAsync();
                if (client.IsNoSuccessResponse(httpResponse))
                {
                    client.ThrowClientRequestException("Error al realizar la solicitud", httpResponse);
                }
                else
                {
                    var enumerableDTO = await client.GetEnumerableDTOFromResponseAsync(httpResponse);
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
            Log.Error(ex.Message);
            if (ex.InnerException == null) return;

            Log.Error(ex.InnerException.Message);
            if (ex.InnerException.InnerException == null) return;

            Log.Error(ex.InnerException.InnerException.Message);
        }

        static void LogWebApiRequestError(WebApiRequestException ex)
        {
            LogError(ex.Message);

            var typeOfContent = ex.NoSuccessResponse.GetType();
            LogError(typeOfContent.FullName);

            if (typeOfContent == typeof(NoSuccessResponseWithJsonStringContent))
            {
                var noSuccessResponse = (NoSuccessResponseWithJsonStringContent)ex.NoSuccessResponse;

                LogError(noSuccessResponse.ReasonPhrase);
                LogError(((int)noSuccessResponse.HttpStatusCode).ToString());
                LogError(noSuccessResponse.ContentType.ToString());
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
            else if (typeOfContent == typeof(NoSuccessResponseWuthHtmlContent))
            {
                var noSuccessResponse = (NoSuccessResponseWuthHtmlContent)ex.NoSuccessResponse;

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