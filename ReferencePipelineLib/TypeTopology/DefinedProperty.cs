using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    public class DefinedProperty : DefinedMember
    {
        public DefinedProperty( MemberDef memberDef ) : base( memberDef )
        {

        }

        public DefinedProperty( PropertyInfo propertyInfo, DefinedType parentType )
            : base( propertyInfo, parentType )
        {

        }

        protected override void Initialize( MemberInfo memberInfo, DefinedType parentType )
        {
            PropertyInfo propertyInfo = memberInfo as PropertyInfo;
            if( propertyInfo != null )
            {
                base.Initialize( propertyInfo, parentType );

                // TBD
                //this.Type = DefinedType.CreateType( propertyInfo.PropertyType );

                this.IsReadOnly = !propertyInfo.CanWrite;

                // TBD: Get these from the accessor methods.
                //this.IsPublic = fieldInfo.IsPublic;
                //this.IsPrivate = fieldInfo.IsPrivate;
                //this.IsStatic = fieldInfo.IsStatic;


            }
            else
            {
                throw new ArgumentException( "is not a PropertyInfo", "memberInfo" );
            }
        }

        public bool IsReadOnly
        {
            get;
            protected set;
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
