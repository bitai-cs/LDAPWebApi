using Bitai.LDAPWebApi;
using Bitai.LDAPWebApi.Configurations.App;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Templates;

var configuration = GetConfiguration(args);

Log.Logger = SetupLoggerConfiguration(configuration, new LoggerConfiguration(), null, out var webApiConfiguration)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting {webApiName}", webApiConfiguration.WebApiName);

    var webAppBuilder = WebApplication.CreateBuilder(args);
    webAppBuilder.Host
        .ConfigureDefaults(args)
        .UseSerilog((hostBuilderContext, serviceProvider, loggerConfiguration) =>
        {
            SetupLoggerConfiguration(configuration, loggerConfiguration, hostBuilderContext, out webApiConfiguration);
        });

    var startup = new Startup(webAppBuilder.Configuration, webAppBuilder.Environment);

    startup.ConfigureServices(webAppBuilder.Services, out webApiConfiguration, out var swaggerUIConfiguration);

    var webApp = webAppBuilder.Build();

    startup.Configure(webApp, webAppBuilder.Environment, webApiConfiguration, swaggerUIConfiguration);

    webApp.Run();

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

IConfiguration GetConfiguration(string[] args)
{
    var environment = Environment.GetEnvironmentVariable(Startup.ENVARNAME_ASPNETCORE_ENVIRONMENT);
    var isDevelopment = environment == Environments.Development;

    var configurationBuilder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile($"appsettings.{environment}.json", true, true);

    if (isDevelopment)
    {
        configurationBuilder.AddUserSecrets<Bitai.LDAPWebApi.Startup>();
    }

    configurationBuilder.AddCommandLine(args);
    configurationBuilder.AddEnvironmentVariables();

    return configurationBuilder.Build();
}

LoggerConfiguration SetupLoggerConfiguration(IConfiguration configuration, LoggerConfiguration loggerConfiguration, HostBuilderContext? hostBuilderContext, out WebApiConfiguration webApiConfiguration)
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
        return minimunLogEventLevel == WebApiLogConfiguration.MinimunLogEventLevel.Verbose ? Serilog.Events.LogEventLevel.Verbose : (minimunLogEventLevel == WebApiLogConfiguration.MinimunLogEventLevel.Debug ? Serilog.Events.LogEventLevel.Debug : (minimunLogEventLevel == WebApiLogConfiguration.MinimunLogEventLevel.Information ? Serilog.Events.LogEventLevel.Information : (minimunLogEventLevel == WebApiLogConfiguration.MinimunLogEventLevel.Warning ? Serilog.Events.LogEventLevel.Warning : (minimunLogEventLevel == WebApiLogConfiguration.MinimunLogEventLevel.Error ? Serilog.Events.LogEventLevel.Error : (minimunLogEventLevel == WebApiLogConfiguration.MinimunLogEventLevel.Fatal ? Serilog.Events.LogEventLevel.Fatal : throw new Exception("Invalid Web Api application log level. Verify web api configuration."))))));
    }
}