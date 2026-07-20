using Serilog;

namespace Bitai.LDAPWebApi.Configurations.App;

/// <summary>
/// Web Api log _configuration model 
/// </summary>
public class WebApiLogConfiguration
{
    /// <summary>
    /// Specifies the meaning and relative importance of a log event.
    /// </summary>        
    public enum MinimunLogEventLevel
    {
        /// <summary>
        /// Anything and everything you might want to know about
        /// a running block of code.
        /// </summary>
        Verbose,

        /// <summary>
        /// Internal system events that aren't necessarily
        /// observable from the outside.
        /// </summary>
        Debug,

        /// <summary>
        /// The lifeblood of operational intelligence - things
        /// happen.
        /// </summary>
        Information,

        /// <summary>
        /// Service is degraded or endangered.
        /// </summary>
        Warning,

        /// <summary>
        /// Functionality is unavailable, invariants are broken
        /// or data is lost.
        /// </summary>
        Error,

        /// <summary>
        /// If you have a pager, it goes off when one of these
        /// occurs.
        /// </summary>
        Fatal
    }




    /// <summary>
    /// Constructor.
    /// Set default property values.
    /// </summary>
    public WebApiLogConfiguration()
    {
        ConsoleLog = new ConsoleLogSetup
        {
#if DEBUG
            Enabled = true,
			MinimunLogEventLevel = MinimunLogEventLevel.Verbose
#else
            Enabled = false,
            MinimunLogEventLevel = MinimunLogEventLevel.Error
#endif
		};

        FileLog = new FileLogSetup
        {
            Enabled = true,
            LogFilePath = "./logs/Bitai.LDAPWebApi-.log",
#if DEBUG
			MinimunLogEventLevel = MinimunLogEventLevel.Verbose
#else
            MinimunLogEventLevel = MinimunLogEventLevel.Error
#endif
		};

    }




    /// <summary>
    /// Console log _configuration
    /// </summary>
    public ConsoleLogSetup ConsoleLog { get; set; }

    /// <summary>
    /// File log _configuration
    /// </summary>
    public FileLogSetup FileLog { get; set; }

    #region Inner 
    /// <summary>
    /// Inner class to configure console log
    /// </summary>
    public class ConsoleLogSetup
    {
        /// <summary>
        /// Enable or disable logging
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Minimun log level. It can be Information, Warning, Error.
        /// </summary>
        public MinimunLogEventLevel MinimunLogEventLevel { get; set; }



        /// <summary>
        /// Constructor
        /// </summary>
        public ConsoleLogSetup()
        {
            Enabled = true;
            MinimunLogEventLevel = MinimunLogEventLevel.Verbose;
        }
    }

    /// <summary>
    /// Inner class to configure file log
    /// </summary>
    public class FileLogSetup
    {
        /// <summary>
        /// Enable or disable logging
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Log file path
        /// </summary>
        public string LogFilePath { get; set; }

        /// <summary>
        /// See <see cref="MinimunLogEventLevel"/>
        /// </summary>
        public MinimunLogEventLevel MinimunLogEventLevel { get; set; }

        /// <summary>
        /// See <see cref="RollingInterval"/>
        /// </summary>
		public RollingInterval RollingInterval { get; set; }

        /// <summary>
        /// Retained file count limit
        /// </summary>
		public short RetainedFileCountLimit { get; set; }

        /// <summary>
        /// Flush to disk interval in minutes
        /// </summary>
		public short FlushToDiskIntervalInMinutes { get; set; }



		/// <summary>
		/// Constructor
		/// </summary>
		public FileLogSetup()
        {
            Enabled = true;
            LogFilePath = "./logs/Bitai.LDAPWebApi-.log";
            MinimunLogEventLevel = MinimunLogEventLevel.Verbose;
			RollingInterval = RollingInterval.Day;
            RetainedFileCountLimit = 30;
            FlushToDiskIntervalInMinutes = 5;
		}
    }

    #endregion
}
