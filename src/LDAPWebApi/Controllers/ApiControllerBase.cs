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
        protected DTO.LDAPCatalogTypes CatalogTypeRoutes => new DTO.LDAPCatalogTypes();



        protected ApiControllerBase(IConfiguration configuration, Configurations.LDAP.LDAPServerProfiles serverProfiles)
        {
            Configuration = configuration;
            ServerProfiles = serverProfiles;
        }



        protected LDAPHelper.ClientConfiguration GetLdapClientConfiguration(string serverProfile, bool useGlobalCatalog)
        {
            if (string.IsNullOrEmpty(serverProfile))
                throw new ArgumentNullException(nameof(serverProfile));

            var ldapServerProfile = this.ServerProfiles.Where(p => p.ProfileId.Equals(serverProfile, StringComparison.OrdinalIgnoreCase)).Single();

            var connectionInfo = new LDAPHelper.ConnectionInfo(ldapServerProfile.Server, ldapServerProfile.GetPort(useGlobalCatalog), ldapServerProfile.GetUseSsl(useGlobalCatalog), ldapServerProfile.ConnectionTimeout);

            var credentials = new LDAPHelper.Credentials(ldapServerProfile.DomainAccountName, ldapServerProfile.DomainAccountPassword);

            var searchLimits = new LDAPHelper.SearchLimits(ldapServerProfile.GetBaseDN(useGlobalCatalog));

            return new LDAPHelper.ClientConfiguration(connectionInfo, credentials, searchLimits);
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
            if (CatalogTypeRoutes.GlobalCatalog.Equals(ldapCatalogType, StringComparison.OrdinalIgnoreCase))
                return true;

            if (CatalogTypeRoutes.LocalCatalog.Equals(ldapCatalogType, StringComparison.OrdinalIgnoreCase))
                return false;

            throw new Exception($"LDAP Catalog type '{ldapCatalogType}' not found.");
        }
    }
}
