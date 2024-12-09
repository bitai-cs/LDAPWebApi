using Serilog;
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

        GrafanaLokiLog = new GrafanaLokiLogSetup
        {
            Enabled = false,
            LokiUrl = "http://localhost:3100",
            Period = new TimeSpan(0, 0, 2),
            BatchPostingLimit = 100,
            AppName = "Bitai.LDAPWebApi",
            MinimunLogEventLevel = MinimunLogEventLevel.Error
        };

        ElasticsearchLog = new ElasticsearchLogSetup
        {
            Enabled = false,
            ElasticsearchNodeUrls = new string[] { "http://localhost:9200" },
            MinimunLogEventLevel = MinimunLogEventLevel.Error
        };
    }




    /// <summary>
    /// Console log configuration
    /// </summary>
    public ConsoleLogSetup ConsoleLog { get; set; }

    /// <summary>
    /// File log configuration
    /// </summary>
    public FileLogSetup FileLog { get; set; }

    /// <summary>
    /// Grafana Loki log configuration
    /// </summary>
    public GrafanaLokiLogSetup GrafanaLokiLog { get; set; }

    /// <summary>
    /// Elasticsearch log configuration
    /// </summary> 
    public ElasticsearchLogSetup ElasticsearchLog { get; set; }




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

    /// <summary>
    /// Inner class to configure Grafana Loki log
    /// </summary>
    public class GrafanaLokiLogSetup
    {
        /// <summary>
        /// Enable or disable logging
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Loki URL
        /// </summary>
        public string LokiUrl { get; set; }

        /// <summary>
        /// Batch posting limit
        /// </summary>
        public int BatchPostingLimit { get; set; }

        /// <summary>
        /// Posting period
        /// </summary>
        public TimeSpan Period { get; set; }

        /// <summary>
        /// Application name
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// See <see cref="MinimunLogEventLevel"/>
        /// </summary>
        public MinimunLogEventLevel MinimunLogEventLevel { get; set; }



        /// <summary>
        /// Constructor
        /// </summary>
        public GrafanaLokiLogSetup()
        {
            Enabled = false;
            LokiUrl = "http://localhost:3100";
            BatchPostingLimit = 100;
            Period = TimeSpan.FromSeconds(2);
            AppName = "Bitai.LDAPWebApi";
            MinimunLogEventLevel = MinimunLogEventLevel.Error;
        }
    }


    /// <summary>
    /// Inner class to configure Elasticsearch log
    /// </summary>
    public class ElasticsearchLogSetup
    {
        /// <summary>
        /// Enable or disable logging
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// URLs of Elasticsearch nodes
        /// </summary>
        public string[] ElasticsearchNodeUrls { get; set; }

        /// <summary>
        /// See <see cref="MinimunLogEventLevel"/>
        /// </summary>
        public MinimunLogEventLevel MinimunLogEventLevel { get; set; }



        /// <summary>
        /// Constructor
        /// </summary>
        public ElasticsearchLogSetup()
        {
            Enabled = false;
            ElasticsearchNodeUrls = new string[] { "http://localhost:9200" };
            MinimunLogEventLevel = MinimunLogEventLevel.Error;
        }



        /// <summary>
        /// Get <see cref="IEnumerable{Uri}"/> from <see cref="ElasticsearchNodeUrls"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Uri> GetElasticsearchNodeUris()
        {
            var uris = new List<Uri>();

            if (ElasticsearchNodeUrls.GetLength(0) > 0)
            {
                foreach (var url in ElasticsearchNodeUrls)
                    uris.Add(new Uri(url));
            }

            return uris;
        }
    }
    #endregion
}