namespace Bitai.LDAPWebApi.DTO
{
	public class LWADisableUserAccountOperationResult: LWAOperationResult
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public LWADisableUserAccountOperationResult()
		{
			//Do not remove this constructor, it is required to deserialize data.
		}

		public LWADisableUserAccountOperationResult(LDAPHelper.DTO.LDAPDisableUserAccountOperationResult model)
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
