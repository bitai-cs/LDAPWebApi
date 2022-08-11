using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bitai.LDAPWebApi.Configurations.App;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Bitai.LDAPWebApi
{
	/// <summary>
	/// Starter program.
	/// </summary>
	public class Program
	{
		/// <summary>
		/// Full name of <see cref="Program"/> 
		/// </summary>
		public static string FullName = typeof(Program).FullName ?? nameof(Program);

		/// <summary>
		/// Application entry point.
		/// </summary>
		/// <param name="args">Program arguments.</param>
		public static void Main(string[] args)
		{
			var configuration = GetConfiguration(args);

			var loggerConfiguration = new LoggerConfiguration();

			Log.Logger = SetupLoggerConfiguration(configuration, loggerConfiguration, true)
				.CreateBootstrapLogger();

			try
			{
				Log.Information("Starting {program}", Program.FullName);

				CreateHostBuilder(args).Build().Run();

				Log.Warning("Terminating {program}", Program.FullName);
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
		/// Initializes a new instance of the Microsoft.Extensions.Hosting.HostBuilder class.
		/// </summary>
		/// <param name="args">Program arguments.</param>
		/// <returns></returns>
		public static IHostBuilder CreateHostBuilder(string[] args) =>
			 Host.CreateDefaultBuilder(args)
				.UseSerilog((hostBuilderContext, serviceProvider, loggerConfiguration) =>
				{
					var configuration = GetConfiguration(args);

					SetupLoggerConfiguration(configuration, loggerConfiguration, false);
				})
				.ConfigureWebHostDefaults(webHostBuilder =>
				{
					webHostBuilder.UseStartup<Startup>();
				});




		private static IConfiguration GetConfiguration(string[] args)
		{
			var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
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

		private static LoggerConfiguration SetupLoggerConfiguration(IConfiguration configuration, LoggerConfiguration loggerConfiguration, bool useBootstrapConfiguration)
		{
			var webApiLogConfiguration = configuration.GetSection(nameof(WebApiLogConfiguration)).Get<WebApiLogConfiguration>();

			if (useBootstrapConfiguration)
			{
				loggerConfiguration = loggerConfiguration
					   .MinimumLevel.Information();
			}
			else
			{
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
			}

			return loggerConfiguration
					.WriteTo.File(webApiLogConfiguration.LogFilePath, rollingInterval: RollingInterval.Day, flushToDiskInterval: new TimeSpan(0, 1, 0), retainedFileCountLimit: 15)
					.WriteTo.Console();
		}
	}
}