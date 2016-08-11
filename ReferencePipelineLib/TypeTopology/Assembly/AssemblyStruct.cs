using ReflectionUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly
{
    public class AssemblyStruct : AssemblyType
    {
        public AssemblyStruct( ObservableType observableType )
            : base( observableType )
        {
            if( !observableType.IsStruct )
            {
                throw new ArgumentException( "must be a struct", "observableType" );
            }
        }

        protected override void InitializeMembers()
        {
            base.InitializeMembers();

            var fields = this.UnderlyingType.Fields.Select( f => AssemblyMember.CreateMember( f, this ) as DefinedMember );
            var fieldList = fields.ToList();
            this.Members = fieldList;
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} struct", this.FullName );
            return toString;
        }

        public override LanguageElement LanguageElement
        {
            get { return LanguageElement.Struct; }
        }
    }
}
