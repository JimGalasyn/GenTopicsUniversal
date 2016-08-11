using ReflectionUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly
{
    public class AssemblyClass : AssemblyType
    {
        public AssemblyClass( ObservableType observableType )
            : base( observableType )
        {
            if( !observableType.IsClass )
            {
                throw new ArgumentException( "must be a class", "observableType" );
            }
        }

        protected override void InitializeMembers()
        {
            base.InitializeMembers();

            var members = this.UnderlyingType.Members.Select( m => 
                AssemblyMember.CreateMember( m, this ) as DefinedMember );
            this.Members = members.ToList();

            if( this.UnderlyingType.HasConstructors )
            {
                var ctors = this.UnderlyingType.Constructors.Select( c => 
                    AssemblyMember.CreateMember( c, this ) as DefinedMember );
                this.Members.AddRange( ctors.ToList() );
            }

            if( this.UnderlyingType.BaseType != null && 
                this.UnderlyingType.BaseType.FullName != "System.Object" )
            {
                // Won't happen for WinRT classes, beacuse they all implement
                // the IInspectable interface, which is projected as System.Object.
                this.BaseType = TypeFactory.CreateAssemblyType( this.UnderlyingType.BaseType );
                this.BaseTypes.Add( this.BaseType );
            }

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

        //public override List<DefinedType> BaseTypes
        //{
        //    get
        //    {
        //        return base.BaseTypes;
        //    }
        //    protected set
        //    {
        //        base.BaseTypes = value;
        //    }
        //}

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
