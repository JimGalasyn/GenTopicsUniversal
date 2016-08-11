using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.EventLogging;
using OsgContentPublishing.GenTopicsUniversal.Configuration;
using OsgContentPublishing.ReferencePipelineLib;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Projected;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly;
using OsgContentPublishing.ReferencePipelineLib.Pipelines;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

namespace OsgContentPublishing.GenTopicsUniversal
{
    /// <summary>
    /// GenTopicsUniversal generates API reference documentation from code comments.
    /// </summary>
    class Program
    {
        static int Main( string[] args )
        {
            int retval = succeededRetVal;

            GenTopicsEventLogger.Log.StartEventLogging();

            retval = AssignArgs( args );

            if( retval == succeededRetVal )
            {
                Generate();
            }

            GenTopicsEventLogger.Log.EndEventLogging();

            return retval;
        }

        public static int Generate()
        {
            int retval = succeededRetVal;

            try
            {
                PipelinesSection pipelinesSection = null;

                if( SatelliteConfigFilePath != null )
                {
                    pipelinesSection = new PipelinesSection( SatelliteConfigFilePath );
                }
                else
                {
                    pipelinesSection = ConfigurationManager.GetSection( pipelinesConfigSectionName ) as PipelinesSection;
                }

                // Throws if configuration has issues.
                List<ContentSet> contentSets = CreateContentSets( pipelinesSection );

                foreach( ContentSet contentSet in contentSets )
                {
                    retval = contentSet.Generate();
                    if( retval == succeededRetVal )
                    {
                        string successMessage = String.Format( 
                            "Content set {0} completed successfully", 
                            contentSet.Name );
                        Debug.WriteLine( successMessage );
                        GenTopicsEventLogger.Log.LogInformational( successMessage );
                    }
                    else
                    {
                        string errorMessage = String.Format( 
                            "Content set {0} completed with errors", 
                            contentSet.Name );
                        Debug.WriteLine( errorMessage );
                        GenTopicsEventLogger.Log.LogError( errorMessage );
                    }
                }
            }
            catch( Exception ex )
            {
                GenTopicsEventLogger.Log.LogException( ex );
                retval = failedRetVal;
            }

            return retval;
        }

        private static List<ContentSet> CreateContentSets( PipelinesSection pipelinesSection )
        {
            List<ContentSet> contentSets = new List<ContentSet>();

            if( pipelinesSection.Pipelines.Count > 0 )
            {
                foreach( PipelineElement pipelineElement in pipelinesSection.Pipelines )
                {
                    Pipeline pipeline = CreatePipeline( pipelineElement );

                    string contentSetName = pipelineElement.ContentSet;
                    var contentSet = contentSets.FirstOrDefault( cs => cs.Name == contentSetName );
                    if( contentSet == null )
                    {
                        contentSet = new ContentSet(
                            contentSetName,
                            pipelineElement.OutputFolderPath,
                            pipelineElement.SiteConfigReferenceRoot );
                        contentSets.Add( contentSet );
                    }

                    contentSet.Pipelines[pipelineElement.Name] = pipeline;
                }
            }
            else
            {
                string errorMessage = "No pipelines specified in configuration.";
                throw new ConfigurationErrorsException( errorMessage );
            }

            return contentSets;
        }

