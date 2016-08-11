using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly
{
    public class AssemblyProperty : AssemblyMember
    {
        public AssemblyProperty( PropertyInfo propertyInfo, AssemblyType parentType )
            : base( propertyInfo, parentType )
        {
        }

        public override DefinedType Type
        {
            get
            {
                if( this._type == null )
                {
                    PropertyInfo propertyInfo = this.UnderlyingMember as PropertyInfo;
                    this._type = TypeFactory.CreateAssemblyType( propertyInfo.PropertyType );
                }

                return this._type;
            }
        }

        public override bool IsPublic
        {
            get
            {
                return this.UnderlyingPropertyInfo.GetGetMethod().IsPublic;
            }
        }

        public override bool IsPrivate
        {
            get
            {
                return this.UnderlyingPropertyInfo.GetGetMethod().IsPrivate;
            }
        }

        public override bool IsStatic
        {
            get
            {
                return this.UnderlyingPropertyInfo.GetGetMethod().IsStatic;
            }
        }

        public AssemblyMethod GetMethod
        {
            get
            {
                if( this._getMethod == null )
                {
                    MethodInfo getMethod = this.UnderlyingPropertyInfo.GetGetMethod();
                    this._getMethod = AssemblyMember.CreateMember( 
                        getMethod, 
                        this.ParentType as AssemblyType ) as AssemblyMethod;
                }

                return this._getMethod;
            }
        }

        public AssemblyMethod SetMethod
        {
            get
            {
                if( this._setMethod == null )
                {
                    MethodInfo setMethod = this.UnderlyingPropertyInfo.GetSetMethod();
                    if( setMethod != null )
                    {
                        this._setMethod = AssemblyMember.CreateMember( 
                            setMethod, 
                            this.ParentType as AssemblyType ) as AssemblyMethod;
                    }
                }

                return this._setMethod;
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
                return( this.SetMethod != null );
            }
        }



        public PropertyInfo UnderlyingPropertyInfo
        {
            get
            {
                return this.UnderlyingMember as PropertyInfo;
            }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0}.{1} property", this.ParentType.Name, this.Name );
            return toString;
        }

        private AssemblyMethod _getMethod;
        private AssemblyMethod _setMethod;

    }
}
