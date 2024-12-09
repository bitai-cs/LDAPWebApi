using Bitai.LDAPWebApi.Configurations.LDAP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;

namespace Bitai.LDAPWebApi.Controllers.Constraints;

/// <summary>
/// Route constraint to check if the route value matches a registered LDAP Server Profile.
/// </summary>
public class LDAPServerProfileRouteConstraint : IRouteConstraint
{
    private readonly LDAPServerProfiles _ldapServerProfiles;



    /// <summary>
    /// Route constraint to check if the route value matches a registered LDAP Server Profile.
    /// </summary>
    /// <remarks>
    /// This constraint is used to validate the route value in the context of the
    /// <see cref="ServerProfilesController"/> controller.
    /// The route value is expected to be a string that matches one of the LDAP Server Profiles
    /// registered in the <see cref="LDAPServerProfiles"/> instance provided in the constructor.
    /// </remarks>
    public LDAPServerProfileRouteConstraint(LDAPServerProfiles ldapServerProfiles)
    {
        this._ldapServerProfiles = ldapServerProfiles;
    }



    /// <summary>
    /// Route constraint to check if the route value matches a registered LDAP Server Profile.
    /// </summary>
    /// <remarks>
    /// This constraint is used to validate the route value in the context of the
    /// <see cref="ServerProfilesController"/> controller.
    /// The route value is expected to be a string that matches one of the LDAP Server Profiles
    /// registered in the <see cref="LDAPServerProfiles"/> instance provided in the constructor.
    /// </remarks>
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (!values.TryGetValue(routeKey, out var routeValue))
            throw new Exception($"Cannot get '{routeKey}' route value.");

        return _ldapServerProfiles.Exists(m => m.ProfileId.Equals(routeValue!.ToString(), StringComparison.OrdinalIgnoreCase));
    }
}
