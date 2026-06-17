using Microsoft.VisualBasic;

namespace Bitai.LDAPWebApi.DTO;

public class LWAOperationExceptionInfo
{
    public string Message {get; set;}
    public string? Source { get; set;}
    public string? StackTrace { get; set; }
    public LWAOperationExceptionInfo? InnerExceptionInfo { get; set;}



    internal LWAOperationExceptionInfo(Exception ex)
    {
        Message = ex.Message;
        Source = ex.Source;
        StackTrace = ex.StackTrace;

        var innerEx = ex.InnerException;
        while(innerEx != null)
            InnerExceptionInfo = new LWAOperationExceptionInfo(innerEx);
    }
}
