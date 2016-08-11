using OsgContentPublishing.ReferencePipelineLib.Deserializers;
using OsgContentPublishing.ReferencePipelineLib.Serializers;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace OsgContentPublishing.ReferencePipelineLib.Pipelines
{
    public class ManagedPipeline : Pipeline
    {
        public ManagedPipeline( string name, string indexTitle, string inputFolder, string outputFolder, string siteConfigReferenceRoot, List<string> namespaces )
            : base( name, indexTitle, inputFolder, outputFolder, siteConfigReferenceRoot, namespaces )
        {
            this.Initialize();
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

        private void Initialize()
        {   
            // Create Deserializer and Serializer.
            string schemaPath = Path.Combine( this.InputFolder, "compound.xsd" );
            this.Source = new ManagedDeserializer( this.InputFolder, schemaPath, this.Namespaces );
        }
    }
}
