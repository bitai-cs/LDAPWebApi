using Bitai.LDAPWebApi.Configurations.App;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.Grafana.Loki;

namespace Bitai.LDAPWebApi;

/// <summary>
/// Entry point for the LDAP Web API application. Handles host building,
/// configuration loading, and Serilog bootstrap logging.
/// </summary>
public class Program
{
    /// <summary>
    /// The application-wide configuration instance, built during startup
    /// from JSON files, environment variables, and command-line arguments.
    /// </summary>
    private static IConfiguration _configuration;

    /// <summary>
    /// Application entry point. Bootstraps the logging system, builds and runs
    /// the generic host, and ensures <see cref="Log.CloseAndFlush"/> is called
    /// on exit or fatal failure.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    public static void Main(string[] args)
    {
        _configuration = GetConfiguration(args);

        Log.Logger = SetupLoggerConfiguration(_configuration, new LoggerConfiguration(), null, out var webApiConfiguration)
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting {webApiName}", webApiConfiguration.WebApiName);

            CreateHostBuilder(args).Build().Run();

            Log.Warning("Terminating {webApiName}", webApiConfiguration.WebApiName);
        }
        catch (Exception ex)
        {
            Log.Fatal("Error when creating Host. Below error details.");
            Log.Fatal("{@error}", ex);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Builds the <see cref="IConfiguration"/> by loading settings from
    /// <c>appsettings.json</c>, the environment-specific <c>appsettings.{Environment}.json</c>,
    /// user secrets (development only), environment variables, and command-line arguments.
    /// </summary>
    /// <param name="args">Command-line arguments to include in the configuration sources.</param>
    /// <returns>The composed <see cref="IConfiguration"/> instance.</returns>
    private static IConfiguration GetConfiguration(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable(Startup.ENVARNAME_ASPNETCORE_ENVIRONMENT);
        var isDevelopment = environment == Environments.Development;

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{environment}.json", true, true);

        if (isDevelopment)
        {
            configurationBuilder.AddUserSecrets<Startup>();
        }

        configurationBuilder.AddCommandLine(args);
        configurationBuilder.AddEnvironmentVariables();

        return configurationBuilder.Build();
    }

    /// <summary>
    /// Configures Serilog with the sinks specified in <see cref="WebApiLogConfiguration"/>
    /// (console, file, Grafana Loki, Elasticsearch) and enriches log events with
    /// application-level properties.
    /// </summary>
    /// <param name="configuration">Source <see cref="IConfiguration"/> for reading log settings.</param>
    /// <param name="loggerConfiguration">The <see cref="LoggerConfiguration"/> to append sinks to.</param>
    /// <param name="hostBuilderContext">Optional context providing hosting environment details; may be null during bootstrap.</param>
    /// <param name="webApiConfiguration">Outputs the resolved <see cref="WebApiConfiguration"/>.</param>
    /// <returns>The enriched <see cref="LoggerConfiguration"/>.</returns>
    private static LoggerConfiguration SetupLoggerConfiguration(IConfiguration configuration, LoggerConfiguration loggerConfiguration, HostBuilderContext? hostBuilderContext, out WebApiConfiguration webApiConfiguration)
    {
        webApiConfiguration = configuration.GetSection(nameof(WebApiConfiguration)).Get<WebApiConfiguration>() ?? new WebApiConfiguration();

        var webApiLogConfiguration = configuration.GetSection(nameof(WebApiLogConfiguration)).Get<WebApiLogConfiguration>() ?? new WebApiLogConfiguration();

        //loggerConfiguration = loggerConfiguration
        //	.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning);

        //loggerConfiguration = loggerConfiguration
        //	.Enrich.FromLogContext();

        loggerConfiguration = loggerConfiguration
            .Enrich.WithProperty("applicationName", webApiConfiguration.WebApiName);

        if (hostBuilderContext != null)
        {
            loggerConfiguration = loggerConfiguration
                .Enrich.WithProperty("assemblyName", hostBuilderContext.HostingEnvironment.ApplicationName)
                .Enrich.WithProperty("environment", hostBuilderContext.HostingEnvironment.EnvironmentName);
        }

        if (webApiLogConfiguration.ConsoleLog.Enabled)
        {
            var logEventLevel = parseLogEventLevel(webApiLogConfiguration.ConsoleLog.MinimunLogEventLevel);

            loggerConfiguration = loggerConfiguration
                .WriteTo.Console(restrictedToMinimumLevel: logEventLevel, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}Properties: {Properties}{NewLine}{Exception}");
        }

        if (webApiLogConfiguration.FileLog.Enabled)
        {
            var logEventLevel = parseLogEventLevel(webApiLogConfiguration.FileLog.MinimunLogEventLevel);

            loggerConfiguration = loggerConfiguration
                .WriteTo.File(new RenderedCompactJsonFormatter(), webApiLogConfiguration.FileLog.LogFilePath, restrictedToMinimumLevel: logEventLevel, rollingInterval: webApiLogConfiguration.FileLog.RollingInterval, flushToDiskInterval: new TimeSpan(0, webApiLogConfiguration.FileLog.FlushToDiskIntervalInMinutes, 0), retainedFileCountLimit: webApiLogConfiguration.FileLog.RetainedFileCountLimit);
        }

        if (webApiLogConfiguration.GrafanaLokiLog.Enabled)
        {
            var logEventLevel = parseLogEventLevel(webApiLogConfiguration.GrafanaLokiLog.MinimunLogEventLevel);

            loggerConfiguration = loggerConfiguration
                .WriteTo.GrafanaLoki(webApiLogConfiguration.GrafanaLokiLog.LokiUrl, textFormatter: new RenderedCompactJsonFormatter(), propertiesAsLabels: new string[] { "applicationName", "assemblyName", "environment", "level", "HealthStatus" }, restrictedToMinimumLevel: logEventLevel, batchPostingLimit: webApiLogConfiguration.GrafanaLokiLog.BatchPostingLimit, period: webApiLogConfiguration.GrafanaLokiLog.Period);
        }

        if (webApiLogConfiguration.ElasticsearchLog.Enabled)
        {
            var logEventLevel = parseLogEventLevel(webApiLogConfiguration.ElasticsearchLog.MinimunLogEventLevel);

            loggerConfiguration = loggerConfiguration
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(webApiLogConfiguration.ElasticsearchLog.GetElasticsearchNodeUris())
                {
                    AutoRegisterTemplate = true,
                });
        }

        return loggerConfiguration;

        Serilog.Events.LogEventLevel parseLogEventLevel(WebApiLogConfiguration.MinimunLogEventLevel minimunLogEventLevel)
        {
            return minimunLogEventLevel == WebApiLogConfiguration.MinimunLogEventLevel.Verbose ? Serilog.Events.LogEventLevel.Verbose : (minimunLogEventLevel == WebApiLogConfiguration.MinimunLogEventLevel.Debug ? Serilog.Events.LogEventLevel.Debug : (minimunLogEventLevel == WebApiLogConfiguration.MinimunLogEventLevel.Information ? Serilog.Events.LogEventLevel.Information : (minimunLogEventLevel == WebApiLogConfiguration.MinimunLogEventLevel.Warning ? Serilog.Events.LogEventLevel.Warning : (minimunLogEventLevel == WebApiLogConfiguration.MinimunLogEventLevel.Error ? Serilog.Events.LogEventLevel.Error : (minimunLogEventLevel == WebApiLogConfiguration.MinimunLogEventLevel.Fatal ? Serilog.Events.LogEventLevel.Fatal : throw new Exception("Invalid Web Api application log level. Verify web api _configuration."))))));
        }
    }

