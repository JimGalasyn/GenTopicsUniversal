using ReflectionUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly
{
    public class AssemblyEnum : AssemblyType
    {
        public AssemblyEnum( ObservableType observableType )
            : base( observableType )
        {
            if( !observableType.IsEnum )
            {
                throw new ArgumentException( "must be an enum", "observableType" );
            }
        }


        protected override void InitializeMembers()
        {
            base.InitializeMembers();

            var fields = this.UnderlyingType.Fields.Select( f => AssemblyMember.CreateMember( f, this ) as DefinedMember );
            this.Members = fields.ToList();
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
