using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class InnerClass : CompoundRef
    {
        public InnerClass( XElement compoundRefElement, DoxType parentType ) : base( compoundRefElement, parentType )
        {
            this.IsInnerClass = true;
        }
    }
}
