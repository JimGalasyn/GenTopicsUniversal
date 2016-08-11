using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using ReflectionUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OsgContentPublishing.ReferencePipelineLib.Deserializers
{
    public class ManagedDeserializer : DoxygenDeserializer
    {
        public ManagedDeserializer( string inputFolder, string schemaPath, List<string> namespaces )
            : base( inputFolder, schemaPath, namespaces )
        {
        }

        public override List<DefinedType> Deserialize()
        {
            return base.Deserialize();
        }
    }
}
