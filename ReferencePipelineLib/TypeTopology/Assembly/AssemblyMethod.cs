using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly
{
    public class AssemblyMethod : AssemblyMember
    {
        public AssemblyMethod( MethodBase methodInfo, AssemblyType parentType )
            : base( methodInfo, parentType )
        {
            this.InitializeOverloadProperties();
        }

        protected void InitializeOverloadProperties()
        {
            var overloadAttributeData = this.AttributeData.FirstOrDefault( a =>
                a.AttributeType.Name == overloadAttributeName );
            if( overloadAttributeData != null )
            {
                this.IsOverload = true;
                this.OverloadName = overloadAttributeData.ConstructorArguments[0].Value as string;
            }
        }

        public string OverloadName
        {
            get;
            private set;
        }

        //public override string Signature
        //{
        //    get
        //    {
        //        return Utilities.GetFullNameParamsString( this.UnderlyingMember as MethodBase );
        //    }
        //}

        public override DefinedType Type
        {
            get
            {
                if( this._type == null )
                {
                    this._type = TypeFactory.CreateAssemblyType( this.UnderlyingMethodInfo.ReturnType );
                }

                return this._type;
            }
        }

        public virtual DefinedType ReturnValue
        {
            get
            {
                return this.Type;
            }
        }

        public override bool IsPublic
        {
            get
            {
                return this.UnderlyingMethodInfo.IsPublic;
            }
        }

        public override bool IsPrivate
        {
            get
            {
                return this.UnderlyingMethodInfo.IsPrivate;
            }
        }

        public override bool IsStatic
        {
            get
            {
                return this.UnderlyingMethodInfo.IsStatic;
            }
        }

        public override bool IsConst
        {
            get
            {
                return false;
            }
        }

        public override bool IsMutable
        {
            get
            {
                return false;
            }
        }


        public override List<DefinedParameter> Parameters
        {
            get
            {
                if( this._parameters == null )
                {
                    ParameterInfo[] parameters = this.UnderlyingMethodInfo.GetParameters();

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

        public MethodInfo UnderlyingMethodInfo 
        {
            get
            {
                return this.UnderlyingMember as MethodInfo;
            }
        }

        public override LanguageElement LanguageElement
        {
            get 
            { 
                return LanguageElement.Method; 
            }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0}.{1} method", this.ParentType.Name, this.Name );
            return toString;
        }


        protected List<DefinedParameter> _parameters;
        protected const string overloadAttributeName = "OverloadAttribute";
    }
}
