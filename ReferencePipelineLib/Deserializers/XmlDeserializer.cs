using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology;

namespace OsgContentPublishing.ReferencePipelineLib.Deserializers
{
    public class XmlDeserializer : Deserializer
    {
        public XmlDeserializer( string inputFolder, string schemaPath, List<string> namespaces )
            : base( inputFolder, namespaces )
        {
            this.AssignSchemaPath( schemaPath );
        }

        public XmlDeserializer( 
            string inputFolder, 
            string schemaPath, 
            List<string> namespaces,
            bool enableLooseTypecomparisons )
            : base( inputFolder, namespaces, enableLooseTypecomparisons )
        {
            this.AssignSchemaPath( schemaPath );
        }

        private void AssignSchemaPath( string schemaPath )
        {
            if( File.Exists( schemaPath ) )
            {
                this.SchemaPath = schemaPath;
            }
            else
            {
                throw new ArgumentException( "Path does not exist", "schemaPath" );
            }
        }

        public override List<DefinedType> Deserialize()
        {
            throw new NotImplementedException();
        }

        public string SchemaPath
        {
            get;
            protected set;
        }
    }
}
