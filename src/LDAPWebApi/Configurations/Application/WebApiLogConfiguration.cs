using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Configurations.App;

/// <summary>
/// Web Api log configuration model 
/// </summary>
public class WebApiLogConfiguration
{
    /// <summary>
    /// Constructor.
    /// Set default property values.
    /// </summary>
    public WebApiLogConfiguration()
		{
        MinimunLogLevel = MinimunLogEventLevel.Information;
        LogFilePath = "./logs/Bitai.LDAPWebApi-.log";
		}



    /// <summary>
    /// Specifies the meaning and relative importance of a log event.
    /// </summary>        
    public enum MinimunLogEventLevel
    {
        /// <summary>
        /// Informative and more serious messages.
        /// </summary>
        Information = 2,
        /// <summary>
        /// Warning and more serious messages.
        /// </summary>
        Warning = 3,
        /// <summary>
        /// Error and more serious messages.
        /// </summary>
        Error = 4,            
    }

    /// <summary>
    /// Minimun log level. It can be Information, Warning, Error.
    /// </summary>
    public MinimunLogEventLevel MinimunLogLevel { get; set; }

    /// <summary>
    /// Log file path.
    /// </summary>
    public string LogFilePath { get; set; }
}
