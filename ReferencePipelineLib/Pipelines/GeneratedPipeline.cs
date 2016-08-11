using OsgContentPublishing.ReferencePipelineLib.Deserializers;
using OsgContentPublishing.ReferencePipelineLib.Serializers;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.Pipelines
{
    public class GeneratedPipeline : Pipeline
    {           
        public GeneratedPipeline( 
            string name, 
            string indexTitle, 
            string nativeFolder, 
            string inputFolder,
            string outputFolder, 
            string siteConfigReferenceRoot, 
            List<string> namespaces )
            : base( name, indexTitle, inputFolder, outputFolder, siteConfigReferenceRoot, namespaces )
        {
            this.ValidateNativeSourcePath( nativeFolder );
            this.NativeSourcePath = nativeFolder;
            this.Initialize();
        }

        public GeneratedPipeline( 
            string name, 
            string indexTitle,
            string nativeFolder, 
            string inputFolder, 
            string outputFolder, 
            string siteConfigReferenceRoot, 
            List<string> namespaces,
            bool enableLooseTypeComparisons )
            : base( name, indexTitle, inputFolder, outputFolder, siteConfigReferenceRoot, namespaces, enableLooseTypeComparisons )
        {
            this.ValidateNativeSourcePath( nativeFolder );
            this.NativeSourcePath = nativeFolder;
            this.Initialize();
        }

        public override List<DefinedType> Deserialize()
        {
            var types = this.Source.Deserialize();
            var disjointSet = ( (GeneratedDeserializer)this.Source ).DisjointSet;

            if( disjointSet != null )
            {
                // Add types at global scope that aren't projected.
                // TBD: Do we really want to do this? Or should we 
                // create a separate collection?
                var globalTypes = disjointSet.Where( t => t.Namespace != null && t.Namespace.IsGlobalNamespace );

                types.AddRange( globalTypes );
            }

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
                    this.Namespaces );
                this.Destination.Serialize();
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
            this.Source = new GeneratedDeserializer(
                this.NativeSourcePath,
                this.InputFolder,
                schemaPath,
                this.Namespaces,
                this.EnableLooseTypeComparisons );
        }

        private void ValidateNativeSourcePath( string nativePath )
        {
            if( !Directory.Exists( nativePath ) )
            {
                string message = String.Format( "{0} does not exist", nativePath );
                throw new ArgumentException( message, "nativePath" );
            }
        }

        public string NativeSourcePath
        {
            get;
            protected set;
        }
    }
}
