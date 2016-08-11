using ReflectionUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly
{
    public class AssemblyInterface : AssemblyType
    {
        public AssemblyInterface( ObservableType observableType )
            : base( observableType )
        {
            if( !observableType.IsInterface )
            {
                throw new ArgumentException( "must be an interface", "observableType" );
            }
        }

        protected override void InitializeMembers()
        {
            base.InitializeMembers();

            var members = this.UnderlyingType.Members.Select( m => AssemblyMember.CreateMember( m, this ) as DefinedMember );
            this.Members = members.ToList();
        }

        protected override void InitializeBaseTypes()
        {
            base.InitializeBaseTypes();

            if( this.UnderlyingType.HasInterfaces )
            {
                this.Interfaces = this.UnderlyingType.Interfaces.Select( ot =>
                    TypeFactory.CreateAssemblyType( ot ) as DefinedType ).ToList();

                this.BaseTypes.AddRange( this.Interfaces );
            }
            else
            {
                this.Interfaces = new List<DefinedType>();
            }
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
