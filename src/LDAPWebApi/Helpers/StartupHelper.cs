using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Helpers
{
    public static class StartupHelpers
    {
        internal static IServiceCollection AddWebApiConfiguration(this IServiceCollection services, IConfiguration configuration, out Configurations.LDAP.WebApiConfiguration webApiConfiguration)
        {
            webApiConfiguration = configuration.GetSection(nameof(Configurations.LDAP.WebApiConfiguration)).Get<Configurations.LDAP.WebApiConfiguration>();

            services.AddSingleton(webApiConfiguration);

            return services;
        }

        internal static IServiceCollection AddLDAPServices(this IServiceCollection services, IConfiguration configuration)
        {
            var ldapServerProfiles = configuration.GetSection(nameof(Configurations.LDAP.LDAPServerProfiles)).Get<Configurations.LDAP.LDAPServerProfiles>();

            services.AddSingleton(ldapServerProfiles);

            var ldapCatalogTypeRoutes = configuration.GetSection(nameof(Configurations.LDAP.LDAPCatalogTypeRoutes)).Get<Configurations.LDAP.LDAPCatalogTypeRoutes>();

            services.AddSingleton(ldapCatalogTypeRoutes);

            return services;
        }

        internal static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
        {
            var _identityServerConfig = configuration.GetSection(nameof(Configurations.Security.IdentityServer)).Get<Configurations.Security.IdentityServer>();

            services.AddSingleton(_identityServerConfig);

            return services;
        }

        internal static IServiceCollection AddSwaggerGenService(this IServiceCollection services, Configurations.LDAP.WebApiConfiguration webApiConfiguration)
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

                #region Uncomment this region to enable IdentityServer4 authority 
                ////setupAction.OperationFilter<Bitai.LDAPWebApi.Configuration.OperationFilters.AuthorizeOperationFilter>();
                //setupAction.OperationFilter<Bitai.LDAPWebApi.Configuration.OperationFilters.AuthorizeCheckOperationFilter>();

                //setupAction.AddSecurityDefinition("OAuth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                //{
                //	Type = Microsoft.OpenApi.Models.SecuritySchemeType.OAuth2,
                //	Flows = new Microsoft.OpenApi.Models.OpenApiOAuthFlows
                //	{
                //		AuthorizationCode = new Microsoft.OpenApi.Models.OpenApiOAuthFlow
                //		{
                //			AuthorizationUrl = new Uri($"{_identityServerConfig.Authority}/connect/authorize"),
                //			TokenUrl = new Uri($"{_identityServerConfig.Authority}/connect/token"),
                //			Scopes = new Dictionary<string, string> {
                //				  { _identityServerConfig.OidcApiName, _identityServerConfig.ApiName }
                //			 }
                //		}
                //	},
                //	Description = "Bitai.LDAPWebApi OpenId Security Scheme"
                //});
                #endregion
            });

            return services;
        }
    }
}