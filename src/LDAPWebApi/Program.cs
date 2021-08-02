using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        /// Application entry point.
        /// </summary>
        /// <param name="args">Program arguments.</param>
        public static void Main(string[] args)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.FromLogContext();
#if DEBUG
            loggerConfiguration = loggerConfiguration
                .MinimumLevel.Information()
                .WriteTo.File(".\\logs\\Bitai.LDAPWebApi-.log", rollingInterval: RollingInterval.Minute, flushToDiskInterval: new TimeSpan(0, 0, 10), retainedFileCountLimit: 3)
                .WriteTo.Console();
#else
            loggerConfiguration = loggerConfiguration
                .MinimumLevel.Warning()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Error)
                .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Error)
                .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Error)
                .WriteTo.File(".\\logs\\Bitai.LDAPWebApi-.log", rollingInterval: RollingInterval.Day, flushToDiskInterval: new TimeSpan(0, 1, 0), retainedFileCountLimit: 15)
                .WriteTo.Console();
#endif
            Log.Logger = loggerConfiguration.CreateLogger();

            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error when creating Host.");
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
                .ConfigureLogging((webHostBuilderContext, logginBuilder) =>
                {
                    logginBuilder.ClearProviders();
                })
                .UseSerilog()
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder.UseStartup<Startup>();
                });
    }
}