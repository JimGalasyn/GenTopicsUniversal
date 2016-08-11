using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using ReflectionUtilities;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly
{
    public class AssemblyAttribute : AssemblyType
    {
        public AssemblyAttribute( ObservableType observableType ) : base( observableType )
        {
        }

        protected override void InitializeMembers()
        {
            base.InitializeMembers();

            var members = this.UnderlyingType.Members.Select( m =>
                AssemblyMember.CreateMember( m, this ) as DefinedMember );
            this.Members = members.ToList();
        }


        public override LanguageElement LanguageElement
        {
            get
            {
                return LanguageElement.Attribute;
            }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} attribute", this.Name );
            return toString;
        }
    }
}
