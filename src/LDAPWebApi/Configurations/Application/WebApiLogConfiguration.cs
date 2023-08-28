﻿using System;
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
        ConsoleLog = new ConsoleSetup
        {
            Enabled = true,
            MinimunLogEventLevel = MinimunLogEventLevel.Verbose
        };

        FileLog = new FileLogSetup
        {
            Enabled = true,
            LogFilePath = "./logs/Bitai.LDAPWebApi-.log",
            MinimunLogEventLevel = MinimunLogEventLevel.Verbose
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





    public ConsoleSetup ConsoleLog { get; set; }

    public FileLogSetup FileLog { get; set; }

    public GrafanaLokiLogSetup GrafanaLokiLog { get; set; }

    public ElasticsearchLogSetup ElasticsearchLog { get; set; }




    #region Inner classes
    public class ConsoleSetup
    {
        /// <summary>
        /// Enable or disable logging
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Minimun log level. It can be Information, Warning, Error.
        /// </summary>
        public MinimunLogEventLevel MinimunLogEventLevel { get; set; }



        public ConsoleSetup()
        {
            Enabled = true;
            MinimunLogEventLevel = MinimunLogEventLevel.Verbose;
        }
    }

    public class FileLogSetup
    {
        /// <summary>
        /// Enable or disable logging
        /// </summary>
        public bool Enabled { get; set; }

        public string LogFilePath { get; set; }

        /// <summary>
        /// See <see cref="MinimunLogEventLevel"/>
        /// </summary>
        public MinimunLogEventLevel MinimunLogEventLevel { get; set; }



        public FileLogSetup()
        {
            Enabled = true;
            LogFilePath = "./logs/Bitai.LDAPWebApi-.log";
            MinimunLogEventLevel = MinimunLogEventLevel.Verbose;
        }
    }

    public class GrafanaLokiLogSetup
    {
        /// <summary>
        /// Enable or disable logging
        /// </summary>
        public bool Enabled { get; set; }

        public string LokiUrl { get; set; }

        public int BatchPostingLimit { get; set; }

        public TimeSpan Period { get; set; }

        public string AppName { get; set; }

        /// <summary>
        /// See <see cref="MinimunLogEventLevel"/>
        /// </summary>
        public MinimunLogEventLevel MinimunLogEventLevel { get; set; }



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

    public class ElasticsearchLogSetup
    {
        /// <summary>
        /// Enable or disable logging
        /// </summary>
        public bool Enabled { get; set; }

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