using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using OsgContentPublishing.ReferencePipelineLib.Deserializers;
using OsgContentPublishing.ReferencePipelineLib.Serializers;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology;

namespace OsgContentPublishing.ReferencePipelineLib.Pipelines
{
    public class RidlPipeline : Pipeline
    {
        public RidlPipeline( string name, string indexTitle, string inputFolder, string winmdPath, string outputFolder, string siteConfigReferenceRoot, List<string> namespaces )
            : base( name, indexTitle, inputFolder, outputFolder, siteConfigReferenceRoot, namespaces )
        {
            this.ValidateWinmdPath( winmdPath );
            this.WinmdPath = winmdPath;
            this.Initialize();
        }

        public RidlPipeline( 
            string name, 
            string indexTitle, 
            string inputFolder, 
            string winmdPath, 
            string outputFolder, 
            string siteConfigReferenceRoot, 
            List<string> namespaces,
            bool enableLooseTypeComparisons )
            : base( name, indexTitle, inputFolder, outputFolder, siteConfigReferenceRoot, namespaces, enableLooseTypeComparisons )
        {
            this.ValidateWinmdPath( winmdPath );
            this.WinmdPath = winmdPath;
            this.Initialize();
        }

        public override List<DefinedType> Deserialize()
        {
            var types = this.Source.Deserialize();
            var disjointSet = ( (RidlDeserializer)this.Source ).DisjointSet;

            // Add types at global scope that aren't projected.
            // TBD: Do we really want to do this? Or should we 
            // create a separate collection?
            var globalTypes = disjointSet.Where( t => t.Namespace != null && t.Namespace.IsGlobalNamespace );

            types.AddRange( globalTypes );

            this.Types = types;

            return this.Types;
        }

        public override void Serialize( List<DefinedType> allTypes )
        {
            if( this.Types != null && this.Types.Count > 0 )
            {
                this.Destination = new HtmlSerializer(
                    this.Types,
                    allTypes,
                    this.OutputFolder,
                    this.SiteConfigReferenceRoot,
                    this.Namespaces,
                    true ); // This is RIDL, so emit only projected types.
                this.Destination.Serialize();

                string intellisenseFileName = Path.GetFileNameWithoutExtension( this.WinmdPath );
                IntellisenseSerializer intellisenseSerializer = new IntellisenseSerializer(
                    intellisenseFileName,
                    this.Types,
                    null,
                    this.OutputFolder,
                    Path.ChangeExtension( intellisenseFileName, "xml" ) );
                intellisenseSerializer.Serialize();
            }
            else
            {
                string msg = String.Format( "Pipeline {0} has no types to serialize", this.Name );
                Debug.WriteLine( msg );
            }
        }

        public override List<DefinedType> Generate()
        {
            this.Deserialize();
            this.Serialize( this.Types );

            return this.Types;
        }

        private void Initialize()
        {
            // Create Deserializer and Serializer.
            string schemaPath = Path.Combine( this.InputFolder, "compound.xsd" );
            this.Source = new RidlDeserializer( 
                this.InputFolder, 
                schemaPath, 
                this.WinmdPath, 
                this.Namespaces, 
                this.EnableLooseTypeComparisons );

        }

        private void ValidateWinmdPath( string winmdPath )
        {
            if( !File.Exists( winmdPath ) )
            {
                string message = String.Format( "{0} does not exist", winmdPath );
                throw new ArgumentException( message, "winmdPath" );
            }
        }

        public string WinmdPath
        {
            get;
            protected set;
        }
    }
}
