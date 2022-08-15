using Bitai.LDAPWebApi.Helpers;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Bitai.LDAPWebApi;

/// <summary>
/// Startup class
/// </summary>
public class Startup
{
	/// <summary>
	/// Full name of <see cref="Startup"/> 
	/// </summary>
	public static string FullName = typeof(Startup).FullName ?? nameof(Startup);

	/// <summary>
	/// See <see cref="IWebHostEnvironment"/>.
	/// </summary>
	public IWebHostEnvironment WebHostEnvironment;

	/// <summary>
	/// See <see cref="IConfiguration"/>.
	/// </summary>
	public IConfiguration Configuration { get; }



	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="configuration">Injected <see cref="IConfiguration"/>.</param>
	/// <param name="webHostEnvironment">Injected <see cref="IWebHostEnvironment"/>.</param>
	public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
	{
		Configuration = configuration;
		WebHostEnvironment = webHostEnvironment;
	}



	/// <summary>
	/// This method gets called by the runtime. 
	/// Use this method to add services to the container.
	/// </summary>
	/// <param name="services">Injected <see cref="IServiceCollection"/></param>
	/// <param name="webApiConfiguration">Return the configuration of the Web API application. See <see cref="WebApiConfiguration"/>.</param>
	public void ConfigureServices(IServiceCollection services, out Configurations.App.WebApiConfiguration webApiConfiguration, out Configurations.Swagger.SwaggerUIConfiguration swaggerUIConfiguration)
	{
		Log.Information("{class} -> {method} starting...", FullName, nameof(ConfigureServices));

		services.AddWebApiConfiguration(Configuration, out webApiConfiguration);

		services.AddWebApiLogConfiguration(Configuration);

		services.AddWebApiScopesConfiguration(Configuration, out var webApiScopesConfiguration);

		services.AddLDAPServerProfiles(Configuration, out var ldapServerProfiles);

		services.AddAuthorityConfiguration(Configuration, out var authorityConfiguration);

		services.RegisterRouteConstraints();

		services.AddControllers();

		services.ConfigureWebApiCors(Configuration);

		services.AddAuthenticationWithIdentityServer(webApiScopesConfiguration, authorityConfiguration);

		services.AddAuthorizationWithApiScopePolicies(webApiScopesConfiguration, authorityConfiguration);

		services.AddCustomHealthChecks(webApiConfiguration, authorityConfiguration, webApiScopesConfiguration, ldapServerProfiles);

		services.AddSwaggerConfiguration(Configuration, out swaggerUIConfiguration);

		services.ConfigureSwaggerGenerator(webApiConfiguration, webApiScopesConfiguration, authorityConfiguration, swaggerUIConfiguration);

		Log.Information("{class} -> {method} completed.", FullName, nameof(ConfigureServices));
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

		app.UseMiddleware<WebApi.Server.ExceptionHandlingMiddleware>();

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

		app.UseStaticFiles();

		app.UseAuthentication();

		app.UseAuthorization();

		app.UseEndpoints(endpoints =>
		{
			endpoints.MapCustomHealthChecks(webApiConfiguration);

			endpoints.MapControllers();
		});

		app.UseSwagger();

		app.UseSwaggerUI(webApiConfiguration, swaggerUIConfiguration);
	}
}