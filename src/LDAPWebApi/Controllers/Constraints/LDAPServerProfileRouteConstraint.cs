using Bitai.LDAPWebApi.Configurations.LDAP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;

namespace Bitai.LDAPWebApi.Controllers.Constraints
{
    public class LDAPServerProfileRouteConstraint : IRouteConstraint
    {
        private readonly LDAPServerProfiles _ldapServerProfiles;

        public LDAPServerProfileRouteConstraint(LDAPServerProfiles ldapServerProfiles)
        {
            this._ldapServerProfiles = ldapServerProfiles;
        }

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (!values.TryGetValue(routeKey, out var routeValue))
                throw new Exception($"Cannot get '{routeKey}' route value.");

            return _ldapServerProfiles.Exists(m => m.ProfileId.Equals(routeValue.ToString(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
