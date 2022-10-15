using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bitai.LDAPWebApi.Configurations.Authorization;

public class AuthorizeCheckOperationFilter : IOperationFilter
{
    private readonly Swagger.SwaggerUIConfiguration _swaggerUIConfiguration;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="swaggerUIConfiguration">See <see cref="Swagger.SwaggerUIConfiguration"/></param>
    public AuthorizeCheckOperationFilter(Swagger.SwaggerUIConfiguration swaggerUIConfiguration)
    {
        _swaggerUIConfiguration = swaggerUIConfiguration;
    }

	/// <summary>
	/// Implementation of <see cref="IOperationFilter.Apply(OpenApiOperation, OperationFilterContext)"/>
	/// </summary>
	/// <param name="operation">See <see cref="OpenApiOperation"/></param>
	/// <param name="context">See <see cref="OperationFilterContext"/></param>
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize = context.MethodInfo.DeclaringType != null && (context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any());

        if (hasAuthorize)
        {
            operation.Responses.Add(StatusCodes.Status401Unauthorized.ToString(), new OpenApiResponse { Description = nameof(HttpStatusCode.Unauthorized) });

            operation.Responses.Add(StatusCodes.Status403Forbidden.ToString(), new OpenApiResponse { Description = nameof(HttpStatusCode.Forbidden) });

            operation.Security = new List<OpenApiSecurityRequirement> {
                new OpenApiSecurityRequirement {
                            [
                                 new OpenApiSecurityScheme {
                                     Reference = new OpenApiReference {
                                            Type = ReferenceType.SecurityScheme,
                                            Id = "OAuth2"
                                     }
                                 }
                            ] = new[] { _swaggerUIConfiguration.SwaggerUITargetApiScope }
                      }
            };
        }
    }
}