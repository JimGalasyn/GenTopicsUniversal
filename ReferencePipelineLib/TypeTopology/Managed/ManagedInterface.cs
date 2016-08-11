using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Managed
{
    public class ManagedInterface : ManagedType
    {
        public ManagedInterface( DoxType doxType )
            : base( doxType )
        {
            if( !doxType.IsInterface )
            {
                throw new ArgumentException( "must be an interface", "doxType" );
            }
        }

        protected override void InitializeMembers()
        {
            base.InitializeMembers();

            //var members = this.UnderlyingType.Members.Select( m => ManagedMember.CreateMember( m, this ) as DefinedMember );
            //this.Members = members.ToList();
        }

        public override string ToString()
        {
            string toString = String.Format( "{0}.{1} interface", this.Namespace, this.Name );
            return toString;
        }
    }
}
