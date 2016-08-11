using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Managed;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    public class DefinedField : DefinedMember
    {
                
        public DefinedField( MemberDef memberDef ) : base( memberDef )
        {

        }


        public DefinedField( FieldInfo fieldInfo, DefinedType parentType )
            : base( fieldInfo, parentType )
        {

        }

        protected override void Initialize( MemberInfo memberInfo, DefinedType parentType )
        {
            FieldInfo fieldInfo = memberInfo as FieldInfo;
            if( fieldInfo != null )
            {
                base.Initialize( fieldInfo, parentType );

                this.IsPublic = fieldInfo.IsPublic;
                this.IsPrivate = fieldInfo.IsPrivate;
                this.IsStatic = fieldInfo.IsStatic;

                //this.Type = ManagedType.CreateType( fieldInfo.FieldType );
            }
            else
            {
                throw new ArgumentException( "is not a FieldInfo", "memberInfo" );
            }
        }

        public override DefinedType Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
