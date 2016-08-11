using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using OsgContentPublishing.ReferencePipelineLib.Deserializers;
using OsgContentPublishing.ReferencePipelineLib.Serializers;

namespace OsgContentPublishing.ReferencePipelineLib.Pipelines
{
    public class ManagedAssemblyPipeline : Pipeline
    {
        public ManagedAssemblyPipeline( string inputFolder, string assemblyPath, string outputFolder, string siteConfigReferenceRoot, List<string> namespaces )
            : base( inputFolder, outputFolder, namespaces )
        {
            this.ValidateAssemblyPath( assemblyPath );
            this.AssemblyPath = assemblyPath;
            this.SiteConfigReferenceRoot = siteConfigReferenceRoot;
            this.Initialize();
        }

        private void Initialize()
        {   
            string schemaPath = Path.Combine( this.InputFolder, "compound.xsd" );
            this.Source = new ManagedAssemblyDeserializer( 
                this.InputFolder, 
                schemaPath, 
                this.AssemblyPath, 
                this.Namespaces );
        }

        public override void Generate()
        {
            var types = this.Source.Deserialize();
            //var disjointSet = ( (RidlDeserializer)this.Source ).DisjointSet;

            //var globalTypes = disjointSet.Where( t => t.Namespace.IsGlobalNamespace );

            //types.AddRange( globalTypes );

            this.Destination = new HtmlSerializer( types, this.OutputFolder, this.SiteConfigReferenceRoot );
            this.Destination.Serialize();
        }



        private void ValidateAssemblyPath( string assemblyPath )
        {
            if( !File.Exists( assemblyPath ) )
            {
                string message = String.Format( "{0} does not exist", assemblyPath );
                throw new ArgumentException( message, "assemblyPath" );
            }
        }

        public string AssemblyPath
        {
            get;
            protected set;
        }

        public string SiteConfigReferenceRoot
        {
            get;
            private set;
        }

    }
}
