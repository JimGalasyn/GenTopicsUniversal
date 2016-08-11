using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Managed
{
    public class ManagedConstructor : ManagedMethod
    {
        public ManagedConstructor( MemberDef memberDef, DoxygenType parentType )
            : base( memberDef, parentType )
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
                throw new NotImplementedException();
                //return this.UnderlyingConstructorInfo.IsPublic;
            }
        }

        public override bool IsPrivate
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingConstructorInfo.IsPrivate;
            }
        }

        public override bool IsStatic
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingConstructorInfo.IsStatic;
            }
        }

        //public List<ManagedParameter> Parameters
        //{
        //    get
        //    {
        //        if( this._parameters == null )
        //        {
        //            ParameterInfo[] parameters = this.UnderlyingConstructorInfo.GetParameters();

        //            if( parameters.Length > 0 )
        //            {
        //                this._parameters = parameters.Select( p => new ManagedParameter( p, this ) ).ToList();
        //            }
        //            else
        //            {
        //                this._parameters = new List<ManagedParameter>();
        //            }
        //        }

        //        return this._parameters;
        //    }
        //}

        //public ConstructorInfo UnderlyingConstructorInfo
        //{
        //    get
        //    {
        //        return this.UnderlyingMember as ConstructorInfo;
        //    }
        //}

        public override string ToString()
        {
            string toString = String.Format( "{0} ctor", this.ParentType.Name );
            return toString;
        }


        //protected List<ManagedParameter> _parameters;

    }
}
