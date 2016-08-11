using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class DoxygenProperty : DoxygenMember
    {
        public DoxygenProperty( MemberDef memberDef, DoxygenType parentType )
            : base( memberDef, parentType )
        {
        }

        public override LanguageElement LanguageElement
        {
            get { return LanguageElement.Property; }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0}.{1} property", this.ParentType.Name, this.Name );
            return toString;
        }
    }
}
