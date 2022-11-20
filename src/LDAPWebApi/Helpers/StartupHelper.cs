using Bitai.LDAPWebApi.Configurations.App;
using Bitai.LDAPWebApi.Configurations.LDAP;
using Bitai.LDAPWebApi.Configurations.Security;
using Bitai.LDAPWebApi.Configurations.Swagger;
using Bitai.LDAPWebApi.Controllers.AuthRequirements;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;

namespace Bitai.LDAPWebApi.Helpers;

/// <summary>
/// <see cref="IServiceCollection"/> and <see cref="IApplicationBuilder"/> 
/// extensions, commonly used in <see cref="Startup"/>.
/// </summary>
public static class StartupHelpers
{
	internal static IServiceCollection AddWebApiConfiguration(this IServiceCollection services, IConfiguration configuration, out Configurations.App.WebApiConfiguration webApiConfiguration)
	{
		Log.Information("{method}", nameof(AddWebApiConfiguration));

		webApiConfiguration = configuration.GetSection(nameof(Configurations.App.WebApiConfiguration)).Get<Configurations.App.WebApiConfiguration>() ?? new WebApiConfiguration();

		services.AddSingleton(webApiConfiguration);

		return services;
	}

	internal static IServiceCollection AddWebApiLogConfiguration(this IServiceCollection services, IConfiguration configuration)
	{
		Log.Information("{method}", nameof(AddWebApiLogConfiguration));

		var webApiLogConfiguration = configuration.GetSection(nameof(WebApiLogConfiguration)).Get<WebApiLogConfiguration>() ?? new WebApiLogConfiguration();

		services.AddSingleton(webApiLogConfiguration);

		return services;
	}

	internal static IServiceCollection AddWebApiScopesConfiguration(this IServiceCollection services, IConfiguration configuration, out Configurations.App.WebApiScopesConfiguration webApiScopesConfiguration)
	{
		Log.Information("{method}", nameof(AddWebApiScopesConfiguration));

		webApiScopesConfiguration = configuration.GetSection(nameof(Configurations.App.WebApiScopesConfiguration)).Get<Configurations.App.WebApiScopesConfiguration>() ?? new WebApiScopesConfiguration();

		return services.AddSingleton(webApiScopesConfiguration);
	}

	internal static IServiceCollection ConfigureWebApiCors(this IServiceCollection services, IConfiguration configuration)
	{
		Log.Information("{method}", nameof(ConfigureWebApiCors));

		var webApiCorsConfiguration = configuration.GetSection(nameof(Configurations.App.WebApiCorsConfiguration)).Get<Configurations.App.WebApiCorsConfiguration>() ?? new WebApiCorsConfiguration();

		services.AddCors(setup =>
		{
			setup.AddDefaultPolicy(builder =>
			{
				builder.AllowAnyMethod();
				builder.AllowAnyHeader();

				if (webApiCorsConfiguration.AllowAnyOrigin)
					builder.AllowAnyOrigin();
				else
					builder.WithOrigins(webApiCorsConfiguration.AllowedOrigins);
			});
		});

		return services;
	}

	internal static IServiceCollection AddLDAPServerProfiles(this IServiceCollection services, IConfiguration configuration, out LDAPServerProfiles ldapServerProfiles)
	{
		Log.Information("{method}", nameof(AddLDAPServerProfiles));

		ldapServerProfiles = configuration.GetSection(nameof(LDAPServerProfiles)).Get<LDAPServerProfiles>() ?? new LDAPServerProfiles();

		ldapServerProfiles.CheckConfigurationIntegrity();

		return services.AddSingleton(ldapServerProfiles);
	}

	internal static IServiceCollection AddAuthorityConfiguration(this IServiceCollection services, IConfiguration configuration, out AuthorityConfiguration authorityConfiguration)
	{
		Log.Information("{method}", nameof(AddAuthorityConfiguration));

		authorityConfiguration = configuration.GetSection(nameof(AuthorityConfiguration)).Get<AuthorityConfiguration>() ?? new AuthorityConfiguration();

		return services.AddSingleton(authorityConfiguration);
	}

