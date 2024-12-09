using Bitai.LDAPWebApi.Configurations.LDAP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;

namespace Bitai.LDAPWebApi.Controllers.Constraints;

/// <summary>
/// Constraint that matches the route value if the value is one of the catalog types
/// specified in the <see cref="DTO.LDAPServerCatalogTypes"/> class.
/// </summary>
public class LDAPCatalogTypeRouteConstraint : IRouteConstraint
{
    private readonly DTO.LDAPServerCatalogTypes _ldapCatalogTypeRoutes = new DTO.LDAPServerCatalogTypes();



    /// <summary>
    /// Constructor
    /// </summary>
    public LDAPCatalogTypeRouteConstraint()
    {
    }



    /// <summary>
    /// Represents a route constraint that matches the route value if the value is one of the catalog types
    /// specified in the <see cref="DTO.LDAPServerCatalogTypes"/> class.
    /// </summary>
    /// <remarks>
    /// This constraint is used to ensure that the route value matches one of the catalog types
    /// in <see cref="DTO.LDAPServerCatalogTypes"/> class. This is used to validate the route value
    /// in the context of the <see cref="Controllers.CatalogTypesController"/> controller.
    /// </remarks>
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (!values.TryGetValue(routeKey, out var routeValue))
            throw new Exception($"Cannot get '{routeKey}' route value.");

        if (_ldapCatalogTypeRoutes.LocalCatalog.Equals(routeValue!.ToString(), StringComparison.OrdinalIgnoreCase))
            return true;

        if (_ldapCatalogTypeRoutes.GlobalCatalog.Equals(routeValue!.ToString(), StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}