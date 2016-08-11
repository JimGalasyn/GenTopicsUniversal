using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class Member
    {
        public Member( XElement element, Compound parentCompound )
        {
            XAttribute refidAttr = element.Attribute( "refid" );
            refid = refidAttr.Value;

            XAttribute kindAttr = element.Attribute( "kind" );
            kind = kindAttr.Value;

            name = Utilities.GetChildElement( element, "name" ).Value;

            //this.SourceFile = Path.ChangeExtension( refid, "xml" );
            this.SourceFile = parentCompound.SourceFile;

            if( parentCompound.kind == "namespace" )
            {
                // Fixup Doxygen bug with RIDL: 
                // namespaces can't have variables; these are actually runtime classes.
                // TBD: This may break non-RIDL code
                if( this.kind == "variable" )
                {
                    this.kind = "class";
                }
            }
        }

        public string SourceFile
        {
            get;
            private set;
        }

        public string refid
        {
            get;
            private set;
        }

        public string kind
        {
            get;
            private set;
        }

        public string name
        {
            get;
            private set;
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} | {1}", this.name, this.kind );
            return toString;
        }
    }
}
