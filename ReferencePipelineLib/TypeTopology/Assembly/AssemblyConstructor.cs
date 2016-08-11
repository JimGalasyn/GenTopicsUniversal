using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly
{
    public class AssemblyConstructor : AssemblyMethod
    {
        public AssemblyConstructor( ConstructorInfo ctorInfo, AssemblyType parentType )
            : base( ctorInfo, parentType )
        {
        }

        public override DefinedType Type
        {
            get
            {
                return this.ParentType;
            }
        }

        public override bool IsPublic
        {
            get
            {
                return this.UnderlyingConstructorInfo.IsPublic;
            }
        }

        public override bool IsPrivate
        {
            get
            {
                return this.UnderlyingConstructorInfo.IsPrivate;
            }
        }

        public override bool IsStatic
        {
            get
            {
                return this.UnderlyingConstructorInfo.IsStatic;
            }
        }

        public override List<DefinedParameter> Parameters
        {
            get
            {
                if( this._parameters == null )
                {
                    ParameterInfo[] parameters = this.UnderlyingConstructorInfo.GetParameters();

                    if( parameters.Length > 0 )
                    {
                        this._parameters = parameters.Select( p => new AssemblyParameter( p, this ) as DefinedParameter ).ToList();
                    }
                    else
                    {
                        this._parameters = new List<DefinedParameter>();
                    }
                }

                return this._parameters;
            }
        }

        public ConstructorInfo UnderlyingConstructorInfo
        {
            get
            {
                return this.UnderlyingMember as ConstructorInfo;
            }
        }
        public override string ToString()
        {
            string toString = String.Format( "{0} ctor", this.ParentType.Name );
            return toString;
        }


        //protected List<ManagedParameter> _parameters;

    }
}
