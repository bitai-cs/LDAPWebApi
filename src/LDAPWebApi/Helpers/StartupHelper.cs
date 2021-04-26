using Bitai.LDAPWebApi.Configurations.LDAP;
using Bitai.LDAPWebApi.Configurations.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Bitai.LDAPWebApi.Helpers
{
    public static class StartupHelpers
    {
        internal static IServiceCollection AddWebApiConfiguration(this IServiceCollection services, IConfiguration configuration, out Configurations.App.WebApiConfiguration webApiConfiguration)
        {
            webApiConfiguration = configuration.GetSection(nameof(Configurations.App.WebApiConfiguration)).Get<Configurations.App.WebApiConfiguration>();

            services.AddSingleton(webApiConfiguration);

            return services;
        }

        internal static IServiceCollection ConfigureWebApiCors(this IServiceCollection services, IConfiguration configuration)
        {
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
            ldapServerProfiles = configuration.GetSection(nameof(LDAPServerProfiles)).Get<LDAPServerProfiles>();

            ldapServerProfiles.CheckConfigurationIntegrity();

            return services.AddSingleton(ldapServerProfiles);
        }

        internal static IServiceCollection AddIdentityServerConfiguration(this IServiceCollection services, IConfiguration configuration, out IdentityServerConfiguration identityServerConfig)
        {
            identityServerConfig = configuration.GetSection(nameof(IdentityServerConfiguration)).Get<IdentityServerConfiguration>();

            return services.AddSingleton(identityServerConfig);
        }

        internal static IServiceCollection ConfigureSwaggerGenerator(this IServiceCollection services, Configurations.App.WebApiConfiguration webApiConfiguration, Configurations.Security.IdentityServerConfiguration identityServerConfiguration)
        {
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
                            AuthorizationUrl = new Uri($"{identityServerConfiguration.Authority}/connect/authorize"),
                            TokenUrl = new Uri($"{identityServerConfiguration.Authority}/connect/token"),
                            Scopes = new Dictionary<string, string> {
                                  { identityServerConfiguration.SwaggerUITargetApiScope, identityServerConfiguration.SwaggerUITargetApiScopeTitle }
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
            services.Configure<RouteOptions>(config =>
            {
                config.ConstraintMap.Add("ldapSvrPf", typeof(Controllers.Constraints.LDAPServerProfileRouteConstraint));
                config.ConstraintMap.Add("ldapCatType", typeof(Controllers.Constraints.LDAPCatalogTypeRouteConstraint));
            });

            return services;
        }

        internal static IServiceCollection AddAuthenticationWithIdentityServer(this IServiceCollection services, IdentityServerConfiguration identityServerConfiguration)
        {
            services.AddAuthentication(co =>
            {
                co.DefaultScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                co.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            })
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = identityServerConfiguration.Authority;
                    options.ApiName = identityServerConfiguration.ApiResource;
                    options.RequireHttpsMetadata = identityServerConfiguration.RequireHttpsMetadata;
                });

            return services;
        }

        internal static void AddCustomHealthChecks(this IHealthChecksBuilder healthChecksBuilder, IdentityServerConfiguration identityServerConfiguration, LDAPServerProfiles ldapServerProfiles)
        {
            healthChecksBuilder = healthChecksBuilder.AddUrlGroup(new Uri(identityServerConfiguration.Authority), name: "Identity Server Authority", tags: new string[] { identityServerConfiguration.Authority });

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
        }
    }
}