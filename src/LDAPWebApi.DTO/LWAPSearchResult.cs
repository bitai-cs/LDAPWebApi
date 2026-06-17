using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Bitai.LDAPWebApi.DTO
{
	public class LWASearchResult: LWAOperationResult
	{
		/// <summary>
		/// LDAP entries found in search.
		/// </summary>
		public IEnumerable<LDAPHelper.DTO.LDAPEntry> Entries { get; set; }




		/// <summary>
		/// Defaulr constructor.
		/// </summary>
		public LWASearchResult()
		{
			//Do not remove this constructor, it is required to deserialize data.

            Entries = new List<LDAPHelper.DTO.LDAPEntry>();
		}

		public LWASearchResult(LDAPHelper.DTO.LDAPSearchResult model): this()
		{
            this.Entries = model.Entries;
			this.OperationMessage = model.OperationMessage;
            if (model.HasErrorObject)
            {
                this.ErrorObject = new LWAOperationExceptionInfo(model.ErrorObject);
                this.ErrorType = model.ErrorType;
            }
            this.IsSuccessfulOperation = model.IsSuccessfulOperation;
            this.RequestLabel = model.RequestLabel;
		}
	}
}