    /// <summary>
    /// Creates and configures the <see cref="IHostBuilder"/> for the application.
    /// Sets up the app configuration pipeline, Kestrel web server, <see cref="Startup"/>
    /// wiring, and Serilog integration.
    /// </summary>
    /// <param name="args">Command-line arguments forwarded to the configuration sources.</param>
    /// <returns>A configured <see cref="IHostBuilder"/> ready to build and run.</returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                 .ConfigureAppConfiguration((hostContext, configApp) =>
                 {
                     var configurationRoot = configApp.Build();

                     //// BITAI: Remains for future implementation.
                     //configApp.AddJsonFile("serilog.json", optional: true, reloadOnChange: true);

                     var env = hostContext.HostingEnvironment;

                     //// BITAI: Remains for future implementation.
                     //configApp.AddJsonFile($"serilog.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                     if (env.IsDevelopment())
                     {
                         configApp.AddUserSecrets<Startup>(true);
                     }

                     //// BITAI: Remains for future implementation.
                     //configurationRoot.AddAzureKeyVaultConfiguration(configApp);

                     configApp.AddEnvironmentVariables();
                     configApp.AddCommandLine(args);
                 })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options => options.AddServerHeader = true);
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog((hostContext, loggerConfig) =>
                {
                    SetupLoggerConfiguration(hostContext.Configuration, loggerConfig, hostContext, out _);
                });
}
