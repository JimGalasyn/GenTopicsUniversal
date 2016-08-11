using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology;

namespace OsgContentPublishing.ReferencePipelineLib.Deserializers
{
    public class NativeDeserializer : DoxygenDeserializer
    {
        public NativeDeserializer( string inputFolder, string schemaPath, List<string> namespaces )
            : base( inputFolder, schemaPath, namespaces )
        {

        }

        public NativeDeserializer( 
            string inputFolder, 
            string schemaPath, 
            List<string> namespaces, 
            bool enableLooseTypecomparisons )
            : base( inputFolder, schemaPath, namespaces, enableLooseTypecomparisons )
        {

        }

        public override List<DefinedType> Deserialize()
        {
            return base.Deserialize();
        }
    }
}
