using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class EnumValue
    {
        public EnumValue( XElement element )
        {
            this.name = Utilities.TryGetChildElementValue( element, "name" );
            this.briefdescription = Utilities.TryGetChildElementValue( element, "briefdescription" );
            this.detaileddescription = Utilities.TryGetChildElementValue( element, "detaileddescription" );
            this.initializer = Utilities.TryGetChildElementValue( element, "initializer" );
        }

        public string name
        {
            get;
            private set;
        }

        public string briefdescription
        {
            get;
            private set;
        }

        public string detaileddescription
        {
            get;
            private set;
        }

        public string initializer
        {
            get;
            private set;
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} {1}", this.name, this.initializer );
            return toString;
        }
    }
}
