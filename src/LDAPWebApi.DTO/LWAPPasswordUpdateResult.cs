using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Bitai.LDAPWebApi.DTO
{
	public class LWAPasswordUpdateResult: LWAOperationResult
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public LWAPasswordUpdateResult() {
			//Do not remove this constructor, it is required to deserialize data.
		}

		public LWAPasswordUpdateResult(LDAPHelper.DTO.LDAPPasswordUpdateResult model)
		{
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
