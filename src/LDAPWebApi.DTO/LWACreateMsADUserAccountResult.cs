namespace Bitai.LDAPWebApi.DTO
{
	public class LWACreateMsADUserAccountResult: LWAOperationResult
	{
		public LDAPHelper.DTO.LDAPMsADUserAccount UserAccount { get; set; }



        /// <summary>
		/// Default constructor.
		/// </summary>
		public LWACreateMsADUserAccountResult()
		{
			//Do not remove this constructor, it is required to deserialize data.

            this.UserAccount = new LDAPHelper.DTO.LDAPMsADUserAccount();
		}

        public LWACreateMsADUserAccountResult(LDAPHelper.DTO.LDAPCreateMsADUserAccountResult model)
        {
            this.UserAccount = model.UserAccount;
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
