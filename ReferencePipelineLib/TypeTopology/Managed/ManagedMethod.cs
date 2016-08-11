using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Managed
{
    public class ManagedMethod : ManagedMember
    {
        public ManagedMethod( MemberDef memberDef, DoxygenType parentType )
            : base( memberDef, parentType )
        {
        }

        public override DefinedType Type
        {
            get
            {
                if( this._type == null )
                {
                    throw new NotImplementedException();
                    //this._type = TypeFactory.CreateType( this.UnderlyingMethodInfo.ReturnType );
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
                throw new NotImplementedException();
                //return this.UnderlyingMethodInfo.IsPublic;
            }
        }

        public override bool IsPrivate
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingMethodInfo.IsPrivate;
            }
        }

        public override bool IsStatic
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingMethodInfo.IsStatic;
            }
        }

        public new List<ManagedParameter> Parameters
        {
            get
            {
                if( this._parameters == null )
                {
                    throw new NotImplementedException();
                    //ParameterInfo[] parameters = this.UnderlyingMethodInfo.GetParameters();

                    //if( parameters.Length > 0 )
                    //{
                    //    this._parameters = parameters.Select( p => new ManagedParameter( p, this ) ).ToList();
                    //}
                    //else
                    //{
                    //    this._parameters = new List<ManagedParameter>();
                    //}
                }

                return this._parameters;
            }
        }

        //public MethodInfo UnderlyingMethodInfo 
        //{
        //    get
        //    {
        //        return this.UnderlyingMember as MethodInfo;
        //    }
        //}

        public override string ToString()
        {
            string toString = String.Format( "{0}.{1} method", this.ParentType.Name, this.Name );
            return toString;
        }


        protected List<ManagedParameter> _parameters;
    }
}
