using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bitai.LDAPWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }



        public static IHostBuilder CreateHostBuilder(string[] args) =>
             Host.CreateDefaultBuilder(args)
                  .ConfigureAppConfiguration((hostContext, configApp) =>
                  {
                      var configurationRoot = configApp.Build();
                      var env = hostContext.HostingEnvironment;

                      configApp
                            .AddJsonFile($"appsettings.json")
                            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                      if (env.IsDevelopment())
                      {
                          configApp.AddUserSecrets<Startup>();
                      }

                      configApp.AddEnvironmentVariables();
                      configApp.AddCommandLine(args);
                  })
                  .ConfigureWebHostDefaults(webBuilder =>
                  {
                      webBuilder.UseStartup<Startup>();
                  });
    }
}