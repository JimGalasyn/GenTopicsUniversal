using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class DoxygenInterface : DoxygenType
    {
        public DoxygenInterface( DoxType doxType )
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

            var members = this.UnderlyingType.MemberDefs.Select( m => DoxygenMember.CreateMember( m, this ) as DefinedMember );
            this.Members = members.ToList();
        }

        public override LanguageElement LanguageElement
        {
            get { return LanguageElement.Interface; }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} interface", this.FullName );
            return toString;
        }
    }
}
