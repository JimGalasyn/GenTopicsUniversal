using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    public class DefinedMethod : DefinedMember
    {
        public DefinedMethod( MemberDef memberDef ) : base( memberDef )
        {

        }

        public DefinedMethod( MethodInfo methodInfo, DefinedType parentType )
            : base( methodInfo, parentType )
        {
            //this.Initialize( methodInfo );
        }

        protected override void Initialize( MemberInfo memberInfo, DefinedType parentType )
        {
            MethodInfo methodInfo = memberInfo as MethodInfo;
            if( methodInfo != null )
            {
                base.Initialize( methodInfo, parentType );

                //this.IsPublic = methodInfo.IsPublic;
                this.IsPrivate = methodInfo.IsPrivate;
                this.IsStatic = methodInfo.IsStatic;

                this.IsAbstract = methodInfo.IsAbstract;
                this.IsVirtual = methodInfo.IsVirtual;

                //this.Type = DefinedType.CreateType( methodInfo.ReturnType );

                ParameterInfo[] parameters = methodInfo.GetParameters();
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
                throw new ArgumentException( "is not a MethodInfo", "memberInfo" );
            }
        }

        public override DefinedType Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsAbstract
        {
            get;
            protected set;
        }

        public bool IsVirtual
        {
            get;
            protected set;
        }

        public List<DefinedParameter> Parameters
        {
            get;
            protected set;
        }
    }
}
