using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Managed
{
    public class ManagedEnum : ManagedType
    {
        public ManagedEnum( DoxType doxType )
            : base( doxType )
        {
            if( !doxType.IsEnum )
            {
                throw new ArgumentException( "must be an enum", "doxType" );
            }
        }


        protected override void InitializeMembers()
        {
            base.InitializeMembers();

            var fields = this.UnderlyingType.Fields.Select( f => ManagedMember.CreateMember( f, this ) as DefinedMember );
            this.Members = fields.ToList();
        }

        public override string ToString()
        {
            string toString = String.Format( "{0}.{1} enum", this.Namespace, this.Name );
            return toString;
        }

    }
}
