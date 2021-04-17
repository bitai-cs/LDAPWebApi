using Bitai.LDAPWebApi.Configurations.LDAP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;

namespace Bitai.LDAPWebApi.Controllers.Constraints
{
    public class LDAPCatalogTypeRouteConstraint : IRouteConstraint
    {
        private readonly LDAPCatalogTypeRoutes _ldapCatalogTypeRoutes;

        public LDAPCatalogTypeRouteConstraint(LDAPCatalogTypeRoutes ldapCatalogTypeRoutes)
        {
            _ldapCatalogTypeRoutes = ldapCatalogTypeRoutes;
        }

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (!values.TryGetValue(routeKey, out var routeValue))
                throw new Exception($"Cannot get '{routeKey}' route value.");

            if (_ldapCatalogTypeRoutes.LocalCatalogRoute.Equals(routeValue.ToString(), StringComparison.OrdinalIgnoreCase))
                return true;

            if (_ldapCatalogTypeRoutes.GlobalCatalogRoute.Equals(routeValue.ToString(), StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}
