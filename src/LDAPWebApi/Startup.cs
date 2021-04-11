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
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebApiConfiguration(this.Configuration, out var webApiConfiguration);

            services.AddLDAPServices(this.Configuration);

            services.AddSecurityServices(this.Configuration);

            ////VIKO: services.AddControllers() executes services.AddCors() 
            //services.AddCors();

            services.Configure<RouteOptions>(config =>
            {
                config.ConstraintMap.Add("ldapSvrPf", typeof(Controllers.Constraints.LDAPServerProfileRouteConstraint));
                config.ConstraintMap.Add("ldapCatType", typeof(Controllers.Constraints.LDAPCatalogTypeRouteConstraint));
            });

            services.AddControllers();

            #region Uncomment this region to enable IdentityServer4 authority 
            //services.AddAuthentication(co =>
            //{
            //	co.DefaultScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            //	co.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            //})
            //	.AddIdentityServerAuthentication(options =>
            //	{
            //		options.Authority = _identityServerConfig.Authority;
            //		options.ApiName = _identityServerConfig.ApiResource;
            //		options.RequireHttpsMetadata = _identityServerConfig.RequireHttpsMetadata;
            //	});
            //////VBG: Another way to set Authnetication
            ////services.AddAuthentication("Bearer")
            ////	.AddIdentityServerAuthentication(options =>
            ////	{
            ////		options.Authority = _identityServerConfig.Authority;
            ////		options.ApiName = _identityServerConfig.ApiResource;
            ////		options.RequireHttpsMetadata = _identityServerConfig.RequireHttpsMetadata;
            ////	});
            #endregion

            ////VIKO: services.AddControllers() executes services.AddAuthorization()
            //services.AddAuthorization();

            services.AddSwaggerGenService(webApiConfiguration);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Injected <see cref="IApplicationBuilder"/></param>
        /// <param name="env">Injected <see cref="IWebHostEnvironment"/></param>
        /// <param name="webApiConfiguration">Injected <see cref="Configurations.LDAP.WebApiConfiguration"/></param>
        /// <param name="identityServerConfig">Injected <see cref="Configurations.Security.IdentityServer"/></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Configurations.LDAP.WebApiConfiguration webApiConfiguration, Configurations.Security.IdentityServer identityServerConfig)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<Bitai.WebApi.Server.ExceptionHandlingMiddleware>();

            ////VIKO: Uncomment this to enable HTTPS redirection
            //app.UseHttpsRedirection();

            app.UseRouting();

#if DEBUG
            app.UseCors(cp =>
            {
                cp.AllowAnyOrigin();
                cp.AllowAnyHeader();
                cp.AllowAnyMethod();
            });
#else //TODO: Implement a secure CORS policy
			app.UseCors(cp => 
			{
				cp.AllowAnyOrigin();
				cp.AllowAnyHeader();
				cp.AllowAnyMethod();
			});
#endif

            #region Uncomment this region to enable IdentityServer4 authority 
            //app.UseAuthentication();
            #endregion

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(setupAction =>
            {
                setupAction.SwaggerEndpoint($"{webApiConfiguration.WebApiBaseUrl}/swagger/{webApiConfiguration.WebApiVersion}/swagger.json", webApiConfiguration.WebApiName);
                #region Uncomment this region to enable IdentityServer4 authority 
                //setupAction.OAuthClientId(identityServerConfig.OidcSwaggerUIClientId);
                //setupAction.OAuthAppName(identityServerConfig.ApiName);
                //setupAction.OAuthUsePkce();
                #endregion
            });

            //app.UseWelcomePage();
        }
    }
}