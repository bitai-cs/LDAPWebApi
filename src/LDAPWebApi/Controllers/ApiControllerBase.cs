using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bitai.WebApi.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Bitai.LDAPWebApi.Controllers
{
    public class ApiControllerBase : ControllerBase
    {
        protected IConfiguration Configuration { get; }
        protected Configurations.LDAP.LDAPServerProfiles ServerProfiles { get; }
        protected Configurations.LDAP.LDAPCatalogTypeRoutes CatalogTypeRoutes { get; }


        protected ApiControllerBase(IConfiguration configuration, Configurations.LDAP.LDAPServerProfiles serverProfiles, Configurations.LDAP.LDAPCatalogTypeRoutes catalogTypeRoutes)
        {
            Configuration = configuration;
            ServerProfiles = serverProfiles;
            CatalogTypeRoutes = catalogTypeRoutes;
        }


        protected LDAPHelper.ClientConfiguration GetLdapClientConfiguration(string serverProfile, bool useGlobalCatalog)
        {
            if (string.IsNullOrEmpty(serverProfile))
                throw new ArgumentNullException(nameof(serverProfile));

            var _ldapServerProfile = this.ServerProfiles.Where(p => p.ProfileId.Equals(serverProfile, StringComparison.OrdinalIgnoreCase)).Single();

            var _connectionInfo = new LDAPHelper.ConnectionInfo(_ldapServerProfile.Server, _ldapServerProfile.GetPort(useGlobalCatalog), _ldapServerProfile.GetUseSsl(useGlobalCatalog), _ldapServerProfile.ConnectionTimeout);

            var _credentials = new LDAPHelper.Credentials(_ldapServerProfile.DomainAccountName, _ldapServerProfile.DomainAccountPassword);

            var _searchLimits = new LDAPHelper.SearchLimits(_ldapServerProfile.GetBaseDN(useGlobalCatalog));

            var _return = new LDAPHelper.ClientConfiguration(_connectionInfo, _credentials, _searchLimits);

            return _return;
        }

        protected async Task<LDAPHelper.Searcher> GetLdapSearcher(LDAPHelper.ClientConfiguration clientConfiguration)
        {
            return await Task.Run(() => new LDAPHelper.Searcher(clientConfiguration));
        }

        [Obsolete("This method may be removed in future versions.")]
        protected LDAPHelper.DTO.RequiredEntryAttributes? VerifyRequiredEntryAttributes(LDAPHelper.DTO.RequiredEntryAttributes? modelValue, LDAPHelper.DTO.RequiredEntryAttributes defaultValueIfEmpty)
        {
            if (!modelValue.HasValue)
                modelValue = defaultValueIfEmpty;

            return modelValue;
        }

        protected bool IsGlobalCatalog(string ldapCatalogType)
        {
            if (CatalogTypeRoutes.GlobalCatalogRoute.Equals(ldapCatalogType, StringComparison.OrdinalIgnoreCase))
                return true;

            if (CatalogTypeRoutes.LocalCatalogRoute.Equals(ldapCatalogType, StringComparison.OrdinalIgnoreCase))
                return false;

            throw new Exception($"LDAP Catalog type '{ldapCatalogType}' not found.");
        }
    }
}
