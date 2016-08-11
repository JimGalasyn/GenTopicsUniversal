using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    public class Constructor : DefinedMember
    {
        public Constructor( MemberDef memberDef ) : base( memberDef )
        {

        }

        public Constructor( ConstructorInfo ctorInfo, DefinedType parentType )
            : base( ctorInfo, parentType )
        {
            //this.Initialize( methodInfo );
        }

        protected override void Initialize( MemberInfo memberInfo, DefinedType parentType )
        {
            ConstructorInfo constructorInfo = memberInfo as ConstructorInfo;
            if( constructorInfo != null )
            {
                base.Initialize( constructorInfo, parentType );

                // TBD: void DefinedType
                //this.Type = DefinedType.CreateType( constructorInfo. );

                ParameterInfo[] parameters = constructorInfo.GetParameters();
                if( parameters.Length > 0 )
                {
                    this.Parameters = parameters.Select( p => new DefinedParameter( p, this ) ).ToList();
                }
                else
                {
                    this.Parameters = new List<DefinedParameter>();
                }
            }
            else
            {
                throw new ArgumentException( "is not a ConstructorInfo", "memberInfo" );
            }
        }

        public override DefinedType Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public List<DefinedParameter> Parameters
        {
            get;
            protected set;
        }

    }
}
