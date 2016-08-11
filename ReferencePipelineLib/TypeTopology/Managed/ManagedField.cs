using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Managed
{
    public class ManagedField : ManagedMember
    {
        public ManagedField( MemberDef memberDef, DoxygenType parentType )
            : base( memberDef, parentType )
        {
        }

        public override DefinedType Type
        {
            get
            {
                if( this._type == null )
                {
                    //FieldInfo fieldInfo = this.UnderlyingMember as FieldInfo;
                    //this._type = TypeFactory.CreateType( fieldInfo.FieldType );
                }

                return this._type;
            }
        }

        public override bool IsPublic
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingFieldInfo.IsPublic;
            }
        }

        public override bool IsPrivate
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingFieldInfo.IsPrivate; 
            }
        }

        public override bool IsStatic
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingFieldInfo.IsStatic;
            }
        }

        //public FieldInfo UnderlyingFieldInfo
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //        //return this.UnderlyingMember as FieldInfo;
        //    }
        //}

        public override string ToString()
        {
            string toString = String.Format( "{0}.{1} field", this.ParentType.Name, this.Name );
            return toString;
        }
    }
}
