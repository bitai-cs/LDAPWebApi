using Bitai.LDAPWebApi.Helpers;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            services.AddHealthChecks()
                .AddCustomHealthChecks(authorityConfiguration, webApiScopesConfiguration, ldapServerProfiles);
            services.AddHealthChecksUI(settings => settings.AddHealthCheckEndpoint("default", $"{webApiConfiguration.WebApiBaseUrl}/hc"))
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
                /* HSTS is generally a browser only instruction. Other callers,
                 * such as phone or desktop apps, do not obey the instruction. 
                 * Even within browsers, a single authenticated call to an API
                 * over HTTP has risks on insecure networks. The secure
                 * approach is to configure API projects to only listen to
                 * and respond over HTTPS.*/
                // Due to the above, UseHsts is disabled
                // app.UseHsts();
            }

            app.UseMiddleware<Bitai.WebApi.Server.ExceptionHandlingMiddleware>();

            /* Do not use RequireHttpsAttribute on Web APIs that receive 
             * sensitive information.RequireHttpsAttribute uses HTTP status 
             * codes to redirect browsers from HTTP to HTTPS. API clients may
             * not understand or obey redirects from HTTP to HTTPS.
             * Such clients may send information over HTTP.
             * Web APIs should either:
             *      Not listen on HTTP.
             *      Close the connection with status code 400(Bad Request) 
             *      and not serve the request. */
            // Due to the above, UseHttpsRedirection is disabled
            //if (env.IsProduction())
            //{
            //    app.UseHttpsRedirection();
            //}            

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