using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.EventLogging
{
    public class GenTopicsEventLogger : EventLogger
    {
        ///////////////////////////////////////////////////////////////////////
        #region Construction

        // Singleton implementation
        // http://msdn.microsoft.com/en-us/library/dn440729(v=pandp.60).aspx

        // Disallow creation by external clients.
        private GenTopicsEventLogger()
        {
            this.AppName = appName;
            this.EventSourceName = eventSourceName;
            this.RollingLogFileName = rollingLogFileName;
        }

        public static GenTopicsEventLogger Log 
        { 
            get 
            { 
                return Instance.Value; 
            } 
        }

        private static readonly Lazy<GenTopicsEventLogger> Instance = 
            new Lazy<GenTopicsEventLogger>( () => new GenTopicsEventLogger() );

        #endregion

        const string appName = "GenTopicsUniversal";
        const string eventSourceName = "OsgContentPublishing.GenTopicsUniversal";
        const string rollingLogFileName = "GenTopicsUniversalLog.txt";
    }
}
