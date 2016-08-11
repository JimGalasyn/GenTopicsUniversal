using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using OsgContentPublishing.ReferencePipelineLib.Deserializers;
using OsgContentPublishing.ReferencePipelineLib.Serializers;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using System.Diagnostics;

namespace OsgContentPublishing.ReferencePipelineLib.Pipelines
{
    public class ManagedAssemblyPipeline : Pipeline
    {
        public ManagedAssemblyPipeline( string name, string indexTitle, string inputFolder, string assemblyPath, string outputFolder, string siteConfigReferenceRoot, List<string> namespaces )
            : base( name, indexTitle, inputFolder, outputFolder, siteConfigReferenceRoot, namespaces )
        {
            this.ValidateAssemblyPath( assemblyPath );
            this.AssemblyPath = assemblyPath;
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

        public override List<DefinedType> Deserialize()
        {
            this.Types = this.Source.Deserialize();

            return this.Types;
        }

        public override List<DefinedType> Generate()
        {
            this.Deserialize();
            this.Serialize( this.Types );

            return this.Types;
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
    }
}