	internal static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services, IConfiguration configuration, out Configurations.Swagger.SwaggerUIConfiguration swaggerConfigration)
	{
		Log.Information("{method}", nameof(AddSwaggerConfiguration));

		swaggerConfigration = configuration.GetSection(nameof(Configurations.Swagger.SwaggerUIConfiguration)).Get<Configurations.Swagger.SwaggerUIConfiguration>() ?? new SwaggerUIConfiguration();

		return services.AddSingleton(swaggerConfigration);
	}

	internal static IServiceCollection ConfigureSwaggerGenerator(this IServiceCollection services, Configurations.App.WebApiConfiguration webApiConfiguration, Configurations.App.WebApiScopesConfiguration webApiScopesConfiguration, Configurations.Security.AuthorityConfiguration authorityConfiguration, Configurations.Swagger.SwaggerUIConfiguration swaggerConfiguration)
	{
		Log.Information("{method}", nameof(ConfigureSwaggerGenerator));

		services.AddSwaggerGen(setupAction =>
		{
			setupAction.SwaggerDoc(webApiConfiguration.WebApiVersion, new Microsoft.OpenApi.Models.OpenApiInfo
			{
				Contact = new Microsoft.OpenApi.Models.OpenApiContact
				{
					Name = webApiConfiguration.WebApiContactName,
					Email = webApiConfiguration.WebApiContactMail,
					Url = new Uri(webApiConfiguration.WebApiContactUrl)
				},
				Title = webApiConfiguration.WebApiTitle,
				Version = webApiConfiguration.WebApiVersion,
				Description = webApiConfiguration.WebApiDescription
			});

			setupAction.IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory,
				$"{webApiConfiguration.GetType().Assembly.GetName().Name}.xml"));

			if (!webApiScopesConfiguration.BypassApiScopesAuthorization)
			{
				setupAction.AddSecurityDefinition("OAuth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
				{
					Type = Microsoft.OpenApi.Models.SecuritySchemeType.OAuth2,
					Flows = new Microsoft.OpenApi.Models.OpenApiOAuthFlows
					{
						AuthorizationCode = new Microsoft.OpenApi.Models.OpenApiOAuthFlow
						{
							AuthorizationUrl = new Uri($"{authorityConfiguration.Authority}/connect/authorize"),
							TokenUrl = new Uri($"{authorityConfiguration.Authority}/connect/token"),
							Scopes = new Dictionary<string, string> {
							  { swaggerConfiguration.SwaggerUITargetApiScope, swaggerConfiguration.SwaggerUITargetApiScopeTitle }
						 }
						}
					},
					Description = "Swagger Generator OpenId security scheme."
				});

				setupAction.OperationFilter<Configurations.Authorization.AuthorizeCheckOperationFilter>();
			}
		});

		return services;
	}

	internal static IServiceCollection RegisterRouteConstraints(this IServiceCollection services)
	{
		Log.Information("{method}", nameof(RegisterRouteConstraints));

		services.Configure<RouteOptions>(config =>
		{
			config.ConstraintMap.Add("ldapSvrPf", typeof(Controllers.Constraints.LDAPServerProfileRouteConstraint));
			config.ConstraintMap.Add("ldapCatType", typeof(Controllers.Constraints.LDAPCatalogTypeRouteConstraint));
		});

		return services;
	}

	internal static IServiceCollection AddAuthenticationWithIdentityServer(this IServiceCollection services, WebApiScopesConfiguration webApiScopesConfiguration, AuthorityConfiguration authorityConfiguration)
	{
		Log.Information("{method}", nameof(AddAuthenticationWithIdentityServer));

		if (!webApiScopesConfiguration.BypassApiScopesAuthorization)
		{
			var authBuilder = services.AddAuthentication(co =>
			{
				co.DefaultScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
				co.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
			});

			if (string.IsNullOrEmpty(authorityConfiguration.Authority))
				throw new Exception($"{nameof(WebApiScopesConfiguration)}/{nameof(WebApiScopesConfiguration.BypassApiScopesAuthorization)} is disabled, the value of {nameof(AuthorityConfiguration)}/{nameof(AuthorityConfiguration.Authority)} must be set.");

			if (string.IsNullOrEmpty(authorityConfiguration.ApiResource))
				throw new Exception($"{nameof(WebApiScopesConfiguration)}/{nameof(WebApiScopesConfiguration.BypassApiScopesAuthorization)} is disabled, the value of {nameof(AuthorityConfiguration)}/{nameof(AuthorityConfiguration.ApiResource)} must be set.");

			if (!authorityConfiguration.RequireHttpsMetadata.HasValue)
				throw new Exception($"{nameof(WebApiScopesConfiguration)}/{nameof(WebApiScopesConfiguration.BypassApiScopesAuthorization)} is disabled, the value of {nameof(AuthorityConfiguration)}/{nameof(AuthorityConfiguration.RequireHttpsMetadata)} must be set.");

			authBuilder.AddIdentityServerAuthentication(options =>
			{
				options.Authority = authorityConfiguration.Authority;
				options.ApiName = authorityConfiguration.ApiResource;
				options.RequireHttpsMetadata = authorityConfiguration.RequireHttpsMetadata.Value;
			});
		}

		return services;
	}

	internal static IServiceCollection AddAuthorizationWithApiScopePolicies(this IServiceCollection services, WebApiScopesConfiguration webApiScopesConfiguration, AuthorityConfiguration authorityConfiguration)
	{
		Log.Information("{method}", nameof(AddAuthorizationWithApiScopePolicies));

		if (webApiScopesConfiguration.BypassApiScopesAuthorization)
		{
			//Add custom policy evaluator to bypass api authorizations
			services.AddSingleton<IPolicyEvaluator, Controllers.PolicyEvaluators.AuthorizationBypassPolicyEvaluator>();
		}

		services.AddAuthorization(options =>
		{
			options.AddPolicy(WebApiScopesConfiguration.GlobalScopeAuthorizationPolicyName, policy =>
			{
				policy.Requirements.Add(new ApiScopeRequirement(new string[] { webApiScopesConfiguration.GlobalScopeName }, authorityConfiguration.Authority ?? "https://localhost/oauth2"));
			});
		});

		return services.AddSingleton<IAuthorizationHandler, ApiScopeRequirementHandler>();
	}

	internal static void AddCustomHealthChecks(this IServiceCollection services, WebApiConfiguration webApiConfiguration, AuthorityConfiguration authorityConfiguration, WebApiScopesConfiguration webApiScopesConfiguration, LDAPServerProfiles ldapServerProfiles)
	{
		Log.Information("{method}", nameof(AddCustomHealthChecks));

		if (!webApiConfiguration.HealthChecksConfiguration.EnableHealthChecks)
			return;

		IHealthChecksBuilder healthChecksBuilder = services.AddHealthChecks();

		if (!webApiScopesConfiguration.BypassApiScopesAuthorization && authorityConfiguration.Authority != null)
			healthChecksBuilder = healthChecksBuilder.AddUrlGroup(new Uri(authorityConfiguration.Authority), name: "OAuth/OpenId Server", tags: new string[] { authorityConfiguration.Authority });

		foreach (var serverProfile in ldapServerProfiles)
		{
			var portForLocalCatalog = serverProfile.GetPort(false);
			var portForGlobalCatalog = serverProfile.GetPort(true);

			string profileTag = $"Profile: {serverProfile.ProfileId}";
			string defaultDomainTag = $"Default Domain: {serverProfile.DefaultDomainName}";

			healthChecksBuilder = healthChecksBuilder.AddTcpHealthCheck(options =>
			{
				options.AddHost(serverProfile.Server, portForLocalCatalog);
			}, name: $"LDAP Port: {serverProfile.Server}:{portForLocalCatalog}", tags: new string[] { profileTag, defaultDomainTag, $"SSL: {serverProfile.UseSSL}" });

			healthChecksBuilder = healthChecksBuilder.AddTcpHealthCheck(options =>
			{
				options.AddHost(serverProfile.Server, portForGlobalCatalog);
			}, name: $"LDAP Port: {serverProfile.Server}:{portForGlobalCatalog}", tags: new string[] { profileTag, defaultDomainTag, $"SSL: {serverProfile.UseSSLforGlobalCatalog}" });

			healthChecksBuilder = healthChecksBuilder.AddPingHealthCheck(options => options.AddHost(serverProfile.Server, serverProfile.HealthCheckPingTimeout), $"LDAP Ping: {serverProfile.Server}", tags: new string[] { profileTag, defaultDomainTag });
		}

		services.AddHealthChecksUI(settings =>
		{
			settings
				.SetHeaderText(webApiConfiguration.HealthChecksConfiguration.HealthChecksHeaderText)
				.SetEvaluationTimeInSeconds(webApiConfiguration.HealthChecksConfiguration.EvaluationTime)
				.MaximumHistoryEntriesPerEndpoint(webApiConfiguration.HealthChecksConfiguration.MaximunHistoryEntries)
				.AddHealthCheckEndpoint(webApiConfiguration.HealthChecksConfiguration.HealthChecksGroupName, $"{webApiConfiguration.WebApiBaseUrl}/{webApiConfiguration.HealthChecksConfiguration.ApiEndPointName}");

#if DEBUG
			//Avoid SSL certificate validation on development environment
			settings
				.UseApiEndpointHttpMessageHandler(handler => {
					return new HttpClientHandler
					{
						ClientCertificateOptions = ClientCertificateOption.Manual,
						ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
					};
				});
#endif
		})
			.AddInMemoryStorage();
	}

	internal static IApplicationBuilder UseSwaggerUI(this IApplicationBuilder app, WebApiConfiguration webApiConfiguration, SwaggerUIConfiguration swaggerUIConfiguration)
	{
		Log.Information("{method}", nameof(UseSwaggerUI));

		return app.UseSwaggerUI(builder =>
		{
			builder.SwaggerEndpoint($"{webApiConfiguration.WebApiBaseUrl}/swagger/{webApiConfiguration.WebApiVersion}/swagger.json", webApiConfiguration.WebApiName);

#region Authorization request user interface 
			builder.OAuthAppName(webApiConfiguration.WebApiTitle);
			builder.OAuthClientId(swaggerUIConfiguration.SwaggerUIClientId);
			builder.OAuthScopes(swaggerUIConfiguration.SwaggerUITargetApiScope);
			builder.OAuthUsePkce();
#endregion
		});
	}

	internal static void MapCustomHealthChecks(this IEndpointRouteBuilder endpoints, WebApiConfiguration webApiConfiguration)
	{
		Log.Information("{method}", nameof(MapCustomHealthChecks));

		if (!webApiConfiguration.HealthChecksConfiguration.EnableHealthChecks)
			return;

		endpoints.MapHealthChecks($"/{webApiConfiguration.HealthChecksConfiguration.ApiEndPointName}", new HealthCheckOptions()
		{
			Predicate = _ => true,
			ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
		});

		endpoints.MapHealthChecksUI(setupOptions =>
		{
			setupOptions.UIPath = $"/{webApiConfiguration.HealthChecksConfiguration.UIPath}";
			setupOptions.ResourcesPath = $"/{webApiConfiguration.HealthChecksConfiguration.UIPath}/resources";
			setupOptions.AddCustomStylesheet("HealthChecksUI.css");
		});
	}
}