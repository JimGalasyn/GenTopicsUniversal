using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class DoxygenConstructor : DoxygenMethod
    {
        public DoxygenConstructor( MemberDef memberDef, DoxygenType parentType )
            : base( memberDef, parentType )
        {
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} ctor", this.ParentType.Name );
            return toString;
        }
    }
}
