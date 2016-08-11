using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Sinks;
using System.Diagnostics;

namespace OsgContentPublishing.EventLogging
{
    public class EventLogger
    {
        ///////////////////////////////////////////////////////////////////////
        #region Public Properties

        public string AppName
        {
            get;
            protected set;
        }

        public string EventSourceName
        {
            get;
            protected set;
        }

        public string RollingLogFileName
        {
            get;
            protected set;
        }

        #endregion

        private bool loggingMessageDisplayed = false;
        public bool EventSourceExists
        {
            get
            {
                try 
                {
                    return (EventLog.SourceExists(this.EventSourceName));
                }
                catch (Exception)
                {
                    if (!loggingMessageDisplayed)
                    {
                        // stdout
                        Console.WriteLine("\nNote: A critical error occurred that could have been diagnosed with the Event Log. Run this tool from an elevated command prompt to enable logging.");
                        loggingMessageDisplayed = true;
                    }
                    return false;
                }
            }
        }

        public void StartEventLogging()
        {
            this.CreateLogSource();
            this.CreateEventListeners();
            this.LogStartup();
        }

        private void CreateLogSource()
        {
            try
            {
                if( !this.EventSourceExists )
                {
                    EventLog.CreateEventSource( this.EventSourceName, eventSourceType );
                }
            }
            catch( Exception ex )
            {
                this.LogException( ex );
            }
        }

        public void LogException( Exception ex )
        {
            this.LogEvent( ex.ToString(), EventLogEntryType.Error );
        }

        public void LogError( string errorMessage )
        {
            this.LogEvent( errorMessage, EventLogEntryType.Error );
        }

        public void LogWarning( string warningMessage )
        {
            this.LogEvent( warningMessage, EventLogEntryType.Warning );
        }

        public void LogInformational( string message )
        {
            this.LogEvent( message, EventLogEntryType.Information );
        }

        public void LogStartup()
        {
            string message = String.Format( startFormatString, this.AppName );
            this.LogInformational( message );
        }

        public void LogShutdown()
        {
            string message = String.Format( shutdownFormatString, this.AppName );
            this.LogInformational( message );
        }

        public void LogEvent( string message, EventLogEntryType entryType )
        {
            if( this.EventSourceExists )
            {
                EventLog.WriteEntry( this.EventSourceName, message, entryType );
            }

            switch( entryType )
            {
                case EventLogEntryType.Error:
                    {
                        GenTopicsEventSource.Log.Failure( message );
                        break;
                    }

                case EventLogEntryType.Warning:
                    {
                        GenTopicsEventSource.Log.Warning( message );
                        break;
                    }

                case EventLogEntryType.Information:
                    {
                        GenTopicsEventSource.Log.Informational( message );
                        break;
                    }

                default:
                    {
                        break;
                    }
            }
        }

        private void CreateEventListeners()
        {
            _consoleEventListener = ConsoleLog.CreateListener();
            _consoleEventListener.EnableEvents(
                GenTopicsEventSource.Log,
                EventLevel.Warning,
                Keywords.All );

            //_rollingFlatFileEventListener = RollingFlatFileLog.CreateListener(
            //    this.RollingLogFileName,
            //    100,
            //    "yyyy-MM-dd",
            //    RollFileExistsBehavior.Overwrite,
            //    RollInterval.Day );
            //_rollingFlatFileEventListener.EnableEvents(
            //    GenTopicsEventSource.Log,
            //    EventLevel.LogAlways,
            //    Keywords.All );
        }

        public void EndEventLogging()
        {
            LogShutdown();

            _consoleEventListener.DisableEvents( GenTopicsEventSource.Log );
            _consoleEventListener.Dispose();

            //_rollingFlatFileEventListener.DisableEvents( GenTopicsEventSource.Log );
            //_rollingFlatFileEventListener.Dispose();
        }

        static EventListener _consoleEventListener;
        static EventListener _rollingFlatFileEventListener;

        const string startFormatString = "Starting {0}";
        const string shutdownFormatString = "Shutting down {0}";
        const string eventSourceType = "Application";
    }
}
