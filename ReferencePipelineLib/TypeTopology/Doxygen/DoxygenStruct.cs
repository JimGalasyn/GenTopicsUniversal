using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class DoxygenStruct : DoxygenType
    {
        public DoxygenStruct( DoxType doxType )
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

            var members = this.UnderlyingType.MemberDefs.Select( m => DoxygenMember.CreateMember( m, this ) as DefinedMember );
            this.Members = members.ToList();
        }

        public override LanguageElement LanguageElement
        {
            get { return LanguageElement.Struct; }
        }


        public override string ToString()
        {
            string toString = String.Format( "{0} struct", this.FullName );
            return toString;
        }
    }
}
