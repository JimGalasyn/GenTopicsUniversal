using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Native
{
    public class NativeField : NativeMember
    {
        public NativeField( MemberDef memberDef, NativeType parentType )
            : base( memberDef, parentType )
        {
        }
    }
}
