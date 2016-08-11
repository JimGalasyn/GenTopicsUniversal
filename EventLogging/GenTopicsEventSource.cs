using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;

namespace OsgContentPublishing.EventLogging
{
    [EventSource( Name = "GenTopicsUniversal" )]
    internal class GenTopicsEventSource : EventSource
    {
        ///////////////////////////////////////////////////////////////////////
        #region Contruction

        // Singleton implementation
        // http://msdn.microsoft.com/en-us/library/dn440729(v=pandp.60).aspx

        // Disallow creation by external clients.
        private GenTopicsEventSource()
        {
        }
        
        private static readonly Lazy<GenTopicsEventSource> Instance = 
            new Lazy<GenTopicsEventSource>( () => new GenTopicsEventSource() );
        
        public static GenTopicsEventSource Log 
        { 
            get 
            { 
                return Instance.Value; 
            } 
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Logging Methods

        [Event( 1, Message = "Application Failure: {0}", Level = EventLevel.Critical, Keywords = Keywords.Diagnostic )]
        public void Failure( string message )
        {
            this.WriteEvent( 1, message );
        }

        [Event( 2, Message = "Starting up {0}", Keywords = Keywords.Perf, Level = EventLevel.Informational )]
        public void Startup( string message )
        {
            this.WriteEvent( 2, message );
        }

        //[Event( 3, Message = "loading page {1} activityID={0}", Opcode = EventOpcode.Start, Task = Tasks.Page, Keywords = Keywords.Page, Level = EventLevel.Informational )]
        //internal void PageStart( int ID, string url )
        //{
        //    if( this.IsEnabled() )
        //    {
        //        this.WriteEvent( 3, ID, url );
        //    }
        //}

        [Event( 4, Message = "Warning: {0}", Keywords = Keywords.Diagnostic, Level = EventLevel.Warning )]
        public void Warning( string message )
        {
            this.WriteEvent( 4, message );
        }

        [Event( 5, Message = "Status: {0}", Keywords = Keywords.Perf, Level = EventLevel.Informational )]
        public void Informational( string message )
        {
            this.WriteEvent( 5, message );
        }

        //[Event( 6, Message = "Trace: {0}", Keywords = Keywords.Diagnostic, Level = EventLevel.Verbose )]
        //public void Trace( string message )
        //{
        //    this.WriteEvent( 6, message );
        //}


        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Logging Event Keywords and Tasks

        public class Keywords
        {
            public const EventKeywords Page = (EventKeywords)1;
            public const EventKeywords DataBase = (EventKeywords)2;
            public const EventKeywords Diagnostic = (EventKeywords)4;
            public const EventKeywords Perf = (EventKeywords)8;
        }

        public class Tasks
        {
            public const EventTask Page = (EventTask)1;
            public const EventTask DBQuery = (EventTask)2;
        }

        #endregion
    }
}
