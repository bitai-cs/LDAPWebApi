using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Bitai.LDAPWebApi.Helpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

namespace Bitai.LDAPWebApi
{
    /// <summary>
    /// Startup class
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configuration instance
        /// </summary>
        public IConfiguration Configuration { get; }



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">Injected <see cref="IConfiguration"/></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }



        /// <summary>
        /// This method gets called by the runtime. 
        /// Use this method to add services to the container.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebApiConfiguration(Configuration, out var webApiConfiguration);

            services.AddLDAPRoutes(Configuration, out var ldapServerProfiles);

            services.AddIdentityServerConfiguration(Configuration, out var identityServerConfiguration);

            services.RegisterRouteConstraints();

            services.AddControllers();

            services.ConfigureWebApiCors(Configuration);

            services.AddAuthenticationWithIdentityServer(identityServerConfiguration);

            ////VIKO: services.AddControllers() executes services.AddAuthorization()
            //services.AddAuthorization();

            var healthChecksBuilder = services.AddHealthChecks();
            healthChecksBuilder.AddCustomHealthChecks(identityServerConfiguration, ldapServerProfiles);

            services.AddHealthChecksUI(settings => settings.AddHealthCheckEndpoint("default", "/hc"))
                .AddInMemoryStorage();

            services.ConfigureSwaggerGenerator(webApiConfiguration, identityServerConfiguration);
        }

        /// <summary>
        /// This method gets called by the runtime. 
        /// Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Injected <see cref="IApplicationBuilder"/></param>
        /// <param name="env">Injected <see cref="IWebHostEnvironment"/></param>
        /// <param name="webApiConfiguration">Injected <see cref="Configurations.App.WebApiConfiguration"/></param>
        /// <param name="identityServerConfig">Injected <see cref="Configurations.Security.IdentityServerConfiguration"/></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Configurations.App.WebApiConfiguration webApiConfiguration, Configurations.Security.IdentityServerConfiguration identityServerConfig)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<Bitai.WebApi.Server.ExceptionHandlingMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapHealthChecksUI(options => options.UIPath = "/hc-ui");

                endpoints.MapControllers();
            });

            //app.UseHealthChecks("/health", new HealthCheckOptions()
            //{
            //    Predicate = _ => true,
            //    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            //})
            //    .UseHealthChecksUI();
            ////.UseHealthChecksUI(options =>
            ////{
            ////    //options.ApiPath = "/health";
            ////    options.UIPath = "/health-ui";
            ////});

            app.UseSwagger();
            app.UseSwaggerUI(builder =>
            {
                builder.SwaggerEndpoint($"{webApiConfiguration.WebApiBaseUrl}/swagger/{webApiConfiguration.WebApiVersion}/swagger.json", webApiConfiguration.WebApiName);

                #region Authorization request user interface 
                builder.OAuthAppName(webApiConfiguration.WebApiTitle);
                builder.OAuthClientId(identityServerConfig.SwaggerUIClientId);
                builder.OAuthScopes(identityServerConfig.ApiScope);
                builder.OAuthUsePkce();
                #endregion
            });

            app.UseWelcomePage();
        }
    }
}