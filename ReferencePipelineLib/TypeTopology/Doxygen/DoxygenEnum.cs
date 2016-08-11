using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class DoxygenEnum : DoxygenType
    {
        public DoxygenEnum( DoxType doxType )
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

            if( this.UnderlyingType.EnumValues != null )
            {
                this.Members = this.UnderlyingType.EnumValues.Select( m => 
                    DoxygenMember.CreateEnumValue( m, this ) as DefinedMember ).ToList();
            }
            else if( this.UnderlyingType.Fields != null )
            {
                this.Members = this.UnderlyingType.Fields.Select( f => 
                    DoxygenMember.CreateMember( f, this ) as DefinedMember ).ToList();
            }
        }

        public override LanguageElement LanguageElement
        {
            get { return LanguageElement.Enum; }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} enum", this.FullName );
            return toString;
        }
    }
}
