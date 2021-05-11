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
using Bitai.LDAPWebApi.Controllers.AuthRequirements;
using Microsoft.AspNetCore.Authorization;

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

            services.AddWebApiScopesConfiguration(Configuration, out var webApiScopesConfiguration);

            services.AddLDAPServerProfiles(Configuration, out var ldapServerProfiles);

            services.AddAuthorityConfiguration(Configuration, out var authorityConfiguration);

            services.RegisterRouteConstraints();

            services.AddControllers();

            services.ConfigureWebApiCors(Configuration);

            services.AddAuthenticationWithIdentityServer(authorityConfiguration);

            services.AddAuthorizationWithApiScopePolicies(webApiScopesConfiguration, authorityConfiguration);

            var healthChecksBuilder = services.AddHealthChecks();
            healthChecksBuilder.AddCustomHealthChecks(authorityConfiguration, ldapServerProfiles);
            services.AddHealthChecksUI(settings => settings.AddHealthCheckEndpoint("default", "/hc"))
                .AddInMemoryStorage();

            services.AddSwaggerConfiguration(Configuration, out var swaggerUIConfiguration);

            services.ConfigureSwaggerGenerator(webApiConfiguration, authorityConfiguration, swaggerUIConfiguration);
        }
        /// <summary>
        /// This method gets called by the runtime. 
        /// Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Injected <see cref="IApplicationBuilder"/></param>
        /// <param name="env">Injected <see cref="IWebHostEnvironment"/></param>
        /// <param name="webApiConfiguration">Injected <see cref="Configurations.App.WebApiConfiguration"/></param>
        /// <param name="swaggerUIConfiguration">Injected <see cref="Configurations.Swagger.SwaggerUIConfiguration"/></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Configurations.App.WebApiConfiguration webApiConfiguration, Configurations.Swagger.SwaggerUIConfiguration swaggerUIConfiguration)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseMiddleware<Bitai.WebApi.Server.ExceptionHandlingMiddleware>();

            if (env.IsProduction())
            {
                app.UseHttpsRedirection();
            }            

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

            app.UseSwagger();

            app.UseSwaggerUI(webApiConfiguration, swaggerUIConfiguration);
        }
    }
}