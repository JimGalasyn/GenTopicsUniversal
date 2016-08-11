using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Projected
{
    public class ProjectedNamespace : ProjectedType
    {
        public ProjectedNamespace( DoxygenType nativeType, AssemblyType assemblyType )
            : base( nativeType, assemblyType )
        {
        }

        public override bool IsNamespace
        {
            get
            {
                return true;
            }
        }
    }
}
