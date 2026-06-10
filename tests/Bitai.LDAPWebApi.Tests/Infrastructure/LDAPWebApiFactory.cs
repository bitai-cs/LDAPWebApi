using Bitai.LDAPWebApi.Configurations.App;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bitai.LDAPWebApi.Tests.Infrastructure;

/// <summary>
/// A custom <see cref="Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory{TEntryPoint}"/>
/// that boots the full ASP.NET Core pipeline using the application's own <see cref="Startup"/>,
/// overlaying test-specific configuration (appsettings.Tests.json) so that
/// <see cref="WebApiConfiguration.WebApiTestConfiguration.EnablePersistentMockLdapDataStore"/>
/// is <c>true</c>. This causes the DI container to register
/// <see cref="Bitai.LDAPHelper.Tests.Mocks.LdapAdapters.MockLdapPersistentConnectionFactoryAdapter"/>
/// instead of the real Novell LDAP adapter, and the middleware seeds the mock LDAP data store.
/// </summary>
public class LDAPWebApiFactory : Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Startup>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Point at the test project directory so relative paths in configuration work.
        builder.UseContentRoot(Directory.GetCurrentDirectory());

        // Overlay the standard appsettings with the test-specific one.
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Remove the existing sources added by the host builder so we control the order.
            config.Sources.Clear();

            config.SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.Tests.json", optional: false, reloadOnChange: false);
        });

        //// Use the application's own Startup class so we exercise real service registration
        //// and middleware. The Startup.ConfigureServices call picks up
        //// WebApiConfiguration.TestConfiguration.EnablePersistentMockLdapDataStore = true
        //// from appsettings.Tests.json and registers the mock LDAP adapter automatically.
        //builder.Configure(app =>
        //{
        //    // Intentionally empty – the real pipeline is configured by Startup.Configure,
        //    // which is invoked because we are using UseStartup<Startup>() below.
        //});

        //builder.UseStartup<Startup>();

        // Suppress Serilog bootstrap (it tries to configure a static logger that conflicts
        // with parallel test runs). We rely on the Microsoft ILogger registered by ASP.NET Core.
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        });
    }
}
