using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Managed
{
    public class ManagedProperty : ManagedMember
    {
        public ManagedProperty( MemberDef memberDef, DoxygenType parentType )
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
                    //PropertyInfo propertyInfo = this.UnderlyingMember as PropertyInfo;
                    //this._type = TypeFactory.CreateType( propertyInfo.PropertyType );
                }

                return this._type;
            }
        }

        public override bool IsPublic
        {
            get
            {
                throw new NotImplementedException();
//                return this.UnderlyingPropertyInfo.GetGetMethod().IsPublic;
            }
        }

        public override bool IsPrivate
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingPropertyInfo.GetGetMethod().IsPrivate;
            }
        }

        public override bool IsStatic
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingPropertyInfo.GetGetMethod().IsStatic;
            }
        }

        //public ManagedMethod GetMethod
        //{
        //    get
        //    {
        //        if( this._getMethod == null )
        //        {
        //            MethodInfo getMethod = this.UnderlyingPropertyInfo.GetGetMethod();
        //            this._getMethod = ManagedMember.CreateMember( 
        //                getMethod, 
        //                this.ParentType as AssemblyType ) as ManagedMethod;
        //        }

        //        return this._getMethod;
        //    }
        //}

        //public ManagedMethod SetMethod
        //{
        //    get
        //    {
        //        if( this._setMethod == null )
        //        {
        //            MethodInfo setMethod = this.UnderlyingPropertyInfo.GetSetMethod();
        //            if( setMethod != null )
        //            {
        //                this._setMethod = ManagedMember.CreateMember( 
        //                    setMethod, 
        //                    this.ParentType as AssemblyType ) as ManagedMethod;
        //            }
        //        }

        //        return this._setMethod;
        //    }
        //}

        //public PropertyInfo UnderlyingPropertyInfo
        //{
        //    get
        //    {
        //        return this.UnderlyingMember as PropertyInfo;
        //    }
        //}

        public override string ToString()
        {
            string toString = String.Format( "{0}.{1} property", this.ParentType.Name, this.Name );
            return toString;
        }

        //private ManagedMethod _getMethod;
        //private ManagedMethod _setMethod;

    }
}
