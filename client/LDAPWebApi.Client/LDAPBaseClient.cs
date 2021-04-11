using Bitai.LDAPHelper.DTO;
using Bitai.WebApi.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitai.LDAPWebApi.Clients
{
    public class LDAPBaseClient<DTOType> : WebApiBaseClient<DTOType>
    {
        public string ServerProfile { get; set; }
        public bool UseGlobalCatalog { get; set; }



        public LDAPBaseClient(string webApiBaseUrl) : base(webApiBaseUrl)
        {
        }

        public LDAPBaseClient(string webApiBaseUrl, string serverProfile, bool useGlobalCatalog) : base(webApiBaseUrl)
        {
            ServerProfile = serverProfile;
            UseGlobalCatalog = useGlobalCatalog;
        }




        public string GetOptionalEntryAttributeName(EntryAttribute? nullable)
        {
            return nullable.HasValue ? nullable.ToString() : string.Empty;
        }

        public string GetOptionalRequiredEntryAttributesName(RequiredEntryAttributes? nullable)
        {
            return nullable.HasValue ? nullable.ToString() : string.Empty;
        }

        public string GetOptionalBooleanValue(bool? nullable)
        {
            return nullable.HasValue ? nullable.ToString() : string.Empty;
        }




        #region Static 
        public static class ControllerNames
        {
            public static readonly string ServerProfilesController = "ServerProfiles";
            public static readonly string CatalogTypesController = "CatalogTypes";
            public static readonly string CredentialsController = "Credentials";
            public static readonly string DirectoryController = "Directory";
        }
        #endregion
    }
}
