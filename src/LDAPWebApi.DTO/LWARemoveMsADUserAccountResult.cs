namespace Bitai.LDAPWebApi.DTO
{
	public class LWARemoveMsADUserAccountResult: LWAOperationResult
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public LWARemoveMsADUserAccountResult()
		{
			//Do not remove this constructor, it is required to deserialize data.
		}

		public LWARemoveMsADUserAccountResult(LDAPHelper.DTO.LDAPRemoveMsADUserAccountResult model)
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
