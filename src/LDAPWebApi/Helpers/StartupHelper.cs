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

namespace Bitai.LDAPWebApi.Helpers
{
    /// <summary>
    /// <see cref="IServiceCollection"/> and <see cref="IApplicationBuilder"/> 
    /// extensions, commonly used in <see cref="Startup"/>.
    /// </summary>
    public static class StartupHelpers
    {
        internal static IServiceCollection AddWebApiConfiguration(this IServiceCollection services, IConfiguration configuration, out Configurations.App.WebApiConfiguration webApiConfiguration)
        {
            Log.Information("{method}", nameof(AddWebApiConfiguration));

            webApiConfiguration = configuration.GetSection(nameof(Configurations.App.WebApiConfiguration)).Get<Configurations.App.WebApiConfiguration>();

            services.AddSingleton(webApiConfiguration);

            return services;
        }

        internal static IServiceCollection AddWebApiLogConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("{method}", nameof(AddWebApiLogConfiguration));

            var webApiLogConfiguration = configuration.GetSection(nameof(WebApiLogConfiguration)).Get<WebApiLogConfiguration>();

            services.AddSingleton(webApiLogConfiguration);

            return services;
        }

        internal static IServiceCollection AddWebApiScopesConfiguration(this IServiceCollection services, IConfiguration configuration, out Configurations.App.WebApiScopesConfiguration webApiScopesConfiguration)
        {
            Log.Information("{method}", nameof(AddWebApiScopesConfiguration));

            webApiScopesConfiguration = configuration.GetSection(nameof(Configurations.App.WebApiScopesConfiguration)).Get<Configurations.App.WebApiScopesConfiguration>();

            return services.AddSingleton(webApiScopesConfiguration);
        }

        internal static IServiceCollection ConfigureWebApiCors(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("{method}", nameof(ConfigureWebApiCors));

            var webApiCorsConfiguration = configuration.GetSection(nameof(Configurations.App.WebApiCorsConfiguration)).Get<Configurations.App.WebApiCorsConfiguration>();

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

            ldapServerProfiles = configuration.GetSection(nameof(LDAPServerProfiles)).Get<LDAPServerProfiles>();

            ldapServerProfiles.CheckConfigurationIntegrity();

            return services.AddSingleton(ldapServerProfiles);
        }

        internal static IServiceCollection AddAuthorityConfiguration(this IServiceCollection services, IConfiguration configuration, out AuthorityConfiguration authorityConfiguration)
        {
            Log.Information("{method}", nameof(AddAuthorityConfiguration));

            authorityConfiguration = configuration.GetSection(nameof(AuthorityConfiguration)).Get<AuthorityConfiguration>();

            return services.AddSingleton(authorityConfiguration);
        }

        internal static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services, IConfiguration configuration, out Configurations.Swagger.SwaggerUIConfiguration swaggerConfigration)
        {
            Log.Information("{method}", nameof(AddSwaggerConfiguration));

            swaggerConfigration = configuration.GetSection(nameof(Configurations.Swagger.SwaggerUIConfiguration)).Get<Configurations.Swagger.SwaggerUIConfiguration>();

            return services.AddSingleton(swaggerConfigration);
        }

        internal static IServiceCollection ConfigureSwaggerGenerator(this IServiceCollection services, Configurations.App.WebApiConfiguration webApiConfiguration, Configurations.Security.AuthorityConfiguration authorityConfiguration, Configurations.Swagger.SwaggerUIConfiguration swaggerConfiguration)
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

        internal static IServiceCollection AddAuthenticationWithIdentityServer(this IServiceCollection services, AuthorityConfiguration authorityConfiguration)
        {
            Log.Information("{method}", nameof(AddAuthenticationWithIdentityServer));

            services.AddAuthentication(co =>
            {
                co.DefaultScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                co.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            })
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = authorityConfiguration.Authority;
                    options.ApiName = authorityConfiguration.ApiResource;
                    options.RequireHttpsMetadata = authorityConfiguration.RequireHttpsMetadata;
                });

            return services;
        }

        internal static IServiceCollection AddAuthorizationWithApiScopePolicies(this IServiceCollection services, WebApiScopesConfiguration webApiScopesConfiguration, AuthorityConfiguration authorityConfiguration)
        {
            Log.Information("{method}", nameof(AddAuthorizationWithApiScopePolicies));

            if (webApiScopesConfiguration.BypassApiScopesAuthorization)
            {
                services.AddSingleton<IPolicyEvaluator, Controllers.PolicyEvaluators.AuthorizationBypassPolicyEvaluator>();
            }

            services.AddAuthorization(options =>
            {
                options.AddPolicy(WebApiScopesConfiguration.GlobalScopeAuthorizationPolicyName, policy =>
                {
                    policy.Requirements.Add(new ApiScopeRequirement(new string[] { webApiScopesConfiguration.GlobalScopeName }, authorityConfiguration.Authority));
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

            if (!webApiScopesConfiguration.BypassApiScopesAuthorization)
                healthChecksBuilder = healthChecksBuilder.AddUrlGroup(new Uri(authorityConfiguration.Authority), name: "OAuth/OpenId Server", tags: new string[] { authorityConfiguration.Authority });

            foreach (var lp in ldapServerProfiles)
            {
                var portLc = lp.GetPort(false);
                var portGc = lp.GetPort(true);

                healthChecksBuilder = healthChecksBuilder.AddTcpHealthCheck(options =>
                {
                    options.AddHost(lp.Server, portLc);
                }, name: $"Connection: {lp.Server}:{portLc}", tags: new string[] { lp.ProfileId, lp.DefaultDomainName, $"SSL:{lp.UseSSL}" });

                healthChecksBuilder = healthChecksBuilder.AddTcpHealthCheck(options =>
                {
                    options.AddHost(lp.Server, portGc);
                }, name: $"Connection: {lp.Server}:{portGc}", tags: new string[] { lp.ProfileId, lp.DefaultDomainName, $"SSL:{lp.UseSSL}" });

                healthChecksBuilder = healthChecksBuilder.AddPingHealthCheck(options => options.AddHost(lp.Server, lp.HealthCheckPingTimeout), $"Ping: {lp.Server}", tags: new string[] { lp.ProfileId, lp.DefaultDomainName, $"SSL:{lp.UseSSL}" });
            }

            services.AddHealthChecksUI(settings =>
            {
                settings
                    .SetHeaderText(webApiConfiguration.HealthChecksConfiguration.HealthChecksHeaderText)
                    .SetEvaluationTimeInSeconds(webApiConfiguration.HealthChecksConfiguration.EvaluationTime)
                    .MaximumHistoryEntriesPerEndpoint(webApiConfiguration.HealthChecksConfiguration.MaximunHistoryEntries)
                    .AddHealthCheckEndpoint(webApiConfiguration.HealthChecksConfiguration.HealthChecksGroupName, $"{webApiConfiguration.WebApiBaseUrl}/{webApiConfiguration.HealthChecksConfiguration.ApiEndPointName}");
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
                setupOptions.AddCustomStylesheet("HealthChecksUI.css");
            });
        }
    }
}