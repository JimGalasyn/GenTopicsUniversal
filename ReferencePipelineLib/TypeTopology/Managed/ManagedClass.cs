using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Managed
{
    public class ManagedClass : ManagedType
    {
        public ManagedClass( DoxType doxType )
            : base( doxType )
        {
            if( !doxType.IsClass )
            {
                throw new ArgumentException( "must be a class", "doxType" );
            }
        }

        protected override void InitializeMembers()
        {
            base.InitializeMembers();

            //var members = this.UnderlyingType.Members.Select( m => ManagedMember.CreateMember( m, this ) as DefinedMember );
            //this.Members = members.ToList();

            //if( this.UnderlyingType.HasConstructors )
            //{
            //    var ctors = this.UnderlyingType.Constructors.Select( c => ManagedMember.CreateMember( c, this ) as DefinedMember );
            //    this.Members.AddRange( ctors.ToList() );
            //}
        }

        public override string ToString()
        {
            string toString = String.Format( "{0}.{1} class", this.Namespace, this.Name );
            return toString;
        }
    }
}
