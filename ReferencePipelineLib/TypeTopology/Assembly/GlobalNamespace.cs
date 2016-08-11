using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly
{
    public class GlobalNamespace : AssemblyNamespace
    {
        public GlobalNamespace( string namespaceFullName )
            : base( namespaceFullName )
        {
            this.Name = String.Empty;
            this.FullName = string.Empty;
            this.UnderlyingType = null;
        }


        public override DefinedType Namespace
        {
            get
            {
                return null;
            }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} namespace", TypeFactory.GlobalNamespaceName );
            return toString;
        }
    }
}
