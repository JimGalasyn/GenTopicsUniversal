using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class DoxygenEvent : DoxygenMethod
    {
        public DoxygenEvent( MemberDef memberDef, DoxygenType parentType )
            : base( memberDef, parentType )
        {
        }

        public override LanguageElement LanguageElement
        {
            get { return LanguageElement.Event; }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0}.{1} event", this.ParentType.Name, this.Name );
            return toString;
        }
    }
}
