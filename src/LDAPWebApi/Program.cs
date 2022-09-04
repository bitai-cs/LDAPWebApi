using Bitai.LDAPWebApi;
using Bitai.LDAPWebApi.Configurations.App;
using Serilog;
using Serilog.Extensions.Logging;

var FullName = nameof(Program);

var configuration = GetConfiguration(args);

Log.Logger = SetupLoggerConfiguration(configuration, new LoggerConfiguration())
	.CreateBootstrapLogger();

try
{
	Log.Information("Starting {program}", FullName);

	var webAppBuilder = WebApplication.CreateBuilder(args);
	webAppBuilder.Host
		.ConfigureDefaults(args)
		.UseSerilog((hostBuilderContext, serviceProvider, loggerConfiguration) =>
		{
			SetupLoggerConfiguration(configuration, loggerConfiguration);
		});

	var startup = new Startup(webAppBuilder.Configuration, webAppBuilder.Environment);

	startup.ConfigureServices(webAppBuilder.Services, out var webApiConfiguration, out var swaggerUIConfiguration);

	var webApp = webAppBuilder.Build();

	startup.Configure(webApp, webAppBuilder.Environment, webApiConfiguration, swaggerUIConfiguration);

	webApp.Run();

	Log.Warning("Terminating {program}", FullName);
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

LoggerConfiguration SetupLoggerConfiguration(IConfiguration configuration, LoggerConfiguration loggerConfiguration)
{
	var webApiLogConfiguration = configuration.GetSection(nameof(WebApiLogConfiguration)).Get<WebApiLogConfiguration>() ?? new WebApiLogConfiguration();

	if (webApiLogConfiguration.MinimunLogLevel == WebApiLogConfiguration.MinimunLogEventLevel.Information)
	{
		loggerConfiguration = loggerConfiguration
			.MinimumLevel.Information();
	}
	else if (webApiLogConfiguration.MinimunLogLevel == WebApiLogConfiguration.MinimunLogEventLevel.Warning)
	{
		loggerConfiguration = loggerConfiguration
			.MinimumLevel.Warning();
	}
	else if (webApiLogConfiguration.MinimunLogLevel == WebApiLogConfiguration.MinimunLogEventLevel.Error)
	{
		loggerConfiguration = loggerConfiguration
			.MinimumLevel.Error();
	}
	else
	{
		throw new Exception("Invalid Web Api application log level. Verify web api configuration.");
	}

	loggerConfiguration = loggerConfiguration
		.Enrich.FromLogContext();

	return loggerConfiguration
			.WriteTo.File(webApiLogConfiguration.LogFilePath, rollingInterval: RollingInterval.Day, flushToDiskInterval: new TimeSpan(0, 1, 0), retainedFileCountLimit: 15)
			.WriteTo.Console();
}