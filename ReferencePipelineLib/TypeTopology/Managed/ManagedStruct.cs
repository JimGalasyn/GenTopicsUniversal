using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Managed
{
    public class ManagedStruct : ManagedType
    {
        public ManagedStruct( DoxType doxType )
            : base( doxType )
        {
            if( !doxType.IsStruct )
            {
                throw new ArgumentException( "must be a struct", "doxType" );
            }
        }

        protected override void InitializeMembers()
        {
            base.InitializeMembers();

            var fields = this.UnderlyingType.Fields.Select( f => ManagedMember.CreateMember( f, this ) as DefinedMember );
            var fieldList = fields.ToList();
            this.Members = fieldList;
        }

        public override string ToString()
        {
            string toString = String.Format( "{0}.{1} struct", this.Namespace, this.Name );
            return toString;
        }
    }
}
