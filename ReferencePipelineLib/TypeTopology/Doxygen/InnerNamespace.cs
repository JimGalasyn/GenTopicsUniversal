using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class InnerNamespace : CompoundRef
    {
        public InnerNamespace( XElement compoundRefElement, DoxType parentType )
            : base( compoundRefElement, parentType )
        {
            this.IsInnerNamespace = true;
        }
    }
}
