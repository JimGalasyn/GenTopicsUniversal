using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class DoxygenClass : DoxygenType
    {
        public DoxygenClass( DoxType doxType )
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

            if( this.UnderlyingType.MemberDefs != null && 
                this.UnderlyingType.MemberDefs.Count > 0 )
            {
                var members = this.UnderlyingType.MemberDefs.Select( m => DoxygenMember.CreateMember( m, this ) as DefinedMember );
                this.Members = members.ToList();
            }
            else
            {
                this.Members = new List<DefinedMember>();
            }

            //var members = this.UnderlyingType.Members.Select( m => ManagedMember.CreateMember( m, this ) as DefinedMember );
            //this.Members = members.ToList();

            //if( this.UnderlyingType.HasConstructors )
            //{
            //    var ctors = this.UnderlyingType.Constructors.Select( c => ManagedMember.CreateMember( c, this ) as DefinedMember );
            //    this.Members.AddRange( ctors.ToList() );
            //}
        }

        public override LanguageElement LanguageElement
        {
            get { return LanguageElement.Class; }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} class", this.FullName );
            return toString;
        }
    }
}
