using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.Deserializers
{
    // XDCMake Reference
    // http://msdn.microsoft.com/en-us/library/ms177247.aspx
    public class XdcDeserializer : XmlDeserializer
    {
        public XdcDeserializer( string inputFolder, string schemaPath, List<string> namespaces )
            : base( inputFolder, schemaPath, namespaces )
        {

        }
    }
}