        private static Pipeline CreatePipeline( PipelineElement pipelineConfigElement )
        {
            Pipeline pipeline = null;

            // Throws if pipeline isn't valid.
            ValidatePipelineConfiguration( pipelineConfigElement );

            List<string> namespaces = pipelineConfigElement.Namespaces.AllKeys.ToList();

            if( pipelineConfigElement.PipelineType == "RidlPipeline" )
            {
                pipeline = new RidlPipeline(
                    pipelineConfigElement.Name,
                    pipelineConfigElement.IndexTitle,
                    pipelineConfigElement.InputFolderPath,
                    pipelineConfigElement.WinmdFilePath,
                    pipelineConfigElement.OutputFolderPath,
                    pipelineConfigElement.SiteConfigReferenceRoot,
                    namespaces,
                    pipelineConfigElement.EnableLooseTypeComparisons );
            }
            else if( pipelineConfigElement.PipelineType == "GeneratedPipeline" )
            {
                pipeline = new GeneratedPipeline(
                    pipelineConfigElement.Name,
                    pipelineConfigElement.IndexTitle,
                    pipelineConfigElement.NativeFolderPath,
                    pipelineConfigElement.InputFolderPath,
                    pipelineConfigElement.OutputFolderPath,
                    pipelineConfigElement.SiteConfigReferenceRoot,
                    namespaces,
                    pipelineConfigElement.EnableLooseTypeComparisons );
            }
            else if( pipelineConfigElement.PipelineType == "NativePipeline" )
            {
                pipeline = new NativePipeline(
                    pipelineConfigElement.Name,
                    pipelineConfigElement.IndexTitle,
                    pipelineConfigElement.InputFolderPath,
                    pipelineConfigElement.OutputFolderPath,
                    pipelineConfigElement.SiteConfigReferenceRoot,
                    namespaces );
            }
            else if( pipelineConfigElement.PipelineType == "ManagedAssemblyPipeline" )
            {
                pipeline = new ManagedAssemblyPipeline(
                    pipelineConfigElement.Name,
                    pipelineConfigElement.IndexTitle,
                    pipelineConfigElement.InputFolderPath,
                    pipelineConfigElement.AssemblyFilePath,
                    pipelineConfigElement.OutputFolderPath,
                    pipelineConfigElement.SiteConfigReferenceRoot,
                    namespaces );
            }
            else if( pipelineConfigElement.PipelineType == "ManagedPipeline" )
            {
                pipeline = new ManagedPipeline(
                    pipelineConfigElement.Name,
                    pipelineConfigElement.IndexTitle,
                    pipelineConfigElement.InputFolderPath,
                    pipelineConfigElement.OutputFolderPath,
                    pipelineConfigElement.SiteConfigReferenceRoot,
                    namespaces );
            }

            return pipeline;
        }

        private static void ValidatePipelineConfiguration( PipelineElement pipelineElement )
        {
            if( !Directory.Exists( pipelineElement.InputFolderPath ) )
            {
                string msg = String.Format(
                    "Could not find input folder at {0}.",
                    pipelineElement.InputFolderPath );
                throw new ConfigurationErrorsException( msg );
            }

            if( !Directory.Exists( pipelineElement.OutputFolderPath ) )
            {
                string msg = String.Format(
                    "Could not find output folder at {0}.",
                    pipelineElement.OutputFolderPath );
                Console.WriteLine( msg );
                throw new ConfigurationErrorsException( msg );
            }

            if( pipelineElement.PipelineType == "RidlPipeline" &&
                !File.Exists( pipelineElement.WinmdFilePath ) )
            {
                string msg = String.Format(
                    "Could not find winmd file at {0}.",
                    pipelineElement.WinmdFilePath );
                Console.WriteLine( msg );
                throw new ConfigurationErrorsException( msg );
            }
        }

        private static string SatelliteConfigFilePath
        {
            get;
            set;
        }

        private static int AssignArgs( string[] args )
        {
            int retval = succeededRetVal;

            if( args.Length == 0 )
            {
                retval = succeededRetVal;
            }
            else if( args.Length == 1 )
            {
                string path = args[0] as string;
                if( File.Exists( path ) )
                {
                    SatelliteConfigFilePath = path;
                }
            }
            else
            {
                retval = failedRetVal;
            }

            if( retval == failedRetVal )
            {
                GenTopicsEventLogger.Log.LogError( usageString );
                Debug.WriteLine( usageString );
            }

            return retval;
        }

        const int succeededRetVal = 0;
        const int failedRetVal = -1;
        const string usageString = "Usage: Edit App.config to set up API reference generation pipelines.";
        const string pipelinesConfigSectionName = "pipelinesGroup/pipelinesSection";
        const int waitArgIndex = 0;
    }
}
