using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using OsgContentPublishing.ReferencePipelineLib.Deserializers;
using OsgContentPublishing.ReferencePipelineLib.Serializers;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using System.Diagnostics;

/// Contains classes that represent pipelines from source code
/// to emitted content.
namespace OsgContentPublishing.ReferencePipelineLib.Pipelines
{
    public abstract class Pipeline
    {
        public Pipeline( string name, string indexTitle, string inputFolder, string outputFolder, string siteConfigReferenceRoot )
        {
            this.ValidateInputs( inputFolder, outputFolder );
            this.Name = name;
            this.IndexTitle = indexTitle;
            this.InputFolder = inputFolder;
            this.OutputFolder = outputFolder;
            this.SiteConfigReferenceRoot = siteConfigReferenceRoot;
            this.Namespaces = null;
        }

        public Pipeline( string name, string indexTitle, string inputFolder, string outputFolder, string siteConfigReferenceRoot, List<string> namespaces )
        {
            this.ValidateInputs( inputFolder, outputFolder );
            this.Name = name;
            this.IndexTitle = indexTitle;
            this.InputFolder = inputFolder;
            this.OutputFolder = outputFolder;
            this.SiteConfigReferenceRoot = siteConfigReferenceRoot;
            this.Namespaces = namespaces;
        }

        public Pipeline( 
            string name, 
            string indexTitle, 
            string inputFolder, 
            string outputFolder, 
            string siteConfigReferenceRoot, 
            List<string> namespaces,
            bool enableLooseTypeComparisons )
        {
            this.ValidateInputs( inputFolder, outputFolder );
            this.Name = name;
            this.IndexTitle = indexTitle;
            this.InputFolder = inputFolder;
            this.OutputFolder = outputFolder;
            this.SiteConfigReferenceRoot = siteConfigReferenceRoot;
            this.Namespaces = namespaces;
            this.EnableLooseTypeComparisons = enableLooseTypeComparisons;
        }

        public Pipeline( string name, string indexTitle, string inputFolder, List<string> outputFolders )
        {
            this.ValidateInputs( inputFolder, outputFolders );
            this.Name = name;
            this.IndexTitle = indexTitle;
            throw new NotImplementedException( "Pipeline ctor" );
        }

        public string Name
        {
            get;
            private set;
        }

        public string IndexTitle
        {
            get;
            private set;
        }

        public string InputFolder
        {
            get;
            protected set;
        }

        public string OutputFolder
        {
            get;
            protected set;
        }

        public string SiteConfigReferenceRoot
        {
            get;
            private set;
        }


        public List<string> Namespaces
        {
            get;
            protected set;
        }

        public Deserializer Source
        {
            get;
            protected set;
        }

        public List<DefinedType> Types
        {
            get;
            protected set;
        }

        public List<Serializer> Destinations
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public Serializer Destination
        {
            get;
            protected set;
        }

        public bool EnableLooseTypeComparisons
        {
            get;
            protected set;
        }


        public abstract List<DefinedType> Generate();

        public abstract List<DefinedType> Deserialize();

        public virtual void Serialize( List<DefinedType> allTypes )
        {
            if( this.Types != null && this.Types.Count > 0 )
            {
                this.Destination = new HtmlSerializer(
                    this.Types,
                    allTypes,
                    this.OutputFolder,
                    this.SiteConfigReferenceRoot,
                    this.Namespaces );
                this.Destination.Serialize();
            }
            else
            {
                string msg = String.Format( "Pipeline {0} has no types to serialize", this.Name );
                Debug.WriteLine( msg );
            }
        }


        private void ValidateInputs( string inputFolder, string outputFolder )
        {
            if( !Directory.Exists( inputFolder ) )
            {
                string message = String.Format( "{0} does not exist", inputFolder );
                throw new ArgumentException( message, "inputFolder" );
            }

            if( !Directory.Exists( outputFolder ) )
            {
                string message = String.Format( "{0} does not exist", outputFolder );
                throw new ArgumentException( message, "outputFolder" );
            }
        }

        private void ValidateInputs( string inputFolder, List<string> outputFolders )
        {
            if( !Directory.Exists( inputFolder ) )
            {
                string message = String.Format( "{0} does not exist", inputFolder );
                throw new ArgumentException( message, "inputFolder" );
            }

            foreach( string folder in outputFolders )
            {
                if( !Directory.Exists( folder ) )
                {
                    string message = String.Format( "{0} does not exist", folder );
                    throw new ArgumentException( message, "outputFolders" );
                }
            }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0}", this.Name );
            return toString;
        }
    }
}
